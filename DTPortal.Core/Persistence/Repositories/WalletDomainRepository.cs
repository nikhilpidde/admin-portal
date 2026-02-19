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
    public class WalletDomainRepository : GenericRepository<WalletDomain, idp_dtplatformContext>, IWalletDomainRepository
    {
        private readonly ILogger _logger;
        public WalletDomainRepository(idp_dtplatformContext context, ILogger logger) :
            base(context, logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsScopeExistsWithNameAsync(string name)
        {
            try
            {
                return await Context.Scopes.AsNoTracking().AnyAsync(u => u.Name == name);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return false;
            }
        }

        public async Task<IEnumerable<WalletDomain>> ListAllScopeAsync()
        {
            try
            {
                return await Context.WalletDomains
                    .Where(u => u.Status != "DELETED").OrderByDescending(s => s.CreatedDate).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("ListAllScopeAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<WalletDomain> GetWalletDomainByIdWithPurposes(int id)
        {
            try
            {
                return await Context.WalletDomains
                    .SingleOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception error)
            {
                _logger.LogError("GetRoleByRoleIdWithActivities::Database exception: {0}", error);
                return null;
            }
        }
    }
}