using System;
using Newtonsoft.Json;

namespace AlphaOmega.PushSharp.Google
{
	public class FirebaseSettings
	{
		[JsonProperty("project_id")]
		public String ProjectId { get; set; }

		[JsonProperty("private_key")]
		public String PrivateKey { get; set; }

		[JsonProperty("client_email")]
		public String ClientEmail { get; set; }

		[JsonProperty("token_uri")]
		public String TokenUri { get; set; }

		public FirebaseSettings()
		{
		}

		public FirebaseSettings(String projectId, String privateKey, String clientEmail, String tokenUri)
		{
			this.ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
			this.PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
			this.ClientEmail = clientEmail ?? throw new ArgumentNullException(nameof(clientEmail));
			this.TokenUri = tokenUri ?? throw new ArgumentNullException(nameof(tokenUri));
		}
	}
}