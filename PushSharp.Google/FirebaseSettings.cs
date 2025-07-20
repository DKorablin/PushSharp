using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
	/// <summary>The settings to connect to the Firebase PUSH server</summary>
	public class FirebaseSettings
	{
		/// <summary>The project identifier.</summary>
		[JsonProperty("project_id")]
		public String ProjectId { get; set; }

		/// <summary>The private key.</summary>
		[JsonProperty("private_key")]
		public String PrivateKey { get; set; }

		/// <summary>The client email associated with current credentials.</summary>
		[JsonProperty("client_email")]
		public String ClientEmail { get; set; }

		/// <summary>The token url.</summary>
		[JsonProperty("token_uri")]
		public String TokenUri { get; set; }

		/// <summary>Create empty instance of <see cref="FirebaseSettings"/></summary>
		public FirebaseSettings()
		{
		}

		/// <summary>Create instance of <see cref="FirebaseSettings"/> with the list of required parameters.</summary>
		/// <param name="projectId">The project identifier.</param>
		/// <param name="privateKey">The private key.</param>
		/// <param name="clientEmail">The client email associated with current credentials.</param>
		/// <param name="tokenUri">The token url.</param>
		/// <exception cref="ArgumentNullException">All parameters are required.</exception>
		public FirebaseSettings(String projectId, String privateKey, String clientEmail, String tokenUri)
		{
			this.ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
			this.PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
			this.ClientEmail = clientEmail ?? throw new ArgumentNullException(nameof(clientEmail));
			this.TokenUri = tokenUri ?? throw new ArgumentNullException(nameof(tokenUri));
		}
	}
}