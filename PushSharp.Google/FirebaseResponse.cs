using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AlphaOmega.PushSharp.Google
{
	public class FirebaseResponse
	{
		[JsonProperty("multicast_id")]
		public Int64 MulticastId { get; set; } = -1;

		[JsonProperty("success")]
		public Int64 NumberOfSuccesses { get; set; } = 0;

		[JsonProperty("failure")]
		public Int64 NumberOfFailures { get; set; } = 0;

		[JsonProperty("canonical_ids")]
		public Int64 NumberOfCanonicalIds { get; set; } = 0;

		[JsonIgnore]
		public FirebaseNotification OriginalNotification { get; set; } = null;

		[JsonProperty("results")]
		public List<FirebaseMessageResult> Results { get; set; } = new List<FirebaseMessageResult>();

		[JsonIgnore]
		public GcmResponseCode ResponseCode { get; set; } = GcmResponseCode.Ok;
	}

	public enum GcmResponseCode
	{
		Ok,
		Error,
		BadRequest,
		ServiceUnavailable,
		InvalidAuthToken,
		InternalServiceError
	}

	public enum GcmResponseStatus
	{
		[EnumMember(Value = nameof(GcmResponseStatus.Ok))]
		Ok,

		[EnumMember(Value = nameof(GcmResponseStatus.Error))]
		Error,

		[EnumMember(Value = nameof(GcmResponseStatus.QuotaExceeded))]
		QuotaExceeded,

		[EnumMember(Value = nameof(GcmResponseStatus.DeviceQuotaExceeded))]
		DeviceQuotaExceeded,

		[EnumMember(Value = nameof(GcmResponseStatus.InvalidRegistration))]
		InvalidRegistration,

		[EnumMember(Value = nameof(GcmResponseStatus.NotRegistered))]
		NotRegistered,

		[EnumMember(Value = nameof(GcmResponseStatus.MessageTooBig))]
		MessageTooBig,

		[EnumMember(Value = nameof(GcmResponseStatus.MissingCollapseKey))]
		MissingCollapseKey,

		[EnumMember(Value = "MissingRegistration")]
		MissingRegistrationId,

		[EnumMember(Value = nameof(GcmResponseStatus.Unavailable))]
		Unavailable,

		[EnumMember(Value = nameof(GcmResponseStatus.MismatchSenderId))]
		MismatchSenderId,

		[EnumMember(Value = nameof(GcmResponseStatus.CanonicalRegistrationId))]
		CanonicalRegistrationId,

		[EnumMember(Value = nameof(GcmResponseStatus.InvalidDataKey))]
		InvalidDataKey,

		[EnumMember(Value = nameof(GcmResponseStatus.InvalidTtl))]
		InvalidTtl,

		[EnumMember(Value = nameof(GcmResponseStatus.InternalServerError))]
		InternalServerError,

		[EnumMember(Value = nameof(GcmResponseStatus.InvalidPackageName))]
		InvalidPackageName
	}
}