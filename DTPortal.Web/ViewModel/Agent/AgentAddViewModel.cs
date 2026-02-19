using DTPortal.Web.Enums;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.Agent
{
    public class AgentAddViewModel
    {

        [Display(Name = "Select Type")]
        [Required(ErrorMessage = "Select type")]
        public AgentIndentifier IdentifierType { get; set; }

        [Display(Name = "Value")]
        [Required(ErrorMessage = "Value is required")]
        public string IdentifierValue { get; set; }
    }
}
