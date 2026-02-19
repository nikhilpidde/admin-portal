using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.MobileVersionConfiguration
{
    public class MobileVersionConfigurationAddViewModel : BaseMobileVersionConfigurationViewModel
    {
        [Required]
        [Display(Name = "OS Version")]
        public string OSVersion { get; set; }

        [Required]
        [Display(Name = "Latest Version")]
        public string LatestVersion { get; set; }

        [Required]
        [Display(Name = "Minimum Version")]
        public string MinimumVersion { get; set; }

        [Display(Name = "Update Link")]
        public string UpdateLink { get; set; }
    }
}
