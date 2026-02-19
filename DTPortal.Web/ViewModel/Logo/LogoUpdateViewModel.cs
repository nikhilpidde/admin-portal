using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.CustomValidations;

namespace DTPortal.Web.ViewModel.Logo
{
    public class LogoUpdateViewModel
    {
        [Required(ErrorMessage = "Please select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)] // 1 MB
        [AllowedExtensions(new string[] { ".svg" })]
        public IFormFile LogoPrimary { get; set; }
    }
}
