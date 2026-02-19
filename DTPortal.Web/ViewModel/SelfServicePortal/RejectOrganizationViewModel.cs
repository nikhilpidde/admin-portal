using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class RejectOrganizationViewModel
    {
        public int OrgFormId { get; set; }

        public IList<string> RejectedReasonList { get; set; }
        public string RejectedReason { get; set; }

        [MaxLength(120, ErrorMessage = "Maximum length allowed is 120 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Remarks")]
        //[Required(ErrorMessage = "Remarks field is required")]
        public string Remarks { get; set; }
    }
}
