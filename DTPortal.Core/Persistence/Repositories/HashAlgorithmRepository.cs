using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class HashAlgorithmRepository :
        GenericRepository<PkiHashAlgorithm, PKIDbContext>, IHashAlgorithmRepository
    {
        public HashAlgorithmRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {

        }
    }
}
