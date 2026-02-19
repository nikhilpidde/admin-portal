using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DTPortal.Core.Persistence.Repositories
{
    public class WalletConsentRepository : GenericRepository<WalletConsent, idp_dtplatformContext>,
        IWalletConsentRepository
    {
        private readonly ILogger _logger;
        public WalletConsentRepository(idp_dtplatformContext context
            ,ILogger logger) : base(context, logger) {
            _logger = logger;
        }
        public async Task<IEnumerable<WalletConsent>> GetAllConsentsAsync()
        {
            try
            {
                return await Context.WalletConsents.ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("WalletConsent::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<IEnumerable<WalletConsent>> GetActiveConsentsAsync()
        {
            try
            {
                return await Context.WalletConsents.
                    Where(u => u.Status == "ACTIVE").ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("WalletConsent::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<IEnumerable<WalletConsent>> GetActiveConsentsByUserIdAsync(string Id)
        {
            try
            {
                return await Context.WalletConsents.
                    Where(u => u.Status == "ACTIVE" && u.Suid == Id).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("WalletConsent::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<IEnumerable<WalletConsent>> GetConsentsByUserIdAsync(string Id)
        {
            try
            {
                return await Context.WalletConsents.
                    Where(u => u.Suid == Id).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("WalletConsent::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<WalletConsent> GetConsentByIdAsync(int Id)
        {
            try
            {
                return await Context.WalletConsents.
                    SingleOrDefaultAsync(u => u.Id == Id);
            }
            catch (Exception error)
            {
                _logger.LogError("WalletConsent::Database exception: {0}", error);
                return null;
            }
        }
    }
}
