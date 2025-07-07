using System;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Firefox
{
    public class FirefoxNotificationException : NotificationException
    {
        public FirefoxNotificationException (FirefoxNotification notification, string msg)
            : base (msg, notification)
        {
            Notification = notification;
        }

        public new FirefoxNotification Notification { get; private set; }
    }
}
