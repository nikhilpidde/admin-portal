using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IClientsPurposeRepository : IGenericRepository<ClientsPurpose>
    {
        public Task<ClientsPurpose> GetByClientIdAsync(string clientId);

        public Task<string> GetPurposesByClientIdAsync(string clientId);

        public Task<bool> IsClientExist(string clientId);
    }
}
