using System;
using System.IO;

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

		/// <summary>Gets the configured APNS server environment.</summary>
		/// <value>The server environment.</value>
		public ApnsServerEnvironment Environment { get; set; }

		/// <summary>Gets the host url to Apple PUSH server.</summary>
		public virtual String Host
			=> this.Environment == ApnsServerEnvironment.Production
				? ApnsSettings.APNS_PRODUCTION_HOST
				: ApnsSettings.APNS_SANDBOX_HOST;

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
	}
}