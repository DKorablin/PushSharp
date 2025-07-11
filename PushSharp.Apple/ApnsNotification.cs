using System;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json.Linq;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsNotification : INotification
	{
		public enum ApnPushType
		{
			/// <summary>The push type for notifications that trigger a user interaction—for example, an alert, badge, or sound.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID as the topic.
			/// If the notification requires immediate action from the user, set notification priority to 10; otherwise use 5.
			/// 
			/// You’re required to use the alert push type on watchOS 6 and later. It’s recommended on macOS, iOS, tvOS, and iPadOS.
			/// </remarks>
			Alert,

			/// <summary>The push type for notifications that deliver content in the background, and don’t trigger any user interactions.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID as the topic.
			/// Always use priority 5. Using priority 10 is an error.
			/// 
			/// You’re required to use the background push type on watchOS 6 and later. It’s recommended on macOS, iOS, tvOS, and iPadOS.
			/// </remarks>
			Background,

			/// <summary>The push type to reload and update a control.</summary>
			/// <remarks>When someone interacts with a web server connected to your app, enable a control to update its state using the controls push type.</remarks>
			Controls,

			/// <summary>The push type for notifications that request a user’s location.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.location-query appended to the end.
			/// 
			/// It’s recommended for iOS and iPadOS.
			/// If the location query requires an immediate response from the Location Push Service Extension, set notification apns-priority to 10; otherwise, use 5.
			/// The location push type supports only token-based authentication.
			/// 
			/// The location push type isn’t available on macOS, tvOS, and watchOS.
			/// </remarks>
			Location,

			/// <summary>The push type for notifications that provide information about an incoming Voice-over-IP (VoIP) call.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.voip appended to the end.
			/// If you’re using certificate-based authentication, you must also register the certificate for VoIP services.
			/// The topic is then part of the 1.2.840.113635.100.6.3.4 or 1.2.840.113635.100.6.3.6 extension.
			/// 
			/// The voip push type isn’t available on watchOS.
			/// It’s recommended on macOS, iOS, tvOS, and iPadOS.
			/// </remarks>
			Voip,

			/// <summary>The push type for notifications that contain update information for a watchOS app’s complications.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.complication appended to the end.
			/// If you’re using certificate-based authentication, you must also register the certificate for WatchKit services.
			/// The topic is then part of the 1.2.840.113635.100.6.3.6 extension.
			/// 
			/// The complication push type isn’t available on macOS, tvOS, and iPadOS. It’s recommended for watchOS and iOS.
			/// </remarks>
			Complication,

			/// <summary>The push type to signal changes to a File Provider extension.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.pushkit.fileprovider appended to the end.
			/// 
			/// The fileprovider push type isn’t available on watchOS. It’s recommended on macOS, iOS, tvOS, and iPadOS.
			/// </remarks>
			Fileprovider,

			/// <summary>The push type for notifications that tell managed devices to contact the MDM server.</summary>
			/// <remarks>
			/// If you set this push type, you must use the topic from the UID attribute in the subject of your MDM push certificate.
			/// 
			/// The mdm push type isn’t available on watchOS. It’s recommended on macOS, iOS, tvOS, and iPadOS.
			/// </remarks>
			Mdm,

			/// <summary>The push type to signal changes to a live activity session.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.push-type.liveactivity appended to the end.
			/// 
			/// The liveactivity push type isn’t available on watchOS, macOS, and tvOS. It’s recommended on iOS and iPadOS.
			/// </remarks>
			Liveactivity,

			/// <summary>The push type for notifications that provide information about updates to your application’s push to talk services.</summary>
			/// <remarks>
			/// If you set this push type, the apns-topic header field must use your app’s bundle ID with.voip-ptt appended to the end.
			/// 
			/// The pushtotalk push type isn’t available on watchOS, macOS, and tvOS. It’s recommended on iOS and iPadOS.
			/// </remarks>
			Pushtotalk,
		}

		public Object Tag { get; set; }

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

		/// <summary>The priority of the notification.</summary>
		/// <remarks>
		/// Specify 10 to send the notification immediately.
		/// Specify 5 to send the notification based on power considerations on the user’s device.
		/// Specify 1 to prioritize the device’s power considerations over all other factors for delivery, and prevent awakening the device.
		/// 
		/// If you omit this header, APNs sets the notification priority to 10.
		/// </remarks>
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

		/// <summary>An identifier you use to merge multiple notifications into a single notification for the user.</summary>
		/// <remarks>
		/// Typically, each notification request displays a new notification on the user’s device.
		/// When sending the same notification more than once, use the same value in this header to merge the requests.
		/// The value of this key must not exceed 64 bytes.
		/// </remarks>
		public String ApnsCollapseId { get; set; }

		public ApnsNotification() : this(String.Empty, new JObject())
		{
		}

		public ApnsNotification(String deviceToken) : this(deviceToken, new JObject())
		{
		}

		public ApnsNotification(String deviceToken, JObject payload)
		{
			this.DeviceToken = deviceToken;
			this.Payload = payload;
		}

		Boolean INotification.IsDeviceRegistrationIdValid()
		{
			var r = new System.Text.RegularExpressions.Regex(@"^[0-9A-F]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			return r.Match(this.DeviceToken).Success;
		}
	}
}