using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IControlledOnboardingService
    {
        Task<ServiceResult> AddTrustedUsersAsync(ControlledOnboardingDTO userList, bool makerCheckerFlag = false);

        Task<TrustedSpocEmailDTO> GetTrustedUserByEmail(string email);
    }
}
