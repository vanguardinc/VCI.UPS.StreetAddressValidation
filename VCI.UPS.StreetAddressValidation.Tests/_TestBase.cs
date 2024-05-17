using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCI.UPS.StreetAddressValidation.Tests
{
    public abstract class TestBase
    {
        protected StreetAddressValidator Validator;

        protected TestBase()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Validator = new StreetAddressValidator(configuration.GetConnectionString("Default"));
        }
    }
}
