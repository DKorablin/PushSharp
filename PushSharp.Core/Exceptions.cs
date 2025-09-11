using System;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The exception is occurred when user subscription token has expired or application has been removed.</summary>
	public class DeviceSubscriptionExpiredException : NotificationException
	{
		/// <summary>The old subscription token.</summary>
		public String OldSubscriptionId { get; set; }

		/// <summary>The new token expiration date.</summary>
		public DateTime ExpiredAt { get; set; }

		/// <summary>Initializes a new instance of the <see cref="DeviceSubscriptionExpiredException"/> class.</summary>
		/// <param name="notification">The notification instance which was expired.</param>
		/// <param name="message">The message to show in the logs.</param>
		public DeviceSubscriptionExpiredException(INotification notification, String message = "Device Subscription has Expired")
			: base(message, notification)
			=> this.ExpiredAt = DateTime.UtcNow;
	}

	/// <summary>The strongly typed exception is occurred when we failed to send notification to the PUSH server.</summary>
	/// <typeparam name="T">The type of push notification.</typeparam>
	public class NotificationException<T> : Exception where T : INotification
	{
		/// <summary>The notification instance that we failed to send.</summary>
		public T Notification { get; }

		/// <summary>Crete instance of <see cref="NotificationException"/> with exception message and push notification instance.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="notification">The push notification.</param>
		public NotificationException(String message, T notification) : base(message)
			=> this.Notification = notification;

		/// <summary>Create instance of <see cref="NotificationException"/> with exception message, push notification instance and inner exception.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="notification">The push notification.</param>
		/// <param name="innerException">The inner exception.</param>
		public NotificationException(String message, T notification, Exception innerException) : base(message, innerException)
			=> this.Notification = notification;
	}

	/// <summary>The exception is occurred when we failed to send notification to the PUSH server.</summary>
	public class NotificationException : NotificationException<INotification>
	{
		/// <summary>Crete instance of <see cref="NotificationException"/> with exception message and push notification instance.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="notification">The push notification.</param>
		public NotificationException(String message, INotification notification) : base(message, notification)
		{
		}

		/// <summary>Create instance of <see cref="NotificationException"/> with exception message, push notification instance and inner exception.</summary>
		/// <param name="message">The exception message.</param>
		/// <param name="notification">The push notification.</param>
		/// <param name="innerException">The inner exception.</param>
		public NotificationException(String message, INotification notification, Exception innerException)
			: base(message, notification, innerException)
		{
		}
	}

	/// <summary>The exception is occurred when server sending to many push notifications.</summary>
	public class RetryAfterException : NotificationException
	{
		/// <summary>Create instance of <see cref="RetryAfterException"/> with required parameters.</summary>
		/// <param name="notification">The push notification.</param>
		/// <param name="message">The exception message.</param>
		/// <param name="retryAfterUtc">Next notification should be send after specified time.</param>
		public RetryAfterException (INotification notification, String message, DateTime retryAfterUtc) : base (message, notification)
			=> this.RetryAfterUtc = retryAfterUtc;

		/// <summary>The time that client should wait before sending next notification.</summary>
		public DateTime RetryAfterUtc { get; }
	}
}