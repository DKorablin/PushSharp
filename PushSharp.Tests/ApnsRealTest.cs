using AlphaOmega.PushSharp.Apple;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection(Settings.DISABLED)]
	public class ApnsRealTest
	{
		[Fact()]
		public void APNS_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new ApnsSettings(ApnsSettings.ApnsServerEnvironment.Production, Settings.Instance.ApnsCertificateFile, Settings.Instance.ApnsCertificatePassword);
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
					Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"Hello PushSharp!\" } }")
				});
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}

		[Fact(Skip = Settings.DISABLED)]
		public void APNS_Feedback_Service()
		{
			var settings = new ApnsSettings(ApnsSettings.ApnsServerEnvironment.Sandbox, Settings.Instance.ApnsCertificateFile, Settings.Instance.ApnsCertificatePassword);
			var config = new ApnsConfiguration(settings);

			/*var fbs = new FeedbackService(config);
			fbs.FeedbackReceived += (String deviceToken, DateTime timestamp) =>
			{
				// Remove the deviceToken from your database
				// timestamp is the time the token was reported as expired
			};
			fbs.Check();*/
		}
	}
}