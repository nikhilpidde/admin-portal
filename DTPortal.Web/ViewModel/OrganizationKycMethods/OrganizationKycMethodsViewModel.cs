using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.OrganizationKycMethods
{
    public class OrganizationKycMethodsViewModel
    {
        [Required]
        [Display(Name = "Organization Name")]
        public string OrganizationId { get; set; }
        public List<SelectListItem> OrganizationList { get; set; }
        public List<string> SelectedKycMethodNames { get; set; }
        public List<string> SelectedKycProfileNames { get; set; }
    }
}
