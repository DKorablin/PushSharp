using AlphaOmega.PushSharp.Apple;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection(Settings.DISABLED)]
	public class ApnsRealTest
	{
		[Fact]
		public void APNS_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new ApnsSettings(
				ApnsSettings.ApnsServerEnvironment.Production,
				Settings.Instance.ApnsCertificateFile,
				Settings.Instance.ApnsCertificateKeyId,
				Settings.Instance.ApnsTeamId)
			{
				AppBundleId = Settings.Instance.ApnsBundleId,
			};

			var config = new ApnsConfiguration(settings);
			var broker = new ApnsServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) =>
			{
				failed++;
			};
			broker.OnNotificationSucceeded += (notification) =>
			{
				succeeded++;
			};

			broker.Start();

			foreach(var dt in Settings.Instance.ApnsDeviceTokens)
			{
				attempted++;
				broker.QueueNotification(new ApnsNotification
				{
					DeviceToken = dt,
					Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"I want cookie!\" } }")
				});
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}