using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using DTPortal.Web.CustomValidations;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class DocumentVerificationViewModel
    {
        public DocumentVerificationViewModel()
        {
            Emails = new List<string>();
        }

        [Required]
        public string OrganizationUID { get; set; }

        [Required]
        public string OrganizationName { get; set; }

        [Required]
        public List<string> Emails { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Signed document")]
        [Required(ErrorMessage = "Signed document is required")]
        public IFormFile SignedPdf { get; set; }

        public string ESealActionMethod { get; set; }

        public string TransactionId { get; set; }

        public string EnablePostPaidOption { get; set; }
    }
}
