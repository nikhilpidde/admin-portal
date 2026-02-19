using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OrganizationCategories
{
    public class OrganisationCategoryViewModel
    {
        public IEnumerable<SelfServiceCategoryDTO> OrgCatogeryFieldList { get; set; }
        public int OrgCategoryId { get; set; }

        public string OrgCategoryName { get; set; }
        public List<SelfServiceFieldDTO> organisationFieldDtos { get; set; }
    }
}
