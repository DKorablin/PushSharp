using System;

namespace AlphaOmega.PushSharp.Core
{
	public class ServiceBrokerConfiguration // : IPushServiceSettings
	{
		public ServiceBrokerConfiguration()
		{
			this.AutoScaleChannels = true;
			this.MaxAutoScaleChannels = 20;
			//this.MinAvgTimeToScaleChannels = 100;
			this.Channels = 1;
			//this.MaxNotificationRequeues = 5;
			//this.NotificationSendTimeout = 15000;
			//this.IdleTimeout = TimeSpan.FromMinutes (5);
		}

		public Boolean AutoScaleChannels { get; set; }

		public Int32 MaxAutoScaleChannels { get; set; }

		//public Int64 MinAvgTimeToScaleChannels { get; set; }

		public Int32 Channels { get; set; }

		//public Int32 MaxNotificationRequeues { get; set; }

		//public Int32 NotificationSendTimeout { get; set; }

		//public TimeSpan IdleTimeout { get;set; }
	}
}