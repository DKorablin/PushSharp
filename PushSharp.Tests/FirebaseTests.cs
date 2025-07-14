using AlphaOmega.PushSharp.Google;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Firebase")]
	public class FirebaseTests
	{
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
	}
}