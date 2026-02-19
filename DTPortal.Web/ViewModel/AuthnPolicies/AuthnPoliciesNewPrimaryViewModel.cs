using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.AuthnPolicies
{
    public class AuthnPoliciesNewPrimaryViewModel
    {
        [Required]
        [Display(Name ="Auhentication Name *")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Display Name *")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Client Verify")]
        public int ClientVerify { get; set; }
        [Required]
        [Display(Name = "String Match")]
        public int StrngMatch { get; set; }
        [Required]
        [Display(Name = "Description *")]
        public string Description { get; set; }
        [Display(Name = "Random Number Present")]
        [Required]
        public int RandPresent { get; set; }

        [Display(Name = "Supports Provisioning")]
        [Required]
        public int SupportsProvisioning { get; set; }

    }
}
