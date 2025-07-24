using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Apple;
using AlphaOmega.PushSharp.Core;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Apns")]
	public class ApnsTests
	{
		private readonly List<String> messages = new List<String>();

		[Fact]
		public void ApnsNotification_ShouldReportFail_WhenServerUnreachable()
		{
			Log.AddTraceListener(new TestLogger(messages));

			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new ApnsSettings(
				ApnsSettings.ApnsServerEnvironment.Production,
				Settings.Instance.ApnsCertificateFile,
				Settings.Instance.ApnsCertificateKeyId,
				Settings.Instance.ApnsTeamId,
				Settings.Instance.ApnsBundleId)
			{
				Host = "null://localhost:433",
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
				broker.QueueNotification(new ApnsNotification(dt)
				{
					Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"Manual test\" } }")
				});
			}

			broker.Stop();

			Log.Trace.Flush();

			Assert.Equal(0, succeeded);
			Assert.Equal(attempted, failed);
		}
	}
}