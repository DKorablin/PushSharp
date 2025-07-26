using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The Firebase service connection factory.</summary>
	public class FirebaseServiceConnectionFactory : IServiceConnectionFactory<FirebaseNotification>
	{
		private readonly FirebaseConfiguration _configuration;

		/// <summary>Create instance of Firebase service connection factory.</summary>
		/// <param name="configuration">The Firebase service configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to Firebase server(s).</exception>
		public FirebaseServiceConnectionFactory(FirebaseConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		IServiceConnection<FirebaseNotification> IServiceConnectionFactory<FirebaseNotification>.Create()
			=> new FirebaseServiceConnection(this._configuration);
	}

	/// <summary>The Firebase service broker.</summary>
	public class FirebaseServiceBroker : ServiceBroker<FirebaseNotification>
	{
		/// <summary>Create instance of Firebase service broker.</summary>
		/// <param name="configuration">The Firebase service connection configuration.</param>
		public FirebaseServiceBroker(FirebaseConfiguration configuration) : base(new FirebaseServiceConnectionFactory(configuration))
		{
		}
	}

	/// <summary>The Firebase service connection that is responsible for message processing between client and Google servers.</summary>
	public class FirebaseServiceConnection : IServiceConnection<FirebaseNotification>
	{
		private readonly HttpClient _client;
		private readonly FirebaseConfiguration _configuration;

		/// <summary>Create instance of <see cref="FirebaseServiceConnection"/> with connection information.</summary>
		/// <param name="configuration">The connection configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to Firebase server(s).</exception>
		public FirebaseServiceConnection(FirebaseConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this._client = new PushSharpHttpClient();
		}

		async Task IServiceConnection<FirebaseNotification>.Send(FirebaseNotification notification, CancellationToken cancellationToken)
		{
			var accessToken = this._configuration.AccessToken;
			var path = this._configuration.Settings.MessageSendUri;
			var json = notification.GetJson();

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				using(var response = await this._client.SendAsync(message, cancellationToken))
				{
					if(response.IsSuccessStatusCode)
						await ProcessOkResponseAsync(response, notification).ConfigureAwait(false);
					else
						await ProcessErrorResponseAsync(response, notification).ConfigureAwait(false);
				}
			}
		}

		private static async Task ProcessOkResponseAsync(HttpResponseMessage httpResponse, FirebaseNotification notification)
		{
			var strResponse = await httpResponse.Content.ReadAsStringAsync();
			var json = JObject.Parse(strResponse);

			Log.Trace.TraceData(System.Diagnostics.TraceEventType.Verbose, 100, json);
		}

		private static async Task ProcessErrorResponseAsync(HttpResponseMessage httpResponse, FirebaseNotification notification)
		{// https://firebase.google.com/docs/reference/fcm/rest/v1/ErrorCode
			String responseBody;

			try
			{
				responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch { responseBody = null; }//TODO: Need check exact exception(s) to handle

			//401 bad auth token
			if(httpResponse.StatusCode == HttpStatusCode.Unauthorized)
				throw new UnauthorizedAccessException("GCM Authorization Failed");

			//First try grabbing the retry-after header and parsing it.
			var retryAfterHeader = httpResponse.Headers.RetryAfter;

			if(retryAfterHeader?.Delta != null)
			{
				var retryAfter = retryAfterHeader.Delta.Value;
				throw new RetryAfterException(notification, "GCM Requested Backoff", DateTime.UtcNow + retryAfter);
			}

			var errorResponse = JsonConvert.DeserializeObject<FirebaseMessageResponse>(responseBody);
			throw new FcmNotificationException(notification, errorResponse);
		}
	}
}