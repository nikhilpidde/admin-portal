using System.Collections.Generic;

using DTPortal.Core.Domain.Lookups;


namespace DTPortal.Web.ViewModel.RoleManagement
{
    public class RoleManagementListViewModel
    {
        public IEnumerable<RoleLookupItem> RoleLookupItems { get; set; }
    }
}
