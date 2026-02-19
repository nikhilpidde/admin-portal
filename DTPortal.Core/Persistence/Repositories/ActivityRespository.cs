using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace DTPortal.Core.Persistence.Repositories
{
    public class ActivityRespository : GenericRepository<Activity, idp_dtplatformContext>,
            IActivityRespository
    {
        private readonly ILogger _logger;
        public ActivityRespository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ActivityLookupItem>> GetActivityLookupItemsAsync()
        {
            try
            {
                return await Context.Activities.
                    Where(x => x.Enabled == true)
                    .Select(x =>
                    new ActivityLookupItem
                    {
                        Id = x.Id,
                        DisplayName = x.DisplayName,
                        McEnabled = x.McEnabled,
                        IsCritical = x.IsCritical,
                        McSupported = x.McSupported,
                        ParentId = (int)x.ParentId
                    }).ToListAsync();
            }
            catch(Exception error)
            {
                _logger.LogError("GetActivityLookupItemsAsync::Database exception: {0}", error);
                return null;
            }
        }

    }
}
