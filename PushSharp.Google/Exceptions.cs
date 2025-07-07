using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Google
{
	public class GcmNotificationException : NotificationException<FirebaseNotification>
	{
		public String Description { get; private set; }

		public GcmNotificationException(FirebaseNotification notification, String msg) : base(msg, notification)
		{
		}

		public GcmNotificationException(FirebaseNotification notification, String msg, String description) : base(msg, notification)
			=> this.Description = description;
	}

	public class GcmMulticastResultException : Exception
	{
		public GcmMulticastResultException() : base("One or more Registration Id's failed in the multicast notification")
		{
			this.Succeeded = new List<FirebaseNotification>();
			this.Failed = new Dictionary<FirebaseNotification, Exception>();
		}

		public List<FirebaseNotification> Succeeded { get; set; }

		public Dictionary<FirebaseNotification, Exception> Failed { get; set; }
	}
}