using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OnboardingTemplate
{
    public class OnboardingTemplateEditViewModel
    {
        public OnboardingTemplateEditViewModel()
        {
            OnboardingSteps = new List<OnboardingStepListItem>();
            SelectedOnboardingSteps = new List<OnboardingStepDTO>();
        }

        [Required(ErrorMessage ="Template name is required")]
        [Display(Name = "Template Name")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Select Method")]
        public string MethodName { get; set; }

        public string PublishedStatus { get; set; }

        public string State { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public IEnumerable<OnboardingMethodDTO> OnboardingMethods { get; set; }

        public IList<OnboardingStepListItem> OnboardingSteps { get; set; }

        public IList<OnboardingStepDTO> SelectedOnboardingSteps { get; set; }
    }
}
