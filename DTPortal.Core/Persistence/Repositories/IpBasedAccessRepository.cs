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
    public class IpBasedAccessRepository : GenericRepository<IpBasedAccess, idp_dtplatformContext>,
            IIpBasedAccessRepository
    {
        public IpBasedAccessRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {

        }

        public async Task<string> GetActiveAllowedSingleIps()
        {
            var activeIps = await Context.IpBasedAccesses.Where(aip => aip.Permission == true && aip.Type == "ALLOWED_SINGLE_IP")
                .Select(ai=>ai.Ip).ToListAsync();

            return string.Join(",", activeIps);
        }

        public async Task<string> GetActiveAllowedMaskedIps()
        {
            var activeIps = await Context.IpBasedAccesses.Where(aip => aip.Permission == true && aip.Type == "ALLOWED_MASKED_IP")
                .Select(ai => ai.Ip).ToListAsync();

            return string.Join(",", activeIps);
        }

        public async Task<string> GetActiveDeniedSingleIps()
        {
            var activeIps = await Context.IpBasedAccesses.Where(aip => aip.Permission == true && aip.Type == "DENIED_SINGLE_IP")
                .Select(ai => ai.Ip).ToListAsync();

            return string.Join(",", activeIps);
        }

        public async Task<string> GetActiveDeniedMaskedIps()
        {
            var activeIps = await Context.IpBasedAccesses.Where(aip => aip.Permission == true && aip.Type == "DENIED_MASKED_IP")
                .Select(ai => ai.Ip).ToListAsync();

            return string.Join(",", activeIps);
        }

        public async Task<bool> IsIPAccessActive()
        {
            return await Context.IpBasedAccesses.AnyAsync(aip => aip.Permission == true);
        }
    }
}
