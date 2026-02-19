using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class AddVendorViewModel
    {
        [Display(Name = "Organization Name")]
        public string orgId {  get; set; }
        public string orgName { get; set; }

        [Display(Name = "Vendor Id")]
        public string vendorId { get; set; }
        public string sponsorOrgId {  get; set; }
        public List<SelectListItem> organizationList { get; set; }
    }
}
