using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DTPortal.Core.Persistence.Repositories
{
    public class ClientsPurposeRepository : GenericRepository<ClientsPurpose, idp_dtplatformContext>, IClientsPurposeRepository
    {
        private readonly ILogger _logger;
        public ClientsPurposeRepository(idp_dtplatformContext context,ILogger logger):base(context, logger)
        {
            _logger = logger;
        } 
        public async Task<ClientsPurpose> GetByClientIdAsync(string clientId)
        {
            try
            {
                return await Context.ClientsPurposes.SingleOrDefaultAsync(u => u.ClientId == clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
        public async Task<string> GetPurposesByClientIdAsync(string clientId)
        {
            try
            {
                return await Context.ClientsPurposes.Where(p => p.ClientId == clientId).Select(p => p.PurposesAllowed).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<bool> IsClientExist(string ClientId)
        {
            try
            {
                return await Context.ClientsPurposes.AsNoTracking().AnyAsync(u => u.ClientId == ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }


    }
}
