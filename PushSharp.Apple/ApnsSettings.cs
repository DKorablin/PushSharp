using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsSettings
	{
		private const String APNS_SANDBOX_HOST = "https://api.push.apple.com";
		private const String APNS_PRODUCTION_HOST = "https://api.development.push.apple.com";

		public enum ApnsServerEnvironment
		{
			Sandbox,
			Production
		}

		/// <summary>Gets the configured APNS server environment</summary>
		/// <value>The server environment.</value>
		public ApnsServerEnvironment Environment { get; set; }

		public virtual String Host
			=> this.Environment == ApnsServerEnvironment.Production
				? APNS_PRODUCTION_HOST
				: APNS_SANDBOX_HOST;

		public String CertificatePassword { get; set; }

		public X509Certificate2 Certificate { get; set; }

		#region New properties
		public String P8PrivateKey { get; set; }
		public String P8PrivateKeyId { get; set; }
		public String TeamId { get; set; }

		/// <summary>The topic for the notification.</summary>
		/// <remarks>
		/// In general, the topic is your app’s bundle ID/app ID.
		/// It can have a suffix based on the type of push notification.
		/// </remarks>
		public String ApnsTopic { get; set; }

		#endregion New properties

		public ApnsSettings(ApnsServerEnvironment environment, String certificatePath, String certificatePassword)
			: this(environment, File.ReadAllBytes(certificatePath), certificatePassword)
		{
		}

		public ApnsSettings(ApnsServerEnvironment environment, Byte[] certificateData, String certificatePassword)
			: this(environment,
				new X509Certificate2(certificateData, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
				certificatePassword)
		{
		}

		public ApnsSettings(ApnsServerEnvironment environment, X509Certificate2 certificate, String certificatePassword)
		{
			this.Environment = environment;
			this.Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			this.CertificatePassword = certificatePassword ?? throw new ArgumentNullException(nameof(certificatePassword));

			ApnsSettings.CheckIsApnsCertificate(certificate, environment);
		}

		private static void CheckIsApnsCertificate(X509Certificate2 certificate, ApnsServerEnvironment environment)
		{
			_ = certificate ?? throw new ArgumentNullException(nameof(certificate), "You must provide a Certificate to connect to APNS with!");

			var issuerName = certificate.IssuerName.Name;
			var commonName = certificate.SubjectName.Name;

			if(!issuerName.Contains("Apple"))
				throw new ArgumentOutOfRangeException("Your Certificate does not appear to be issued by Apple!  Please check to ensure you have the correct certificate!");

			if(!Regex.IsMatch(commonName, "Apple.*?Push Services") && !commonName.Contains("Website Push ID:"))
				throw new ArgumentOutOfRangeException("Your Certificate is not a valid certificate for connecting to Apple's APNS servers");

			if(commonName.Contains("Development") && environment != ApnsServerEnvironment.Sandbox)
				throw new ArgumentOutOfRangeException("You are using a certificate created for connecting only to the Sandbox APNS server but have selected a different server environment to connect to.");

			if(commonName.Contains("Production") && environment != ApnsServerEnvironment.Production)
				throw new ArgumentOutOfRangeException("You are using a certificate created for connecting only to the Production APNS server but have selected a different server environment to connect to.");
		}
	}
}