using System;

namespace PushSharp.Google
{
    public class GcmConfiguration
    {
        private const string GCM_SEND_URL = "https://gcm-http.googleapis.com/gcm/send";

        public GcmConfiguration (string senderAuthToken)
        {
            this.SenderAuthToken = senderAuthToken;
            this.GcmUrl = GCM_SEND_URL;

            this.ValidateServerCertificate = false;
        }

        public GcmConfiguration (string optionalSenderID, string senderAuthToken, string optionalApplicationIdPackageName, string bearerToken = null)
        {
            this.SenderID = optionalSenderID;
            this.SenderAuthToken = senderAuthToken;
            this.BearerToken = bearerToken;
            this.ApplicationIdPackageName = optionalApplicationIdPackageName;
            this.GcmUrl = GCM_SEND_URL;

            this.ValidateServerCertificate = false;
        }

        public string SenderID { get; private set; }

        public string SenderAuthToken { get; private set; }

        public string BearerToken { get; }

        public string ApplicationIdPackageName { get; private set; }

        public bool ValidateServerCertificate { get; set; }

        public string GcmUrl { get; set; }

        public void OverrideUrl (string url)
        {
            GcmUrl = url;
        }
    }
}

