using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Blackberry
{
    public class BlackberryNotificationException : NotificationException
    {
        public BlackberryNotificationException (BlackberryMessageStatus msgStatus, string desc, BlackberryNotification notification) 
            :base (desc + " - " + msgStatus, notification)
        {
            Notification = notification;
            MessageStatus = msgStatus;
        }

        public new BlackberryNotification Notification { get; set; }

        public BlackberryMessageStatus MessageStatus { get; private set; }
    }
}

