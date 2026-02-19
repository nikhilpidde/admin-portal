using DTPortal.Web.ViewModel.Scopes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.WalletDomain
{
    public class WalletDomainNewViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        public string Description { get; set; }


        [Required]
        [Display(Name = "Purposes")]
        public string Purposes { get; set; }

        public bool isPurposePresent { get; set; }


        public IEnumerable<PurposeListItem> PurposeLists { get; set; }
    }
}
