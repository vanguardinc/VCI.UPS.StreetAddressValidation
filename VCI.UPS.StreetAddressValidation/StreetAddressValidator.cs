using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VCI.UPS.StreetAddressValidation.Entities;

namespace VCI.UPS.StreetAddressValidation
{
    public class StreetAddressValidator
    {
        private readonly string ClientId;
        private readonly string ClientSecret;
        
        private HttpClient HttpClient;

        private const string TokenEndpoint = "https://onlinetools.ups.com/security/v1/oauth/token";
        private const string AddressValidationEndpoint = "https://onlinetools.ups.com/api/addressvalidation//v1/1?regionalrequestindicator=string&maximumcandidatelistsize=1";

        public StreetAddressValidator(string connectionString)
        {
            DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();

            dbConnectionStringBuilder.ConnectionString = connectionString;

            ClientId = (string)dbConnectionStringBuilder["ClientId"];
            ClientSecret = (string)dbConnectionStringBuilder["ClientSecret"];

            HttpClient = new HttpClient();
        }

        public StreetAddressValidator(string clientId, string clientSecret)
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

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", clientCredentials);

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
                throw;
            }
        }

        private async Task<AddressValidationResponse> ValidateAddressAsync(Address address)
        {
            string accessToken = await GetAccessTokenAsync();

            var request = new AddressValidationRequest(address);

            var postData = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await HttpClient.PostAsync(AddressValidationEndpoint, postData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<AddressValidationResponse>(result);
            }
            else
            {
                throw new Exception("Failed to validate address: " + response.ReasonPhrase);
            }
        }

        public async Task<AddressValidationResult> ValidateAddressAsync(string street, string city, string state, string postalCode, string countryCode)
        {
            if (String.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase) || String.Equals(countryCode, "PR", StringComparison.OrdinalIgnoreCase))
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
