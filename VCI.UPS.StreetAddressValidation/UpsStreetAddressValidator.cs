using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VCI.UPS.StreetAddressValidation.Entities;

namespace VCI.UPS.StreetAddressValidation
{
    public class UpsStreetAddressValidator
    {
        private readonly string ClientId;
        private readonly string ClientSecret;
        
        private HttpClient HttpClient;

        private const string TokenEndpoint = "https://onlinetools.ups.com/security/v1/oauth/token";
        private const string AddressValidationEndpoint = "https://onlinetools.ups.com/api/addressvalidation//v1/1?regionalrequestindicator=string&maximumcandidatelistsize=1";

        public UpsStreetAddressValidator(string connectionString)
        {
            DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();

            dbConnectionStringBuilder.ConnectionString = connectionString;

            ClientId = (string)dbConnectionStringBuilder["ClientId"];
            ClientSecret = (string)dbConnectionStringBuilder["ClientSecret"];

            HttpClient = new HttpClient();
        }

        public UpsStreetAddressValidator(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;

            HttpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientCredentials);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<OAuthTokenResponse>(jsonContent);

                    return tokenData.AccessToken;
                }
                else
                {
                    throw new Exception("Failed to retrieve access token.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        private async Task<AddressValidationResponse> ValidateAddressAsync(Address address)
        {
            string accessToken = await GetAccessTokenAsync();

            JObject json = JObject.Parse($@"{{
                ""XAVRequest"": {{
                    ""AddressKeyFormat"": {{
                        ""AddressLine"": [
                            ""{address.Street}""
                        ],
                        ""PoliticalDivision2"": ""{address.City}"",
                        ""PoliticalDivision1"": ""{address.State}"",
                        ""PostcodePrimaryLow"": ""{address.Postcode}"",
                        ""CountryCode"": ""{address.CountryCode}""
                    }}
                }}
            }}");

            var postData = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await HttpClient.PostAsync(AddressValidationEndpoint, postData);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AddressValidationResponse>(result);
            }
            else
            {
                Console.WriteLine(response.ToString());
                throw new Exception("Failed to validate address.");

            }
        }
        public async Task<AddressValidationResult> ValidateAddress(string street, string city, string state, string postalCode, string countryCode)
        {
            if (string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase) || string.Equals(countryCode, "PR", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var addressToValidate = new Address(street, city, state, postalCode, countryCode);
                    var response = await ValidateAddressAsync(addressToValidate);
                    return new AddressValidationResult(response, addressToValidate);
                }
                catch (Exception ex)
                {
                    return new AddressValidationResult(ex);
                }
            }
            else
            {
                return new AddressValidationResult("Validation can only be completed on US and Puerto Rican addresses", AddressValidationResult.ResponseStatus.NotUSAddress);
            }
        }
    }
}
