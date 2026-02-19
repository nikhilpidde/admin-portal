using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class PKIConfigurationRepository : GenericRepository<PkiConfiguration, PKIDbContext>,
        IPKIConfigurationRepository
    {
        public PKIConfigurationRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<PkiConfiguration> GetConfigurationByNameAsync(string name)
        {
            return await Context.PkiConfigurations.SingleOrDefaultAsync(x => x.Name == name);
        }
    }
}
