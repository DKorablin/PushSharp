using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace AlphaOmega.PushSharp.Apple
{
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
			var payload = JsonConvert.SerializeObject(new { iss = this.Settings.TeamId, iat = PushHttpClient.GetUnixTimestamp() });
			var headerBase64 = Base64UrlEncode(header);
			var payloadBase64 = Base64UrlEncode(payload);

			var dataToSign = headerBase64 + "." + payloadBase64;
			var dataToSignBytes = Encoding.UTF8.GetBytes(dataToSign);

			Byte[] signatureBytes = SignWithECDsa(this.Settings.P8Certificate, dataToSignBytes);

			String encodedSignature = Base64UrlEncode(signatureBytes);

			this._token = dataToSign + "." + encodedSignature;
			this._tokenExpiration = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes);
		}

		private void RefreshAccessToken1()
		{
			var header = JsonConvert.SerializeObject(new { alg = "ES256", kid = this.Settings.KeyId });
			var payload = JsonConvert.SerializeObject(new { iss = this.Settings.TeamId, iat = PushHttpClient.GetUnixTimestamp() });
			var headerBase64 = Base64UrlEncode(header);
			var payloadBase64 = Base64UrlEncode(payload);

			var dataToSign = headerBase64 + "." + payloadBase64;
			var dataToSignBytes = Encoding.UTF8.GetBytes(dataToSign);

			var privateKeyBytes = Convert.FromBase64String(CleanP8Key(this.Settings.P8Certificate));
			var keyParams = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(privateKeyBytes);
			var q = keyParams.Parameters.G.Multiply(keyParams.D).Normalize();

			var parameters = new ECParameters
			{
				Curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id),
				D = keyParams.D.ToByteArrayUnsigned(),
				Q =
			{
				X = q.XCoord.GetEncoded(),
				Y = q.YCoord.GetEncoded()
			}
			};

			using(var dsa = ECDsa.Create(parameters))
			{
				var signature = dsa.SignData(dataToSignBytes, 0, dataToSignBytes.Length, HashAlgorithmName.SHA256);
				var signatureBase64 = Base64UrlEncode(signature);

				this._token = dataToSign + "." + signatureBase64;
				this._tokenExpiration = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes);
			}

			String CleanP8Key(String p8Key)
			{
				// If we have an empty p8Key, then don't bother doing any tasks.
				if(String.IsNullOrEmpty(p8Key))
					return p8Key;

				var lines = p8Key.Split('\n').ToList();

				if(lines.Count > 0 && lines[0].StartsWith("-----BEGIN PRIVATE KEY-----"))
					lines.RemoveAt(0);

				if(lines.Count > 0 && lines[lines.Count - 1].StartsWith("-----END PRIVATE KEY-----"))
					lines.RemoveAt(lines.Count - 1);

				var result = String.Join(String.Empty, lines);

				return result;
			}
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

		private static Byte[] SignWithECDsa(String p8PrivateKey, Byte[] data)
		{
			// Parse the .p8 key using BouncyCastle
			AsymmetricKeyParameter key;

			using(var reader = new StringReader(p8PrivateKey))
			{
				var pemReader = new PemReader(reader);
				var keyObject = pemReader.ReadObject();

				if(keyObject is AsymmetricCipherKeyPair keyPair)
					key = keyPair.Private;
				else if(keyObject is AsymmetricKeyParameter privateKey)
					key = privateKey;
				else
					throw new Exception("Unable to read the private key from p8.");
			}

			var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
			signer.Init(true, key);
			signer.BlockUpdate(data, 0, data.Length);
			Byte[] signatureDer = signer.GenerateSignature();

			// Convert DER-encoded signature to raw R||S per JWT spec
			return ConvertDerToConcatenated(signatureDer, 64);
		}

		private static Byte[] ConvertDerToConcatenated(Byte[] derSignature, Int32 outputLength)
		{
			var sig = Org.BouncyCastle.Asn1.Asn1Sequence.GetInstance(derSignature);
			var r = ((Org.BouncyCastle.Asn1.DerInteger)sig[0]).Value.ToByteArrayUnsigned();
			var s = ((Org.BouncyCastle.Asn1.DerInteger)sig[1]).Value.ToByteArrayUnsigned();

			Int32 intLength = outputLength / 2;

			Byte[] result = new Byte[outputLength];
			Buffer.BlockCopy(PadToLength(r, intLength), 0, result, 0, intLength);
			Buffer.BlockCopy(PadToLength(s, intLength), 0, result, intLength, intLength);

			return result;
		}

		private static Byte[] PadToLength(Byte[] value, Int32 length)
		{
			if(value.Length == length)
				return value;

			if(value.Length > length)
				throw new Exception("Value length exceeds expected length.");

			Byte[] padded = new Byte[length];
			Buffer.BlockCopy(value, 0, padded, length - value.Length, value.Length);
			return padded;
		}
	}
}