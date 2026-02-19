using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DTPortal.Core.Persistence.Repositories
{
    public class OperationsAuthSchemeRepository : GenericRepository<OperationsAuthscheme, idp_dtplatformContext>,
        IOperationsAuthSchemeRepository
    {
        private readonly ILogger _logger;
        public OperationsAuthSchemeRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<OperationsAuthscheme> GetOperationsAuthschemeByOperationName(string name)
        {
            try
            {
                return await Context.OperationsAuthschemes.SingleOrDefaultAsync(o => o.OperationName == name);
            }
            catch (Exception error)
            {
                _logger.LogError("GetOperationsAuthschemeByOperationName::Database exception: {0}", error);
                return null;
            }
        }
    }
}
