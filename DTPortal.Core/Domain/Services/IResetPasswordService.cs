using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Domain.Services
{
    public interface IResetPasswordService
    {
        Task<GetAllUserSecurityQueResponse> GetUserSecurityQuestions(int userId);
        Task<ValidateUserSecQueResponse> ValidateUserSecurityQuestions(ValidateUserSecQueRequest request);
        Task<Response> ResetPassword(ResetPasswordRequest request);
    }
}
