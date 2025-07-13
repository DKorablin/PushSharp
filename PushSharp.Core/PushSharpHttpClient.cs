using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AlphaOmega.PushSharp.Core
{
	public class PushSharpHttpClient : HttpClient
	{
		private const String UserAgentName = "PushSharp";
		private const String UserAgentVersion = "4.1";

		private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public PushSharpHttpClient(HttpMessageHandler handler)
			: base(handler)
		{
			this.DefaultRequestHeaders.UserAgent.Clear();
			this.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(UserAgentName, UserAgentVersion));
		}

		public PushSharpHttpClient()
			: this(new HttpClientHandler())
		{
		}

		public static Int32 GetUnixTimestamp()
		{
			TimeSpan span = DateTime.UtcNow - UNIX_EPOCH;
			return Convert.ToInt32(span.TotalSeconds);
		}
	}
}