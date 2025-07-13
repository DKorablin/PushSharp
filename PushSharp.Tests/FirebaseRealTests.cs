using AlphaOmega.PushSharp.Google;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
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
				notification.message.token = regId;
				notification.message.data = JObject.Parse("{ \"somekey\" : \"I want cookie\" }");

				broker.QueueNotification(notification);
			}

			broker.Stop();

			Assert.Equal(attempted, succeeded);
			Assert.Equal(0, failed);
		}
	}
}