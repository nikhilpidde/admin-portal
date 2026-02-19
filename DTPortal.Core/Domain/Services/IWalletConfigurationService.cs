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
    public interface IWalletConfigurationService
    {
        public Task<ServiceResult> GetWalletConfiguration();
        public Task<ServiceResult> UpdateWalletConfiguration(WalletConfigurationResponse walletConfigurationResponse);
        public Task<ServiceResult> GetConfiguration();
        public Task<ServiceResult> GetWalletConfigurationDetails();
    }
}
