using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;
using Microsoft.AspNetCore.Http;

namespace DTPortal.Web.ViewModel.Consent
{
    public class ConsentAddViewModel
    {
        [MaxLength(10000, ErrorMessage = "Maximum length allowed is 10000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Consent")]
        [Required(ErrorMessage = "Consent field is required")]
        public string Consent { get; set; }

        [MaxLength(10000, ErrorMessage = "Maximum length allowed is 10000 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Privacy Consent")]
        [Required(ErrorMessage = "Privacy Consent field is required")]
        public string PrivacyConsent { get; set; }

        [Display(Name = "Consent Type")]
        [Required(ErrorMessage = "Select Consent Type")]
        public ConsentType? ConsentType { get; set; }

        [Display(Name = "Consent Required")]
        public bool ConsentRequired { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Data Privacy")]
        public IFormFile DataPrivacy { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Terms And Conditions")]
        public IFormFile TermsAndConditions { get; set; }
    }
}
