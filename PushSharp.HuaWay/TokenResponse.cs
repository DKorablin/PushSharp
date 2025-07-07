using System;

namespace AlphaOmega.PushSharp.HuaWay
{
	internal class TokenResponse
	{
		public String access_token { get; set; }

		public Int32 expires_in { get; set; }

		public String token_type { get; set; }
	}
}