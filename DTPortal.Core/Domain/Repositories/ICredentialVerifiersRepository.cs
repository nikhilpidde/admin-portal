using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface ICredentialVerifiersRepository : IGenericRepository<CredentialVerifier>
    {
        public Task<bool> IsCredentialAlreadyExists(CredentialVerifierDTO credentialVerifierDTO);
        public Task<List<string>> GetCredentialsListByOrganizationIdAsync(string organizationId);
        public Task<IEnumerable<CredentialVerifier>> GetCredentialListDataByOrganizationIdAsync(string organizationId);
        public Task<IEnumerable<CredentialVerifier>> GetActiveCredentialVerifierListAsync();
        public Task<IEnumerable<CredentialVerifier>> GetActiveCredentialListByOrganizationIdAsync(string orgId);
        public Task<IEnumerable<CredentialVerifier>> GetCredentialVerifierListByIssuerIdAsync(string organizationId);
    }
}
