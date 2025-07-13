using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Google
{
	public class FirebaseServiceConnectionFactory : IServiceConnectionFactory<FirebaseNotification>
	{
		private readonly FirebaseConfiguration _configuration;

		public FirebaseServiceConnectionFactory(FirebaseConfiguration configuration)
			=> this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		public IServiceConnection<FirebaseNotification> Create()
			=> new FirebaseServiceConnection(this._configuration);
	}

	public class FirebaseServiceBroker : ServiceBroker<FirebaseNotification>
	{
		public FirebaseServiceBroker(FirebaseConfiguration configuration) : base(new FirebaseServiceConnectionFactory(configuration))
		{
		}
	}

	public class FirebaseServiceConnection : IServiceConnection<FirebaseNotification>
	{
		private readonly HttpClient _client;
		private readonly FirebaseConfiguration _configuration;

		public FirebaseServiceConnection(FirebaseConfiguration configuration)
		{
			this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this._client = new PushSharpHttpClient();
		}

		async Task IServiceConnection<FirebaseNotification>.Send(FirebaseNotification notification)
		{
			var token = this._configuration.AccessToken;
			var path = this._configuration.FirebaseSendUrl;
			var json = notification.GetJson();

			using(var message = new HttpRequestMessage(HttpMethod.Post, path))
			{
				message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				using(var response = await this._client.SendAsync(message))
				{
					if(response.IsSuccessStatusCode)
						await this.ProcessOkResponseAsync(response, notification).ConfigureAwait(false);
					else
						await this.ProcessErrorResponseAsync(response, notification).ConfigureAwait(false);
				}
			}
		}

		private async Task ProcessOkResponseAsync(HttpResponseMessage httpResponse, FirebaseNotification notification)
		{
			var result = new FirebaseResponse()
			{
				ResponseCode = GcmResponseCode.Ok,
				OriginalNotification = notification
			};

			var strResponse = await httpResponse.Content.ReadAsStringAsync();
			var json = JObject.Parse(strResponse);

			result.NumberOfCanonicalIds = json.Value<Int64>("canonical_ids");
			result.NumberOfFailures = json.Value<Int64>("failure");
			result.NumberOfSuccesses = json.Value<Int64>("success");

			var jsonResults = json["results"] as JArray ?? new JArray();

			foreach(var r in jsonResults)
			{
				var msgResult = new FirebaseMessageResult()
				{
					MessageId = r.Value<String>("message_id"),
					CanonicalRegistrationId = r.Value<String>("registration_id"),
					ResponseStatus = GcmResponseStatus.Ok,
				};

				if(!String.IsNullOrEmpty(msgResult.CanonicalRegistrationId))
					msgResult.ResponseStatus = GcmResponseStatus.CanonicalRegistrationId;
				else if(r["error"] != null)
				{
					var err = r.Value<String>("error") ?? "";
					msgResult.ResponseStatus = GetGcmResponseStatus(err);
				}

				result.Results.Add(msgResult);
			}

			var index = 0;
			var multicastException = new GcmMulticastResultException();
			//Loop through every result in the response
			// We will raise events for each individual result so that the consumer of the library
			// can deal with individual registrationId's for the notification
			foreach(var r in result.Results)
			{
				FirebaseNotification singleResultNotification = FirebaseNotification.ForSingleResult(result, index);
				singleResultNotification.message_id = r.MessageId;

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
						var oldRegistrationId = String.Empty;

						if(singleResultNotification.message.token != null)
							oldRegistrationId = singleResultNotification.message.token;
						else if(!String.IsNullOrEmpty(singleResultNotification.message.topic))
							oldRegistrationId = singleResultNotification.message.topic;

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
						var oldRegistrationId = String.Empty;

						if(singleResultNotification.message.token != null)
							oldRegistrationId = singleResultNotification.message.token;
						else if(!String.IsNullOrEmpty(singleResultNotification.message.topic))
							oldRegistrationId = singleResultNotification.message.topic;

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

		private async Task ProcessErrorResponseAsync(HttpResponseMessage httpResponse, FirebaseNotification notification)
		{// https://firebase.google.com/docs/reference/fcm/rest/v1/ErrorCode
			String responseBody;

			try
			{
				responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
			} catch { responseBody = null; }//TODO: Need check exact exception(s) to handle

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