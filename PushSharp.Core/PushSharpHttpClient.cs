using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>Custom HTTP client with combined improvements.</summary>
	public class PushSharpHttpClient : HttpClient
	{
		private const String UserAgentName = "PushSharp";
		private const String UserAgentVersion = "4.1";

		private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>Create instance of <see cref="PushSharpHttpClient"/> with required parameters.</summary>
		/// <param name="handler">Base message handler to use.</param>
		public PushSharpHttpClient(HttpMessageHandler handler)
			: base(handler)
		{
			this.DefaultRequestHeaders.UserAgent.Clear();
			this.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(UserAgentName, UserAgentVersion));
		}

		/// <summary>Create instance of <see cref="PushSharpHttpClient"/> without parameters.</summary>
		public PushSharpHttpClient()
			: this(new HttpClientHandler())
		{
		}

		/// <summary>Gets the Unix timestamp that is count starts at the Unix Epoch on January 1st, 1970 at UTC.</summary>
		/// <returns>The Unix timestamp based on current date and time.</returns>
		public static Int32 GetUnixTimestamp()
		{
			TimeSpan span = DateTime.UtcNow - UNIX_EPOCH;
			return Convert.ToInt32(span.TotalSeconds);
		}
	}
}