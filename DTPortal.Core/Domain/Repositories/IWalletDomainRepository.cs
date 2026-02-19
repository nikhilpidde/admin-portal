using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IWalletDomainRepository : IGenericRepository<WalletDomain>
    {
        public Task<bool> IsScopeExistsWithNameAsync(string name);

        public Task<IEnumerable<WalletDomain>> ListAllScopeAsync();

        public Task<WalletDomain> GetWalletDomainByIdWithPurposes(int id);
    }
}
