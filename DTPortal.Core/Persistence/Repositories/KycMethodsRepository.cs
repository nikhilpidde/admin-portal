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
    public class KycMethodsRepository : GenericRepository<KycMethod, idp_dtplatformContext>,
            IKycMethodsRepository
    {
        private readonly ILogger _logger;
        public KycMethodsRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<KycMethod>> GetKycMethodsListAsync()
        {
            try
            {
                var kycMethods = await Context.KycMethods
                    .AsNoTracking()
                    .ToListAsync();

                return kycMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving KYC methods from database.");
                throw;
            }
        }
    }
}
