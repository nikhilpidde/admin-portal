using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IPKIConfigurationRepository : IGenericRepository<PkiConfiguration>
    {
        Task<PkiConfiguration> GetConfigurationByNameAsync(string name);
    }
}
