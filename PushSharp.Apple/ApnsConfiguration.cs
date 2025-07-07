using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AlphaOmega.PushSharp.Core;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsConfiguration
	{
		private const Int32 tokenExpirationMinutes = 50;

		private static readonly ConcurrentDictionary<String, Tuple<String, DateTime>> tokens = new ConcurrentDictionary<String, Tuple<String, DateTime>>();

		public ApnsSettings Settings { get; private set; }

		public List<X509Certificate2> AdditionalCertificates { get; private set; } = new List<X509Certificate2>();

		public Boolean AddLocalAndMachineCertificateStores { get; set; } = false;

		public Int32 MillisecondsToWaitBeforeMessageDeclaredSuccess { get; set; } = 3000;

		public Int32 FeedbackIntervalMinutes { get; set; } = 10;

		public Boolean FeedbackTimeIsUTC { get; set; } = false;

		public Boolean ValidateServerCertificate { get; set; } = false;

		public Int32 ConnectionTimeout { get; set; } = 10000;

		public Int32 MaxConnectionAttempts { get; set; } = 3;

		/// <summary>The internal connection to APNS servers batches notifications to send before waiting for errors for a short time.</summary>
		/// <remarks>
		/// This value will set a maximum size per batch.
		/// The default value is 1000.
		/// You probably do not want this higher than 7500.
		/// </remarks>
		/// <value>The size of the internal batch.</value>
		public Int32 InternalBatchSize { get; set; } = 1000;

		/// <summary>How long the internal connection to APNS servers should idle while collecting notifications in a batch to send.</summary>
		/// <remarks>Setting this value too low might result in many smaller batches being used.</remarks>
		/// <value>The internal batching wait period.</value>
		public TimeSpan InternalBatchingWaitPeriod { get; set; } = TimeSpan.FromMilliseconds(750);

		/// <summary>How many times the internal batch will retry to send in case of network failure. The default value is 1.</summary>
		/// <value>The internal batch failure retry count.</value>
		public Int32 InternalBatchFailureRetryCount { get; set; } = 1;

		/// <summary>Gets or sets the keep alive period to set on the APNS socket</summary>
		public TimeSpan KeepAlivePeriod { get; set; } = TimeSpan.FromMinutes(20);

		/// <summary>Gets or sets the keep alive retry period to set on the APNS socket</summary>
		public TimeSpan KeepAliveRetryPeriod { get; set; } = TimeSpan.FromSeconds(30);

		public String AccessToken => this.CreateAccessToken();

		public ApnsConfiguration(ApnsSettings settings)
			=> this.Settings = settings;

		private String CreateAccessToken()
		{
			var header = JsonConvert.SerializeObject(new { alg = "ES256", kid = CleanP8Key(this.Settings.P8PrivateKeyId) });
			var payload = JsonConvert.SerializeObject(new { iss = this.Settings.TeamId, iat = PushHttpClient.GetEpochTimestamp() });
			var headerBase64 = Base64UrlEncode(header);
			var payloadBase64 = Base64UrlEncode(payload);
			var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
			var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

			var privateKeyBytes = Convert.FromBase64String(CleanP8Key(this.Settings.P8PrivateKey));
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
				var signature = dsa.SignData(unsignedJwtBytes, 0, unsignedJwtBytes.Length, HashAlgorithmName.SHA256);
				var signatureBase64 = Convert.ToBase64String(signature);
				return $"{unsignedJwtData}.{signatureBase64}";
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
			return Convert.ToBase64String(bytes);
		}
	}
}