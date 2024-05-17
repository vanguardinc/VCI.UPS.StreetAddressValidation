using citizenkraft.UpsStreetAddressValidation.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace citizenkraft.UpsStreetAddressValidation
{
    internal class UpsOAuthStreetAddressValidator
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenEndpoint;
        private readonly string _addressValidationEndpoint;
        private HttpClient _httpClient;

        public UpsOAuthStreetAddressValidator(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tokenEndpoint = "https://onlinetools.ups.com/security/v1/oauth/token";
            _addressValidationEndpoint = "https://onlinetools.ups.com/api/addressvalidation//v1/1?regionalrequestindicator=string&maximumcandidatelistsize=1";
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientCredentials);
            try
            {
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    return tokenData.access_token;
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
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync(_addressValidationEndpoint, postData);
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
