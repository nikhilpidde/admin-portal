using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class RevokeCertificateViewModel
    {
        public string OrganizationUID { get; set; }

        public string OrganizationName { get; set; }

        //[Required(ErrorMessage = "Please select a reason")]
        //[Display(Name = "Revoke Reason")]
        //public int? RevokeReasonId { get; set; }

        [MaxLength(255, ErrorMessage = "Maximum length allowed is 255 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Remarks")]
        [Required(ErrorMessage = "Remarks field is required")]
        public string Remarks { get; set; }

        //public IEnumerable<RevokeReasonDTO> RevokeReasons { get; set; }
    }
}
