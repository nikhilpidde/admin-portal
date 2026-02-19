using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class WalletConfigurationRepository : GenericRepository<WalletConfiguration, idp_dtplatformContext>,
            IWalletConfigurationRepository
    {
        private readonly ILogger _logger;
        public WalletConfigurationRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<WalletConfiguration>> GetWalletConfigurationList()
        {
            try
            {
                return await Context.WalletConfigurations.AsNoTracking().ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetWalletConfigurationList::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<string> GetCredentialFormats()
        {
            try
            {
                var credentialFormatData = await Context.WalletConfigurations.FirstOrDefaultAsync(d => d.Name == "Credentials_Formats");

                return credentialFormatData.Value;
            }
            catch (Exception error)
            {
                _logger.LogError("GetCredentialFormats::Database exception: {0}", error);
                return null;
            }
        }
    }
}
