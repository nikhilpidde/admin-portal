using System.Threading.Tasks;

using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IDashboardService
    {
        Task<CumulativeCountDTO> GetCumulativeCountAsync();

        Task<GraphDTO> GetGraphCountAsync();

        Task<GraphDTO> GetGraphCountAsync(string serviceProviderName);
    }
}
