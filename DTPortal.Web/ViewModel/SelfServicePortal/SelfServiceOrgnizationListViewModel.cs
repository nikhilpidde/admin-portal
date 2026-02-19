using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class SelfServiceOrgnizationListViewModel
    {
        public IEnumerable<SelfServiceOrganizationDTO> OrganizationList { get; set; }
    }
}
