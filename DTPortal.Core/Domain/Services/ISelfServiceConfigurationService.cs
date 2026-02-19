using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ISelfServiceConfigurationService
    {
        Task<ServiceResult> GetAllConfigCategories();
        Task<ServiceResult> GetCategoryFieldNameById(int id);
        Task<ServiceResult> GetCategoryByOrganizationId(string organizationId);
        Task<ServiceResult> UpdateCatogeryFields(OrgCategoryFieldDetailsDTO fieldsDto);
    }
}
