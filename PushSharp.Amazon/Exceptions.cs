using System;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Amazon
{
	public class AdmRateLimitExceededException : NotificationException
	{
		public AdmRateLimitExceededException(String reason, AdmNotification notification)
			: base("Rate Limit Exceeded (" + reason + ")", notification)
		{
			this.Notification = notification;
			this.Reason = reason;
		}

		public new AdmNotification Notification { get; set; }

		public String Reason { get; private set; }
	}

	public class AdmMessageTooLargeException : NotificationException
	{
		public AdmMessageTooLargeException(AdmNotification notification)
			: base("ADM Message too Large, must be <= 6kb", notification)
			=> this.Notification = notification;

		public new AdmNotification Notification { get; set; }
	}
}