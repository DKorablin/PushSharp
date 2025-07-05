using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PushSharp.Core;

namespace PushSharp.Google
{
	public class FirebaseServiceConnectionFactory : IServiceConnectionFactory<GcmNotification>
	{
		public FirebaseServiceConnectionFactory(FirebaseConfiguration configuration)
			=> this.Configuration = configuration;

		public FirebaseConfiguration Configuration { get; private set; }

		public IServiceConnection<GcmNotification> Create()
			=> new FirebaseServiceConnection(this.Configuration);
	}

	public class FirebaseServiceBroker : ServiceBroker<GcmNotification>
	{
		public FirebaseServiceBroker(FirebaseConfiguration configuration) : base(new FirebaseServiceConnectionFactory(configuration))
		{
		}
	}

	public class FirebaseServiceConnection : IServiceConnection<GcmNotification>
	{
		private readonly HttpClient _client;

		public FirebaseConfiguration Configuration { get; private set; }

		public FirebaseServiceConnection(FirebaseConfiguration configuration)
		{
			this.Configuration = configuration;
			this._client = new HttpClient();

			this._client.DefaultRequestHeaders.UserAgent.Clear();
			this._client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PushSharp", "3.0"));
		}

		public async Task Send(GcmNotification notification)
		{
			String token = await this.Configuration.GetJwtTokenAsync();

			var json = notification.GetJson();

			var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

			this._client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
			var response = await _client.PostAsync(this.Configuration.FirebaseSendUrl, content);

			if(response.IsSuccessStatusCode)
				await ProcessOkResponseAsync(response, notification).ConfigureAwait(false);
			else
				await ProcessErrorResponseAsync(response, notification).ConfigureAwait(false);
		}

		private async Task ProcessOkResponseAsync(HttpResponseMessage httpResponse, GcmNotification notification)
		{
			var result = new GcmResponse()
			{
				ResponseCode = GcmResponseCode.Ok,
				OriginalNotification = notification
			};

			var str = await httpResponse.Content.ReadAsStringAsync();
			var json = JObject.Parse(str);

			result.NumberOfCanonicalIds = json.Value<Int64>("canonical_ids");
			result.NumberOfFailures = json.Value<Int64>("failure");
			result.NumberOfSuccesses = json.Value<Int64>("success");

			var jsonResults = json["results"] as JArray ?? new JArray();

			foreach(var r in jsonResults)
			{
				var msgResult = new GcmMessageResult();

				msgResult.MessageId = r.Value<String>("message_id");
				msgResult.CanonicalRegistrationId = r.Value<String>("registration_id");
				msgResult.ResponseStatus = GcmResponseStatus.Ok;

				if(!string.IsNullOrEmpty(msgResult.CanonicalRegistrationId))
					msgResult.ResponseStatus = GcmResponseStatus.CanonicalRegistrationId;
				else if(r["error"] != null)
				{
					var err = r.Value<string>("error") ?? "";
					msgResult.ResponseStatus = GetGcmResponseStatus(err);
				}

				result.Results.Add(msgResult);
			}

			Int32 index = 0;
			var multicastException = new GcmMulticastResultException();
			//Loop through every result in the response
			// We will raise events for each individual result so that the consumer of the library
			// can deal with individual registrationId's for the notification
			foreach(var r in result.Results)
			{
				var singleResultNotification = GcmNotification.ForSingleResult(result, index);

				singleResultNotification.MessageId = r.MessageId;

				switch(r.ResponseStatus)
				{
				case GcmResponseStatus.Ok:// Success
					multicastException.Succeeded.Add(singleResultNotification);
					break;
				case GcmResponseStatus.CanonicalRegistrationId:
					{
						//Need to swap reg id's
						//Swap Registrations Id's
						var newRegistrationId = r.CanonicalRegistrationId;
						var oldRegistrationId = string.Empty;

						if(singleResultNotification.RegistrationIds?.Count > 0)
							oldRegistrationId = singleResultNotification.RegistrationIds[0];
						else if(!String.IsNullOrEmpty(singleResultNotification.To))
							oldRegistrationId = singleResultNotification.To;

						multicastException.Failed.Add(singleResultNotification,
							new DeviceSubscriptionExpiredException(singleResultNotification)
							{
								OldSubscriptionId = oldRegistrationId,
								NewSubscriptionId = newRegistrationId
							});
					}
					break;
				case GcmResponseStatus.Unavailable:// Unavailable
					multicastException.Failed.Add(singleResultNotification, new GcmNotificationException(singleResultNotification, "Unavailable Response Status"));
					break;
				case GcmResponseStatus.NotRegistered://Bad registration Id
					{
						var oldRegistrationId = string.Empty;

						if(singleResultNotification.RegistrationIds != null && singleResultNotification.RegistrationIds.Count > 0)
							oldRegistrationId = singleResultNotification.RegistrationIds[0];
						else if(!string.IsNullOrEmpty(singleResultNotification.To))
							oldRegistrationId = singleResultNotification.To;

						multicastException.Failed.Add(singleResultNotification,
							new DeviceSubscriptionExpiredException(singleResultNotification)
							{
								OldSubscriptionId = oldRegistrationId
							});
					}
					break;
				default:
					multicastException.Failed.Add(singleResultNotification, new GcmNotificationException(singleResultNotification, "Unknown Failure: " + r.ResponseStatus));
					break;
				}

				index++;
			}

			// If we only have 1 total result, it is not *multicast*, 
			if(multicastException.Succeeded.Count + multicastException.Failed.Count == 1)
			{
				// If not multicast, and succeeded, don't throw any errors!
				if(multicastException.Succeeded.Count == 1)
					return;
				// Otherwise, throw the one single failure we must have
				throw multicastException.Failed.First().Value;
			}

			// If we get here, we must have had a multicast message
			// throw if we had any failures at all (otherwise all must be successful, so throw no error
			if(multicastException.Failed.Count > 0)
				throw multicastException;
		}

		private async Task ProcessErrorResponseAsync(HttpResponseMessage httpResponse, GcmNotification notification)
		{
			String responseBody;

			try
			{
				responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch { responseBody = null; }//TODO: Need check exact exception to handle

			//401 bad auth token
			if(httpResponse.StatusCode == HttpStatusCode.Unauthorized)
				throw new UnauthorizedAccessException("GCM Authorization Failed");

			if(httpResponse.StatusCode == HttpStatusCode.BadRequest)
				throw new GcmNotificationException(notification, "HTTP 400 Bad Request", responseBody);

			if((Int32)httpResponse.StatusCode >= 500 && (Int32)httpResponse.StatusCode < 600)
			{
				//First try grabbing the retry-after header and parsing it.
				var retryAfterHeader = httpResponse.Headers.RetryAfter;

				if(retryAfterHeader?.Delta != null)
				{
					var retryAfter = retryAfterHeader.Delta.Value;
					throw new RetryAfterException(notification, "GCM Requested Backoff", DateTime.UtcNow + retryAfter);
				}
			}

			throw new GcmNotificationException(notification, "GCM HTTP Error: " + httpResponse.StatusCode, responseBody);
		}

		static GcmResponseStatus GetGcmResponseStatus(String str)
		{
			var enumType = typeof(GcmResponseStatus);

			foreach(var name in Enum.GetNames(enumType))
			{
				var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();

				if(enumMemberAttribute.Value.Equals(str, StringComparison.InvariantCultureIgnoreCase))
					return (GcmResponseStatus)Enum.Parse(enumType, name);
			}

			//Default
			return GcmResponseStatus.Error;
		}
	}
}