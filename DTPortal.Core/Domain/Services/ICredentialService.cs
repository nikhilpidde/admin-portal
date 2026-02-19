using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ICredentialService
    {
        public Task<ServiceResult> GetCredentialList();
        public Task<ServiceResult> GetActiveCredentialList(string token);
        public Task<ServiceResult> GetCredentialListByOrgId(string orgId);
        public Task<ServiceResult> GetCredentialById(int Id);
        public Task<ServiceResult> CreateCredentialAsync(CredentialDTO credential);
        public Task<ServiceResult> UpdateCredential(CredentialDTO credential);
        public Task<ServiceResult> TestCredential(string userId, string credentialId);
        public Task<ServiceResult> GetUserProfile(string userId, string credentialId);
        public Task<ServiceResult> GetCredentialByUid(string Id);
        public Task<ServiceResult> GetCredentialOfferByUid(string Id, string token);
        public Task<ServiceResult> ActivateCredential(string credentialId);
        public Task<ServiceResult> RejectCredential(string credentialId, string remarks);
        public Task<ServiceResult> GetCredentialDetails(string credentialUid);
        public Task<ServiceResult> GetCredentialNameIdListAsync(string organizationId);
        public Task<ServiceResult> GetVerifiableCredentialList(string orgId);
        public Task<ServiceResult> GetCredentialNameIdListAsync();
        public Task<ServiceResult> SendToApproval(string credentialId, string signedDocument);
        public Task<ServiceResult> GetAuthSchemesList();
    }
}
