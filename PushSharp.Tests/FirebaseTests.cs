using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Core;
using AlphaOmega.PushSharp.Google;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	public class FirebaseTests
	{
		private List<String> messages = new List<String>();

		[Fact]
		public void FirebaseNotification_Priority_Should_Serialize_As_String_High()
		{
			var n = new FirebaseNotification();
			n.message.android.priority = GcmNotificationPriority.High;

			var str = n.GetJson();

			Assert.Contains("high", str);
		}

		[Fact]
		public void FirebaseNotification_Priority_Should_Serialize_As_String_Normal()
		{
			var n = new FirebaseNotification();
			n.message.android.priority = GcmNotificationPriority.Normal;

			var str = n.GetJson();

			Assert.Contains("normal", str);
		}

		[Fact]
		public void FirebaseNotification_ShouldReportFail_WhenServerUnreachable()
		{
			Log.AddTraceListener(new TestLogger(messages));

			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = new FirebaseSettings(nameof(FirebaseSettings.ProjectId), nameof(FirebaseSettings.PrivateKey), nameof(FirebaseSettings.ClientEmail), nameof(FirebaseSettings.TokenUri))
			{
				TokenUri = "null://localhost:433",
				MessageSendUri = "null://localhost:433",
			};

			var config = new FirebaseConfiguration(settings);
			var broker = new FirebaseServiceBroker(config);
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

			broker.Stop();
		}
	}
}