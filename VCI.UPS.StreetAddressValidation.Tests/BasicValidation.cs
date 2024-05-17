namespace VCI.UPS.StreetAddressValidation.Tests
{
    public class BasicValidation : TestBase
    {
        public BasicValidation() : base() { }

        [Fact]
        public async Task ProperlyFormattedAddressShouldValidate()
        {
            var response = await Validator.ValidateAddressAsync("13100 W Lisbon Rd", "Brookfield", "WI", "53005", "US");

            Assert.Single(response.AddressCandidates);

            var candidate = response.AddressCandidates.First();

            Assert.Equal("BROOKFIELD", candidate.City);
            Assert.Equal("US", candidate.CountryCode);
            Assert.Equal("53005", candidate.Postcode);
            Assert.Equal("BROOKFIELD WI 53005-2507", candidate.Region);
            Assert.Equal("WI", candidate.State);
            Assert.Equal("13100 W LISBON RD", candidate.Street);
        }

        [Fact]
        public async Task IncorrectlyFormattedAddressShouldValidate()
        {
            var response = await Validator.ValidateAddressAsync("13100 Lisbon Rd", "Brookfield", "WI", "", "US");

            Assert.Single(response.AddressCandidates);

            var candidate = response.AddressCandidates.First();

            Assert.Equal("BROOKFIELD", candidate.City);
            Assert.Equal("US", candidate.CountryCode);
            Assert.Equal("53005", candidate.Postcode);
            Assert.Equal("BROOKFIELD WI 53005-2507", candidate.Region);
            Assert.Equal("WI", candidate.State);
            Assert.Equal("13100 W LISBON RD", candidate.Street);
        }
    }
}