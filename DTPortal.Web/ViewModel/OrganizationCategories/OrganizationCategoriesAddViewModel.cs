using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OrganizationCategories
{
    public class OrganizationCategoriesAddViewModel
    {
        public int OrgCategoryId { get; set; }
        public string OrgCategoryName { get; set; }
        public List<SelfServiceFieldDTO> organisationFieldDtos { get; set; }
    }
}
