using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class PluginDataRepository : GenericRepository<PkiPluginDatum, PKIDbContext>,
        IPluginDataRepository
    {
        public PluginDataRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<PkiPluginDatum>> GetAllPluginsDataAsync()
        {
            return await Context.PkiPluginData.AsNoTracking()
                .Where(p => p.Status.ToLower() != "deleted")
                  .Include(p => p.PkiHsmData.HsmPlugin)
                      .Include(p => p.PkiCaData.CaPlugin)
                      .ToListAsync();
        }

        public async Task<PkiPluginDatum> GetPluginDataAsync(int id)
        {
            return await Context.PkiPluginData.AsNoTracking()
                .Include(p => p.PkiHsmData)
                .Include(P => P.PkiHsmData.KeyData)
                .Include(p => p.PkiCaData)
                .Include(p => p.PkiServerConfigurationData)
                .SingleOrDefaultAsync(p => p.Id == id && p.Status.ToLower() != "deleted");
        }

        public async Task<PkiPluginDatum> GetCompletePluginDataAsync(int id)
        {
            return await Context.PkiPluginData.AsNoTracking()
                .Include(p => p.PkiHsmData).ThenInclude(p => p.HsmPlugin)
                .Include(p => p.PkiHsmData).ThenInclude(p => p.HashAlgorithm)
                .Include(p => p.PkiHsmData).ThenInclude(p => p.KeyData).ThenInclude(p => p.KeyAlgorithm)
                .Include(p => p.PkiHsmData).ThenInclude(p => p.KeyData).ThenInclude(p => p.KeySize)
                .Include(p => p.PkiCaData).ThenInclude(p => p.CaPlugin)
                .Include(p => p.PkiCaData).ThenInclude(p => p.Procedure)
                .Include(p => p.PkiServerConfigurationData)
                .SingleOrDefaultAsync(p => p.Id == id && p.Status.ToLower() != "deleted");
        }

        public async Task<bool> IsPluginExistsAsync(int PkiHsmPluginId, int PkiCaPluginId)
        {
            return await Context.PkiPluginData.AnyAsync(x => x.PkiHsmData.HsmPluginId == PkiHsmPluginId
                                                && x.PkiCaData.CaPluginId == PkiCaPluginId && x.Status.ToLower() != "deleted");
        }
    }
}
