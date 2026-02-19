using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ICredentialVerifiersService
    {
        public Task<ServiceResult> GetCredentialVerifierDTOsListAsync();
        public Task<ServiceResult> UpdateCredentialVerifierAsync(CredentialVerifierDTO credentialVerifierDTO);
        public Task<ServiceResult> CreateCredentialVerifierAsync(CredentialVerifierDTO credentialVerifierDTO);
        public Task<ServiceResult> GetCredentialVerifierByIdAsync(int id);
        public Task<ServiceResult> GetCredentialVerifiersListByOrganizationIdAsync(string organizationId);
        public Task<ServiceResult> GetCredentialsListByOrganizationId(string organizationId);
        public Task<ServiceResult> GetActiveCredentialVerifiersListAsync(string token);
        public Task<ServiceResult> GetActiveCredentialVerifiersListByOrganizationIdAsync(string orgId, string token);
        public Task<ServiceResult> GetCredentialVerifierListByIssuerId(string orgId);
        public Task<ServiceResult> ActivateCredentialById(int id);
        public Task<ServiceResult> RejectCredentialById(int id, string remarks);
    }
}
