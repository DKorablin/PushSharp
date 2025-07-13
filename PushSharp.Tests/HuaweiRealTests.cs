using System;
using AlphaOmega.PushSharp.Huawei;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Real")]
	public class HuaweiRealTests
	{
		[Fact]
		public void Huawei_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var config = new HuaweiConfiguration(
				Settings.Instance.HuaweiClientSecret,
				Settings.Instance.HuaweiProjectId,
				Settings.Instance.HuaweiApplicationId);

			var broker = new HuaweiServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) =>
			{
				failed++;
			};
			broker.OnNotificationSucceeded += (notification) =>
			{
				succeeded++;
			};

			broker.Start();

			foreach(var regId in Settings.Instance.HuaweiRegistrationIds)
			{
				attempted++;

				var notification = new HuaweiNotification();
				notification.Message.token = new String[] { regId };
				notification.Message.data = "{ \"somekey\" : \"I want cookie\" }";

				broker.QueueNotification(notification);
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}