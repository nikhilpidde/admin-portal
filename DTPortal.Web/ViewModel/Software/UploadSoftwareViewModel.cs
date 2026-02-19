using DTPortal.Web.CustomValidations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.Software
{
    public class UploadSoftwareViewModel
    {
        [Display(Name = "Software Zip")]
        [DataType(DataType.Upload)]
        [MaxFileSize(200 * 1024 * 1024)] // 200 MB
        [AllowedExtensions(new string[] { ".zip" })]
        [Required(ErrorMessage = "Please select a file")]
        public IFormFile SoftwareZip { get; set; }

        [Display(Name = "Manual")]
        [DataType(DataType.Upload)]
        //[MaxFileSize(1 * 1024 * 1024)] // 1 MB
        [AllowedExtensions(new string[] { ".pdf" })]
        [Required(ErrorMessage = "Please select a file")]
        public IFormFile Mannual { get; set; }

        [Required]
        [Display(Name = "Software Version")]
        public string SoftwareVersion { get; set; }

        [Required]
        [Display(Name = "Software Name")]
        public string SoftwareName { get; set; }
    }
}
