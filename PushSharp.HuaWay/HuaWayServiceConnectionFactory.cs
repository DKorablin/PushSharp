using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWayServiceConnectionFactory : IServiceConnectionFactory<HuaWayNotification>
	{
		private readonly HuaWayConfiguration _configuration;

		public HuaWayServiceConnectionFactory(HuaWayConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		public IServiceConnection<HuaWayNotification> Create()
			=> new HuaWayServiceConnection(this._configuration);
	}

	public class HuaWayServiceBroker : ServiceBroker<HuaWayNotification>
	{
		public HuaWayServiceBroker(HuaWayConfiguration configuration) : base(new HuaWayServiceConnectionFactory(configuration))
		{
		}
	}

	public class HuaWayServiceConnection : IServiceConnection<HuaWayNotification>
	{
		private readonly HttpClient _client;
		private readonly HuaWayConfiguration _configuration;

		public HuaWayServiceConnection(HuaWayConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this._client = new HttpClient();

			this._client.DefaultRequestHeaders.UserAgent.Clear();
			this._client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PushSharp", "4.1"));
		}

		async Task IServiceConnection<HuaWayNotification>.Send(HuaWayNotification notification)
			=> await this.SendWithRetry(notification, true);

		private async Task SendWithRetry(HuaWayNotification notification, Boolean withRetryOnTokenExpiration)
		{
			var token = this._configuration.AccessToken;
			var json = notification.GetJson();
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			this._client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
			var response = await this._client.PostAsync(this._configuration.HuaWaySendUrl, content);

			if(response.IsSuccessStatusCode)
				await this.ProcessSuccessResponseAsync(response, notification, withRetryOnTokenExpiration).ConfigureAwait(false);
			else
				await this.ProcessFailureResponseAsync(response, notification).ConfigureAwait(false);
		}

		private async Task ProcessFailureResponseAsync(HttpResponseMessage httpResponse, HuaWayNotification notification)
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
				exc = new HuaWayException(notification, httpResponse.StatusCode);
				break;
			}

			if(responseBody != null)
				exc.Data.Add("Response", responseBody);
			throw exc;
		}

		private async Task ProcessSuccessResponseAsync(HttpResponseMessage httpResponse, HuaWayNotification notification, Boolean withRetryOnTokenExpiration)
		{
			var strResponse = await httpResponse.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<HuaWayResponse>(strResponse);

			switch(response.Code)
			{
			case HuaWayResponse.ReturnCode.Success:
				return;
			case HuaWayResponse.ReturnCode.OAuthTokenExpired:
				if(withRetryOnTokenExpiration)
				{
					this._configuration.RefreshToken();
					await this.SendWithRetry(notification, false);
				} else
					throw new HuaWayException(notification, response);
				break;
			default:
				if(Enum.IsDefined(typeof(HuaWayResponse.ReturnCode), response.Code))
					throw new HuaWayException(notification, response);
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