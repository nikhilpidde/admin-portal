using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface ICredentialRepository : IGenericRepository<Credential>
    {
        public Task<IEnumerable<Credential>> GetCredentialListAsync();
        public Task<IEnumerable<Credential>> GetCredentialListByOrgIdAsync(string orgId);
        public Task<Credential> GetCredentialByIdAsync(int Id);
        public Task<Credential> GetCredentialByUidAsync(string Uid);
        public Task<bool> CreateCredential(Credential credential);
        public Task<bool> UpdateCredential(Credential credential);
        public Task<IEnumerable<Credential>> GetActiveCredentialListAsync();
        public Task<bool> IsCredentialExistsAsync(string credentialName);
        public Task<List<string>> GetCredentialNameIdListAsync(string orgId);
        public Task<List<Credential>> GetVerifiableCredentialList(List<string> credentialIdList);
        public Task<bool> IsCredentialDisplayExistAsync(string displayName);
        public Task<bool> IsCredentialExistsAsync(string credentialName, string credentialId);
        public Task<bool> IsCredentialDisplayExistAsync(string displayName, string credentialId);
    }
}
