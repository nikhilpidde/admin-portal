using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.WalletPurpose
{
    public class WalletPurposeEditViewModel
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

        public string Status {  get; set; }
    }
}
