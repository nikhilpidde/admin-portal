using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IIdValidationService
    {
        public ServiceResult ValidateSignedDataAsync(string signedData, string serviceName);
        public Task<IList<IdValidationSummaryResponse>> GetAllServiceProvidersDetails();

        public Task<PaginatedList<IdValidationResponseDTO>> GetIdValidationLogReportAsync(
            string identifier = "",
            string logMessageType = "",
            string orgId = "",
            List<string> kycMethods = null,
            string fromDate = "",
            string toDate = "",
            int page = 1,
            int perPage = 10);

        //public Task<PaginatedList<IdValidationResponseDTO>> GetOrgIdValidationLogReportAsync(
        //    string orgId,
        //    string identifier = "",
        //    string logMessageType = "",
        //    int page = 1,
        //    int perPage = 10);
    
        public Task<KycSummaryDTO> GetIdValidationSummaryAsync();

        public Task<OrgKycSummaryDTO> GetOrganizationIdValidationSummaryAsync(string orgId);
    }
}
