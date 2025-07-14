using System;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The exception is occurred when user subscription token has expired or application has been removed.</summary>
	public class DeviceSubscriptionExpiredException : NotificationException
	{
		/// <summary>The old subscription token.</summary>
		public String OldSubscriptionId { get; set; }

		/// <summary>The new subscription token (If available).</summary>
		public String NewSubscriptionId { get; set; }

		/// <summary>The new token expiration date.</summary>
		public DateTime ExpiredAt { get; set; }

		/// <summary>Initializes a new instance of the <see cref="DeviceSubscriptionExpiredException"/> class.</summary>
		/// <param name="notification">The notification instance which was expired.</param>
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