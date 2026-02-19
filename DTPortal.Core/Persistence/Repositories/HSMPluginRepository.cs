using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class HSMPluginRepository : GenericRepository<PkiHsmPlugin, PKIDbContext>,
        IHSMPluginRepository
    {
        public HSMPluginRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {

        }
    }
}
