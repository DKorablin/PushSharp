using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using PushSharp.Core;
using System.Linq;
using Newtonsoft.Json;

namespace PushSharp.Google
{
	public class FirebaseNotification : INotification
	{
		public static FirebaseNotification ForSingleResult(FirebaseResponse response, Int32 resultIndex)
		{
			var result = new FirebaseNotification
			{
				Tag = response.OriginalNotification.Tag,
				MessageId = response.OriginalNotification.MessageId,
			};

			if(response.OriginalNotification.RegistrationIds != null && response.OriginalNotification.RegistrationIds.Count >= (resultIndex + 1))
				result.RegistrationIds.Add(response.OriginalNotification.RegistrationIds[resultIndex]);

			result.CollapseKey = response.OriginalNotification.CollapseKey;
			result.Data = response.OriginalNotification.Data;
			result.DelayWhileIdle = response.OriginalNotification.DelayWhileIdle;
			result.ContentAvailable = response.OriginalNotification.ContentAvailable;
			result.DryRun = response.OriginalNotification.DryRun;
			result.Priority = response.OriginalNotification.Priority;
			result.To = response.OriginalNotification.To;

			return result;
		}

		public static FirebaseNotification ForSingleRegistrationId(FirebaseNotification msg, String registrationId)
		{
			var result = new FirebaseNotification
			{
				Tag = msg.Tag,
				MessageId = msg.MessageId,
				To = null,
				CollapseKey = msg.CollapseKey,
				Data = msg.Data,
				DelayWhileIdle = msg.DelayWhileIdle,
				ContentAvailable = msg.ContentAvailable,
				DryRun = msg.DryRun,
				Priority = msg.Priority,
			};
			result.RegistrationIds.Add(registrationId);

			return result;
		}

		public Boolean IsDeviceRegistrationIdValid()
			=> this.RegistrationIds?.Any() == true;

		[JsonIgnore]
		public Object Tag { get; set; }

		[JsonProperty("message_id")]
		public String MessageId { get; internal set; }

		/// <summary>Registration ID of the Device(s).  Maximum of 1000 registration Id's per notification</summary>
		[JsonProperty("registration_ids")]
		public List<String> RegistrationIds { get; set; } = new List<String>();

		/// <summary>Registration ID or Group/Topic to send notification to.  Overrides RegistrationIds.</summary>
		/// <value>To.</value>
		[JsonProperty("to")]
		public String To { get; set; }

		/// <summary>Only the latest message with the same collapse key will be delivered</summary>
		[JsonProperty("collapse_key")]
		public String CollapseKey { get; set; } = String.Empty;

		/// <summary>JSON Payload to be sent in the message</summary>
		[JsonProperty("data")]
		public JObject Data { get; set; } = null;

		/// <summary>Notification JSON payload</summary>
		/// <value>The notification payload.</value>
		[JsonProperty("notification")]
		public JObject Notification { get; set; }

		/// <summary>If true, GCM will only be delivered once the device's screen is on</summary>
		[JsonProperty("delay_while_idle")]
		public Boolean? DelayWhileIdle { get; set; } = null;

		/// <summary>Time in seconds that a message should be kept on the server if the device is offline. Default (and maximum) is 4 weeks.</summary>
		[JsonProperty("time_to_live")]
		public Int32? TimeToLive { get; set; }

		/// <summary>If true, dry_run attribute will be sent in payload causing the notification not to be actually sent, but the result returned simulating the message</summary>
		[JsonProperty("dry_run")]
		public Boolean? DryRun { get; set; }

		/// <summary>A string containing the package name of your application. When set, messages will only be sent to registration IDs that match the package name</summary>
		[JsonProperty("restricted_package_name")]
		public String RestrictedPackageName { get; set; }

		/// <summary>On iOS, use this field to represent content-available in the APNS payload. When a notification or message is sent and this is set to true, an inactive client app is awoken. On Android, data messages wake the app by default. On Chrome, currently not supported.</summary>
		/// <value>The content available.</value>
		[JsonProperty("content_available")]
		public Boolean? ContentAvailable { get; set; }

		/// <summary>Corresponds to iOS APNS priorities (Normal is 5 and high is 10).  Default is Normal.</summary>
		/// <value>The priority.</value>
		[JsonProperty("priority"), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
		public GcmNotificationPriority? Priority { get; set; }

		public String GetJson()
		{
			// If 'To' was used instead of RegistrationIds, let's make RegistrationId's null
			// so we don't serialize an empty array for this property
			// otherwise, google will complain that we specified both instead
			if(this.RegistrationIds != null && this.RegistrationIds.Count <= 0 && !String.IsNullOrEmpty(this.To))
				this.RegistrationIds = null;

			// Ignore null values
			return JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}
	}

	public enum GcmNotificationPriority
	{
		[EnumMember(Value = "normal")]
		Normal = 5,
		[EnumMember(Value = "high")]
		High = 10
	}
}