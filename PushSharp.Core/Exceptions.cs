using System;

namespace AlphaOmega.PushSharp.Core
{
	public class DeviceSubscriptionExpiredException : NotificationException
	{
		public String OldSubscriptionId { get; set; }

		public String NewSubscriptionId { get; set; }

		public DateTime ExpiredAt { get; set; }

		public DeviceSubscriptionExpiredException(INotification notification) : base("Device Subscription has Expired", notification)
			=> this.ExpiredAt = DateTime.UtcNow;
	}

	public class NotificationException<T> : Exception where T : INotification
	{
		public T Notification { get; set; }

		public NotificationException(String message, T notification) : base(message)
			=> this.Notification = notification;

		public NotificationException(String message, T notification, Exception innerException) : base(message, innerException)
			=> this.Notification = notification;
	}

	public class NotificationException : NotificationException<INotification>
	{
		public NotificationException(String message, INotification notification) : base(message, notification)
		{
		}

		public NotificationException(String message, INotification notification, Exception innerException)
			: base(message, notification, innerException)
		{
		}
	}

	public class RetryAfterException : NotificationException
	{
		public RetryAfterException (INotification notification, String message, DateTime retryAfterUtc) : base (message, notification)
			=> this.RetryAfterUtc = retryAfterUtc;

		public DateTime RetryAfterUtc { get; set; }
	}
}