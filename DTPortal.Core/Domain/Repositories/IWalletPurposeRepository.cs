using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IWalletPurposeRepository : IGenericRepository<WalletPurpose>
    {
        public Task<bool> IsPurposeExistsWithNameAsync(string name);

        public Task<IEnumerable<WalletPurpose>> ListAllPurposeAsync();

        public Task<WalletPurpose> GetPurposeById(int id);

        public Task<WalletPurpose> GetPurposeByNameAsync(string name);
    }
}
