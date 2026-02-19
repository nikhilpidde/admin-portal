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
    public class TimeBasedAccessRepository : GenericRepository<TimeBasedAccess, idp_dtplatformContext>,
        ITimeBasedAccessRepository
    {
        private readonly ILogger _logger;
        public TimeBasedAccessRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<TimeBasedAccess>> EnumActiveTimeBasedAccess()
        {
            try
            {
                return await Context.TimeBasedAccesses.Where(t => t.Deny == true && t.Status == "ACTIVE").ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("EnumActiveTimeBasedAccess::Database exception: {0}", error);
                return null;
            }
        }

    }
}
