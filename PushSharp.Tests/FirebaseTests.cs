using System;
using System.Collections.Generic;
using AlphaOmega.PushSharp.Core;
using AlphaOmega.PushSharp.Google;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Firebase")]
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

		[Fact(Skip = Settings.AUTOBUILD_DISABLED)]
		public void FirebaseNotification_ShouldReportFail_WhenServerUnreachable()
		{
			Log.AddTraceListener(new TestLogger(messages));

			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var settings = Settings.Firebase;
			settings.TokenUri = "null://localhost:433";
			settings.MessageSendUri = "null://localhost:433";

			var config = new FirebaseConfiguration(Settings.Firebase);
			var broker = new FirebaseServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) =>
			{
				failed++;
			};
			broker.OnNotificationSucceeded += (notification) =>
			{
				succeeded++;
			};

			broker.Start();

			foreach(var regId in Settings.Instance.GcmRegistrationIds)
			{
				attempted++;

				var notification = new FirebaseNotification();
				notification.validate_only = true;
				notification.message.token = regId;
				notification.message.data = JObject.Parse("{ \"somekey\" : \"I want cookie\" }");

				broker.QueueNotification(notification);
			}

			broker.Stop();

			Assert.Equal(0, succeeded);
			Assert.Equal(attempted, failed);
		}
	}
}