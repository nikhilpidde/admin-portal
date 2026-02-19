using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IPluginDataRepository : IGenericRepository<PkiPluginDatum>
    {
        Task<IEnumerable<PkiPluginDatum>> GetAllPluginsDataAsync();
        Task<PkiPluginDatum> GetPluginDataAsync(int id);
        Task<PkiPluginDatum> GetCompletePluginDataAsync(int id);
        Task<bool> IsPluginExistsAsync(int PkiCaPluginId, int PkiHsmPluginId);
    }
}
