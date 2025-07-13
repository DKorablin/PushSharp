using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace AlphaOmega.PushSharp.Apple
{
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

		/// <summary>Gets the configured APNS server environment.</summary>
		/// <value>The server environment.</value>
		public ApnsServerEnvironment Environment { get; set; }

		/// <summary>Gets or sets for all instances the host url to Apple PUSH notification service.</summary>
		public virtual String Host
		{
			get => ApnsSettings.Hosts[this.Environment];
			set => ApnsSettings.Hosts[this.Environment] = value;
		}

		/// <summary>Private key you downloaded when you created your APNS Auth Key</summary>
		public String P8Certificate { get; set; }

		/// <summary>10-character key ID from your Apple Developer account</summary>
		public String KeyId { get; set; }

		/// <summary>Apple Developer Team ID</summary>
		public String TeamId { get; set; }

		/// <summary>The topic for the notification.</summary>
		/// <remarks>
		/// In general, the topic is your app’s bundle ID/app ID.
		/// It can have a suffix based on the type of push notification.
		/// </remarks>
		public String AppBundleId { get; set; }

		/// <summary>Create instance of APNS settings</summary>
		/// <param name="environment">The environment to use for sending messages.</param>
		/// <param name="p8CertificatePath">The path to P8 certificate file.</param>
		/// <param name="keyId">The Key ID of the p8 file.</param>
		/// <param name="teamId">The team identifier.</param>
		/// <exception cref="FileNotFoundException">File <paramref name="p8CertificatePath"/> not found</exception>
		/// <exception cref="ArgumentNullException">All arguments are mandatory</exception>
		public ApnsSettings(ApnsServerEnvironment environment, String p8CertificatePath, String keyId, String teamId)
		{
			if(String.IsNullOrWhiteSpace(p8CertificatePath))
				throw new ArgumentNullException(nameof(p8CertificatePath));
			if(String.IsNullOrWhiteSpace(keyId))
				throw new ArgumentNullException(nameof(keyId));

			if(!File.Exists(p8CertificatePath))
				throw new FileNotFoundException("P8 certificate file not found", p8CertificatePath);

			this.Environment = environment;
			this.P8Certificate = File.ReadAllText(p8CertificatePath);
			this.KeyId = keyId;
			this.TeamId = teamId ?? throw new ArgumentNullException(nameof(teamId));
		}

		private ISigner CreateSigner()
		{
			AsymmetricKeyParameter key;
			using(var reader = new StringReader(this.P8Certificate))
			{
				var pemReader = new PemReader(reader);
				var keyObject = pemReader.ReadObject();

				if(keyObject is AsymmetricCipherKeyPair keyPair)
					key = keyPair.Private;
				else if(keyObject is AsymmetricKeyParameter privateKey)
					key = privateKey;
				else
					throw new ApplicationException("Unable to read the private key from p8.");
			}

			var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
			signer.Init(true, key);
			return signer;
		}

		internal Byte[] SignWithECDsa(Byte[] dataToSign)
		{
			var signer = this.CreateSigner();
			signer.BlockUpdate(dataToSign, 0, dataToSign.Length);
			Byte[] signatureDer = signer.GenerateSignature();

			// Convert DER-encoded signature to raw R||S per JWT spec
			return ConvertDerToConcatenated(signatureDer);
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