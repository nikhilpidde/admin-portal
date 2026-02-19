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
    public class PasswordPolicyRepository : GenericRepository<PasswordPolicy, idp_dtplatformContext>,
            IPasswordPolicyRepository
    {
        private readonly ILogger _logger;
        public PasswordPolicyRepository(idp_dtplatformContext context,
            ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
    }
}
