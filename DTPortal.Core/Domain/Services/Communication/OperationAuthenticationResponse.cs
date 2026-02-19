using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class ValidateOperationAuthNResult
    {
        public string tempSession { get; set; }
        public string authenticationScheme { get; set; }
        public string RandomCode { get; set; }
        public string Fido2Options { get; set; }
    }

    public class ValidateOperationAuthNResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public ValidateOperationAuthNResult result { get; set; }
    }

    public class OperationAuthSchmesResponse : BaseResponse<OperationsAuthscheme>
    {
        public OperationAuthSchmesResponse(OperationsAuthscheme category) : base(category) { }

        public OperationAuthSchmesResponse(string message) : base(message) { }
    }

}
