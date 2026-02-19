using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class PrivilegeSelectionWithOrgViewModel
    {
        public string organizationId { get; set; }
        public List<PrivilegeSelectionViewModel> privileges { get; set; }
    }
}
