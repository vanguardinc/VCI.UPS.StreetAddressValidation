using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCI.UPS.StreetAddressValidation.Entities
{
    [Serializable()]
    internal class AddressValidationRequest
    {
        public XAVRequest XAVRequest { get; set; }

        public AddressValidationRequest(Address address)
        {
            XAVRequest = new XAVRequest(address);
        } 
    }
}
