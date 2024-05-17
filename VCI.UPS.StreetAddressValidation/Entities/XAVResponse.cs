using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCI.UPS.StreetAddressValidation.Extensions;

namespace VCI.UPS.StreetAddressValidation.Entities
{
    internal class XAVResponse
    {
        public Response Response { get; set; }
        public string ValidAddressIndicator { get; set; }
        [JsonConverter(typeof(SingleValueArrayConverter<Candidate>))]
        public List<Candidate> Candidate { get; set; }
        public string NoCandidatesIndicator { get; set; }
        public string AmbiguousAddressIndicator { get; set; }
        public bool HasCandidates { get { return Candidate != null && Candidate.Any(); } }
    }

}
