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
    public interface IProvisionStatusService
    {
        public Task<ServiceResult> GetProvisionStatus(string Suid, string credentialId);
        public Task<ServiceResult> AddProvisionStatus(string Suid, string credentialId,string status, string documentId);
        //public Task<ServiceResult> UpdateProvisionStatus(ProvisionStatusDTO provisionStatus);
        public Task<ServiceResult> DeleteProvision(string credentialId, string Suid);
        public Task<ServiceResult> RevokeProvision(string credentialId, string documentId);
    }
}
