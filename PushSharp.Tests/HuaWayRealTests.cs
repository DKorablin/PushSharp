using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Google;
using AlphaOmega.PushSharp.HuaWay;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Real")]
	public class HuaWayRealTests
	{
		[Fact]
		public void HuaWay_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new HuaWaySettings()
			{
				ApplicationId = Settings.Instance.HuaWayApplicationId,
				ClientId = Settings.Instance.HuaWayClientId,
				ClientSecret = Settings.Instance.HuaWayClientSecret,
				ProjectId = Settings.Instance.HuaWayProjectId,
			};

			var config = new HuaWayConfiguration(settings);
			var broker = new HuaWayServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) =>
			{
				failed++;
			};
			broker.OnNotificationSucceeded += (notification) =>
			{
				succeeded++;
			};

			broker.Start();

			foreach(var regId in Settings.Instance.HuaWayRegistrationIds)
			{
				attempted++;

				var notification = new HuaWayNotification();
				notification.Message.token = new String[] { regId };
				notification.Message.data = JObject.Parse("{ \"somekey\" : \"somevalue\" }");

				broker.QueueNotification(notification);
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}