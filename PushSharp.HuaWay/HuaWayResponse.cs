using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWayResponse
	{
		public enum ReturnCode : Int64
		{
			/// <summary>Success</summary>
			Success = 80000000,

			/// <summary>Some tokens are successfully sent. Tokens identified by illegal_tokens are those failed to be sent.</summary>
			/// <remarks>Verify these tokens in the return value.</remarks>
			PartialSuccess = 80100000,

			/// <summary>Some request parameters are incorrect.</summary>
			/// <remarks>Verify the request parameters as prompted in the response.</remarks>
			InvalidArguments = 80100001,

			/// <summary>The number of tokens must be 1 when a message is sent in synchronous mode.</summary>
			/// <remarks>Verify the token field in the request.</remarks>
			SingleTokenRequired = 80100002,

			/// <summary>Incorrect message structure.</summary>
			/// <remarks>Verify the parameters in the message structure as prompted in the response.</remarks>
			InvalidMessageStructure = 80100003,

			/// <summary>The message expiration time is earlier than the current time.</summary>
			/// <remarks>Verify the message field ttl.</remarks>
			InvalidTtlField = 80100004,

			/// <summary>The collapse_key message field is invalid.</summary>
			/// <remarks>Verify the message field collapse_key.</remarks>
			InvalidCollapseKey = 80100013,

			/// <summary>A maximum of 100 topic-based messages can be sent at the same time.</summary>
			/// <remarks>Increase the interval for sending topic-based messages.</remarks>
			TooManyTopics = 80100017,

			/// <summary>OAuth authentication error.</summary>
			/// <remarks>
			/// The access token in the Authorization parameter in the request HTTP header failed to be authenticated.
			/// Ensure that the access token is correct.
			/// </remarks>
			OAuthError = 80200001,

			/// <summary>OAuth token expired.</summary>
			/// <remarks>
			/// The access token in the Authorization parameter in the request HTTP header expired.
			/// Obtain a new access token.
			/// </remarks>
			OAuthTokenExpired = 80200003,

			/// <summary>The current app does not have the permission to send messages.</summary>
			/// <remarks>
			/// 1. Sign in to AppGallery Connect and verify that Push Kit is enabled.
			/// 2. If HMS Core Push SDK 2.0 is integrated, remove the backslash (\) from the escape character in the access token, then URL-encode the token.
			/// 3. Check whether the token of the user matches that of the app.
			/// 4. Check the value of msg type: If the value is 1 or 3, the message can be sent. If the value is 2, the message cannot be sent.
			/// 5. If Push Kit functions normally in the Chinese mainland and result code 80300002 is returned only for devices where your app is running outside the Chinese mainland, you need to enable Push Kit for devices outside the Chinese mainland.
			/// 6. Check whether there are errors in the body of the message sent.
			/// 7. Test the message push function in AppGallery Connect. If the test is successful, an error occurs when you call the API.
			/// 8. In the multi-sender scenario, check the API prototype.
			/// </remarks>
			InsufficientPermission = 80300002,

			/// <summary>All tokens are invalid.</summary>
			/// <remarks>
			/// 1. In principle, the tokens of different apps on the same device are different. Actually, the same tokens may exist by mistake.
			/// 2. The package name and app ID configured for the app on the device are different from those obtained in AppGallery Connect.
			/// 3. Check whether the access token URL is correct.
			/// 4. Check whether the message sending URL is correct. SDK 2.0 URL: https://api.push.hicloud.com/pushsend.do SDK 3.0+ URL: https://push-api.cloud.huawei.com/v1/[appId]/messages:send
			/// </remarks>
			InvalidToken = 80300007,

			/// <summary>The message body size exceeds the default value (4096 bytes).</summary>
			/// <remarks>Reduce the message body size.</remarks>
			ExceededMessageBoddy = 80300008,

			/// <summary>The number of tokens in the message body exceeds the default value.</summary>
			/// <remarks>Reduce the number of tokens and send them in batches.</remarks>
			ExceededTokensNumber = 80300010,

			/// <summary>Failed to request the OAuth service.</summary>
			/// <remarks>Check the OAuth 2.0 client ID and client key.</remarks>
			OAuthRequestFailure = 80600003,

			/// <summary>An internal error of the system occurs.</summary>
			/// <remarks>Contact Huawei technical support.</remarks>
			InternalError = 81000001,
		}

		[JsonProperty("code")]
		public ReturnCode Code { get; set; }

		[JsonProperty("msg")]
		public String Message { get; set; }

		[JsonProperty("requestId")]
		public String RequestId { get; set; }
	}
}
