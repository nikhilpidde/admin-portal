using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IVerificationMethodRepository : IGenericRepository<VerificationMethod>
    {
        public Task<IEnumerable<VerificationMethod>> GetVerificationMethodsListAsync();
        public Task<IEnumerable<VerificationMethod>> GetVerificationMethodsByOrganizationIdAsync();
        public Task<VerificationMethod> GetVerificationMethodByUidAsync(string methodUid);
        public Task<IEnumerable<VerificationMethod>> GetVerificationMethodsListByPageAsync
            (int pageNumber, int pageSize);
        public Task<VerificationMethod> GetVerificationMethodDetailsByCodeAsync(string Code);
        public Task<Dictionary<string, string>> GetVerificationMethodNameCodePair();
        public Task<bool> IsMethodCodeNameExist(string methodCode, string methodName);
    }
}
