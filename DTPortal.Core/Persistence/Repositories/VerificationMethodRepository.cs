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
    public class VerificationMethodRepository : GenericRepository<VerificationMethod, idp_dtplatformContext>,
            IVerificationMethodRepository
    {
        private readonly ILogger _logger;
        public VerificationMethodRepository(idp_dtplatformContext context, ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<VerificationMethod>> GetVerificationMethodsListAsync()
        {
            try
            {
                var verificationMethods = await Context.VerificationMethods
                    .AsNoTracking()
                    .OrderByDescending(v => v.Id)
                    .ToListAsync();

                return verificationMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving verification methods from database.");
                throw;
            }
        }

        public async Task<bool> IsMethodCodeNameExist(string methodCode,string methodName)
        {
            try
            {
                return await Context.VerificationMethods
                    .AsNoTracking()
                    .AnyAsync(vm => vm.MethodCode == methodCode || vm.MethodName == methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of method code or name in database.");
                throw;
            }
        }

        public async Task<IEnumerable<VerificationMethod>> GetVerificationMethodsByOrganizationIdAsync()
        {
            try
            {
                var verificationMethodsList = await Context.VerificationMethods
                    .AsNoTracking()
                    .Include(o => o.OrganizationVerificationMethods)
                    .ToListAsync();

                return verificationMethodsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving organization verification methods from database.");
                throw;
            }
        }

        public async Task<VerificationMethod> GetVerificationMethodByUidAsync(string methodUid)
        {
            try
            {
                var verificationMethods = await Context.VerificationMethods
                    .AsNoTracking()
                    .Include(o => o.OrganizationVerificationMethods)
                    .Where(u=>u.MethodUid==methodUid).
                    FirstOrDefaultAsync();

                return verificationMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving verification methods from database.");
                throw;
            }
        }

        public async Task<IEnumerable<VerificationMethod>> GetVerificationMethodsListByPageAsync
            (int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var verificationMethods = await Context.VerificationMethods
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return verificationMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving verification methods from database.");
                throw;
            }
        }

        public async Task<VerificationMethod> GetVerificationMethodDetailsByCodeAsync(string Code)
        {
            try
            {
                var verificationMethod = await Context.VerificationMethods
                    .AsNoTracking()
                    .FirstOrDefaultAsync(vm => vm.MethodCode == Code);
                return verificationMethod;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving verification method details from database.");
                throw;
            }
        }

        public async Task<Dictionary<string,string>> GetVerificationMethodNameCodePair()
        {
            try
            {
                var methodPairs = await Context.VerificationMethods
                    .AsNoTracking()
                    .Select(vm => new { vm.MethodCode, vm.MethodName })
                    .ToListAsync();
                return methodPairs.ToDictionary(mp => mp.MethodCode, mp => mp.MethodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving verification method name-code pairs from database.");
                return new Dictionary<string, string>();
            }
        }
    }
}
