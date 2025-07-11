using System;
using System.ComponentModel;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWayNotification : INotification
	{
		public class MessageNotification
		{
			/// <summary>Custom message payload.</summary>
			/// <remarks>The value can be a JSON string for notification messages, and can be a common string or JSON string for data messages.</remarks>
			/// <example>
			/// "your data" or "{'param1':'value1','param2':'value2'}"
			/// If the message body contains message.data and does not contain message.notification or message.android.notification, the message is a data message.
			/// If a data message is sent to a web app, the originData field in the received data message indicates the content of the data message.
			/// </example>
			public String data { get; set; }

			/// <summary>Notification message content.</summary>
			public Notification notification { get; set; }

			/// <summary>Android message push control.</summary>
			/// <remarks>This parameter is mandatory for Android notification messages.</remarks>
			public AndroidConfig android { get; set; }

			/// <summary>iOS message push control.</summary>
			/// <remarks>This parameter is mandatory for iOS messages.</remarks>
			public ApnsConfig apns { get; set; }

			/// <summary>Web app message push control.</summary>
			/// <remarks>This parameter is mandatory for web app notification messages.</remarks>
			public WebPushConfig webpush { get; set; }

			/// <summary>Push token of the target user of a message.</summary>
			/// <remarks>If the same token is added to the array, the user will receive the same message. Do not enter a duplicate token.</remarks>
			/// <example>["token1","token2"]</example>
			public String[] token { get; set; } = Array.Empty<String>();

			/// <summary>[Android] Topic subscribed by the target user of a message.</summary>
			/// <remarks>
			/// You must and can only set one of token, topic, and condition.
			/// </remarks>
			public String topic { get; set; }

			/// <summary>[Android] Condition (topic combination expression) for sending a message to the target user.</summary>
			/// <remarks>
			/// You must and can only set one of token, topic, and condition.
			/// </remarks>
			/// <example>
			/// A Boolean expression of target topics can be specified to send messages based on a combination of condition expressions.
			/// Syntax and restrictions:
			/// Boolean operation
			/// &&: logical AND
			/// ||: logical OR
			/// !: logical NOT
			/// (): priority control
			/// in: keywords
			/// Restrictions:
			/// A maximum of five topics can be included in the expression.
			/// Example:
			/// "'TopicA' in topics && ('TopicB' in topics || 'TopicC' in topics)"
			/// The preceding expression indicates that messages are sent to devices that subscribe to topics A and B or topic C. Devices that subscribe to a single topic do not receive the messages.
			/// </example>
			public String condition { get; set; }

			public class Notification
			{
				/// <summary>Notification message title.</summary>
				public String title { get; set; } = " ";//Illegal payload, title cannot be null in body. Code: 80100003

				/// <summary>Notification message content.</summary>
				public String body { get; set; } = " ";//Illegal payload, content cannot be null in body. Code: 80100003

				/// <summary>URL of the custom large icon on the right of a notification message.</summary>
				/// <remarks>
				/// If this parameter is not set, the icon is not displayed. The URL must be an HTTPS URL.
				/// 
				/// The icon file size must be less than 512 KB.
				/// The recommended icon size is 40 x 40 dp and corner radiuses are 8 dp.
				/// Beyond the recommended specifications, the image may be compressed or displayed incompletely.
				/// </remarks>
				/// <example>Example: https://example.com/image.png</example>
				public String image { get; set; }
			}

			public class AndroidConfig
			{
				/// <summary>
				/// Mode for the Push Kit server to cache messages sent to an offline device.
				/// These cached messages will be delivered once the device goes online again.
				/// </summary>
				/// <remarks>
				/// 0: Only the latest message sent by each app to the user device is cached.
				/// -1: All messages are cached.
				/// 1-100: message cache group ID. Messages are cached by group.Each group can cache only one message for each app.
				/// For example, if you send 10 messages and set collapse_key to 1 for the first five messages and to 2 for the rest, the latest message whose value of collapse_key is 1 and the latest message whose value of collapse_key is 2 are sent to the user after the user's device goes online.
				/// </remarks>
				[DefaultValue(-1)]
				public Int32? collapse_key { get; set; }

				/// <summary>Message cache time, in seconds.</summary>
				/// <remarks>
				/// When a user device is offline, the Push Kit server caches messages.
				/// If the user device goes online within the message cache time, the messages are delivered.
				/// Otherwise, the messages are discarded.
				/// The default value is 86400 (1 day), and the maximum value is 1296000 (15 days).
				/// </remarks>
				public String ttl { get; set; }

				/// <summary>Tag of a message in a batch delivery task.</summary>
				/// <remarks>The tag is returned to your server when Push Kit sends the message receipt. Your server can analyze message delivery statistics based on bi_tag.</remarks>
				public String bi_tag { get; set; }

				/// <summary>Unique receipt ID that associates with the receipt URL and configuration of the downlink message.</summary>
				public String receipt_id { get; set; }

				/// <summary>State of a mini program when a quick app sends a data message.</summary>
				/// <remarks>
				/// The options are as follows:
				/// 1: development state
				/// 2: production state
				/// </remarks>
				[DefaultValue(2)]
				public Int32? fast_app_target { get; set; }

				/// <summary>Custom message payload.</summary>
				/// <remarks>If the data parameter is set, the value of the message.data field is overwritten.</remarks>
				public String data { get; set; }

				/// <summary>Android notification message structure.</summary>
				public AndroidNotification notification { get; set; }

				public class AndroidNotification
				{
					/// <summary>[mandatory] Title of an Android notification message.</summary>
					/// <remarks>
					/// If the title parameter is set, the value of the <see cref="MessageNotification.Notification.title"/> field is overwritten.
					/// Before a message is sent, you must set at least one of <see cref="title"/> and <see cref="MessageNotification.Notification.title"/>.
					/// </remarks>
					/// <see cref="MessageNotification.Notification.title"/>
					public String title { get; set; }

					/// <summary>[mandatory] Body of an Android notification message.</summary>
					/// <remarks>
					/// If the body parameter is set, the value of the <see cref="MessageNotification.Notification.body"/> field is overwritten.
					/// Before a message is sent, you must set at least one of <see cref="body"/> and <see cref="MessageNotification.Notification.body"/>.
					/// </remarks>
					/// <see cref="MessageNotification.Notification.body"/>
					public String body { get; set; }

					/// <summary>Custom small icon on the left of a notification message.</summary>
					/// <remarks>
					/// The icon file must be stored in the /res/raw directory of an app.
					/// For example, the value /raw/ic_launcher indicates the local icon file ic_launcher.xxx stored in /res/raw.
					/// Currently, supported file formats include PNG and JPG.
					/// </remarks>
					public String icon { get; set; }

					/// <summary>Custom notification bar button color in #RRGGBB format, where RR indicates the red hexadecimal color, GG indicates the green hexadecimal color, and BB indicates the blue hexadecimal color.</summary>
					public String color { get; set; }

					/// <summary>Custom message notification ringtone, which is valid during channel creation.</summary>
					/// <remarks>
					/// The ringtone file must be stored in the /res/raw directory of an app.
					/// For example, the value /raw/shake indicates the local ringtone file /res/raw/shake.xxx stored in /res/raw.
					/// Currently, various file formats such as MP3, WAV, and MPEG are supported.
					/// If this parameter is not set, the default system ringtone will be used.
					/// </remarks>
					public String sound { get; set; }

					/// <summary>Indicates whether to use the default ringtone.</summary>
					/// <remarks>
					/// true: The default ringtone is used.
					/// false: A custom ringtone is used.
					/// </remarks>
					public Boolean? default_sound { get; set; }

					/// <summary>Message tag. Messages that use the same message tag in the same app will be overwritten by the latest message.</summary>
					public String tag { get; set; }

					/// <summary>Message tapping action.</summary>
					/// <remarks>This parameter is mandatory for Android notification messages.</remarks>
					public ClickAction click_action { get; set; } = new ClickAction();

					/// <summary>ID in a string format of the localized message body.</summary>
					public String body_loc_key { get; set; }

					/// <summary>Variables of the localized message body.</summary>
					/// <example>"body_loc_args":["1","2","3"]</example>
					public String[] body_loc_args { get; set; }

					/// <summary>ID in a string format of the localized message title.</summary>
					public String title_loc_key { get; set; }

					/// <summary>Variables of the localized message title.</summary>
					/// <example>"title_loc_args":["1","2","3"]</example>
					public String[] title_loc_args { get; set; }

					/// <summary>
					/// Multi-language parameter.
					/// body_loc_key and title_loc_key are read from multi_lang_key first.
					/// If they are not read from multi_lang_key, they will be read from the local character string of the APK.
					/// </summary>
					/// <remarks>A maximum of three languages can be set.</remarks>
					public Object multi_lang_key { get; set; }

					/// <summary>Custom channel for displaying notification messages.</summary>
					/// <remarks>Custom channels are supported in the Android O version or later.</remarks>
					public String channel_id { get; set; }

					/// <summary>Brief description of a notification message to an Android app.</summary>
					public String notify_summary { get; set; }

					/// <summary>URL of the custom large icon on the right of a notification message.</summary>
					/// <remarks>
					/// The function is the same as that of the <see cref="HuaWayNotification.MessageNotification.Notification.image"/> field.
					/// If the image parameter is set, the value of the <see cref="HuaWayNotification.MessageNotification.Notification.image"/> field is overwritten.
					/// The URL must be an HTTPS URL.
					/// </remarks>
					/// <example>https://example.com/image.png</example>
					/// <see cref="HuaWayNotification.MessageNotification.Notification.image"/>
					public String image { get; set; }

					/// <summary>Notification bar style.</summary>
					/// <remarks>
					/// 0: default style
					/// 1: large text
					/// 3: inbox style
					/// </remarks>
					public Int32? style { get; set; }

					/// <summary>Android notification message title in large text style.</summary>
					/// <remarks>
					/// This parameter is mandatory when style is set to 1.
					/// When the notification bar is displayed after big_title is set, big_title instead of title is used.
					/// </remarks>
					/// <see cref="style"/>
					public String big_title { get; set; }

					/// <summary>Android notification message body in large text style.</summary>
					/// <remarks>
					/// This parameter is mandatory when style is set to 1.
					/// When the notification bar is displayed after big_body is set, big_body instead of body is used.
					/// </remarks>
					/// <see cref="style"/>
					public String big_body { get; set; }

					/// <summary>Message display duration, in milliseconds.</summary>
					/// <remarks>Messages are automatically deleted after the duration expires.</remarks>
					public Int32? auto_clear { get; set; }

					/// <summary>Unique notification ID of a message.</summary>
					/// <remarks>
					/// If a message does not contain the ID or the ID is -1, NC will generate a unique ID for the message.
					/// Different notification messages can use the same notification ID, so that new messages can overwrite old messages.
					/// </remarks>
					public Int32? notify_id { get; set; }

					/// <summary>Message group.</summary>
					/// <example>
					/// If 10 messages that contain the same value of group are sent to a device, the device displays only the latest message and the total number of messages received in the group, but does not display these 10 messages.
					/// </example>
					public String group { get; set; }

					/// <summary>Android notification message badge control.</summary>
					public BadgeNotification badge { get; set; }

					/// <summary>Content displayed on the status bar after the device receives a notification message.</summary>
					/// <remarks>
					/// Due to the restrictions of the Android native mechanism, the content will not be displayed on the status bar on the device running Android 5.0 (API level 21) or later even if this field is set.
					/// </remarks>
					public String ticker { get; set; }

					/// <summary>Time when Android notification messages are delivered, in the UTC timestamp format.</summary>
					/// <remarks>If you send multiple messages at the same time, they will be sorted based on this value and displayed in the Android notification panel.</remarks>
					/// <example>2014-10-02T15:01:23.045123456Z</example>
					public String when { get; set; }

					/// <summary>Android notification message priority, which determines the message notification behavior of a user device.</summary>
					/// <remarks>
					/// LOW: low-priority (silent) message
					/// NORMAL: medium-priority message
					/// </remarks>
					public String importance { get; set; }

					/// <summary>Indicates whether to use the default vibration mode.</summary>
					public Boolean? use_default_vibrate { get; set; }

					/// <summary>Indicates whether to use the default breathing light mode.</summary>
					public Boolean? use_default_light { get; set; }

					/// <summary>Custom vibration mode for an Android notification message.</summary>
					/// <remarks>
					/// Each array element adopts the format of [0-9]+|[0-9]+[sS]|[0-9]+[.][0-9]{1,9}|[0-9]+[.][0-9]{1,9}[sS], for example, ["3.5S","2S","1S","1.5S"].
					/// A maximum of ten array elements are supported.
					/// The value of each element is an integer ranging from 1 to 60. EMUI 11 is not supported.
					/// </remarks>
					/// <example>"vibrate_config":["1","3"]</example>
					public String[] vibrate_config { get; set; }

					/// <summary>Android notification message visibility.</summary>
					/// <remarks>
					/// VISIBILITY_UNSPECIFIED: The visibility is not specified. This value is equivalent to PRIVATE.
					/// PUBLIC: The content of a received notification message is displayed on the lock screen.
					/// SECRET: A received notification message is not displayed on the lock screen.
					/// PRIVATE: If you have set a lock screen password and enabled Hide notification content under Settings > Notifications, the content of a received notification message is hidden on the lock screen.
					/// </remarks>
					public String visibility { get; set; }

					/// <summary>Custom breathing light color.</summary>
					public LightSettings light_settings { get; set; }

					/// <summary>Indicates whether to display notification messages in the foreground when an app is running in the foreground.</summary>
					public Boolean? foreground_show { get; set; }

					/// <summary>ID of the user-app relationship.</summary>
					/// <remarks>The value contains a maximum of 64 characters.</remarks>
					public String profile_id { get; set; }

					/// <summary>Content in inbox style.</summary>
					/// <remarks>
					/// A maximum number of five content records are supported and each record can contain at most 1024 characters.
					/// This parameter is mandatory when <see cref="style"/> is set to 3.
					/// </remarks>
					/// <see cref="style"/>
					/// <example>"inbox_content":["content1","content2","content3"]</example>
					public String[] inbox_content { get; set; }

					/// <summary>Action buttons of a notification message.</summary>
					/// <remarks>A maximum of three buttons can be set.</remarks>
					/// <example>"buttons":[{"name":"Open app","action_type":"1"}]</example>
					public Button[] buttons { get; set; }

					public class Button
					{
						/// <summary>[mandatory] Button name, which cannot exceed 40 characters.</summary>
						public String name { get; set; }

						/// <summary>Button action.</summary>
						/// <remarks>
						/// 0: Open the app home page
						/// 1: open a custom app page
						/// 2: open a specified web page
						/// 3: delete a notification message
						/// 4: share a notification message(this action is supported only on Huawei device)
						/// </remarks>
						public Int32 action_type { get; set; }

						/// <summary>Method of opening a custom app page.</summary>
						/// <remarks>
						/// 0: open the page through intent
						/// 1: open the page through action
						/// This parameter is mandatory when <see cref="action_type"/> is set to 1.
						/// </remarks>
						/// <see cref="action_type"/>
						public Int32 intent_type { get; set; }

						/// <summary>
						/// When action_type is set to 1, set this parameter to an action or the URI of the app page to be opened based on the value of intent_type.
						/// When action_type is set to 2, set this parameter to the URL of the web page to be opened.
						/// </summary>
						/// <example>https://example.com/image.png</example>
						public String intent { get; set; }

						/// <summary>
						/// When action_type is set to 0 or 1, this parameter is used to transparently transmit data to an app after a button is tapped. The parameter is optional and its value must be key-value pairs in format of {"key1":"value1","key2":"value2",...}.
						/// When action_type is set to 4, this parameter indicates content to be shared and is mandatory.
						/// </summary>
						/// <remarks>The maximum length is 1024 characters.</remarks>
						public String data { get; set; }
					}

					public class ClickAction
					{
						/// <summary>[Mandatory] Message tapping action type.</summary>
						/// <remarks>
						/// 1: tap to open a custom app page
						/// 2: tap to open a specified URL
						/// 3: tap to start the app
						/// </remarks>
						public Int32 type { get; set; } = 1;

						/// <summary>When <see cref="type"/> is set to 1, you must set at least one of intent and action.</summary>
						/// <see cref="type"/>
						public String intent { get; set; }

						/// <summary>URL to be opened.</summary>
						/// <remarks>This parameter is mandatory when <see cref="type"/> is set to 2.</remarks>
						/// <example>https://example.com/image.png</example>
						/// <see cref="type"/>
						public String url { get; set; }

						/// <summary>Action corresponding to the activity of the page to be opened when the custom app page is opened through the action.</summary>
						/// <remarks>When <see cref="type"/> is set to 1 (open a custom page), you must set at least one of intent and action.</remarks>
						/// <see cref="type"/>
						public String action { get; set; } = "ACTION_VIEW";
					}

					/// <remarks>
					/// To display the badge number after messages are received, class is mandatory, and add_num and set_num are optional.
					/// If both of add_num and set_num are left empty, the badge number increases by 1 by default.
					/// </remarks>
					public class BadgeNotification
					{
						/// <summary>Accumulative badge number, which is an integer ranging from 1 to 99.</summary>
						/// <example>
						/// A user has N unread messages on an app.
						/// If add_num is set to 3, the number displayed on the app badge increases by 3 each time a message that contains add_num is received, that is, N+3.
						/// </example>
						public Int32? add_num { get; set; }

						/// <summary>[mandatory] Full path of the app entry activity class.</summary>
						/// <example>com.example.hmstest.MainActivity</example>
						[JsonProperty("class")]
						public String @class { get; set; }

						/// <summary>Badge number, which is an integer ranging from 0 to 99.</summary>
						/// <example>
						/// If set_num is set to 10, the number displayed on the app badge is 10 no matter how many messages are received.
						/// If both set_num and add_num are set, the value of set_num prevails.
						/// </example>
						public Int32? set_num { get; set; }
					}

					public class LightSettings
					{
						/// <summary>[mandatory] Breathing light color.</summary>
						/// <remarks>This parameter is mandatory when <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/> is set.</remarks>
						/// <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/>
						public Color color { get; set; }

						/// <summary>[mandatory] Interval when a breathing light is on, in the format of \d+|\d+[sS]|\d+.\d{1,9}|\d+.\d{1,9}[sS].</summary>
						/// <remarks>This parameter is mandatory when <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/> is set.</remarks>
						/// <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/>
						public String light_on_duration { get; set; }

						/// <summary>[mandatory] Interval when a breathing light is off, in the format of \d+|\d+[sS]|\d+.\d{1,9}|\d+.\d{1,9}[sS].</summary>
						/// <remarks>This parameter is mandatory when <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/> is set.</remarks>
						/// <see cref="MessageNotification.AndroidConfig.AndroidNotification.light_settings"/>
						public String light_off_duration { get; set; }
					}

					public class Color
					{
						/// <summary>Alpha setting of the RGB color.</summary>
						/// <remarks>The default value is 1, and the value range is [0, 1].</remarks>
						public Single? alpha { get; set; }

						/// <summary>Red setting of the RGB color.</summary>
						/// <remarks>The default value is 0, and the value range is [0, 1].</remarks>
						public Single? red { get; set; }

						/// <summary>Green setting of the RGB color.</summary>
						/// <remarks>The default value is 0, and the value range is [0, 1].</remarks>
						public Single? green { get; set; }

						/// <summary>Blue setting of the RGB color.</summary>
						/// <remarks>The default value is 0, and the value range is [0, 1].</remarks>
						public Single? blue { get; set; }
					}
				}
			}

			public class ApnsConfig
			{
				/// <summary>APNs message headers.</summary>
				public JObject headers { get; set; }

				/// <summary>APNs message payload.</summary>
				/// <remarks>
				/// If title and body are set in the message payload, the values of the message.notification.title and message.notification.body fields are overwritten.
				/// Before a message is sent, you must set at least one of title and body and at least one of message.notification.title and message.notification.body.
				/// </remarks>
				public JObject payload { get; set; }

				/// <summary>HMS parameter for APNs.</summary>
				public HmsOptions hms_options { get; set; }

				public class HmsOptions
				{
					/// <summary>Target user type.</summary>
					/// <remarks>
					/// 1: test user
					/// 2: formal user
					/// 3: VoIP user
					/// </remarks>
					public Int32 target_user_type { get; set; }
				}
			}

			public class WebPushConfig
			{
				/// <summary>Headers of a message sent through the WebPush agent.</summary>
				public Headers headers { get; set; }

				/// <summary>Structure of a notification message sent through the WebPush agent.</summary>
				public WebNotification notification { get; set; }

				/// <summary>WebPush agent parameter.</summary>
				public HmsOptions hms_options { get; set; }

				public class Headers
				{
					/// <summary>Message cache time, in seconds, for example, 20, 20s, or 20S.</summary>
					public String ttl { get; set; }

					/// <summary>Message ID, which can be used to overwrite undelivered messages.</summary>
					public String topic { get; set; }
				}

				public class WebNotification
				{
					/// <summary>Title of a web app notification message. If the title parameter is set, the value of the message.notification.title field is overwritten.</summary>
					/// <remarks>Before a message is sent, you must set at least one of title and message.notification.title.</remarks>
					public String title { get; set; }

					/// <summary>Body of a web app notification message.</summary>
					/// <remarks>
					/// If the body parameter is set, the value of the message.notification.body field is overwritten
					/// Before a message is sent, you must set at least one of body and message.notification.body.
					/// </remarks>
					public String body { get; set; }

					/// <summary>Small icon URL.</summary>
					public String icon { get; set; }

					/// <summary>Large image URL.</summary>
					public String image { get; set; }

					/// <summary>Language</summary>
					public String lang { get; set; }

					/// <summary>Notification message group tag.</summary>
					/// <remarks>
					/// Multiple same tags are collapsed and the latest one is displayed.
					/// This function is used only for mobile phone browsers.
					/// </remarks>
					public String tag { get; set; }

					/// <summary>Browser icon URL, which only applies to mobile phone browsers and is used to replace the default browser icon.</summary>
					public String badge { get; set; }

					/// <summary>Text direction</summary>
					/// <remarks>
					/// auto: from left to right (default value)
					/// ltr: from left to right
					/// rtl: from right to left
					/// </remarks>
					public String dir { get; set; }

					/// <summary>Vibration interval, in milliseconds</summary>
					/// <remarks>[100,200,300]</remarks>
					public Int32[] vibrate { get; set; }

					/// <summary>Message reminding flag.</summary>
					public Boolean? renotify { get; set; }

					/// <summary>Indicates that notification messages should remain active until a user taps or closes them.</summary>
					public Boolean? require_interaction { get; set; }

					/// <summary>Message sound-free and vibration-free reminding flag.</summary>
					public Boolean? silent { get; set; }

					/// <summary>Standard Unix timestamp.</summary>
					public Int64 timestamp { get; set; }

					/// <summary>Message action.</summary>
					public WebActions[] actions { get; set; }

					public class WebActions
					{
						/// <summary>Action name.</summary>
						public String action { get; set; }

						/// <summary>URL for the button icon of an action.</summary>
						public String icon { get; set; }

						/// <summary>Title of an action.</summary>
						public String title { get; set; }
					}
				}

				public class HmsOptions
				{
					/// <summary>Default URI for redirection when no action is performed.</summary>
					public String link { get; set; }
				}
			}
		}

		[JsonIgnore]
		public Object Tag { get; set; }

		/// <summary>Message structure, which must contain the valid message payload and valid sending object.</summary>
		[JsonProperty("message")]
		public MessageNotification Message { get; set; } = new MessageNotification();

		/// <summary>Indicates whether a message is a test message.</summary>
		/// <remarks>The test message is only used to verify format validity and is not pushed to user devices.</remarks>
		[JsonProperty("validate_only")]
		[DefaultValue(false)]
		public Boolean? ValidateOnly { get; set; }

		public Boolean IsDeviceRegistrationIdValid()
			=> this.Message?.token?.Length > 0 && !String.IsNullOrWhiteSpace(this.Message.token[0]);

		public String GetJson()
			=> JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
	}
}
