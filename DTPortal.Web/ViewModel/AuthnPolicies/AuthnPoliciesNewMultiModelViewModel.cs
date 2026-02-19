using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.AuthnPolicies
{
    public class AuthnPoliciesNewMultiModelViewModel
    {
        //[Required]
        //[Display(Name = "Auhentication Name *")]
        //public string Name { get; set; }
        [Required]
        [Display(Name = "Display Name *")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Description *")]
        public string Description { get; set; }

        public string AuthSchemeSequence { get; set; }

        [Required]
        [Display(Name = "Authentication Policies *")]
        public List<string> AuthScheme { get; set; }
        public List<SelectListItem> primaryAuthlist { get; set; }

        [Display(Name = "Supports Provisioning")]
        [Required]
        public int SupportsProvisioning { get; set; }
    }
}
