using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace AlphaOmega.PushSharp.Apple
{
	/// <summary>The APNS settings information.</summary>
	public class ApnsSettings
	{
		private static readonly Dictionary<ApnsServerEnvironment, String> Hosts = new Dictionary<ApnsServerEnvironment, String>() {
			{ ApnsServerEnvironment.Development, "https://api.sandbox.push.apple.com" },
			{ ApnsServerEnvironment.Production, "https://api.push.apple.com" },
		};

		/// <summary>The type of server to use to send messages.</summary>
		public enum ApnsServerEnvironment
		{
			/// <summary>Sandbox server</summary>
			Development,

			/// <summary>Production server</summary>
			Production
		}

		/// <summary>The information expiration timeout.</summary>
		public static Int32 TokenExpirationMinutes { get; set; } = 60;

		/// <summary>Gets the configured APNS server environment.</summary>
		/// <value>The server environment.</value>
		public ApnsServerEnvironment Environment { get; }

		/// <summary>Gets or sets for all instances the host url to Apple PUSH notification service.</summary>
		public String Host
		{
			get => ApnsSettings.Hosts[this.Environment];
			set => ApnsSettings.Hosts[this.Environment] = value;
		}

		private String _p8Certificate;
		/// <summary>Private key you downloaded when you created your APNS Auth Key</summary>
		/// <exception cref="ArgumentNullException">value is required</exception>
		/// <exception cref="InvalidOperationException">Invalid p8 certificate specified</exception>
		public String P8Certificate {
			get => this._p8Certificate;
			set{
				if(String.IsNullOrEmpty(value))
					throw new ArgumentNullException(nameof(value), "The contents of p8 certificate should not be empty");

				this.P8Signer = CreateSigner(value);
				this._p8Certificate = value;
			}
		}

		/// <summary>10-character key ID from your Apple Developer account</summary>
		public String KeyId { get; }

		/// <summary>Apple Developer Team ID</summary>
		public String TeamId { get; }

		/// <summary>The topic for the notification.</summary>
		/// <remarks>
		/// In general, the topic is your app’s bundle ID/app ID.
		/// It can have a suffix based on the type of push notification.
		/// </remarks>
		public String AppBundleId { get; }

		internal ISigner P8Signer { get; private set; }

		/// <summary>Create instance of APNS settings</summary>
		/// <param name="environment">The environment to use for sending messages.</param>
		/// <param name="keyId">The Key ID of the p8 file.</param>
		/// <param name="teamId">The team identifier.</param>
		/// <param name="bundleId">The topic for the notification. In general, the topic is your app’s bundle ID/app ID.</param>
		/// <exception cref="ArgumentException">Invalid <paramref name="environment"/> value specified</exception>
		/// <exception cref="ArgumentNullException">All arguments are mandatory</exception>
		public ApnsSettings(ApnsServerEnvironment environment, String keyId, String teamId, String bundleId)
		{
			if(!Enum.IsDefined(typeof(ApnsServerEnvironment), environment))
				throw new ArgumentException("Invalid environment value specified", nameof(environment));
			if(String.IsNullOrWhiteSpace(keyId))
				throw new ArgumentNullException(nameof(keyId));
			if(String.IsNullOrWhiteSpace(teamId))
				throw new ArgumentNullException(nameof(teamId));
			if(String.IsNullOrWhiteSpace(bundleId))
				throw new ArgumentNullException(nameof(bundleId));

			this.Environment = environment;
			this.KeyId = keyId;
			this.TeamId = teamId;
			this.AppBundleId = bundleId;
		}

		/// <summary>Read P8 certificate from file system.</summary>
		/// <param name="p8CertificatePath">The path to P8 certificate file</param>
		/// <exception cref="FileNotFoundException">File <paramref name="p8CertificatePath"/> not found</exception>
		public void LoadP8CertificateFromFile(String p8CertificatePath)
		{
			if(!File.Exists(p8CertificatePath))
				throw new FileNotFoundException("P8 certificate file not found", p8CertificatePath);

			this.P8Certificate = File.ReadAllText(p8CertificatePath);
		}

		private static ISigner CreateSigner(String p8Certificate)
		{
			AsymmetricKeyParameter key;
			using(var reader = new StringReader(p8Certificate))
			{
				var pemReader = new PemReader(reader);
				var keyObject = pemReader.ReadObject();

				if(keyObject is AsymmetricCipherKeyPair keyPair)
					key = keyPair.Private;
				else if(keyObject is AsymmetricKeyParameter privateKey)
					key = privateKey;
				else
				{
					Exception exc = new InvalidOperationException("Unable to read the private key from p8.");
					exc.Data.Add(nameof(keyObject), keyObject?.ToString() ?? "null");
					throw exc;
				}
			}

			ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
			signer.Init(true, key);
			return signer;
		}

		internal Byte[] SignWithECDsa(Byte[] dataToSign)
		{
			var signer = this.P8Signer;
			if(signer == null)
				throw new InvalidOperationException($"{nameof(P8Certificate)} is not specified");

			signer.BlockUpdate(dataToSign, 0, dataToSign.Length);
			Byte[] signatureDer = signer.GenerateSignature();

			// Convert DER-encoded signature to raw R||S per JWT spec
			return ConvertDerToConcatenated(signatureDer);
		}

		internal Byte[] SignWithECDsa2(Byte[] dataToSign)
		{
			var lines = this.P8Certificate.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			String p8CertTrimmed = String.Join(String.Empty, Array.FindAll(lines, l => l != "-----BEGIN PRIVATE KEY-----" && l != "-----END PRIVATE KEY-----"));

			var key = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(p8CertTrimmed));
			var keyParams = key.Parameters.G.Multiply(key.D).Normalize();

			using(var dsa = ECDsa.Create(new ECParameters
			{
				Curve = ECCurve.CreateFromValue(key.PublicKeyParamSet.Id),
				D = key.D.ToByteArrayUnsigned(),
				Q = new ECPoint
				{
					X = keyParams.XCoord.GetEncoded(),
					Y = keyParams.YCoord.GetEncoded()
				}
			}))
				return dsa.SignData(dataToSign, 0, dataToSign.Length, HashAlgorithmName.SHA256);
		}

		private static Byte[] ConvertDerToConcatenated(Byte[] derSignature)
		{
			const Int32 OutputLength = 64;

			var sig = Org.BouncyCastle.Asn1.Asn1Sequence.GetInstance(derSignature);
			var r = ((Org.BouncyCastle.Asn1.DerInteger)sig[0]).Value.ToByteArrayUnsigned();
			var s = ((Org.BouncyCastle.Asn1.DerInteger)sig[1]).Value.ToByteArrayUnsigned();

			Int32 intLength = OutputLength / 2;

			Byte[] result = new Byte[OutputLength];
			Buffer.BlockCopy(PadToLength(r, intLength), 0, result, 0, intLength);
			Buffer.BlockCopy(PadToLength(s, intLength), 0, result, intLength, intLength);

			return result;
		}

		private static Byte[] PadToLength(Byte[] value, Int32 length)
		{
			if(value.Length == length)
				return value;

			if(value.Length > length)
				throw new InvalidOperationException("Value length exceeds expected length.");

			Byte[] padded = new Byte[length];
			Buffer.BlockCopy(value, 0, padded, length - value.Length, value.Length);
			return padded;
		}
	}
}