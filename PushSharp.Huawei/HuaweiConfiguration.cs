using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Huawei
{
	/// <summary>The Huawei configuration information for sending push messages using Huawei server(s).</summary>
	/// <see href="https://developer.huawei.com/consumer/en/doc/HMSCore-References-V5/https-send-api-0000001050986197-V5"/>
	/// <see href="https://developer.huawei.com/consumer/en/doc/HMSCore-Guides-V5/open-platform-oauth-0000001053629189-V5#EN-US_TOPIC_0000001053629189__section12493191334711"/>
	public class HuaweiConfiguration
	{
#pragma warning disable S1075 // URIs should not be hardcoded
		private const String TOKEN_V3_URL = "https://oauth-login.cloud.huawei.com/oauth2/v3/token";
#pragma warning restore S1075 // URIs should not be hardcoded

		private readonly Object _tokenLock = new Object();
		private TokenResponse _token;
		private DateTime _tokenExpiration = DateTime.MinValue;

		#region Settings
		private readonly String _clientSecret;

		/// <summary>https://push-api.cloud.huawei.com/v2/projectid/messages:send</summary>
		private readonly String _projectId;

		/// <summary>https://push-api.cloud.huawei.com/v1/appid/messages:send</summary>
		private readonly String _applicationId;//TODO: Old version. Should migrate to the _projectId (V2 API)
		#endregion Settings

		/// <summary>Absolute url to send PUSh messages using configuration parameters.</summary>
		public String HuaweiSendUrl
		{
			get
			{
				if(this._projectId != null)
					return $"https://push-api.cloud.huawei.com/v2/{this._projectId}/messages:send";
				else if(this._applicationId != null)
					return $"https://push-api.cloud.huawei.com/v1/{this._applicationId}/messages:send";
				else
					throw new InvalidOperationException($"{nameof(this._projectId)} or {nameof(this._applicationId)} is required.");
			}
		}

		/// <summary>Create instance of <see cref="HuaweiConfiguration"/> with required parameters.</summary>
		/// <param name="clientSecret">The client secret.</param>
		/// <param name="projectId">The project identifier (V2 PUSH endpoint).</param>
		/// <param name="applicationId">The application identifier (V1 PUSH endpoint).</param>
		/// <exception cref="ArgumentException">The <paramref name="projectId"/> OR <paramref name="applicationId"/> are required for valid configuration.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="clientSecret"/> is required.</exception>
		public HuaweiConfiguration(String clientSecret, String projectId, String applicationId)
		{
			if(projectId == null && applicationId == null)
				throw new ArgumentException($"{nameof(projectId)} OR {nameof(applicationId)} is required");
			if(projectId != null && applicationId != null)
				throw new ArgumentException($"Only one {nameof(projectId)} OR {nameof(applicationId)} is required");

			this._clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));

			this._projectId = projectId;
			this._applicationId = applicationId;
		}

		/// <summary>The last used and valid access token to send PUSH messages.</summary>
		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.UtcNow)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.UtcNow)
							this.RefreshTokenAsync().Wait();

				return this._token.access_token;
			}
		}

		internal async Task RefreshTokenAsync()
		{
			Dictionary<String, String> payload = new Dictionary<String, String>()
			{
				{ "grant_type", "client_credentials" },//Set this parameter to client_credentials, which indicates the client credential mode.
				{ "client_id", this._applicationId ?? this._projectId },//OAuth 2.0 client ID obtained during integration preparations. For an app created in AppGallery Connect, set this parameter to its app ID.
				{ "client_secret", this._clientSecret }//Client secret allocated to the OAuth 2.0 client ID during integration preparations. For an app created in AppGallery Connect, set this parameter to its app secret.
			};

			using(var message = new HttpRequestMessage(HttpMethod.Post, TOKEN_V3_URL))
			using(var form = new FormUrlEncodedContent(payload.ToArray()))
			{
				message.Content = form;

				using(var client = new PushSharpHttpClient())
				using(var response = await client.SendAsync(message))
				{
					String content = await response.Content.ReadAsStringAsync();

					if(!response.IsSuccessStatusCode)
					{
						var exc = new HttpRequestException("Huawei error while requesting JWT token");
						exc.Data.Add("Response", content);
						throw exc;
					}

					this._token = JsonConvert.DeserializeObject<TokenResponse>(content);
					this._tokenExpiration = DateTime.UtcNow.AddMinutes(this._token.expires_in - 1);
				}
			}
		}
	}
}