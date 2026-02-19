using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IQrCredentialVerifiersService
    {
        public Task<ServiceResult> GetQrCredentialVerifierDTOsListAsync();
        public Task<ServiceResult> UpdateQrCredentialVerifierAsync(QrCredentialVerifierDTO qrCredentialVerifierDTO);
        public Task<ServiceResult> CreateQrCredentialVerifierAsync(QrCredentialVerifierDTO qrCredentialVerifierDTO);
        public Task<ServiceResult> GetQrCredentialVerifierByIdAsync(int id);
        public Task<ServiceResult> GetQrCredentialVerifiersListByOrganizationIdAsync(string organizationId);
        public Task<ServiceResult> GetQrCredentialsListByOrganizationId(string organizationId);
        public Task<ServiceResult> GetActiveQrCredentialVerifiersListAsync(string token);
        public Task<ServiceResult> GetActiveQrCredentialVerifiersListByOrganizationIdAsync(string orgId,string token);
        public Task<ServiceResult> GetCredentialVerifierListByIssuerId(string orgId);
        public Task<ServiceResult> ActivateQrCredentialById(int id);
        public Task<ServiceResult> RejectQrCredentialById(int id, string remarks);
    }
}
