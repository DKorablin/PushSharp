using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsServiceConnectionFactory : IServiceConnectionFactory<ApnsNotification>
	{
		private readonly ApnsConfiguration _configuration;

		public ApnsServiceConnectionFactory(ApnsConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		public IServiceConnection<ApnsNotification> Create()
			=> new ApnsServiceConnection(this._configuration);
	}

	public class ApnsServiceBroker : ServiceBroker<ApnsNotification>
	{
		public ApnsServiceBroker(ApnsConfiguration configuration)
			: base(new ApnsServiceConnectionFactory(configuration))
		{
		}
	}

	public class ApnsServiceConnection : IServiceConnection<ApnsNotification>
	{
		private readonly HttpClient _client;
		private readonly ApnsConfiguration _configuration;

		public ApnsServiceConnection(ApnsConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			this._client = new HttpClient()
			{
				BaseAddress = new Uri(this._configuration.Settings.Host),
			};
			this._client.DefaultRequestHeaders.UserAgent.Clear();
			this._client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PushSharp", "4.1"));
		}

		async Task IServiceConnection<ApnsNotification>.Send(ApnsNotification notification)
		{
			var token = this._configuration.AccessToken;
			var path = $"/3/device/{notification.DeviceToken}";
			var json = notification.Payload.ToString(Newtonsoft.Json.Formatting.None);

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Version = new Version(2, 0);
				message.Content = new StringContent(json);

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				message.Headers.TryAddWithoutValidation(":method", "POST");
				message.Headers.TryAddWithoutValidation(":path", path);
				message.Headers.Add("apns-push-type", notification.ApnsPushType.ToString().ToLowerInvariant()); // required for iOS 13+

				if(notification.ApnsId != null)
					message.Headers.Add("apns-id", notification.ApnsId.Value.ToString("D"));
				if(notification.ApnsExpiration != null)
					message.Headers.Add("apns-expiration", notification.ApnsExpiration.Value.Ticks.ToString());
				if(notification.ApnsPriority != null)
					message.Headers.Add("apns-priority", notification.ApnsPriority.ToString());
				message.Headers.Add("apns-topic", this._configuration.Settings.ApnsTopic);

				using(var response = await this._client.SendAsync(message))
				{
					var strResponse = await response.Content.ReadAsStringAsync();
					if(!response.IsSuccessStatusCode)
					{
						var errorResponse = JsonConvert.DeserializeObject<ApnsResponse>(strResponse);
						throw new ApnsNotificationException2(errorResponse, notification);
					}
				}
			}
		}
	}
}