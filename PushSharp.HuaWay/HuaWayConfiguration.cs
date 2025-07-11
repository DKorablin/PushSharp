using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWayConfiguration
	{
		private const String TOKEN_URL = "https://oauth-login.cloud.huawei.com/oauth2/v2/token";

		private readonly Object _tokenLock = new Object();
		private TokenResponse _token;
		private DateTime _tokenExpiration = DateTime.MinValue;

		private readonly HuaWaySettings _settings;

		public String HuaWaySendUrl
		{
			get
			{
				if(this._settings.ProjectId != null)
					return $"https://push-api.cloud.huawei.com/v2/{this._settings.ProjectId}/messages:send";
				else if(this._settings.ApplicationId != null)
					return $"https://push-api.cloud.huawei.com/v1/{this._settings.ApplicationId}/messages:send";
				else
					throw new InvalidOperationException($"{nameof(this._settings.ProjectId)} or {nameof(this._settings.ApplicationId)} is required.");
			}
		}

		public HuaWayConfiguration(HuaWaySettings settings)
			=> this._settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.Now)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.Now)
							this.RefreshToken();

				return this._token.access_token;
			}
		}

		internal void RefreshToken()
		{
			var requestToken = GetTokenRequest();

			using(var message = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL))
			using(var form = new MultipartFormDataContent())
			{
				form.Add(new StringContent(requestToken));
				message.Content = form;

				using(var client = new HttpClient())
				using(var response = client.SendAsync(message).Result)
				{
					String content = response.Content.ReadAsStringAsync().Result;

					if(!response.IsSuccessStatusCode)
					{
						var exc = new HttpRequestException("HuaWay error while requesting JWT token");
						exc.Data.Add("Response", content);
						throw exc;
					}

					this._token = JsonConvert.DeserializeObject<TokenResponse>(content);
					this._tokenExpiration = DateTime.Now.AddMinutes(this._token.expires_in - 1);
				}
			}

			String GetTokenRequest()
				=> JsonConvert.SerializeObject(new TokenRequest() { grant_type = "client_credentials", client_id = this._settings.ClientId, client_secret = this._settings.ClientSecret });
		}
	}
}