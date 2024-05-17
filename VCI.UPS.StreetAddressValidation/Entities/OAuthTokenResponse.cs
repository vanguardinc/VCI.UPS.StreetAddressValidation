using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VCI.UPS.StreetAddressValidation.Entities
{
    public class OAuthTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
