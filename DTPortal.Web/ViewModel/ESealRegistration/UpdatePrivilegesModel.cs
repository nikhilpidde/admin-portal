using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class UpdatePrivilegesModel
    {
        public string OrganizationId { get; set; }
        public List<string> SelectedPrivileges { get; set; }
    }
}
