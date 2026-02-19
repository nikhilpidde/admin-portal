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
    public class KycProfilesRepository : GenericRepository<KycProfile, idp_dtplatformContext>,
    IKycProfilesRepository
    {
        private readonly ILogger _logger;
        public KycProfilesRepository(idp_dtplatformContext context, ILogger logger) :
            base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<KycProfile>> ListAllKycProfilesAsync()
        {
            try
            {
                return await Context.KycProfiles.AsNoTracking().ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("ListAllKycProfilesAsync::Database exception: {0}", error);
                return null;
            }
        }
        public async Task<KycProfile> GetKycProfileByNameAsync(string Name)
        {
            try
            {
                return await Context.KycProfiles.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Name == Name);
            }
            catch (Exception error)
            {
                _logger.LogError("GetKycProfileByNameAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
