using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IMobileVersionConfigurationService
    {
        Task<IEnumerable<MobileVersionDTO>> GetAllSupportedMobileVersionsAsync(string token);

        Task<MobileVersionDTO> GetSupportedMobileVersionByIdAsync(int id, string token);

        Task<ServiceResult> AddSupportedMobileVersionAsync(MobileVersionDTO mobileVersion, string token);

        Task<ServiceResult> UpdateSupportedMobileVersionAsync(MobileVersionDTO mobileVersion, string token);
    }
}
