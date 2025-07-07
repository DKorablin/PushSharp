using System;

namespace AlphaOmega.PushSharp.HuaWay
{
	internal class TokenRequest
	{
		public String grant_type { get; set; }

		public String client_id { get; set; }

		public String client_secret { get; set; }
	}
}