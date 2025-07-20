using System;

namespace AlphaOmega.PushSharp.Huawei
{
	/// <summary>The response with access token.</summary>
	internal class TokenResponse
	{
		/// <summary>App-level access token.</summary>
		public String access_token { get; set; }

		/// <summary>Remaining validity period of an access token, in seconds.</summary>
		public Int32 expires_in { get; set; }

		/// <summary>Type of the returned access token. The value is always Bearer.</summary>
		public String token_type { get; set; }
	}
}