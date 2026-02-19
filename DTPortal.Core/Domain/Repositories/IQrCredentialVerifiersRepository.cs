using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IQrCredentialVerifiersRepository : IGenericRepository<QrCredentialVerifier>
    {
        public Task<bool> IsCredentialAlreadyExists(QrCredentialVerifierDTO credentialVerifierDTO);
        public Task<List<string>> GetCredentialsListByOrganizationIdAsync(string organizationId);
        public Task<IEnumerable<QrCredentialVerifier>> GetCredentialListDataByOrganizationIdAsync(string organizationId);
        public Task<IEnumerable<QrCredentialVerifier>> GetActiveCredentialVerifierListAsync();
        public Task<IEnumerable<QrCredentialVerifier>> GetActiveCredentialListByOrganizationIdAsync(string orgId);
        public Task<IEnumerable<QrCredentialVerifier>> GetCredentialVerifierListByIssuerIdAsync(string organizationId);
    }
}
