using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.Purposes
{
    public class PurposeEditViewModel
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

        [Display(Name = "user consent required")]
        public bool UserConsent { get; set; }

        [Display(Name ="Status")]
        public string Status { get; set; }
    }
}
