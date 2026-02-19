using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace DTPortal.Core.Persistence.Repositories
{
    public class UserSecurityQueRepository : GenericRepository<UserSecurityQue, idp_dtplatformContext>,
        IUserSecurityQueRepository
    {
        private readonly ILogger _logger;
        public UserSecurityQueRepository(idp_dtplatformContext context,
            ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<UserSecurityQue>> GetAllUserSecQueAnsAsync(int userId)
        {
            {
                return await Context.UserSecurityQues.AsNoTracking().Where(uu => uu.UserId == userId).ToListAsync();
            }
        }

    }
}
