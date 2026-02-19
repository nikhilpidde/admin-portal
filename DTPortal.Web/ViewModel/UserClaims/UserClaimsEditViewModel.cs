using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserClaims
{
    public class UserClaimsEditViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Require user consent for this claim")]
        public bool UserConsent { get; set; }

        [Required]
        [Display(Name = "Set as default Attribute")]
        public bool DefaultClaim { get; set; }

        [Required]
        [Display(Name = "Include in public metadata")]
        public bool Metadata { get; set; }
    }
}
