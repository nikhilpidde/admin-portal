using DTPortal.Core.DTOs;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class BusinessRequirementViewModel
    {
        public IEnumerable<string> SoftwareList { get; set; } = new List<string>();

        public IList<QuestionsDTO> QuestionAnswerList { get; set; } = new List<QuestionsDTO>();

        public int OrganizationFormId { get; set; }

        [Required(ErrorMessage = "Software recommendation is required")]
        public string SoftwareRecommended { get; set; }
    }
}
