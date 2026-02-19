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
    public class KeySizeRepository : GenericRepository<PkiKeySize, PKIDbContext>,
        IKeySizeRepository
    {
        public KeySizeRepository(PKIDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<PkiKeySize>> GetAllKeysSizeByKeyAlgorithmIdAsync(int id)
        {
            return await Context.PkiKeySizes
                .Where(s => s.KeyAlgorithmId == id)
                .ToListAsync();
        }
    }
}
