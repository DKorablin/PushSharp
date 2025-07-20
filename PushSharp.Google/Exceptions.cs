using System;
using System.Collections.Generic;
using System.Net;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The notification exception received from Firebase server while sending PUSH notification.</summary>
	public class FcmNotificationException : NotificationException<FirebaseNotification>
	{
		/// <summary>The notification description information.</summary>
		public FirebaseMessageResponse Description { get; private set; }

		/// <summary>Create instance of <see cref="FcmNotificationException"/> with notification instance and received message.</summary>
		/// <param name="notification">The PUSH notification instance.</param>
		/// <param name="msg">The exception message.</param>
		public FcmNotificationException(FirebaseNotification notification, String msg) : base(msg, notification)
		{
		}

		/// <summary>Create instance of <see cref="FcmNotificationException"/> with notification instance, message and detailed description.</summary>
		/// <param name="notification">The PUSH notification instance.</param>
		/// <param name="description">The error description.</param>
		public FcmNotificationException(FirebaseNotification notification, FirebaseMessageResponse description) : base(description.error.message, notification)
			=> this.Description = description;
	}

	/// <summary>The exception occurred  while sending multiple PUSH messages.</summary>
	public class FcmMulticastResultException : Exception
	{
		/// <summary>Create instance of <see cref="FcmMulticastResultException"/> with default exception message.</summary>
		public FcmMulticastResultException() : base("One or more Registration Id's failed in the multicast notification")
		{
			this.Succeeded = new List<FirebaseNotification>();
			this.Failed = new Dictionary<FirebaseNotification, Exception>();
		}

		/// <summary>The list of successfully sent messages.</summary>
		public List<FirebaseNotification> Succeeded { get; set; }

		/// <summary>The list of messages that could not be send.</summary>
		public Dictionary<FirebaseNotification, Exception> Failed { get; set; }
	}
}