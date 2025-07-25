﻿using AlphaOmega.PushSharp.Google;
using AlphaOmega.PushSharp.Tests.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests.Real
{
	public class FirebaseRealTests
	{
		[Fact(Skip = Settings.AUTOBUILD_DISABLED)]
		public void Firebase_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

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

				var notification = new FirebaseNotification()
				{
					validate_only = true,
					message =
					{
						token = regId,
						data = JObject.Parse(@"{ ""somekey"" : ""I want cookie"" }"),
					},
				};

				broker.QueueNotification(notification);
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}