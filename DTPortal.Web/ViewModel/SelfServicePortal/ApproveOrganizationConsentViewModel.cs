using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class ApproveOrganizationConsentViewModel
    {
        public int OrganizationFormId { get; set; }

        //[Display(Name = "MyTrust Email")]
        [Required(ErrorMessage = "Email is required")]
        public string AdminUgpassEmail { get; set; }
    }
}
