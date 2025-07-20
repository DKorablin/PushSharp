using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The configuration instance to connect and send PUSH message to Firebase server(s).</summary>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages"/>
	public class FirebaseConfiguration
	{
#pragma warning disable S1075
		private const String TOKEN_URL = "https://oauth2.googleapis.com/token";
#pragma warning restore S1075

		private readonly Object _tokenLock = new Object();
		private DateTime _tokenExpiration = DateTime.MinValue;
		private FirebaseTokenResponse _token;

		private readonly FirebaseSettings _settings;

		/// <summary>The Firebase PUSH message send Url.</summary>
		public String FirebaseSendUrl => $"https://fcm.googleapis.com/v1/projects/{this._settings.ProjectId}/messages:send";

		/// <summary>The access token that is used to send PUSH messages based on settings information.</summary>
		public String AccessToken
		{
			get
			{
				if(this._tokenExpiration < DateTime.UtcNow)
					lock(this._tokenLock)
						if(this._tokenExpiration < DateTime.UtcNow)
							this.RefreshAccessTokenAsync().Wait();

				return this._token.AccessToken;
			}
		}

		/// <summary>Create instance of Firebase configuration instance.</summary>
		/// <param name="jsonFileContents">The JSON file contents with Firebase service account information.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="jsonFileContents"/> is required.</exception>
		public FirebaseConfiguration(String jsonFileContents)
		{
			if(String.IsNullOrWhiteSpace(jsonFileContents))
				throw new ArgumentNullException(nameof(jsonFileContents));

			this._settings = JsonConvert.DeserializeObject<FirebaseSettings>(jsonFileContents);
		}

		/// <summary>Create instance of Firebase configuration instance.</summary>
		/// <param name="settings">The Firebase service account settings information.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="settings"/> is required.</exception>
		public FirebaseConfiguration(FirebaseSettings settings)
			=> this._settings = settings ?? throw new ArgumentNullException(nameof(settings));

		private async Task RefreshAccessTokenAsync()
		{
			using(var message = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL))
			using(var form = new MultipartFormDataContent())
			{
				var authToken = this.GetTokenRequest();
				form.Add(new StringContent(authToken), "assertion");
				form.Add(new StringContent("urn:ietf:params:oauth:grant-type:jwt-bearer"), "grant_type");
				message.Content = form;

				using(var client = new PushSharpHttpClient())
				using(var response = await client.SendAsync(message))
				{
					var content = await response.Content.ReadAsStringAsync();

					if(!response.IsSuccessStatusCode)
					{
						var exc = new HttpRequestException("Firebase error while requesting JWT token");
						exc.Data.Add("Response", content);
						throw exc;
					}

					this._token = JsonConvert.DeserializeObject<FirebaseTokenResponse>(content);
					this._tokenExpiration = DateTime.UtcNow.AddSeconds(this._token.ExpiresIn - 10);
				}
			}
		}

		private String GetTokenRequest()
		{
			String header = JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" });
			String payload = JsonConvert.SerializeObject(new
			{
				iss = this._settings.ClientEmail,
				aud = this._settings.TokenUri,
				scope = "https://www.googleapis.com/auth/firebase.messaging",
				iat = PushSharpHttpClient.GetUnixTimestamp(),
				exp = PushSharpHttpClient.GetUnixTimestamp() + 3600 /* has to be short lived */
			});

			String headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
			String payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
			String unsignedJwtData = $"{headerBase64}.{payloadBase64}";
			Byte[] unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

			var privateKey = ParsePkcs8PrivateKeyPem(this._settings.PrivateKey);
			var signer = new RsaDigestSigner(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
			signer.Init(true, privateKey);
			signer.BlockUpdate(unsignedJwtBytes, 0, unsignedJwtBytes.Length);

			var signature = signer.GenerateSignature();
			var signatureBase64 = Convert.ToBase64String(signature);

			return $"{unsignedJwtData}.{signatureBase64}";
		}

		private static AsymmetricKeyParameter ParsePkcs8PrivateKeyPem(String key)
		{
			using(StringReader keyReader = new StringReader(key))
			{
				PemReader pemReader = new PemReader(keyReader);
				Object pemObject = pemReader.ReadObject();

				if(pemObject is AsymmetricKeyParameter keyParameter)
					return keyParameter;// PKCS#8 keys are typically returned as AsymmetricKeyParameter, not AsymmetricCipherKeyPair
				else if(pemObject is AsymmetricCipherKeyPair keyPair)
					return keyPair.Private;// handle case of key pair
				else
					throw new InvalidOperationException("Invalid private key format");
			}
		}
	}
}