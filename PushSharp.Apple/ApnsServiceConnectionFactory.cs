using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Apple
{
	/// <summary>The APNS service connection factory.</summary>
	public class ApnsServiceConnectionFactory : IServiceConnectionFactory<ApnsNotification>
	{
		private readonly ApnsConfiguration _configuration;

		/// <summary>Create instance of APNS service connection factory.</summary>
		/// <param name="configuration">The APNS service configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to APNS server(s).</exception>
		public ApnsServiceConnectionFactory(ApnsConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		IServiceConnection<ApnsNotification> IServiceConnectionFactory<ApnsNotification>.Create()
			=> new ApnsServiceConnection(this._configuration);
	}

	/// <summary>The APNS service broker.</summary>
	public class ApnsServiceBroker : ServiceBroker<ApnsNotification>
	{
		/// <summary>Create instance of APNS service broker.</summary>
		/// <param name="configuration">The APNS service configuration information.</param>
		public ApnsServiceBroker(ApnsConfiguration configuration)
			: base(new ApnsServiceConnectionFactory(configuration))
		{
		}
	}

	/// <summary>The APNS service connection that is responsible for message processing between client and Apple server(s).</summary>
	public class ApnsServiceConnection : IServiceConnection<ApnsNotification>
	{
		/// <summary>To deliver the notifications, you’re required to have some header fields.</summary>
		/// <remarks>
		/// In addition to the preceding data, add the following header fields in to your request.
		/// Other headers are optional or may depend on whether you’re using token-based or certificate-based authentication.
		/// </remarks>
		private readonly struct RequestHeaders
		{
			/// <summary>The value of this header must accurately reflect the contents of your notification’s payload.</summary>
			/// <remarks>
			/// If there’s a mismatch, or if the header is missing on required systems, APNs may return an error, delay the delivery of the notification, or drop it altogether.
			/// </remarks>
			public const String ApnsPushType = "apns-push-type";

			/// <summary>The date at which the notification is no longer valid.</summary>
			/// <remarks>
			/// This value is a UNIX epoch expressed in seconds (UTC).
			/// If the value is nonzero, APNs stores the notification and tries to deliver it at least once, repeating the attempt as needed until the specified date.
			/// If the value is 0, APNs attempts to deliver the notification only once and doesn't store it.
			/// </remarks>
			public const String ApnsExpiration = "apns-expiration";

			/// <summary>The priority of the notification.</summary>
			/// <remarks>
			/// If you omit this header, APNs sets the notification priority to 10.
			/// Specify 10 to send the notification immediately.
			/// Specify 5 to send the notification based on power considerations on the user’s device.
			/// Specify 1 to prioritize the device’s power considerations over all other factors for delivery, and prevent awakening the device.
			/// </remarks>
			public const String ApnsPriority = "apns-priority";

			/// <summary>
			/// The topic for the notification.
			/// In general, the topic is your app’s bundle ID/app ID.
			/// It can have a suffix based on the type of push notification.
			/// </summary>
			public const String ApnsTopic = "apns-topic";

			/// <summary>An identifier you use to merge multiple notifications into a single notification for the user.</summary>
			/// <remarks>
			/// Typically, each notification request displays a new notification on the user’s device.
			/// When sending the same notification more than once, use the same value in this header to merge the requests.
			/// The value of this key must not exceed 64 bytes.
			/// </remarks>
			public const String ApnsCollapseId = "apns-collapse-id";

			/// <summary>The value POST</summary>
			public const String Method = ":method";

			/// <summary>The path to the device token.</summary>
			/// <remarks>
			/// The value of this header is /3/device/&lt;device_token&gt;, where &lt;device_token&gt; is the hexadecimal bytes that identify the user’s device.
			/// Your app receives the bytes for this device token when registering for remote notifications.
			/// </remarks>
			public const String Path = ":path";
		}

		private readonly struct ResponseHeaders
		{
			/// <summary>A canonical UUID that’s the unique ID for the notification.</summary>
			/// <remarks>
			/// If an error occurs when sending the notification, APNs includes this value when reporting the error to your server.
			/// Canonical UUIDs are 32 lowercase hexadecimal digits, displayed in five groups separated by hyphens in the form 8-4-4-4-12.
			/// If you omit this header, APNs creates a UUID for you and returns it in its response.
			/// </remarks>
			public const String ApnsId = "apns-id";

			/// <summary>An identifier that is only available in the Development environment.</summary>
			/// <remarks>Use this to query Delivery Log information for the corresponding notification in Push Notifications Console.</remarks>
			public const String ApnsUniqueId = "apns-unique-id";
		}

		private readonly HttpClient _client;
		private readonly ApnsConfiguration _configuration;

		/// <summary>Create instance of APNS service connection.</summary>
		/// <param name="configuration">The connection configuration information.</param>
		/// <exception cref="ArgumentNullException">Configuration is required to connect to APNS server(s).</exception>
		public ApnsServiceConnection(ApnsConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			var handler = new WinHttpHandler();
			this._client = new PushSharpHttpClient(handler)
			{
				BaseAddress = new Uri(this._configuration.Settings.Host),
			};
		}

		async Task IServiceConnection<ApnsNotification>.Send(ApnsNotification notification, CancellationToken cancellationToken)
		{
			var accessToken = this._configuration.AccessToken;
			var path = $"/3/device/{notification.DeviceToken}";
			var json = notification.Payload.ToString(Formatting.None);

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Version = new Version(2, 0);
				message.Content = new StringContent(json);

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				message.Headers.TryAddWithoutValidation(RequestHeaders.Method, "POST");
				message.Headers.TryAddWithoutValidation(RequestHeaders.Path, path);
				message.Headers.Add(RequestHeaders.ApnsPushType, notification.ApnsPushType.ToString().ToLowerInvariant()); // required for iOS 13+

				if(this._configuration.Settings.AppBundleId != null)
					message.Headers.Add(RequestHeaders.ApnsTopic, this._configuration.Settings.AppBundleId);

				if(notification.ApnsId != null)
					message.Headers.Add(ResponseHeaders.ApnsId, notification.ApnsId.Value.ToString("D"));
				if(notification.ApnsExpiration != null)
					message.Headers.Add(RequestHeaders.ApnsExpiration, notification.ApnsExpiration.Value.Ticks.ToString());
				if(notification.ApnsPriority != null)
					message.Headers.Add(RequestHeaders.ApnsPriority, notification.ApnsPriority.ToString());
				if(notification.ApnsCollapseId != null)
					message.Headers.Add(RequestHeaders.ApnsCollapseId, notification.ApnsCollapseId);

				using(var response = await this._client.SendAsync(message, cancellationToken))
				{
					if(response.IsSuccessStatusCode)
						this.ProcessOkResponse(response, notification);
					else
						await ProcessErrorResponseAsync(response, notification);
				}
			}
		}

		private void ProcessOkResponse(HttpResponseMessage httpResponse, ApnsNotification notification)
		{
			if(notification.ApnsId == null)
			{
				String apnsId = httpResponse.Headers.GetValues(ResponseHeaders.ApnsId).Single();
				notification.ApnsId = Guid.Parse(apnsId);

				if(this._configuration.Settings.Environment == ApnsSettings.ApnsServerEnvironment.Development)
				{
					String apnsUniqueId = httpResponse.Headers.GetValues(ResponseHeaders.ApnsUniqueId).Single();
					notification.ApnsUniqueId = apnsUniqueId;
				}
			}
		}

		private static async Task ProcessErrorResponseAsync(HttpResponseMessage httpResponse, ApnsNotification notification)
		{
			String response;

			try
			{
				response = await httpResponse.Content.ReadAsStringAsync();
			} catch { response = null; }

			var errorResponse = JsonConvert.DeserializeObject<ApnsResponse>(response);
			throw new ApnsNotificationException(httpResponse.StatusCode, errorResponse, notification);
		}
	}
}