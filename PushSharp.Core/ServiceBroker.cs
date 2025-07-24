using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;

namespace AlphaOmega.PushSharp.Core
{
	/// <inheritdoc/>
	public class ServiceBroker<TNotification> : IServiceBroker<TNotification> where TNotification : INotification
	{
		private readonly BlockingCollection<TNotification> _notifications;
		private readonly List<ServiceWorker<TNotification>> _workers;
		private readonly Object _lockWorkers;

		private Boolean _running;

		/// <inheritdoc/>
		public event NotificationSuccessDelegate<TNotification> OnNotificationSucceeded;

		/// <inheritdoc/>
		public event NotificationFailureDelegate<TNotification> OnNotificationFailed;

		/// <summary>The size of parallel workers.</summary>
		[DefaultValue(1)]
		public UInt32 ScaleSize { get; private set; } = 1;

		/// <summary>Gets the service connection instance that is responsible for message distribution.</summary>
		public IServiceConnectionFactory<TNotification> ServiceConnectionFactory { get; set; }

		static ServiceBroker()
		{
			ServicePointManager.DefaultConnectionLimit = 100;
			ServicePointManager.Expect100Continue = false;
		}

		/// <summary>Create instance of <see cref="ServiceBroker&lt;TNotification&gt;"/> with push server connection factory.</summary>
		/// <param name="connectionFactory">The connection factory instance.</param>
		/// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> is required.</exception>
		public ServiceBroker(IServiceConnectionFactory<TNotification> connectionFactory)
		{
			this.ServiceConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

			this._lockWorkers = new Object();
			this._workers = new List<ServiceWorker<TNotification>>();
			this._running = false;

			this._notifications = new BlockingCollection<TNotification>();
		}

		/// <summary>Adds new push notification message to send.</summary>
		/// <param name="notification">The notification to send to the user.</param>
		public virtual void QueueNotification(TNotification notification)
			=> this._notifications.Add(notification);

		/// <inheritdoc/>
		public void Start()
		{
			Log.Trace.TraceEvent(TraceEventType.Start, 109, "Starting Service Broker...");

			if(this._running)
				return;

			this._running = true;
			this.ChangeScale(this.ScaleSize);
		}

		/// <inheritdoc/>
		public void Stop(Boolean immediately = false)
		{
			Log.Trace.TraceEvent(TraceEventType.Stop, 112, $"Stopping Service Broker... {nameof(this._running)}={this._running}; {nameof(immediately)}={immediately};");

			if(!this._running)
				throw new OperationCanceledException("ServiceBroker has already been signaled to Stop");

			this._running = false;
			this._notifications.CompleteAdding();

			lock(this._lockWorkers)
			{
				// Kill all workers right away
				if(immediately)
					this._workers.ForEach(sw => sw.Cancel());

				var all = Array.ConvertAll(this._workers.ToArray(), w => w.WorkerTask);

				Log.Trace.TraceEvent(TraceEventType.Stop, 113, "Waiting for all tasks ({0}) to finish", all.Length);
				Task.WaitAll(all);
				Log.Trace.TraceEvent(TraceEventType.Stop, 114, "All tasks completed successfully");

				this._workers.Clear();
			}
		}

		/// <summary>Change count of workers to use for notifications distribution.</summary>
		/// <param name="newScaleSize">The new scale size.</param>
		/// <exception cref="ArgumentOutOfRangeException">The size should be more that 0.</exception>
		public void ChangeScale(UInt32 newScaleSize)
		{
			if(newScaleSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(newScaleSize), "Must be Greater than Zero");

			Log.Trace.TraceEvent(TraceEventType.Verbose, 110, $"Changing workers scale... {nameof(this._running)}={this._running}; {nameof(newScaleSize)}={newScaleSize};");

			this.ScaleSize = newScaleSize;

			if(!this._running)
				return;

			lock(this._lockWorkers)
			{
				// Scale down
				while(this._workers.Count > this.ScaleSize)
				{
					this._workers[0].Cancel();
					this._workers.RemoveAt(0);
				}

				// Scale up
				while(this._workers.Count < this.ScaleSize)
				{
					var worker = new ServiceWorker<TNotification>(this, this.ServiceConnectionFactory.Create());
					this._workers.Add(worker);
					worker.Start();
				}

				Log.Trace.TraceEvent(TraceEventType.Verbose, 111, "Scaled Changed to: {0}", this._workers.Count);
			}
		}

		IEnumerable<TNotification> IServiceBroker<TNotification>.TakeMany()
			=> this._notifications.GetConsumingEnumerable();

		Boolean IServiceBroker<TNotification>.IsCompleted => this._notifications.IsCompleted;

		void IServiceBroker<TNotification>.RaiseNotificationSucceeded(TNotification notification)
			=> this.OnNotificationSucceeded?.Invoke(notification);

		void IServiceBroker<TNotification>.RaiseNotificationFailed(TNotification notification, AggregateException exception)
			=> OnNotificationFailed?.Invoke(notification, exception);
	}

	class ServiceWorker<TNotification> where TNotification : INotification
	{
		public ServiceWorker(IServiceBroker<TNotification> broker, IServiceConnection<TNotification> connection)
		{
			this.Broker = broker ?? throw new ArgumentNullException(nameof(broker));
			this.Connection = connection;

			this.CancelTokenSource = new CancellationTokenSource();
		}

		public IServiceBroker<TNotification> Broker { get; private set; }

		public IServiceConnection<TNotification> Connection { get; private set; }

		public CancellationTokenSource CancelTokenSource { get; private set; }

		public Task WorkerTask { get; private set; }

		public void Start()
		{
			this.WorkerTask = Task.Factory.StartNew(async delegate
			{
				while(!this.CancelTokenSource.IsCancellationRequested && !this.Broker.IsCompleted)
				{
					try
					{
						var toSend = new List<Task>();
						foreach(var n in this.Broker.TakeMany())
						{
							var t = this.Connection.Send(n);
							// Keep the continuation
							var count = t.ContinueWith(ct =>
							{
								var cn = n;
								var exc = t.Exception;

								if(exc == null)
									this.Broker.RaiseNotificationSucceeded(cn);
								else
								{
									Log.Trace.TraceData(TraceEventType.Error, 108, exc);
									this.Broker.RaiseNotificationFailed(cn, exc);
								}
							});

							// Let's wait for the continuation not the task itself
							toSend.Add(count);
						}

						if(toSend.Count <= 0)
							continue;

						try
						{
							Log.Trace.TraceInformation("Waiting for all tasks ({0}) to finish", toSend.Count);
							await Task.WhenAll(toSend).ConfigureAwait(false);
							Log.Trace.TraceInformation("All tasks completed successfully");
						} catch(Exception exc)
						{
							Log.Trace.TraceData(TraceEventType.Error, 102, exc);
						}
					} catch(Exception exc)
					{
						Log.Trace.TraceData(TraceEventType.Error, 103, exc);
					}
				}

				if(this.CancelTokenSource.IsCancellationRequested)
					Trace.TraceInformation("Cancellation was requested");
				if(this.Broker.IsCompleted)
					Trace.TraceInformation("Broker IsCompleted");

				Log.Trace.TraceEvent(TraceEventType.Verbose, 115, "Broker Task Ended");
			}, this.CancelTokenSource.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();

			this.WorkerTask.ContinueWith(t => Log.Trace.TraceData(TraceEventType.Error, 104, t.Exception),
			TaskContinuationOptions.OnlyOnFaulted);
		}

		public void Cancel()
			=> this.CancelTokenSource.Cancel();
	}
}