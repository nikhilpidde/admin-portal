using DTPortal.Core.DTOs;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface ISelfPortalService
    {
        Task<IEnumerable<SelfOrganizationNewDTO>> GetAllSelfServiceOrganizationListAsync();
        Task<SelfOrganizationNewDTO> GetSelfServiceNewOrganizationDetailsAsync(int OrgDetailsId);


        Task<SelfServiceOrganizationDTO> GetSelfServiceOrganizationDetailsAsync(int id);

        Task<ServiceResult> ApproveOrganizationAsync(SelfServiceOrganizationDTO organizationDTO, bool makerCheckerFlag = false);

        Task<ServiceResult> RejectOrganizationAsync(SelfServiceOrganizationDTO organizationDTO, bool makerCheckerFlag = false);
        Task<ServiceResult> ApproveOrganizationNewAsync(SelfOrganizationNewDTO organizationDTO, bool makerCheckerFlag = false);

        Task<ServiceResult> RejectOrganizationNewAsync(SelfOrganizationNewDTO organizationDTO, bool makerCheckerFlag = false);

        Task<ServiceResult> GetRejectedReasonAsync();
        Task<BusinessRequirementsDTO> GetQuestionsAsync(int id);

        Task<ServiceResult> RecommendSoftwareAsync(RecommendedSoftwareDTO recommendedSoftwareDTO);
    }
}
