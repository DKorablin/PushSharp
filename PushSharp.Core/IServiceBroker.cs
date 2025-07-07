using System;

namespace AlphaOmega.PushSharp.Core
{
	public interface IServiceBroker<TNotification> where TNotification : INotification
	{
		event NotificationSuccessDelegate<TNotification> OnNotificationSucceeded;

		event NotificationFailureDelegate<TNotification> OnNotificationFailed;

		System.Collections.Generic.IEnumerable<TNotification> TakeMany();

		Boolean IsCompleted { get; }

		void RaiseNotificationSucceeded(TNotification notification);

		void RaiseNotificationFailed(TNotification notification, AggregateException ex);

		void Start();

		void Stop(Boolean immediately = false);
	}
}