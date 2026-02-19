using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IOrganizationCategoriesService
    {
        Task<ServiceResult> GetAllCategories();
        Task<ServiceResult> GetCategoryFieldNameById(int id);

        Task<ServiceResult> GetAllCategoryFields();
        Task<ServiceResult> UpdateCatogeryFields(OrgCategoryFieldDetailsDTO fieldsDto);

        Task<ServiceResult> SaveCatogeryFields(OrganizationCategoryAddRequestDTO fieldsDto);
        Task<ServiceResult> DeleteCategoryAsync(int id);
    }
}
