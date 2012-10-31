using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Globalization;

namespace WaMedia.Common.OAuth
{
    internal class OAuthHelper
    {
        private DateTime _tokenExpiration;
        private string _clientId;
        private string _clientSecret;
        private string _scope;
        private string _acsBaseAddress;

        internal OAuthHelper(string clientId, string clientSecret, string scope = AzureMediaServicePreview.AUTH_SCOPE, string acsBaseAddress = AzureMediaServicePreview.BASE_ACS_ADDRESS)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            this._scope = scope;
            this._acsBaseAddress = acsBaseAddress;
        }

        private void GetToken()
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.BaseAddress = this._acsBaseAddress;
                NameValueCollection data = new NameValueCollection()
                    {
                      {
                        "grant_type",
                        "client_credentials"
                      },
                      {
                        "client_id",
                        this._clientId
                      },
                      {
                        "client_secret",
                        this._clientSecret
                      },
                      {
                        "scope",
                        this._scope
                      }
                    };
                using (MemoryStream memoryStream = new MemoryStream(webClient.UploadValues("/v2/OAuth2-13", "POST", data)))
                {
                    OAuth2TokenResponse oauth2TokenResponse = (OAuth2TokenResponse)new DataContractJsonSerializer(typeof(OAuth2TokenResponse)).ReadObject((Stream)memoryStream);
                    this.AccessToken = oauth2TokenResponse.AccessToken;
                    this._tokenExpiration = DateTime.Now.AddSeconds((double)(oauth2TokenResponse.ExpirationInSeconds - 10));
                }
            }
        }

        internal void AddAccessTokenToRequest(WebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (((NameValueCollection)request.Headers)["Authorization"] != null)
                return;
            if (DateTime.Now > this._tokenExpiration)
                this.GetToken();
            ((NameValueCollection)request.Headers)
                .Add("Authorization", 
                        string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Bearer {0}", new object[1]
                        {
                            (object) this.AccessToken
                        })
                );
        }

        internal string AccessToken { get; set; }
    }
}
