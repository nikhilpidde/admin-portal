using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IWalletDomainService
    {
        public Task<WalletDomainResponse> CreateDomainAsync(WalletDomain walletDomain);
        public Task<WalletDomain> GetWalletDomainAsync(int id);
        public Task<WalletDomainResponse> UpdateWalletDomainAsync(WalletDomain walletDomain);

        public Task<IEnumerable<WalletDomain>> ListWalletDomainAsync();

        public Task<WalletDomainResponse> DeleteWalletDomainAsync(int id, string updatedBy);

        public Task<ServiceResult> GetWalletDomainsList();
    }
}
