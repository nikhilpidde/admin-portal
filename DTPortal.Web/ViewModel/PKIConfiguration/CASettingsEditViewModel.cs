using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using DTPortal.Web.CustomValidations;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Web.ViewModel.PKIConfiguration
{
    public class CASettingsEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Url")]
        public string Url { get; set; }

        [Display(Name = "Certificate Authority")]
        public string CertificateAuthority { get; set; }

        [Display(Name = "End Entity Profile Name")]
        public string EndEntityProfileName { get; set; }

        [Display(Name = "Certificate Profile Name")]
        public string CertificateProfileName { get; set; }

        [Display(Name = "Client Authentication Certificate")]
        public string ClientAuthenticationCertificate { get; set; }

        [Display(Name = "Client Authentication Certificate Password")]
        public string ClientAuthenticationCertificatePassword { get; set; }

        [Required]
        [Display(Name = "Issuer Dn")]
        public string IssuerDn { get; set; }

        [Required]
        [Display(Name = "Certificate Validity (in days)")]
        public int? CertificateValidity { get; set; }

        [Required]
        [Display(Name = "Procedure")]
        public int? ProcedureId { get; set; }

        [Required]
        [Display(Name = "CA Plugin")]
        public int? CAPluginId { get; set; }

        [Required]
        [Display(Name = "Statging Certificate Procedure RSA256")]
        public string StatgingCertificateProcedureRSA256 { get; set; }

        [Required]
        [Display(Name = "Statging Certificate Procedure RSA512")]
        public string StatgingCertificateProcedureRSA512 { get; set; }

        [Required]
        [Display(Name = "Statging Certificate Procedure EC256")]
        public string StatgingCertificateProcedureEC256 { get; set; }

        [Required]
        [Display(Name = "Statging Certificate Procedure EC512")]
        public string StatgingCertificateProcedureEC512 { get; set; }

        [Required]
        [Display(Name = "Test Certificate Procedure RSA256")]
        public string TestCertificateProcedureRSA256 { get; set; }

        [Required]
        [Display(Name = "Test Certificate Procedure RSA512")]
        public string TestCertificateProcedureRSA512 { get; set; }

        [Required]
        [Display(Name = "Test Certificate Procedure EC256")]
        public string TestCertificateProcedureEC256 { get; set; }

        [Required]
        [Display(Name = "Test Certificate Procedure EC512")]
        public string TestCertificateProcedureEC512 { get; set; }

        //[Required(ErrorMessage = "Please select Signing Issuer Certificate")]
        [Display(Name = "Signing Certificate Issuer")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile SigningCertificateIssuerFile { get; set; }

        //[Required(ErrorMessage = "Please select Signing Root Certificate")]
        [Display(Name = "Signing Certificate Root")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile SigningCertificateRootFile { get; set; }

        //[Required(ErrorMessage = "Please select OCSP Signer Certificate")]
        [Display(Name = "OCSP Signer Certificate")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile OCSPSignerCertificateFile { get; set; }

        //[Required(ErrorMessage = "Please select Signing Certificate Chain")]
        [Display(Name = "Signing Certificate Chain")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile SigningCertificateChainFile { get; set; }

        //[Required(ErrorMessage = "Please select Timestamping Certificate")]
        [Display(Name = "Timestamping Certificate")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile TimestampingCertificateFile { get; set; }

        //[Required(ErrorMessage = "Please select Timestamping Certificate Chain")]
        [Display(Name = "Timestamping Certificate Chain")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".cer", ".crt" })]
        public IFormFile TimestampingCertificateChainFile { get; set; }

        public IEnumerable<PkiProcedure> Procedures { get; set; }

        public IEnumerable<PkiCaPlugin> CAPlugins { get; set; }
    }
}
