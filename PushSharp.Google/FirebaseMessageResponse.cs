using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The response to Firebase push message request.</summary>
	public class FirebaseMessageResponse
	{
		/// <summary>Error codes are common across all Firebase and Google Cloud Platform services.</summary>
		public enum FirebaseResponseStatus
		{
			/// <summary>Unknown server error. Typically a server bug.</summary>
			/// <remarks>This error code is also assigned to local response parsing (unmarshal) errors, and a wide range of other low-level I/O errors that are not easily diagnosable.</remarks>
			UNKNOWN,

			/// <summary>Internal server error. Typically a server bug.</summary>
			INTERNAL,

			/// <summary>Cloud Messaging service is temporarily unavailable.</summary>
			/// <remarks>This error code is also assigned to local network errors (connection refused, no route to host).</remarks>
			UNAVAILABLE,

			/// <summary>One or more arguments specified in the request were invalid. </summary>
			INVALID_ARGUMENT,

			/// <summary>Request cannot be executed in the current system state, such as deleting a non-empty directory.</summary>
			FAILED_PRECONDITION,

			/// <summary>Client specified an invalid range.</summary>
			OUT_OF_RANGE,

			/// <summary>Request not authenticated due to missing, invalid or expired OAuth token.</summary>
			UNAUTHENTICATED,

			/// <summary>Client does not have sufficient permission.</summary>
			/// <remarks>This can happen because the OAuth token does not have the right scopes, the client does not have permission, or the API has not been enabled for the client project.</remarks>
			PERMISSION_DENIED,

			/// <summary>Specified resource not found, or the request is rejected due to undisclosed reasons such as whitelisting.</summary>
			NOT_FOUND,

			/// <summary>Concurrency conflict, such as read-modify-write conflict.</summary>
			/// <remarks>
			/// Only used by a few legacy services.
			/// Most services use <see cref="ABORTED"/> or <see cref="ALREADY_EXISTS"/> instead of this.
			/// Refer to the service-specific documentation to see which one to handle in your code.
			/// </remarks>
			CONFLICT,

			/// <summary>Concurrency conflict, such as read-modify-write conflict.</summary>
			ABORTED,

			/// <summary>The resource that a client tried to create already exists.</summary>
			ALREADY_EXISTS,

			/// <summary>Either out of resource quota or reaching rate limiting.</summary>
			RESOURCE_EXHAUSTED,

			/// <summary>Request cancelled by the client.</summary>
			CANCELLED,

			/// <summary>Unrecoverable data loss or data corruption.</summary>
			/// <remarks>The client should report the error to the user.</remarks>
			DATA_LOSS,

			/// <summary>Request deadline exceeded.</summary>
			/// <remarks>
			/// This will happen only if the caller sets a deadline that is shorter than the target API’s default deadline (i.e. requested deadline is not enough for the server to process the request), and the request did not finish within the deadline.
			/// This error code is also assigned to local connection and read timeouts.
			/// </remarks>
			DEADLINE_EXCEEDED,

			/// <summary>APNs certificate or web push auth API key was invalid or missing.</summary>
			THIRD_PARTY_AUTH_ERROR,
			/// <summary>Sending limit exceeded for the message target.</summary>
			QUOTA_EXCEEDED,

			/// <summary>The authenticated sender ID is different from the sender ID for the registration token.</summary>
			/// <remarks>This usually means the sender and the target registration token are not in the same Firebase project.</remarks>
			SENDER_ID_MISMATCH,

			/// <summary>App instance was unregistered from FCM.</summary>
			/// <remarks>This usually means that the device registration token used is no longer valid and a new one must be used.</remarks>
			UNREGISTERED,
		}

		/// <summary>The failed response to Firebase push message request.</summary>
		public class FirebaseErrorResponse
		{
			/// <summary>The details of failed response to Firebase push message request.</summary>
			public class FirebaseErrorDetailsResponse
			{
				/// <summary>The type of error from Firebase server side.</summary>
				[JsonProperty("@type", NullValueHandling = NullValueHandling.Ignore)]
				public String type { get; set; }

				/// <summary>Strongly typed error code.</summary>
				public FirebaseResponseStatus errorCode { get; set; }
			}

			/// <summary>The HTTP status code</summary>
			public Int32 code { get; set; }

			/// <summary>User friendly message.</summary>
			public String message { get; set; }

			/// <summary>Strongly typed status error.</summary>
			public FirebaseResponseStatus? status { get; set; }

			/// <summary>The detailed error information.</summary>
			public FirebaseErrorDetailsResponse[] details { get; set; }
		}

		/// <summary>Error codes are common across all Firebase and Google Cloud Platform services.</summary>
		public FirebaseErrorResponse error { get; set; }
	}
}