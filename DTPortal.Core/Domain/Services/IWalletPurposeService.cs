using System;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IWalletPurposeService
    {
        public Task<WalletPurposeResponse> CreatePurposeAsync(WalletPurpose purpose);

        public Task<WalletPurpose> GetPurposeAsync(int id);

        public Task<int> GetPurposeIdByNameAsync(string name);

        public Task<WalletPurposeResponse> UpdatePurposeAsync(WalletPurpose purpose);

        public Task<IEnumerable<WalletPurpose>> GetPurposeListAsync();
        public Task<IEnumerable<string>> GetPurposesListAsync();

        public Task<WalletPurposeResponse> DeletePurposeAsync(int id, string UUID);
    }
}
