using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace AlphaOmega.PushSharp.Core
{
	public static class PushHttpClient
	{
		static PushHttpClient()
		{
			ServicePointManager.DefaultConnectionLimit = 100;
			ServicePointManager.Expect100Continue = false;
		}

		public static async Task<PushHttpResponse> RequestAsync(PushHttpRequest request)
		{
			var httpRequest = HttpWebRequest.CreateHttp(request.Url);
			httpRequest.Proxy = null;

			httpRequest.Headers = request.Headers;

			if(!String.IsNullOrEmpty(request.Body))
			{
				var requestStream = await httpRequest.GetRequestStreamAsync();

				var requestBody = request.Encoding.GetBytes(request.Body);

				await requestStream.WriteAsync(requestBody, 0, requestBody.Length);
			}

			HttpWebResponse httpResponse = null;

			try
			{
				Stream responseStream;
				try
				{
					httpResponse = await httpRequest.GetResponseAsync() as HttpWebResponse;

					responseStream = httpResponse.GetResponseStream();
				} catch(WebException webEx)
				{
					httpResponse = webEx.Response as HttpWebResponse;

					responseStream = httpResponse.GetResponseStream();
				}

				var body = String.Empty;

				using(var sr = new StreamReader(responseStream))
					body = await sr.ReadToEndAsync();

				var responseEncoding = Encoding.ASCII;
				try
				{
					responseEncoding = Encoding.GetEncoding(httpResponse.ContentEncoding);
				} catch(ArgumentException)
				{
				}

				var response = new PushHttpResponse
				{
					Body = body,
					Headers = httpResponse.Headers,
					Uri = httpResponse.ResponseUri,
					Encoding = responseEncoding,
					LastModified = httpResponse.LastModified,
					StatusCode = httpResponse.StatusCode
				};
				return response;
			} finally
			{
				httpResponse?.Close();
				httpResponse?.Dispose();
			}
		}

		public static Int32 GetEpochTimestamp()
		{
			TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1);
			return Convert.ToInt32(span.TotalSeconds);
		}

	}

	public class PushHttpRequest
	{
		public String Url { get; set; }

		public String Method { get; set; } = "GET";

		public String Body { get; set; } = String.Empty;

		public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();

		public Encoding Encoding { get; set; } = Encoding.ASCII;
	}

	public class PushHttpResponse
	{
		public HttpStatusCode StatusCode { get; set; }

		public String Body { get; set; }

		public WebHeaderCollection Headers { get; set; }

		public Uri Uri { get; set; }

		public Encoding Encoding { get; set; }

		public DateTime LastModified { get; set; }
	}
}