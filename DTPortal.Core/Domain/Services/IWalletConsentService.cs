using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IWalletConsentService
    {
        public Task<ServiceResult> GetAllConsentAsync();

        public Task<ServiceResult> GetActiveConsentAsync();

        public Task<ServiceResult> GetConsentsByUserIdAsync(string Id);

        public Task<ServiceResult> GetActiveConsentsByUserIdAsync(string Id);
        public Task<ServiceResult> AddConsent(WalletConsentDTO walletConsentDTO);
        public Task<ServiceResult> RevokeConsent(int id);
    }
}
