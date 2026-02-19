using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OnboardingTemplate
{
    public class OnbaordingTemplateListViewModel
    {
        public IEnumerable<OnboardingTemplateDTO> Templates { get; set; }
    }
}
