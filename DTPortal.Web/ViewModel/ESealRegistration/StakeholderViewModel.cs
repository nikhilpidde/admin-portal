using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.ESealRegistration
{

    public class StakeholderViewModel
    {
        public string id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }

        //[Display(Name = "SPOC MyTrust Email")]
        [Required(ErrorMessage = "SPOC Email is required")]
        public string spocUgpassEmail { get; set; }
        public string referenceId { get; set; }
        public string organizationUid { get; set; }
        public bool status { get; set; }
        public string onboardingTime { get; set; }
        public string referredBy { get; set; }
        public string creationTime { get; set; }

        [Display(Name = "Stakeholder Type")]
        [Required(ErrorMessage = "Stakeholder Type is required")]
        public string stakeholderType { get; set; }
    }
}
