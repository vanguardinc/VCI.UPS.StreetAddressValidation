using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCI.UPS.StreetAddressValidation.Entities
{
    public class AddressValidationResult
    {
        public enum ResponseStatus
        {
            CorrectionsFound,
            NoCorrectionFound,
            ErrorInResponse,
            Exception,
            NotUSAddress
        }
        public ResponseStatus Status { get; set; }
        public string ResponseMessage { get; set; }
        public List<Address> AddressCandidates { get; set; }
        public ErrorDetail ErrorDetail { get; set; }

        internal AddressValidationResult(string message, ResponseStatus status)
        {
            ResponseMessage = message;
            Status = status;
        }
        internal AddressValidationResult(Exception ex)
        {
            ResponseMessage = ex.Message;
            Status = ResponseStatus.Exception;
        }
        internal AddressValidationResult(AddressValidationResponse response, Address addressToValidate)
        {
            if (response.XAVResponse != null)
            {
                if (response.XAVResponse.HasCandidates && response.XAVResponse.Candidate.Count == 1 && response.XAVResponse.Candidate.First().AddressKeyFormat.CompareTo(addressToValidate) == 0)
                {
                    ResponseMessage = "Corrected address was the same as the submitted address";
                    Status = ResponseStatus.NoCorrectionFound;
                }
                else if (response.XAVResponse.HasCandidates)
                {
                    ResponseMessage = string.Format("{0} correction{1} found", response.XAVResponse.Candidate.Count, response.XAVResponse.Candidate.Count == 1 ? " was" : "s were");
                    AddressCandidates = response.XAVResponse.Candidate.Select(x => x.AddressKeyFormat).ToList();
                    Status = ResponseStatus.CorrectionsFound;
                }
                else
                {
                    ResponseMessage = "No corrections were found";
                    Status = ResponseStatus.NoCorrectionFound;
                }
            }
            else
            {
                Status = ResponseStatus.ErrorInResponse;
                ResponseMessage = "An error has occured";
                ErrorDetail = response.Fault.detail.Errors.ErrorDetail;
            }
        }
    }
}
