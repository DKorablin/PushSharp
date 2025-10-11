using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Huawei
{
	/// <summary>The Huawei service connection factory.</summary>
	public class HuaweiServiceConnectionFactory : IServiceConnectionFactory<HuaweiNotification>
	{
		private readonly HuaweiConfiguration _configuration;

		/// <summary>Create instance of Huawei service connection factory.</summary>
		/// <param name="configuration">The Huawei service configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to Huawei server(s).</exception>
		public HuaweiServiceConnectionFactory(HuaweiConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		IServiceConnection<HuaweiNotification> IServiceConnectionFactory<HuaweiNotification>.Create()
			=> new HuaweiServiceConnection(this._configuration);
	}

	/// <summary>The Huawei service broker.</summary>
	public class HuaweiServiceBroker : ServiceBroker<HuaweiNotification>
	{
		/// <summary>Create instance of Huawei service broker.</summary>
		/// <param name="configuration">The Huawei service configuration information.</param>
		public HuaweiServiceBroker(HuaweiConfiguration configuration) : base(new HuaweiServiceConnectionFactory(configuration))
		{
		}
	}

	/// <summary>The Huawei service connection that is responsible for message processing between client and Huawei server(s).</summary>
	public class HuaweiServiceConnection : IServiceConnection<HuaweiNotification>
	{
		private readonly HttpClient _client;
		private readonly HuaweiConfiguration _configuration;

		/// <summary>Create instance of Huawei service connection.</summary>
		/// <param name="configuration">The connection configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to Huawei server(s).</exception>
		public HuaweiServiceConnection(HuaweiConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this._client = new PushSharpHttpClient();
		}

		async Task IServiceConnection<HuaweiNotification>.Send(HuaweiNotification notification, CancellationToken cancellationToken)
			=> await this.SendWithRetry(notification, true, cancellationToken);

		private async Task SendWithRetry(HuaweiNotification notification, Boolean withRetryOnTokenExpiration, CancellationToken cancellationToken)
		{
			var token = this._configuration.AccessToken;
			var path = this._configuration.HuaweiSendUrl;
			var json = notification.GetJson();

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Content = new StringContent(json, Encoding.UTF8, "application/json");

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				using(var response = await this._client.SendAsync(message, cancellationToken))
				{
					if(response.IsSuccessStatusCode)
						await this.ProcessSuccessResponseAsync(response, notification, withRetryOnTokenExpiration, cancellationToken).ConfigureAwait(false);
					else
						await ProcessFailureResponseAsync(response, notification).ConfigureAwait(false);
				}
			}
		}

		private static async Task ProcessFailureResponseAsync(HttpResponseMessage httpResponse, HuaweiNotification notification)
		{
			String responseBody;

			try
			{
				responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch { responseBody = null; }//TODO: Need check exact exception(s) to handle

			Exception exc;
			switch(httpResponse.StatusCode)
			{
			case System.Net.HttpStatusCode.Unauthorized:
				exc = new UnauthorizedAccessException("Verify the access token in the Authorization parameter in the request HTTP header.");
				break;
			default:
				exc = new HuaweiException(notification, httpResponse.StatusCode);
				break;
			}

			if(responseBody != null)
				exc.Data.Add("Response", responseBody);
			throw exc;
		}

		private async Task ProcessSuccessResponseAsync(HttpResponseMessage httpResponse, HuaweiNotification notification, Boolean withRetryOnTokenExpiration, CancellationToken cancallationToken)
		{
			var strResponse = await httpResponse.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<HuaweiResponse>(strResponse);

			switch(response.Code)
			{
			case HuaweiResponse.ReturnCode.Success:
				return;
			case HuaweiResponse.ReturnCode.OAuthTokenExpired:
				if(withRetryOnTokenExpiration)
				{
					await this._configuration.RefreshTokenAsync();
					await this.SendWithRetry(notification, false, cancallationToken);
				} else
					throw new HuaweiException(notification, response);
				break;
			default:
				if(Enum.IsDefined(typeof(HuaweiResponse.ReturnCode), response.Code))
					throw new HuaweiException(notification, response);
				else
				{
					Exception exc1 = new InvalidOperationException($"{response.Message}. Code: {response.Code}");
					exc1.Data.Add("Response", strResponse);
					throw exc1;
				}
			}
		}
	}
}