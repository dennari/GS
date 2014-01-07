using CommonDomain;
using Growthstories.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Growthstories.Sync
{


    public class AuthToken : IAuthToken
    {

        [JsonProperty(PropertyName = "access_token", Required = Required.Always)]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "expires_in", Required = Required.Always)]
        public int ExpiresIn { get; set; }
        [JsonProperty(PropertyName = "refresh_token", Required = Required.Always)]
        public string RefreshToken { get; set; }

        public AuthToken(string accessToken, int expiresIn, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }


    }



}
