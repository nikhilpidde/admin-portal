using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IQrCredentialService
    {
        public Task<ServiceResult> GetCredentialList();
        public Task<ServiceResult> GetActiveCredentialList();
        public Task<ServiceResult> GetCredentialListByOrgId(string orgId);
        public Task<ServiceResult> GetCredentialById(int Id);
        public Task<ServiceResult> CreateCredentialAsync(QrCredentialDTO credential);
        public Task<ServiceResult> UpdateCredential(QrCredentialDTO credential);
        public Task<ServiceResult> TestCredential(QrTestCredentialRequest request);
        public Task<ServiceResult> GetCredentialByUid(string Id);
        public Task<ServiceResult> GetCredentialOfferByUid(string Id,string token);
        public Task<ServiceResult> ActivateCredential(string credentialId);
        public Task<ServiceResult> RejectCredential(string credentialId, string remarks);
        public Task<ServiceResult> GetCredentialDetails(string credentialUid);
        public Task<ServiceResult> GetCredentialNameIdListAsync(string organizationId);
        public Task<ServiceResult> GetVerifiableCredentialList(string orgId);
        public Task<ServiceResult> GetCredentialNameIdListAsync();
        public Task<ServiceResult> SendToApproval(string credentialId, string signedDocument);
    }
}
