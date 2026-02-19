using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class SelfServiceNewOrganization
    {
        public IEnumerable<SelfOrganizationNewDTO> OrganizationList { get; set; }
    }
}
