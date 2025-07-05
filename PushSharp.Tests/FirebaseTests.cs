using PushSharp.Google;
using Xunit;

namespace PushSharp.Tests
{
	[Collection("Firebase")]
	public class FirebaseTests
	{
		[Fact]
		public void FirebaseNotification_Priority_Should_Serialize_As_String_High()
		{
			var n = new FirebaseNotification()
			{
				Priority = GcmNotificationPriority.High,
			};

			var str = n.GetJson();

			Assert.Contains(str, "high");
		}

		[Fact]
		public void FirebaseNotification_Priority_Should_Serialize_As_String_Normal()
		{
			var n = new FirebaseNotification()
			{
				Priority = GcmNotificationPriority.Normal,
			};

			var str = n.GetJson();

			Assert.Contains(str, "normal");
		}
	}
}