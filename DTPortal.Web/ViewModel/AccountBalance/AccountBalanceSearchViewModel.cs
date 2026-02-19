using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.AccountBalance
{
    public class AccountBalanceSearchViewModel
    {
        [Display(Name = "Search By")]
        [Required(ErrorMessage = "Select Search By")]
        public UserType? IdentifierType { get; set; }

        [Display(Name = "UID")]
        [Required(ErrorMessage = "UID is required")]
        public string UID { get; set; }

        public object AccountBalanceDetails { get; set; }
    }
}
