using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IConsentService
    {
        Task<IEnumerable<ConsentDTO>> GetAllConsentsAsync(string token);

        Task<ConsentDTO> GetConsentAsync(int consentId, string token);

        Task<ServiceResult> AddConsentAsync(ConsentRequestBodyDTO requestBodyDTO, string token);

        Task<ServiceResult> UpdateConsentAsync(ConsentRequestBodyDTO requestBodyDTO, string token);

        Task<ServiceResult> EnableConsentAsync(int consentId, string token);

        Task<ServiceResult> DisableConsentAsync(int consentId, string token);
    }
}
