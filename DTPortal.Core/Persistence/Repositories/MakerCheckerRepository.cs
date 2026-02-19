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
    public class MakerCheckerRepository : GenericRepository<MakerChecker,
        idp_dtplatformContext>, IMakerCheckerRepository
    {
        private readonly ILogger _logger;
        public MakerCheckerRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<MakerChecker>> GetAllRequestsByMakerId(int id)
        {
            try
            {
                var list = await Context.MakerCheckers.
                    Where(t => t.MakerId == id).ToListAsync();
                return list.AsEnumerable().OrderByDescending(x => x.CreatedDate);
            }
            catch (Exception error)
            {
                _logger.LogError("GetAllRequestsByMakerId::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<MakerChecker>> GetAllRequestsByCheckerRoleId222(int id)
        {
            try
            {
                return await Context.MakerCheckers.
                    Where(t => t.ActivityId == id).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetAllRequestsByCheckerRoleId222::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<MakerChecker>> GetAllRequestsByCheckerRoleId(int id)
        {
            //await Context.MakerCheckers.
            //    Where(t => t.ActivityId == t.Activity.RoleActivities.Where
            //    (x=>x.RoleId == id  && x.ActivityId == t.ActivityId)).ToListAsync();

            try
            {
                var result = new List<MakerChecker>()
                { };

                var roleActivities = await Context.RoleActivities.Where
                    (x => x.RoleId == id).ToListAsync();

                foreach (var item in roleActivities)
                {
                    var res = await Context.MakerCheckers.Where
                        (x => x.ActivityId == item.ActivityId && x.State == "PENDING")
                        .ToListAsync();
                    result = result.Concat(res).ToList();
                }

                return result.AsEnumerable().OrderByDescending(x => x.CreatedDate);
            }
            catch (Exception error)
            {
                _logger.LogError("GetAllRequestsByCheckerRoleId222::Database exception: {0}", error);
                return null;
            }
        }
    }
}
