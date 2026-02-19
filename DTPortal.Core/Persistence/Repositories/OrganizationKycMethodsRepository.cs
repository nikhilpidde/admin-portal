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
    public class OrganizationKycMethodsRepository : GenericRepository<OrganizationKycMethod, idp_dtplatformContext>,
            IOrganizationKycMethodsRepository
    {
        private readonly ILogger _logger;
        public OrganizationKycMethodsRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<OrganizationKycMethod> GetOrganizationKycMethodsByOrgIdAsync(string orgId)
        {
            try
            {
                return await Context.OrganizationKycMethods
                    .Where(kyc => kyc.OrganizationId == orgId)
                    .AsNoTracking()
                    .SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetOrganizationKycMethodsByOrgIdAsync::Database exception: {0}", ex);
                throw;
            }
        }
    }
}
