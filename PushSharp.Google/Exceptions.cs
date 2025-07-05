using System;
using System.Collections.Generic;
using PushSharp.Core;

namespace PushSharp.Google
{
    public class GcmNotificationException : NotificationException
    {
        public GcmNotificationException (FirebaseNotification notification, string msg) : base (msg, notification)
        {
            Notification = notification;
        }

        public GcmNotificationException (FirebaseNotification notification, string msg, string description) : base (msg, notification)
        {
            Notification = notification;
            Description = description;
        }

        public new FirebaseNotification Notification { get; private set; }
        public string Description { get; private set; }
    }

    public class GcmMulticastResultException : Exception
    {
        public GcmMulticastResultException () : base ("One or more Registration Id's failed in the multicast notification")
        {
            Succeeded = new List<FirebaseNotification> ();
            Failed = new Dictionary<FirebaseNotification, Exception> ();
        }

        public List<FirebaseNotification> Succeeded { get;set; }

        public Dictionary<FirebaseNotification, Exception> Failed { get;set; }
    }
}

