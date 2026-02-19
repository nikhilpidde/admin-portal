using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OnboardingTemplate
{
    public class OnboardingTemplateAddViewModel
    {
        public OnboardingTemplateAddViewModel()
        {
            OnboardingSteps = new List<OnboardingStepListItem>();
            SelectedOnboardingSteps = new List<OnboardingStepDTO>();
        }

        [Required(ErrorMessage ="Template name is required")]
        [Display(Name = "Template Name")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Select Method")]
        [Display(Name = "Method")]
        public string MethodName { get; set; }

        public IEnumerable<OnboardingMethodDTO> OnboardingMethods { get; set; }

        public IList<OnboardingStepListItem> OnboardingSteps { get; set; }

        public IList<OnboardingStepDTO> SelectedOnboardingSteps { get; set; }
    }
}
