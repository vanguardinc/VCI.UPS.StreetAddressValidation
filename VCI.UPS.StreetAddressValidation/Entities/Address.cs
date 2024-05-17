using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VCI.UPS.StreetAddressValidation.Extensions;

namespace VCI.UPS.StreetAddressValidation.Entities
{

    public class Address : IComparable
    {
        [JsonConverter(typeof(SingleValueStringArrayConverter))]
        [JsonProperty("AddressLine")]
        public string Street { get; set; }
        [JsonProperty("PoliticalDivision2")]
        public string City { get; set; }
        [JsonProperty("PoliticalDivision1")]
        public string State { get; set; }
        [JsonProperty("PostcodePrimaryLow")]
        public string Postcode { get; set; }
        [JsonProperty("PostcodeExtendedLow")]
        public string PostcodeExtension { get; set; }
        public string Region { get; set; }
        public string CountryCode { get; set; }

        public Address(string address, string city, string state, string postalCode, string countryCode)
        {
            Street = address;
            City = city;
            State = state;
            CountryCode = countryCode;
            if (postalCode != null && postalCode.Contains('-'))
            {
                var splitPostcode = postalCode.Split('-');
                Postcode = splitPostcode.First();
                PostcodeExtension = splitPostcode.Last();
            }
            else
            {
                Postcode = postalCode;
                PostcodeExtension = string.Empty;
            }
            Region = string.Empty;
        }

        public Address() { }

        public string ToHTMLString()
        {
            return string.Format("{0} <br/> {1}, {2} {3}{4}", Street, City, State, Postcode, !string.IsNullOrEmpty(PostcodeExtension) ? "-" + PostcodeExtension : string.Empty);

        }

        public int CompareTo(object obj)
        {
            if (obj == null) return -1;

            Address otherAddress = obj as Address;

            if (Street.ToLower().Trim().Equals(otherAddress.Street.ToLower().Trim()) && City.ToLower().Trim().Equals(otherAddress.City.ToLower().Trim()) && State.ToLower().Trim().Equals(otherAddress.State.ToLower().Trim()) && Postcode.ToLower().Trim().Equals(otherAddress.Postcode.ToLower().Trim()) && PostcodeExtension.ToLower().Trim().Equals(otherAddress.PostcodeExtension.ToLower().Trim()))
                return 0;

            return 1;
        }
    }
}
