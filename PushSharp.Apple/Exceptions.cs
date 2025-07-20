using System;
using System.Net;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Apple
{
	/// <summary>Exception information while sending message to APNS server.</summary>
	public class ApnsNotificationException : NotificationException<ApnsNotification>
	{
		/// <summary>The error HTTP status code received from APNS server.</summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>The error details.</summary>
		public ApnsResponse Error { get; }

		/// <summary>Create instance of <see cref="ApnsNotificationException"/>.</summary>
		/// <param name="statusCode">The HTTP status code received from server.</param>
		/// <param name="error">The error details.</param>
		/// <param name="notification">The notification that was sent.</param>
		public ApnsNotificationException(HttpStatusCode statusCode, ApnsResponse error, ApnsNotification notification)
			: base("APNS notification error: '" + error.reason + "'", notification) {
			this.StatusCode = statusCode;
			this.Error = error;
		}
	}

	/// <summary>[Not used] Exception occurred when we failed to connect to APNS server.</summary>
	public class ApnsConnectionException : Exception
	{
		/// <summary>Create instance of <see cref="ApnsConnectionException"/> with detailed exception information.</summary>
		/// <param name="message">The message for current stack trace.</param>
		/// <param name="innerException">The inner exception information.</param>
		public ApnsConnectionException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}
}