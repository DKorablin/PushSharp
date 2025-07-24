using System;
using System.Threading.Tasks;
using System.Net.Http;
using AlphaOmega.PushSharp.Core;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AlphaOmega.PushSharp.Windows
{
    public class WnsAccessTokenManager
    {
        Task renewAccessTokenTask = null;
        String accessToken = null;
        HttpClient http;

        public WnsAccessTokenManager (WnsConfiguration configuration)
        {
            http = new HttpClient ();
            Configuration = configuration;
        }

        public WnsConfiguration Configuration { get; private set; }

        public async Task<String> GetAccessToken ()
        {
            if (accessToken == null) {
                if (renewAccessTokenTask == null) {
                    Log.Trace.TraceInformation("Renewing Access Token");
                    renewAccessTokenTask = RenewAccessToken ();
                    await renewAccessTokenTask;
                } else {
                    Log.Trace.TraceInformation("Waiting for access token");
                    await renewAccessTokenTask;
                }
            }

            return accessToken;
        }

        public void InvalidateAccessToken (String currentAccessToken)
        {
            if (accessToken == currentAccessToken)
                accessToken = null;
        }

        async Task RenewAccessToken ()
        {
            var p = new Dictionary<String, String> {
                { "grant_type", "client_credentials" },
                { "client_id", Configuration.PackageSecurityIdentifier },
                { "client_secret", Configuration.ClientSecret },
                { "scope", "notify.windows.com" }
            };

            var result = await http.PostAsync ("https://login.live.com/accesstoken.srf", new FormUrlEncodedContent (p));

            var data = await result.Content.ReadAsStringAsync ();

            var token = String.Empty;
            var tokenType = String.Empty;

            try {
                var json = JObject.Parse (data);
                token = json.Value<String> ("access_token");
                tokenType = json.Value<String> ("token_type");
            } catch {
            }

            if (!String.IsNullOrEmpty (token) && !String.IsNullOrEmpty (tokenType)) {
                accessToken = token;
            } else {
                accessToken = null;
                throw new UnauthorizedAccessException ("Could not retrieve access token for the supplied Package Security Identifier (SID) and client secret");
            }
        }
    }
}

