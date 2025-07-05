using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;

namespace PushSharp.Google
{
	public class FirebaseConfiguration
	{
		private const String TOKEN_URL = "https://oauth2.googleapis.com/token";

		private DateTime? _firebaseTokenExpiration;
		private FirebaseTokenResponse _firebaseToken;

		private readonly FirebaseSettings _settings;

		public String FirebaseSendUrl => $"https://fcm.googleapis.com/v1/projects/{this._settings.ProjectId}/messages:send";

		public FirebaseConfiguration(String jsonFileContents)
		{
			if(String.IsNullOrWhiteSpace(jsonFileContents))
				throw new ArgumentNullException(nameof(jsonFileContents));

			this._settings = JsonConvert.DeserializeObject<FirebaseSettings>(jsonFileContents);
		}

		public FirebaseConfiguration(FirebaseSettings settings)
			=> this._settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public async Task<String> GetJwtTokenAsync()
		{
			if(this._firebaseToken != null && this._firebaseTokenExpiration > DateTime.UtcNow)
				return _firebaseToken.AccessToken;

			using(var message = new HttpRequestMessage(HttpMethod.Post, TOKEN_URL))
			using(var form = new MultipartFormDataContent())
			{
				var authToken = this.GetMasterToken();
				form.Add(new StringContent(authToken), "assertion");
				form.Add(new StringContent("urn:ietf:params:oauth:grant-type:jwt-bearer"), "grant_type");
				message.Content = form;

				using(var client = new HttpClient())
				using(var response = await client.SendAsync(message))
				{
					var content = await response.Content.ReadAsStringAsync();

					if(!response.IsSuccessStatusCode)
						throw new HttpRequestException("Firebase error when creating JWT token: " + content);

					this._firebaseToken = JsonConvert.DeserializeObject<FirebaseTokenResponse>(content);
					this._firebaseTokenExpiration = DateTime.UtcNow.AddSeconds(this._firebaseToken.ExpiresIn - 10);

					return String.IsNullOrWhiteSpace(this._firebaseToken.AccessToken) || this._firebaseTokenExpiration < DateTime.UtcNow
						? throw new InvalidOperationException("Couldn't deserialize firebase token response")
						: this._firebaseToken.AccessToken;
				}
			}
		}

		private String GetMasterToken()
		{
			String header = JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" });
			String payload = JsonConvert.SerializeObject(new
			{
				iss = this._settings.ClientEmail,
				aud = this._settings.TokenUri,
				scope = "https://www.googleapis.com/auth/firebase.messaging",
				iat = GetEpochTimestamp(),
				exp = GetEpochTimestamp() + 3600 /* has to be short lived */
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

		private static Int32 GetEpochTimestamp()
		{
			TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1);
			return Convert.ToInt32(span.TotalSeconds);
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