using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PushSharp.Google;
using Xunit;

namespace PushSharp.Tests
{
	[Collection("Real")]
	public class FirebaseRealTests
	{
		[Fact]
		public void Firebase_Send_Single()
		{
			var succeeded = 0;
			var failed = 0;
			var attempted = 0;

			var config = new FirebaseConfiguration(Settings.Firebase);
			var broker = new FirebaseServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) => failed++;
			broker.OnNotificationSucceeded += (notification) => succeeded++;

			broker.Start();

			foreach(var regId in Settings.Instance.GcmRegistrationIds)
			{
				attempted++;

				broker.QueueNotification(new FirebaseNotification
				{
					RegistrationIds = new List<String> {
						regId
					},
					Data = JObject.Parse("{ \"somekey\" : \"somevalue\" }")
				});
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}