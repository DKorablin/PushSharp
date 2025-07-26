using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Apple;
using AlphaOmega.PushSharp.Core;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	public class ApnsTests
	{
		const String TestCert = @"-----BEGIN PRIVATE KEY-----
MIIBQgIBADATBgcqhkjOPQIBBggqhkjOPQMBBwSCASYwggEiAgEBBCBMVEQzbhtz
WvE/uCMg6vZq2/8I9dqCfUk6q+jhaHStiaCB+jCB9wIBATAsBgcqhkjOPQEBAiEA
/////wAAAAEAAAAAAAAAAAAAAAD///////////////8wWwQg/////wAAAAEAAAAA
AAAAAAAAAAD///////////////wEIFrGNdiqOpPns+u9VXaYhrxlHQawzFOw9jvO
PD4n0mBLAxUAxJ02CIbnBJNqZnjhE50mt4GffpAEQQRrF9Hy4SxCR/i85uVjpEDy
dwN9gS3rM6D0oTlF2JjClk/jQuL+Gn+bjufrSnwPnhYrzjNXazFezsu2QGg3v1H1
AiEA/////wAAAAD//////////7zm+q2nF56E87nKwvxjJVECAQE=
-----END PRIVATE KEY-----";

		private readonly List<String> messages = new List<String>();

		[Fact]
		public void ApnsNotification_ShouldReportFail_WhenServerUnreachable()
		{
			Log.AddTraceListener(new TestLogger(messages));

			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new ApnsSettings(
				ApnsSettings.ApnsServerEnvironment.Development,
				nameof(ApnsSettings.KeyId),
				nameof(ApnsSettings.TeamId),
				nameof(ApnsSettings.AppBundleId))
			{
				Host = "null://localhost:433",
				P8Certificate = TestCert,
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

			try
			{

				broker.Start();
				Assert.Fail("It should fail with exception");

			} catch(ArgumentException)
			{
			}

			/*foreach(var dt in new String[] { "deviceToken1", "deviceToken2", })
			{
				attempted++;
				broker.QueueNotification(new ApnsNotification(dt)
				{
					Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"Manual test\" } }")
				});
			}*/

			broker.Stop();

			Log.Trace.Flush();

			/*Assert.Equal(0, succeeded);
			Assert.Equal(attempted, failed);*/
		}

		[Fact]
		public void Apns_Settings_Should_ReadCertificate()
		{
			var settings = new ApnsSettings(
				ApnsSettings.ApnsServerEnvironment.Development,
				nameof(ApnsSettings.KeyId),
				nameof(ApnsSettings.TeamId),
				nameof(ApnsSettings.TeamId))
			{
				P8Certificate = TestCert,
			};

			Assert.NotNull(settings.P8Signer);

			var configuration = new ApnsConfiguration(settings);
			var accessToken = configuration.AccessToken;
			Assert.False(String.IsNullOrWhiteSpace(accessToken));
		}
	}
}