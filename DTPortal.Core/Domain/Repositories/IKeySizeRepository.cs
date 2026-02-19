using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IKeySizeRepository : IGenericRepository<PkiKeySize>
    {
        Task<IEnumerable<PkiKeySize>> GetAllKeysSizeByKeyAlgorithmIdAsync(int id);
    }
}
