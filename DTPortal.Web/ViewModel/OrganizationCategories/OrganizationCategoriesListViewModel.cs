using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OrganizationCategories
{
    public class OrganizationCategoriesListViewModel
    {
        public IEnumerable<SelfServiceCategoryDTO> OrgCatogeryFieldList { get; set; }
        public int OrgCategoryId { get; set; }

        public string OrgCategoryName { get; set; }
        public List<OrganizationFieldDTO> organisationFieldDtos { get; set; }
    }


}
