using System;
using System.Net;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Apple
{
	public enum ApnsNotificationErrorStatusCode
	{
		NoErrors = 0,
		ProcessingError = 1,
		MissingDeviceToken = 2,
		MissingTopic = 3,
		MissingPayload = 4,
		InvalidTokenSize = 5,
		InvalidTopicSize = 6,
		InvalidPayloadSize = 7,
		InvalidToken = 8,
		Shutdown = 10,
		ConnectionError = 254,
		Unknown = 255
	}

	public class ApnsNotificationException2 : NotificationException<ApnsNotification>
	{
		public HttpStatusCode StatusCode { get; }
		public ApnsResponse Error { get; }

		public ApnsNotificationException2(HttpStatusCode statusCode, ApnsResponse error, ApnsNotification notification)
			: base(error.reason, notification) {
			this.StatusCode = statusCode;
			this.Error = error;
		}
	}

	public class ApnsNotificationException : NotificationException<ApnsNotification>
	{
		public ApnsNotificationException(Byte errorStatusCode, ApnsNotification notification)
			: this(ToErrorStatusCode(errorStatusCode), notification)
		{ }

		public ApnsNotificationException(ApnsNotificationErrorStatusCode errorStatusCode, ApnsNotification notification)
			: base("Apns notification error: '" + errorStatusCode + "'", notification)
			=> this.ErrorStatusCode = errorStatusCode;

		public ApnsNotificationException(ApnsNotificationErrorStatusCode errorStatusCode, ApnsNotification notification, Exception innerException)
			: base("Apns notification error: '" + errorStatusCode + "'", notification, innerException)
			=> this.ErrorStatusCode = errorStatusCode;

		public ApnsNotificationErrorStatusCode ErrorStatusCode { get; private set; }

		private static ApnsNotificationErrorStatusCode ToErrorStatusCode(Byte errorStatusCode)
			=> Enum.TryParse(errorStatusCode.ToString(), out ApnsNotificationErrorStatusCode result)
				? result
				: ApnsNotificationErrorStatusCode.Unknown;
	}

	public class ApnsConnectionException : Exception
	{
		public ApnsConnectionException(String message) : base(message)
		{
		}

		public ApnsConnectionException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}
}