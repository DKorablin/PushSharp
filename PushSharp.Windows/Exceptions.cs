using System;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Windows
{
    public class WnsNotificationException : NotificationException
    {
        public WnsNotificationException (WnsNotificationStatus status) : base (status.ErrorDescription, status.Notification) 
        {
            Notification = status.Notification;
            Status = status;
        }

        public new WnsNotification Notification { get; set; }
        public WnsNotificationStatus Status { get; private set; }

        public override string ToString ()
        {
            return base.ToString() + " Status = " + Status.HttpStatus;
        }
    }
}

