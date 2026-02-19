using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IOrganizationPriceSlabDefinitionService
    {
        Task<IEnumerable<OrganizationPriceSlabDefinitionDTO>> GetAllPriceSlabDefinitionsAsync();
        Task<IList<OrganizationPriceSlabDefinitionDTO>> GetPriceSlabDefinitionAsync(int serviceId, string organizationUid);
        Task<ServiceResult> AddPriceSlabDefinitionAsync(IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false);
        Task<ServiceResult> UpdatePriceSlabDefinitionAsync(IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false);
    }
}