using DTPortal.Web.ViewModel.Scopes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Scopes
{
    public class ScopesNewViewModel
    {

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
        [Display(Name = "Require user consent for this scope")]
        public bool UserConsent { get; set; }

        [Required]
        [Display(Name = "Set as default scope")]
        public bool DefaultScope { get; set; }

        [Required]
        [Display(Name = "Include in public metadata")]
        public bool Metadata { get; set; }

        [Required]
        [Display(Name = "Required Claims")]
        public bool isClaimPresent { get; set; }

        [Required]
        [Display(Name = "Save Consent Data")]
        public bool SaveConsent { get; set; }

        public string claims { get; set; }
        public IEnumerable<ClaimListItem> ClaimLists { get; set; }
    }
}
