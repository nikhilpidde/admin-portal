using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using DTPortal.Web.CustomValidations;

namespace DTPortal.Web.ViewModel.PKIConfiguration
{
    public class OtherSettingsEditViewModel : BaseOtherSettingsViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Log Path")]
        public string LogPath { get; set; }

        [Required]
        [Display(Name = "Log Level")]
        public string LogLevel { get; set; }

        [Required]
        [Display(Name = "Config Directory Path")]
        public string ConfigDirectoryPath { get; set; }

        [Required]
        [Display(Name = "Log Queue IP")]
        public string LogQueueIP { get; set; }

        [Required]
        [Display(Name = "Log Queue Port")]
        public int? LogQueuePort { get; set; }

        [Required]
        [Display(Name = "Log Queue User Name")]
        public string LogQueueUserName { get; set; }

        [Required]
        [Display(Name = "Log Queue Password")]
        public string LogQueuePassword { get; set; }

        [Required]
        [Display(Name = "Jre64 Bit Directory")]
        public string Jre64BitDirectory { get; set; }

        [Required]
        [Display(Name = "IDP Url")]
        public string IDPUrl { get; set; }

        [Required]
        [Display(Name = "TSA Url")]
        public string TSAUrl { get; set; }

        [Required]
        [Display(Name = "OCSP Url")]
        public string OCSPUrl { get; set; }

        [Required]
        [Display(Name = "PKI Service Url")]
        public string PKIServiceUrl { get; set; }

        [Required]
        [Display(Name = "Signature Service Url")]
        public string SignatureServiceUrl { get; set; }

        [Required]
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        [Display(Name = "Enable Dss")]
        public bool EnableDss { get; set; }

        [Display(Name = "Dss Client")]
        public bool DssClient { get; set; }

        [Display(Name = "Sign Locally")]
        public bool SignLocally { get; set; }

        [Display(Name = "Log Call Stack")]
        public bool LogCallStack { get; set; }

        [Display(Name = "Staging Environment")]
        public bool StagingEnvironment { get; set; }

        [Display(Name = "Introspect")]
        public bool Introspect { get; set; }

        [Display(Name = "Hand Signature")]
        public bool HandSignature { get; set; }

        //[Required(ErrorMessage = "Please select Signature Image")]
        [Display(Name = "Signature Image")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)] // 1 MB
        [AllowedExtensions(new string[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile SignatureImageFile { get; set; }
        
        [Required]
        [Display(Name = "Signing Log Queue")]
        public string SigningLogQueue { get; set; }

        [Required]
        [Display(Name = "RA Log Queue")]
        public string RALogQueue { get; set; }

        [Required]
        [Display(Name = "Central Log Queue")]
        public string CentralLogQueue { get; set; }
    }
}
