using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
    public class FirebaseMessageResult
    {
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public String MessageId { get; set; }

        [JsonProperty("registration_id", NullValueHandling = NullValueHandling.Ignore)]
        public String CanonicalRegistrationId { get; set; }

        [JsonIgnore]
        public GcmResponseStatus ResponseStatus { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public String Error
        {
            get 
            {
                switch (this.ResponseStatus)
                {
                case GcmResponseStatus.Ok:
                    return null;
                case GcmResponseStatus.Unavailable:
                    return nameof(GcmResponseStatus.Unavailable);
                case GcmResponseStatus.QuotaExceeded:
                    return nameof(GcmResponseStatus.QuotaExceeded);
                case GcmResponseStatus.NotRegistered:
                    return nameof(GcmResponseStatus.NotRegistered);
                case GcmResponseStatus.MissingRegistrationId:
                    return "MissingRegistration";
                case GcmResponseStatus.MissingCollapseKey:
                    return nameof(GcmResponseStatus.MissingCollapseKey);
                case GcmResponseStatus.MismatchSenderId:
                    return nameof(GcmResponseStatus.MismatchSenderId);
                case GcmResponseStatus.MessageTooBig:
                    return nameof(GcmResponseStatus.MessageTooBig);
                case GcmResponseStatus.InvalidTtl:
                    return nameof(GcmResponseStatus.InvalidTtl);
                case GcmResponseStatus.InvalidRegistration:
                    return nameof(GcmResponseStatus.InvalidRegistration);
                case GcmResponseStatus.InvalidDataKey:
                    return nameof(GcmResponseStatus.InvalidDataKey);
                case GcmResponseStatus.InternalServerError:
                    return nameof(GcmResponseStatus.InternalServerError);
                case GcmResponseStatus.Error:
                    return nameof(GcmResponseStatus.Error);
				case GcmResponseStatus.DeviceQuotaExceeded:
				case GcmResponseStatus.CanonicalRegistrationId:
				default:
                    return null;
                }
            }
        }
    }
}
