using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Apple;
using AlphaOmega.PushSharp.Core;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests.Real
{
	public class ApnsRealTest
	{
		private readonly List<String> messages = new List<String>();

		[Fact(Skip = Settings.AUTOBUILD_DISABLED)]
		public void APNS_Send_Single()
		{
			Log.AddTraceListener(new TestLogger(messages));
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new ApnsSettings(
				ApnsSettings.ApnsServerEnvironment.Development,
				Settings.Instance.ApnsCertificateKeyId,
				Settings.Instance.ApnsTeamId,
				Settings.Instance.ApnsBundleId);
			settings.LoadP8CertificateFromFile(Settings.Instance.ApnsCertificateFile);

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
				broker.QueueNotification(new ApnsNotification(dt)
				{
					Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"Manual test\" } }")
				});
			}

			broker.Stop();

			Log.Trace.Flush();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}