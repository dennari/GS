using CommonDomain;
using Growthstories.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Growthstories.Sync
{


    public class HttpAuthTokenResponse : IAuthTokenResponse
    {

        [JsonProperty(PropertyName = "access_token", Required = Required.Always)]
        public string AccessToken { get; private set; }
        [JsonProperty(PropertyName = "expires_in", Required = Required.Always)]
        public int ExpiresIn { get; private set; }
        [JsonProperty(PropertyName = "refresh_token", Required = Required.Always)]
        public string RefreshToken { get; private set; }

        public HttpAuthTokenResponse(string accessToken, int expiresIn, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }


    }



}
