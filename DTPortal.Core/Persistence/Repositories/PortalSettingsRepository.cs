using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;

namespace DTPortal.Core.Persistence.Repositories
{
    public class PortalSettingsRepository : GenericRepository<PortalSetting, idp_dtplatformContext>,
            IPortalSettingsRepository
    {
        public PortalSettingsRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            
        }

        public async Task<PortalSetting> GetSettingByNameAsync(string name)
        {
            return await Context.PortalSettings.SingleOrDefaultAsync(x => x.Name == name);
        }
    }
}
