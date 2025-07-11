using System;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsResponse
	{
		/// <summary>Strongly typed reason error code.</summary>
		public enum ApnsNotification2ErrorStatusCode
		{
			/// <summary>The collapse identifier exceeds the maximum allowed size.</summary>
			BadCollapseId,
			/// <summary>The specified device token is invalid.</summary>
			/// <remarks>Verify that the request contains a valid token and that the token matches the environment.</remarks>
			BadDeviceToken,
			/// <summary>The apns-expiration value is invalid.</summary>
			BadExpirationDate,
			/// <summary>The apns-id value is invalid.</summary>
			BadMessageId,
			/// <summary>The apns-priority value is invalid.</summary>
			BadPriority,
			/// <summary>The apns-topic value is invalid.</summary>
			BadTopic,
			/// <summary>The device token doesn't match the specified topic.</summary>
			DeviceTokenNotForTopic,
			/// <summary>One or more headers are repeated.</summary>
			DuplicateHeaders,
			/// <summary>Idle timeout.</summary>
			IdleTimeout,
			/// <summary>The apns-push-type value is invalid.</summary>
			InvalidPushType,
			/// <summary>The device token isn’t specified in the request :path.</summary>
			/// <remarks>Verify that the :path header contains the device token.</remarks>
			MissingDeviceToken,
			/// <summary>The apns-topic header of the request isn’t specified and is required.</summary>
			/// <remarks>The apns-topic header is mandatory when the client is connected using a certificate that supports multiple topics.</remarks>
			MissingTopic,
			/// <summary>The message payload is empty.</summary>
			PayloadEmpty,
			/// <summary>Pushing to this topic is not allowed.</summary>
			TopicDisallowed,
			/// <summary>The certificate is invalid.</summary>
			BadCertificate,
			/// <summary>The client certificate is for the wrong environment.</summary>
			BadCertificateEnvironment,
			/// <summary>The provider token is stale and a new token should be generated.</summary>
			ExpiredProviderToken,
			/// <summary>The specified action is not allowed.</summary>
			Forbidden,
			/// <summary>The provider token is not valid, or the token signature can’t be verified.</summary>
			InvalidProviderToken,
			/// <summary>No provider certificate was used to connect to APNs, and the authorization header is missing or no provider token is specified.</summary>
			MissingProviderToken,
			/// <summary>The key ID in the provider token isn’t related to the key ID of the token used in the first push of this connection.</summary>
			/// <remarks>To use this token, open a new connection.</remarks>
			UnrelatedKeyIdInToken,
			/// <summary>The request contained an invalid :path value.</summary>
			BadPath,
			/// <summary>The specified :method value isn’t POST.</summary>
			MethodNotAllowed,
			/// <summary>The device token has expired.</summary>
			ExpiredToken,
			/// <summary>The device token is inactive for the specified topic.</summary>
			/// <remarks>There is no need to send further pushes to the same device token, unless your application retrieves the same device token, refer to Registering your app with APNs.</remarks>
			Unregistered,
			/// <summary>The message payload is too large.</summary>
			PayloadTooLarge,
			/// <summary>The provider’s authentication token is being updated too often.</summary>
			/// <remarks>Update the authentication token no more than once every 20 minutes.</remarks>
			TooManyProviderTokenUpdates,
			/// <summary>Too many requests were made consecutively to the same device token.</summary>
			TooManyRequests,
			/// <summary>An internal server error occurred.</summary>
			InternalServerError,
			/// <summary>The service is unavailable.</summary>
			ServiceUnavailable,
			/// <summary>The APNs server is shutting down.</summary>
			Shutdown,
		}

		/// <summary>The error code (specified as a string) indicating the reason for the failure.</summary>
		public String reason { get; set; }

		/// <summary>The time, represented in milliseconds since Epoch, at which APNs confirmed the token was no longer valid for the topic.</summary>
		/// <remarks>This key is included only when the error in the :status field is 410.</remarks>
		public Int32? timestamp { get; set; }

		public ApnsNotification2ErrorStatusCode? ReasonTyped
			=> Enum.TryParse<ApnsNotification2ErrorStatusCode>(this.reason, out var result)
				? result
				: (ApnsNotification2ErrorStatusCode?)null;
	}
}