using System;
using System.Text;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Apple
{
	/// <summary>The configuration instance to connect and send PUSH message to APNS server(s).</summary>
	/// <see href="https://developer.apple.com/documentation/usernotifications/sending-notification-requests-to-apns"/>
	public class ApnsConfiguration
	{
		private readonly Object _tokenLock = new Object();
		private DateTime _tokenExpiration = DateTime.MinValue;
		private String _token;

		/// <summary>The APNS connection settings.</summary>
		public ApnsSettings Settings { get; }

		/// <summary>The access token that is used to send PUSH messages based on settings information.</summary>
		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.UtcNow)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.UtcNow)
							this.RefreshAccessToken();

				return this._token;
			}
		}

		/// <summary>Create instance of APNS configuration instance.</summary>
		/// <param name="settings">The settings instance.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="settings"/> is required.</exception>
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
			this._tokenExpiration = DateTime.UtcNow.AddMinutes(ApnsSettings.TokenExpirationMinutes);
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