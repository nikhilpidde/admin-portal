using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IRateCardService
    {
        Task<IEnumerable<RateCardDTO>> GetAllRateCardsAsync();

        Task<RateCardDTO> GetRateCardAsync(int id);

        Task<bool> IsRateCardExists(int serviceId, string serviceFor);

        Task<ServiceResult> AddRateCardAsync(RateCardDTO rateCard, bool makerCheckerFlag = false);

        Task<ServiceResult> UpdateRateCardAsync(RateCardDTO rateCard, bool makerCheckerFlag = false);

        Task<ServiceResult> EnableRateCardAsync(int id, string uuid, bool makerCheckerFlag = false);

        Task<ServiceResult> DisableRateCardAsync(int id, string uuid, bool makerCheckerFlag = false);

        Task<ServiceResult> DeleteRateCardAsync(int id, string uuid, bool makerCheckerFlag = false);
    }
}
