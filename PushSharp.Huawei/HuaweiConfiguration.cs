using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Huawei
{
	/// <summary></summary>
	/// <see href="https://developer.huawei.com/consumer/en/doc/HMSCore-References-V5/https-send-api-0000001050986197-V5"/>
	/// <see href="https://developer.huawei.com/consumer/en/doc/HMSCore-Guides-V5/open-platform-oauth-0000001053629189-V5#EN-US_TOPIC_0000001053629189__section12493191334711"/>
	public class HuaweiConfiguration
	{
		private const String TOKEN_V3_URL = "https://oauth-login.cloud.huawei.com/oauth2/v3/token";

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

		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.Now)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.Now)
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
					this._tokenExpiration = DateTime.Now.AddMinutes(this._token.expires_in - 1);
				}
			}
		}
	}
}