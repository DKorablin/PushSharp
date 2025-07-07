using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
	internal class FirebaseTokenResponse
	{
		[JsonProperty("access_token")]
		public String AccessToken{ get; set; }

		[JsonProperty("token_type")]
		public String TokenType { get; set; }

		[JsonProperty("expires_in")]
		public Int32 ExpiresIn { get; set; }
	}
}