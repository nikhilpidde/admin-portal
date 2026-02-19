using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IBeneficiaryService
    {
        Task<ServiceResult> GetBeneficiaryPrivilegesAsync();
        Task<ServiceResult> GetAllBeneficiariesListByOrgIdAsync(string orgId);

        Task<ServiceResult> AddBeneficiaryAsync(BeneficiaryAddDTO beneficiaryAddDTO, string createdBy, bool makerCheckerFlag = false);

        Task<ServiceResult> GetBeneficiaryDetailsById(int id);
        Task<ServiceResult> EditBeneficiaryAsync(BeneficiaryUpdateDTO beneficiaryDTO,string createdBy, bool makerCheckerFlag = false);
        Task<ServiceResult> AddMultipleBeneficiariesAsync(IList<BeneficiaryAddDTO> beneficiaries, string createdBy, bool makerCheckerFlag = false);
    }
}