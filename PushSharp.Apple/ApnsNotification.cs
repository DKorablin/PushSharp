using System;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json.Linq;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsNotification : INotification
	{
		public enum ApnPushType
		{
			Background,
			Alert,
			Voip
		}

		private static readonly Object nextIdentifierLock = new Object();
		private static Int32 nextIdentifier = 1;

		private static Int32 GetNextIdentifier()
		{
			lock(nextIdentifierLock)
			{
				if(nextIdentifier >= Int32.MaxValue - 10)
					nextIdentifier = 1;

				return nextIdentifier++;
			}
		}

		/// <summary>
		/// DO NOT Call this unless you know what you are doing!
		/// </summary>
		public static void ResetIdentifier()
		{
			lock(nextIdentifierLock)
				nextIdentifier = 0;
		}

		public Object Tag { get; set; }

		public Int32 Identifier { get; private set; }

		public String DeviceToken { get; set; }

		public JObject Payload { get; set; }

		/// <summary>
		/// The date at which the notification is no longer valid.
		/// This value is a UNIX epoch expressed in seconds (UTC).
		/// If the value is nonzero, APNs stores the notification and tries to deliver it at least once, repeating the attempt as needed until the specified date.
		/// If the value is 0, APNs attempts to deliver the notification only once and doesn’t store it.
		/// 
		/// A single APNs attempt may involve retries over multiple network interfaces and connections of the destination device.
		/// Often these retries span over some time period, depending on the network characteristics.
		/// In addition, a push notification may take some time on the network after APNs sends it to the device.
		/// APNs uses best efforts to honor the expiry date without any guarantee.
		/// If the value is nonzero, the notification may deliver after the specified timestamp.
		/// If the value is 0, the notification may deliver with some delay.
		/// 
		/// If you omit this header, APNs stores the push according to APNs storage policy.
		/// </summary>
		public DateTime? ApnsExpiration { get; set; }

		public Int32? ApnsPriority { get; set; }

		/// <summary>The value of this header must accurately reflect the contents of your notification’s payload.</summary>
		/// <remarks>If there’s a mismatch, or if the header is missing on required systems, APNs may return an error, delay the delivery of the notification, or drop it altogether.</remarks>
		public ApnPushType ApnsPushType { get; set; } = ApnPushType.Alert;

		/// <summary>A canonical UUID that’s the unique ID for the notification.</summary>
		/// <remarks>
		/// If an error occurs when sending the notification, APNs includes this value when reporting the error to your server.
		/// Canonical UUIDs are 32 lowercase hexadecimal digits, displayed in five groups separated by hyphens in the form 8-4-4-4-12.
		/// If you omit this header, APNs creates a UUID for you and returns it in its response.
		/// </remarks>
		public Guid? ApnsId { get; set; } = null;

		public const Int32 DEVICE_TOKEN_BINARY_MIN_SIZE = 32;
		public const Int32 DEVICE_TOKEN_STRING_MIN_SIZE = 64;
		public const Int32 MAX_PAYLOAD_SIZE = 2048; //will be 4096 soon
		public static readonly DateTime DoNotStore = DateTime.MinValue;
		private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public ApnsNotification() : this(String.Empty, new JObject())
		{
		}

		public ApnsNotification(String deviceToken) : this(deviceToken, new JObject())
		{
		}

		public ApnsNotification(String deviceToken, JObject payload)
		{
			if(!String.IsNullOrEmpty(deviceToken) && deviceToken.Length < DEVICE_TOKEN_STRING_MIN_SIZE)
				throw new NotificationException("Invalid DeviceToken Length", this);

			this.DeviceToken = deviceToken;
			this.Payload = payload;

			this.Identifier = GetNextIdentifier();
		}

		public Boolean IsDeviceRegistrationIdValid()
		{
			var r = new System.Text.RegularExpressions.Regex(@"^[0-9A-F]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			return r.Match(this.DeviceToken).Success;
		}
	}
}