using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.SelfServiceConfiguration
{
    public class SelfServiceCategoryViewModel
    {
        public IEnumerable<SelfServiceCategoryDTO> OrgCatogeryFieldList { get; set; }
        public int OrgCategoryId { get; set; }
        public List<SelfServiceFieldDTO> organisationFieldDtos { get; set; }
    }
}
