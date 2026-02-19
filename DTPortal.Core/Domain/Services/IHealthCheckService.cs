using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IHealthCheckService
    {
        Task<IEnumerable<ServiceHealthDTO>> GetServiceCheckAsync();

        Task<ServiceHealthDTO> GetPKITimestampServiceCheckAsync();

        Task<IEnumerable<ServiceHealthHistory>> GetServiceHealthHistoryAsync(string serviceName);
    }
}
