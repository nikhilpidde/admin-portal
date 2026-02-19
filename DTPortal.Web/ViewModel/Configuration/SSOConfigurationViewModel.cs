using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Configuration
{
    public class SSOConfigurationViewModel
    {
        [Required]
        [Display(Name = "Session Timeout (Minute)")]
        public int SSOSessionTimeout { get; set; }

        [Required]
        [Display(Name = "Temporary Session Timeout (Minute)")]
        public int SSOTemporarySessionTimeout { get; set; }

        [Required]
        [Display(Name = "Ideal Timeout (Minute)")]
        public int SSOIdealTimeout { get; set; }

        [Required]
        [Display(Name = "Access Token Timeout (Minute)")]
        public int SSOAccessTokenTimeout { get; set; }

        [Required]
        [Display(Name = "Authorization Code Timeout (Minute)")]
        public int SSOAuthorizationCodeTimeout { get; set; }

        [Required]
        [Display(Name = "Active Sessions Per User")]
        public int SSOActiveSessionsPerUser { get; set; }

        [Required]
        [Display(Name = "Wrong pin count")]
        public int SSOWrongPin { get; set; }

        [Required]
        [Display(Name = "Wrong Code count")]
        public int SSOWrongCode { get; set; }

        [Required]
        [Display(Name = "Deny count")]
        public int SSODenyCount { get; set; }

        [Required]
        [Display(Name = "Account Lock Time (Hour)")]
        public int SSOAccountLockTime { get; set; }

        [Required]
        [Display(Name = "Central Log Connection Url")]
        public string CentralLogConnection { get; set; }

        [Required]
        [Display(Name = "Central Log Queue Name")]
        public string CentralLogQueueName { get; set; }

        [Required]
        [Display(Name = "Service Log Connection Url")]
        public string ServiceLogConnection { get; set; }

        [Required]
        [Display(Name = "Service Log Queue Name")]
        public string ServiceLogQueueName { get; set; }

        [Required]
        [Display(Name = "Admin Log Connection Url")]
        public string AdminLogConnection { get; set; }

        [Required]
        [Display(Name = "Admin Log Queue Name")]
        public string AdminLogQueueName { get; set; }

        [Required]
        [Display(Name = "PKI Service Base Address")]

        public string PKIServiceBaseAddress { get; set; }

        [Required]
        [Display(Name = "PKI Service Generate Signature Url")]
        public string PKIServiceGenerateSignatureUri { get; set; }

        [Required]
        [Display(Name = "PKI Service Verify Signature Url")]
        public string PKIServiceVerifySignatureUri { get; set; }

        //[Required]
        //[Display(Name = "Redis Server Connection")]
        //public string RedisServerConnection { get; set; }

        [Required]
        [Display(Name = "RA Service Base Address")]
        public string RAbaseAddress { get; set; }

        [Required]
        [Display(Name = "RA Status Update Url")]
        public string RAstatusUpdateUri { get; set; }


        [Display(Name = "IDP Database Connection Url")]
        public string IDPDatabaseConnection { get; set; }


        [Display(Name = "RA Database Connection Url")]
        public string RADatabaseConnection { get; set; }
    }
}
