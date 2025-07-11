using System;
using System.Linq;
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
		private readonly struct Headers
		{
			public const String ApnsPushType = "apns-push-type";
			public const String ApnsId = "apns-id";
			public const String ApnsUniqueId = "apns-unique-id";
			public const String ApnsExpiration = "apns-expiration";
			public const String ApnsPriority = "apns-priority";
			public const String ApnsTopic = "apns-topic";
			public const String ApnsCollapseId = "apns-collapse-id";

			public const String Method = ":method";
			public const String Path = ":path";
		}

		private readonly HttpClient _client;
		private readonly ApnsConfiguration _configuration;

		public ApnsServiceConnection(ApnsConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			var handler = new WinHttpHandler();
			this._client = new HttpClient(handler)
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
			var json = notification.Payload.ToString(Formatting.None);

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Version = new Version(2, 0);
				message.Content = new StringContent(json);

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				message.Headers.TryAddWithoutValidation(Headers.Method, "POST");
				message.Headers.TryAddWithoutValidation(Headers.Path, path);
				message.Headers.Add(Headers.ApnsPushType, notification.ApnsPushType.ToString().ToLowerInvariant()); // required for iOS 13+

				if(this._configuration.Settings.AppBundleId != null)
					message.Headers.Add(Headers.ApnsTopic, this._configuration.Settings.AppBundleId);

				if(notification.ApnsId != null)
					message.Headers.Add(Headers.ApnsId, notification.ApnsId.Value.ToString("D"));
				if(notification.ApnsExpiration != null)
					message.Headers.Add(Headers.ApnsExpiration, notification.ApnsExpiration.Value.Ticks.ToString());
				if(notification.ApnsPriority != null)
					message.Headers.Add(Headers.ApnsPriority, notification.ApnsPriority.ToString());
				if(notification.ApnsCollapseId != null)
					message.Headers.Add(Headers.ApnsCollapseId, notification.ApnsCollapseId);

				using(var response = await this._client.SendAsync(message))
				{
					if(response.IsSuccessStatusCode)
					{
						if(notification.ApnsId == null)
						{
							String apnsId = response.Headers.GetValues(Headers.ApnsId).Single();
							notification.ApnsId = Guid.Parse(apnsId);
						}
					} else
					{
						var strResponse = await response.Content.ReadAsStringAsync();
						var errorResponse = JsonConvert.DeserializeObject<ApnsResponse>(strResponse);
						throw new ApnsNotificationException2(response.StatusCode, errorResponse, notification);
					}
				}
			}
		}
	}
}