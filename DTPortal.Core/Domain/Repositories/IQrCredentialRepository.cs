using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IQrCredentialRepository : IGenericRepository<QrCredential>
    {
        public Task<IEnumerable<QrCredential>> GetCredentialListAsync();
        public Task<IEnumerable<QrCredential>> GetCredentialListByOrgIdAsync(string orgId);
        public Task<QrCredential> GetCredentialByIdAsync(int Id);
        public Task<QrCredential> GetCredentialByUidAsync(string Uid);
        public Task<bool> CreateCredential(QrCredential credential);
        public Task<bool> UpdateCredential(QrCredential credential);
        public Task<IEnumerable<QrCredential>> GetActiveCredentialListAsync();
        public Task<bool> IsCredentialExistsAsync(string credentialName);
        public Task<List<string>> GetCredentialNameIdListAsync(string orgId);
        public Task<List<QrCredential>> GetVerifiableCredentialList(List<string> credentialIdList);
        public Task<bool> IsCredentialExistsAsync(string credentialName, string credentialId);
    }
}
