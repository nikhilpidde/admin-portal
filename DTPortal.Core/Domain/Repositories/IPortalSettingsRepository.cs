using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IPortalSettingsRepository : IGenericRepository<PortalSetting>
    {
        Task<PortalSetting> GetSettingByNameAsync(string name);
    }
}
