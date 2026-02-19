using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class HSMDataRepository : GenericRepository<PkiHsmDatum, PKIDbContext>,
        IHSMDataRepository
    {
        public HSMDataRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {

        }
    }
}
