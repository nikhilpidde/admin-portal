using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IAgentService
    {
        Task<IEnumerable<AgentListDTO>> GetAllOnboardingAgent();

        Task<ServiceResult> AddOnboardingAgent(List<AgentDTO> agent);

        Task<ServiceResult> AgentStatus(int id);

        Task<ServiceResult> DeleteAgent(int id);

        Task<AgentListDTO> GetAgentById(int id);
    }
}
