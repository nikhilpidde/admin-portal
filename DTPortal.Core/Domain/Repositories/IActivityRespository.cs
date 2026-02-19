using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;


namespace DTPortal.Core.Domain.Repositories
{
    public interface IActivityRespository : IGenericRepository<Activity>
    {
        Task<IEnumerable<ActivityLookupItem>> GetActivityLookupItemsAsync();
    }
}
