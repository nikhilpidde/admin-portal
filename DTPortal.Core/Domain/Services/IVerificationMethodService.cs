using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IVerificationMethodService
    {
        public Task<ServiceResult> GetVerificationMethodsList();
        public Task<ServiceResult> GetVerificationMethodById(int Id);
        public Task<ServiceResult> CreateVerificationMethod(VerificationMethod verificationMethod, bool makerCheckerFlag = false);
        public Task<ServiceResult> UpdateVerificationMethod(VerificationMethod verificationMethod, bool makerCheckerFlag = false);
        public Task<ServiceResult> DeleteVerificationMethod(int Id);
        public Task<ServiceResult> GetVerificationMethodsStatistics();
        public Task<ServiceResult> GetVerificationMethodsAnalytics();
        public Task<ServiceResult> GetVerificationMethodsListByPage(int pageNumber, int pageSize);
        public Task<ServiceResult> GetVerificationMethodDetailsById(int Id);
        public Task<ServiceResult> GetAttributesByMethodCodeAsync(string methodCode);
        public Task<ServiceResult> GetVerificationMethodsByOrganizationId(string organizationId);
    }
}
