using System;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The service broker that is responsible to distributing messages between push servers.</summary>
	/// <typeparam name="TNotification">The strongly typed notification to send to specific push server.</typeparam>
	public interface IServiceBroker<TNotification> where TNotification : INotification
	{
		/// <summary>The event is fired when notification push message successfully sent.</summary>
		event NotificationSuccessDelegate<TNotification> OnNotificationSucceeded;

		/// <summary>The event is fired when we failed to send push notification message.</summary>
		event NotificationFailureDelegate<TNotification> OnNotificationFailed;

		/// <summary>Take messages from queue to send using service broker instance.</summary>
		/// <returns>The list of messages to send using service broker.</returns>
		System.Collections.Generic.IEnumerable<TNotification> TakeMany();

		/// <summary>Gets te status of notification queue.</summary>
		Boolean IsCompleted { get; }

		/// <summary>Invoke event when push notification message fired successfully.</summary>
		/// <param name="notification">The sent notification instance.</param>
		void RaiseNotificationSucceeded(TNotification notification);

		/// <summary>Invoke event when we failed to send push notification message.</summary>
		/// <param name="notification">The notification instance that we failed to send.</param>
		/// <param name="exception">The reason why we can't send push notification message.</param>
		void RaiseNotificationFailed(TNotification notification, AggregateException exception);

		/// <summary>Start sending push notification messages.</summary>
		void Start();

		/// <summary>Stop sending push notification messages.</summary>
		/// <param name="immediately">Force to stop sending or wait while all thread are finished.</param>
		void Stop(Boolean immediately = false);
	}
}