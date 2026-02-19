using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IServiceDefinitionService
    {
        Task<ServiceDefinitionDTO> GetServiceDefinitionsAsync(int id);

        Task<IEnumerable<ServiceDefinitionDTO>> GetServiceDefinitionsByStakeholderAsync(string stakeholder);

        Task<IEnumerable<ServiceDefinitionDTO>> GetServiceDefinitionsAsync();
    }
}