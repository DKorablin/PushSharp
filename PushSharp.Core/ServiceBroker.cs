using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net;

namespace AlphaOmega.PushSharp.Core
{
	public class ServiceBroker<TNotification> : IServiceBroker<TNotification> where TNotification : INotification
	{
		private readonly BlockingCollection<TNotification> _notifications;
		private readonly List<ServiceWorker<TNotification>> _workers;
		private readonly Object _lockWorkers;

		private Boolean _running;

		public event NotificationSuccessDelegate<TNotification> OnNotificationSucceeded;
		public event NotificationFailureDelegate<TNotification> OnNotificationFailed;

		public Int32 ScaleSize { get; private set; }

		public IServiceConnectionFactory<TNotification> ServiceConnectionFactory { get; set; }

		static ServiceBroker()
		{
			ServicePointManager.DefaultConnectionLimit = 100;
			ServicePointManager.Expect100Continue = false;
		}

		public ServiceBroker(IServiceConnectionFactory<TNotification> connectionFactory)
		{
			this.ServiceConnectionFactory = connectionFactory;

			this._lockWorkers = new Object();
			this._workers = new List<ServiceWorker<TNotification>>();
			this._running = false;

			this._notifications = new BlockingCollection<TNotification>();
			this.ScaleSize = 1;
		}

		public virtual void QueueNotification(TNotification notification)
			=> this._notifications.Add(notification);

		public IEnumerable<TNotification> TakeMany()
			=> this._notifications.GetConsumingEnumerable();

		public Boolean IsCompleted => this._notifications.IsCompleted;

		public void Start()
		{
			if(this._running)
				return;

			this._running = true;
			this.ChangeScale(this.ScaleSize);
		}

		public void Stop(Boolean immediately = false)
		{
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

				Log.Info("Stopping: Waiting on Tasks");
				Task.WaitAll(all);
				Log.Info("Stopping: Done Waiting on Tasks");

				this._workers.Clear();
			}
		}

		public void ChangeScale(Int32 newScaleSize)
		{
			if(newScaleSize <= 0)
				throw new ArgumentOutOfRangeException("newScaleSize", "Must be Greater than Zero");

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

				Log.Debug("Scaled Changed to: " + this._workers.Count);
			}
		}

		public void RaiseNotificationSucceeded(TNotification notification)
			=> this.OnNotificationSucceeded?.Invoke(notification);

		public void RaiseNotificationFailed(TNotification notification, AggregateException exception)
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
									this.Broker.RaiseNotificationFailed(cn, exc);
							});

							// Let's wait for the continuation not the task itself
							toSend.Add(count);
						}

						if(toSend.Count <= 0)
							continue;

						try
						{
							Log.Info("Waiting on all tasks {0}", toSend.Count);
							await Task.WhenAll(toSend).ConfigureAwait(false);
							Log.Info("All Tasks Finished");
						} catch(Exception ex)
						{
							Log.Error("Waiting on all tasks Failed: {0}", ex);
						}
						Log.Info("Passed WhenAll");

					} catch(Exception ex)
					{
						Log.Error("Broker.Take: {0}", ex);
					}
				}

				if(this.CancelTokenSource.IsCancellationRequested)
					Log.Info("Cancellation was requested");
				if(this.Broker.IsCompleted)
					Log.Info("Broker IsCompleted");

				Log.Debug("Broker Task Ended");
			}, this.CancelTokenSource.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();

			this.WorkerTask.ContinueWith(t =>
			{
				var ex = t.Exception;
				if(ex != null)
					Log.Error("ServiceWorker.WorkerTask Error: {0}", ex);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		public void Cancel()
			=> this.CancelTokenSource.Cancel();
	}
}