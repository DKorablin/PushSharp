using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The Firebase response with access token information.</summary>
	internal class FirebaseTokenResponse
	{
		/// <summary>The access toke.</summary>
		[JsonProperty("access_token")]
		public String AccessToken{ get; set; }

		/// <summary>The type of token.</summary>
		[JsonProperty("token_type")]
		public String TokenType { get; set; }

		/// <summary>The token expiration in minutes.</summary>
		[JsonProperty("expires_in")]
		public Int32 ExpiresIn { get; set; }
	}
}