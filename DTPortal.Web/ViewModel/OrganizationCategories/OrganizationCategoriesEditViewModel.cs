using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OrganizationCategories
{
    public class OrganizationCategoriesEditViewModel
    {
        public string OrgCategoryName { get; set; }
        public int OrgCategoryId { get; set; }

        public List<SelfServiceFieldDTO> organisationFieldDtos { get; set; }
    }
}
