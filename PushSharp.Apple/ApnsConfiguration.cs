using System;
using System.Text;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Apple
{
	/// <summary></summary>
	/// <see href="https://developer.apple.com/documentation/usernotifications/sending-notification-requests-to-apns"/>
	public class ApnsConfiguration
	{
		private readonly Object _tokenLock = new Object();
		private DateTime _tokenExpiration = DateTime.MinValue;
		private const Int32 TokenExpirationMinutes = 60;
		private String _token;

		public ApnsSettings Settings { get; }

		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.Now)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.Now)
							this.RefreshAccessToken();

				return this._token;
			}
		}

		public ApnsConfiguration(ApnsSettings settings)
			=> this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));

		private void RefreshAccessToken()
		{
			var header = JsonConvert.SerializeObject(new { alg = "ES256", kid = this.Settings.KeyId });
			var payload = JsonConvert.SerializeObject(new { iss = this.Settings.TeamId, iat = PushSharpHttpClient.GetUnixTimestamp() });
			var headerBase64 = Base64UrlEncode(header);
			var payloadBase64 = Base64UrlEncode(payload);

			var dataToSign = headerBase64 + "." + payloadBase64;
			var dataToSignBytes = Encoding.UTF8.GetBytes(dataToSign);

			Byte[] signatureBytes = this.Settings.SignWithECDsa(dataToSignBytes);
			String encodedSignature = Base64UrlEncode(signatureBytes);

			this._token = dataToSign + "." + encodedSignature;
			this._tokenExpiration = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes);
		}

		private static String Base64UrlEncode(String str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);
			return Base64UrlEncode(bytes);
		}

		private static String Base64UrlEncode(Byte[] payload)
			=> Convert.ToBase64String(payload)
				.TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_');
	}
}