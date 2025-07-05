using System;
using Newtonsoft.Json;

namespace PushSharp.Google
{
	public class FirebaseSettings {

		[JsonProperty("project_id")]
		public String ProjectId { get; set; }
	
		[JsonProperty("private_key")]
		public String PrivateKey{ get; set; }
	
		[JsonProperty("client_email")]
		public String ClientEmail { get; set; }
	
		[JsonProperty("token_uri")]
		public String TokenUri { get; set; }
	}
}