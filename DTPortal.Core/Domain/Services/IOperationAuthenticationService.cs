using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Domain.Services
{
    public interface IOperationAuthenticationService
    {
        Task<ValidateOperationAuthNResponse> ValidateOperationAuthN(ValidateOperationAuthNRequest request);
        Task<Response> VerifyOperationAuthData(VerifyOperationAuthDataRequest request);
        Task<OperationsAuthscheme> GetOperationsAuthschemeById(int id);
        Task<OperationsAuthscheme> GetOperationsAuthschemeByName(string name);
        Task<OperationAuthSchmesResponse> UpdateOperationsAuthscheme(OperationsAuthscheme perationsAuthscheme);
        Task<IEnumerable<OperationsAuthscheme>> ListAllOperationsAuthschemes();
    }
}
