using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The Firebase notification object.</summary>
	public class FirebaseNotification : INotification
	{
		/// <summary>Message to send by Firebase Cloud Messaging Service.</summary>
		public class MessageNotification
		{
			/// <summary>Output Only. The identifier of the message sent, in the format of projects/*/messages/{message_id}.</summary>
			public String name { get; set; }

			/// <summary>Input only. Arbitrary key/value payload, which must be UTF-8 encoded.</summary>
			/// <remarks>
			/// The key should not be a reserved word ("from", "message_type", or any word starting with "google." or "gcm.notification.").
			/// When sending payloads containing only data fields to iOS devices, only normal priority ("apns-priority": "5") is allowed in ApnsConfig.
			/// </remarks>
			/// <example>{ "name": "wrench", "mass": "1.3kg", "count": "3" }</example>
			public JObject data { get; set; } = null;

			/// <summary>Input only. Basic notification template to use across all platforms.</summary>
			public Notification notification { get; set; }

			/// <summary>Input only. Android specific options for messages sent through FCM connection server.</summary>
			public AndroidConfig android { get; set; } = new AndroidConfig();

			#region Union field target can be only one of the following:
			/// <summary>Registration token to send a message to.</summary>
			public String token { get; set; }

			/// <summary>Topic name to send a message to, e.g. "weather". Note: "/topics/" prefix should not be provided.</summary>
			public String topic { get; set; }

			/// <summary>Condition to send a message to, e.g. "'foo' in topics &amp;&amp; 'bar' in topics".</summary>
			public String condition { get; set; }
			#endregion End of list of possible types for union field target.

			/// <summary>Basic notification template to use across all platforms</summary>
			public class Notification
			{
				/// <summary>The notification's title</summary>
				public String title { get; set; }

				/// <summary>The notification's body text</summary>
				public String body { get; set; }

				/// <summary>Contains the URL of an image that is going to be downloaded on the device and displayed in a notification.</summary>
				/// <remarks>
				/// JPEG, PNG, BMP have full support across platforms.
				/// Animated GIF and video only work on iOS.
				/// WebP and HEIF have varying levels of support across platforms and platform versions.
				/// Android has 1MB image size limit.
				/// </remarks>
				public String image { get; set; }
			}

			/// <summary>Input only. Android specific options for messages sent through FCM connection server.</summary>
			public class AndroidConfig
			{
				/// <summary>An identifier of a group of messages that can be collapsed, so that only the last message gets sent when delivery can be resumed.</summary>
				/// <remarks>A maximum of 4 different collapse keys is allowed at any given time.</remarks>
				public String collapse_key { get; set; }

				/// <summary>Message priority. Can take "normal" and "high" values.</summary>
				/// <remarks>For more information, see Setting the priority of a message.</remarks>
				[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
				public GcmNotificationPriority? priority { get; set; }

				/// <summary>How long (in seconds) the message should be kept in FCM storage if the device is offline.</summary>
				/// <remarks>
				/// The maximum time to live supported is 4 weeks, and the default value is 4 weeks if not set.
				/// Set it to 0 if want to send the message immediately.
				/// In JSON format, the Duration type is encoded as a string rather than an object, where the string ends in the suffix "s" (indicating seconds) and is preceded by the number of seconds, with nanoseconds expressed as fractional seconds.
				/// For example, 3 seconds with 0 nanoseconds should be encoded in JSON format as "3s", while 3 seconds and 1 nanosecond should be expressed in JSON format as "3.000000001s".
				/// The ttl will be rounded down to the nearest second.
				/// </remarks>
				/// <example>A duration in seconds with up to nine fractional digits, ending with 's'. Example: "3.5s".</example>
				public String ttl { get; set; }

				/// <summary>Package name of the application where the registration token must match in order to receive the message.</summary>
				public String restricted_package_name { get; set; }

				/// <summary>Arbitrary key/value payload.</summary>
				/// <remarks>If present, it will override google.firebase.fcm.v1.Message.data.</remarks>
				/// <example>{ "name": "wrench", "mass": "1.3kg", "count": "3" }</example>
				public JObject data { get; set; }

				/// <summary>Notification to send to android devices.</summary>
				public AndroidNotification notification { get; set; }

				/// <summary>Options for features provided by the FCM SDK for Android.</summary>
				public JObject fcm_options { get; set; }

				/// <summary>If set to true, messages will be allowed to be delivered to the app while the device is in direct boot mode.</summary>
				public Boolean? direct_boot_ok { get; set; }

				/// <summary>Notification to send to android devices.</summary>
				public class AndroidNotification
				{
					/// <summary>The notification's title.</summary>
					/// <remarks>If present, it will override google.firebase.fcm.v1.Notification.title.</remarks>
					public String title { get; set; }

					/// <summary>The notification's body text.</summary>
					/// <remarks>If present, it will override google.firebase.fcm.v1.Notification.body.</remarks>
					public String body { get; set; }

					/// <summary>The notification's icon. Sets the notification icon to myicon for drawable resource myicon.</summary>
					/// <remarks>If you don't send this key in the request, FCM displays the launcher icon specified in your app manifest.</remarks>
					public String icon { get; set; }

					/// <summary>The notification's icon color, expressed in #rrggbb format.</summary>
					public String color { get; set; }

					/// <summary>The sound to play when the device receives the notification.</summary>
					/// <remarks>Supports "default" or the filename of a sound resource bundled in the app. Sound files must reside in /res/raw/.</remarks>
					public String sound { get; set; }

					/// <summary>Identifier used to replace existing notifications in the notification drawer.</summary>
					/// <remarks>
					/// If not specified, each request creates a new notification.
					/// If specified and a notification with the same tag is already being shown, the new notification replaces the existing one in the notification drawer.
					/// </remarks>
					public String tag { get; set; }

					/// <summary>The action associated with a user click on the notification.</summary>
					/// <remarks>If specified, an activity with a matching intent filter is launched when a user clicks on the notification.</remarks>
					public String click_action { get; set; }

					/// <summary>The key to the body string in the app's string resources to use to localize the body text to the user's current localization.</summary>
					public String body_loc_key { get; set; }

					/// <summary>Variable string values to be used in place of the format specifiers in <see cref="body_loc_key"/> to use to localize the body text to the user's current localization.</summary>
					public String[] body_loc_args { get; set; }

					/// <summary>The key to the title string in the app's string resources to use to localize the title text to the user's current localization.</summary>
					public String title_loc_key { get; set; }

					/// <summary>Variable string values to be used in place of the format specifiers in <see cref="title_loc_key"/> to use to localize the title text to the user's current localization.</summary>
					public String[] title_loc_args { get; set; }

					/// <summary>The notification's channel id (new in Android O).</summary>
					/// <remarks>
					/// The app must create a channel with this channel ID before any notification with this channel ID is received.
					/// If you don't send this channel ID in the request, or if the channel ID provided has not yet been created by the app, FCM uses the channel ID specified in the app manifest.
					/// </remarks>
					public String channel_id { get; set; }

					/// <summary>Sets the "ticker" text, which is sent to accessibility services.</summary>
					/// <remarks>Prior to API level 21 (Lollipop), sets the text that is displayed in the status bar when the notification first arrives.</remarks>
					public String ticker { get; set; }

					/// <summary>When set to false or unset, the notification is automatically dismissed when the user clicks it in the panel.</summary>
					/// <remarks>When set to true, the notification persists even when the user clicks it.</remarks>
					public Boolean? sticky { get; set; }

					/// <summary>Set the time that the event in the notification occurred.</summary>
					/// <remarks>
					/// Notifications in the panel are sorted by this time.
					/// A point in time is represented using protobuf.Timestamp.
					/// </remarks>
					/// <example>"2014-10-02T15:01:23Z", "2014-10-02T15:01:23.045123456Z" or "2014-10-02T15:01:23+05:30"</example>
					public DateTime? event_time { get; set; }

					/// <summary>Set whether or not this notification is relevant only to the current device.</summary>
					/// <remarks>
					/// Some notifications can be bridged to other devices for remote display, such as a Wear OS watch.
					/// This hint can be set to recommend this notification not be bridged.
					/// </remarks>
					public Boolean local_only { get; set; }

					/// <summary>Set the relative priority for this notification.</summary>
					/// <remarks>
					/// Priority is an indication of how much of the user's attention should be consumed by this notification.
					/// Low-priority notifications may be hidden from the user in certain situations, while the user might be interrupted for a higher-priority notification.
					/// The effect of setting the same priorities may differ slightly on different platforms.
					/// Note this priority differs from AndroidMessagePriority.
					/// This priority is processed by the client after the message has been delivered, whereas AndroidMessagePriority is an FCM concept that controls when the message is delivered.
					/// </remarks>
					public String notification_priority { get; set; }

					/// <summary>If set to true, use the Android framework's default sound for the notification.</summary>
					/// <remarks>Default values are specified in config.xml.</remarks>
					public Boolean? default_sound { get; set; }

					/// <summary>If set to true, use the Android framework's default vibrate pattern for the notification.</summary>
					/// <remarks>
					/// Default values are specified in config.xml.
					/// If default_vibrate_timings is set to true and <see cref="vibrate_timings"/> is also set, the default value is used instead of the user-specified <see cref="vibrate_timings"/>.
					/// </remarks>
					public Boolean? default_vibrate_timings { get; set; }

					/// <summary>If set to true, use the Android framework's default LED light settings for the notification.</summary>
					/// <remarks>
					/// Default values are specified in config.xml.
					/// If default_light_settings is set to true and <see cref="light_settings"/> is also set, the user-specified <see cref="light_settings"/> is used instead of the default value.
					/// </remarks>
					public Boolean? default_light_settings { get; set; }

					/// <summary>Set the vibration pattern to use.</summary>
					/// <remarks>
					/// Pass in an array of protobuf.Duration to turn on or off the vibrator.
					/// The first value indicates the Duration to wait before turning the vibrator on.
					/// The next value indicates the Duration to keep the vibrator on.
					/// Subsequent values alternate between Duration to turn the vibrator off and to turn the vibrator on.
					/// If <see cref="vibrate_timings"/> is set and default_vibrate_timings is set to true, the default value is used instead of the user-specified <see cref="vibrate_timings"/>.
					/// </remarks>
					/// <example>A duration in seconds with up to nine fractional digits, ending with 's'. Example: "3.5s".</example>
					public JObject vibrate_timings { get; set; }

					/// <summary>The push visibility.</summary>
					public JObject visibility { get; set; }

					/// <summary>Sets the number of items this notification represents.</summary>
					/// <remarks>May be displayed as a badge count for launchers that support badging.</remarks>
					/// <example>
					/// This might be useful if you're using just one notification to represent multiple new messages but you want the count here to represent the number of total new messages.
					/// If zero or unspecified, systems that support badging use the default, which is to increment a number displayed on the long-press menu each time a new notification arrives.
					/// </example>
					public Int32? notification_count { get; set; }

					/// <summary>Settings to control the notification's LED blinking rate and color if LED is available on the device.</summary>
					/// <remarks>The total blinking time is controlled by the OS.</remarks>
					public JObject light_settings { get; set; }

					/// <summary>Contains the URL of an image that is going to be displayed in a notification.</summary>
					/// <remarks>If present, it will override google.firebase.fcm.v1.Notification.image.</remarks>
					public String image { get; set; }

					/// <summary>Setting to control when a notification may be proxied.</summary>
					public JObject proxy { get; set; }
				}
			}
		}

		/// <inheritdoc/>
		[JsonIgnore]
		public Object Tag { get; set; }

		/// <summary>Flag for testing the request without actually delivering the message.</summary>
		public Boolean? validate_only { get; set; }

		/// <summary>Message to send.</summary>
		public MessageNotification message { get; set; } = new MessageNotification();

		/// <summary>The output notification message identifier.</summary>
		[JsonIgnore]
		public String message_id { get; internal set; }

		Boolean INotification.IsDeviceRegistrationIdValid()
			=> !String.IsNullOrWhiteSpace(this.message.token);

		/// <summary>Gets notification in JSON format.</summary>
		/// <returns>String representation in JSON format.</returns>
		public String GetJson()
			=> JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
	}

	/// <summary>Priority of a message to send to Android devices.</summary>
	/// <remarks>Note this priority is an FCM concept that controls when the message is delivered.</remarks>
	public enum GcmNotificationPriority
	{
		/// <summary>
		/// Default priority for data messages.
		/// Normal priority messages won't open network connections on a sleeping device, and their delivery may be delayed to conserve the battery.
		/// For less time-sensitive messages, such as notifications of new email or other data to sync, choose normal delivery priority.
		/// </summary>
		[EnumMember(Value = "normal")]
		Normal,

		/// <summary>
		/// Default priority for notification messages.
		/// FCM attempts to deliver high priority messages immediately, allowing the FCM service to wake a sleeping device when possible and open a network connection to your app server.
		/// Apps with instant messaging, chat, or voice call alerts, for example, generally need to open a network connection and make sure FCM delivers the message to the device without delay.
		/// Set high priority if the message is time-critical and requires the user's immediate interaction, but beware that setting your messages to high priority contributes more to battery drain compared with normal priority messages.
		/// </summary>
		[EnumMember(Value = "high")]
		High
	}
}