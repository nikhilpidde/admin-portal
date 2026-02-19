using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ITimeBasedAccessService
    {
        Task<TimeBasedAccessResponse> CreateTimeBasedAccessAsync(TimeBasedAccess timeBasedAccessService);
        Task<TimeBasedAccess> GetTimeBasedAccessAsync(int id);
        Task<TimeBasedAccessResponse> UpdateTimeBasedAccessAsync(TimeBasedAccess timeBasedAccessService);
        Task<TimeBasedAccessResponse> DeleteTimeBasedAccessAsync(int id);
        Task<IEnumerable<TimeBasedAccess>> ListTimeBasedAccessAsync();
    }
}
