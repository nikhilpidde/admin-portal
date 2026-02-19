using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IPriceSlabDefinitionService
    {
        Task<IEnumerable<PriceSlabDefinitionDTO>> GetAllPriceSlabDefinitionsAsync();
        Task<PriceSlabDefinitionDTO> GetPriceSlabDefinitionAsync(int serviceId);
        Task<IList<PriceSlabDefinitionDTO>> GetPriceSlabDefinitionAsync(int serviceId, string stakeholder);
        Task<ServiceResult> AddPriceSlabDefinitionAsync(IList<PriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false);
        Task<ServiceResult> UpdatePriceSlabDefinitionAsync(IList<PriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false);
    }
}