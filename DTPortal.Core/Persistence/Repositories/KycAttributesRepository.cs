using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class KycAttributesRepository : GenericRepository<KycAttribute, idp_dtplatformContext>,
    IKycAttributesRepository
    {
        private readonly ILogger _logger;
        public KycAttributesRepository(idp_dtplatformContext context, ILogger logger) :
            base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<KycAttribute>> ListAllKycAttributesAsync()
        {
            try
            {
                return await Context.KycAttributes.AsNoTracking().ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("ListAllKycAttributesAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
