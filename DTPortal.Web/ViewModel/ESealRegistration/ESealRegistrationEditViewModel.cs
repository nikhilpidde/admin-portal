using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using DTPortal.Web.Enums;
using DTPortal.Web.CustomValidations;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class ESealRegistrationEditViewModel : BaseESealRegistrationViewModel
    {
        public ESealRegistrationEditViewModel()
        {
            SignatoryEmailList = new List<string>();
            OrganizationUsersList = new List<OrganizationUser>();
        }

        public int OrganizationId { get; set; }

        public string OrganizationUID { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "Please enter valid Organization name")]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization Email")]
        [Required(ErrorMessage = "Invalid email address")]
        [DataType(DataType.EmailAddress)]
        public string OrganizationEmail { get; set; }

        [Display(Name = "Email Domain")]
        public string EmailDomain { get; set; }

        [Display(Name = "Unique Registered Number")]
        public string UniqueRegdNo { get; set; }

        [Display(Name = "Tax Number (TAN)")]
        public string TaxNo { get; set; }

        [Display(Name = "Corporate Office Address (Building, Street)")]
        [Required(ErrorMessage = "Address is required")]
        public string CorporateOfficeAddress1 { get; set; }

        [Display(Name = "Corporate Office Address (Area, City)")]
        [Required(ErrorMessage = "Address is required")]
        public string CorporateOfficeAddress2 { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please select country")]
        public string Country { get; set; }

        [Display(Name = "Zipcode")]
        //[Required(ErrorMessage = "Invalid zipcode")]
        //[MaxLength(5, ErrorMessage = "Must be a maximum of 5 characters")]
        //[Range(10101, 10513, ErrorMessage = "Invalid zipcode")]
        public string Pincode { get; set; }

        [Display(Name = "Board Of Director Email/Phone")]
        public string Directors { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string SignatoryEmail { get; set; }

        [Display(Name = "Organization Status")]
        public string Status { get; set; }

        public List<string> SignatoryEmailList { get; set; }

        //[Required]
        public List<OrganizationUser> OrganizationUsersList { get; set; }

        public List<string> DirectorsEmailList { get; set; }

        public IList<SignatureTemplatesDTO> TemplateList { get; set; }

        [Display(Name = "SPOC MyTrust Email")]
        [Required(ErrorMessage = "Invalid email address")]
        [DataType(DataType.EmailAddress)]
        public string SpocUgpassEmail { get; set; }

        [Display(Name = "Agent URL")]
        public string AgentUrl { get; set; }

        public bool EnablePostPaidOption { get; set; }

        [Display(Name = "Select signature template")]
        [Required(ErrorMessage = "Select signature template")]
        public int SignatureTemplate { get; set; }

        [Display(Name = "Select ESeal Template")]
        //[Required(ErrorMessage = "Select ESeal Template")]
        public int ESealTemplate { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(100 * 1024)] // 100kb
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        [Display(Name = "E-seal Image")]
        //[Required(ErrorMessage = "Please select a file")]
        public IFormFile ESealImage { get; set; }

        public string ResizedEsealImage { get; set; }

        public string ESealImageBase64 { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)] // 1 MB
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Authorized Letter for Signatories")]
        //[Required(ErrorMessage = "Please select a file")]
        public IFormFile AuthorizedLetterForSignatories { get; set; }

        public string AuthorizedLetterForSignatoriesBase64 { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Incorporation")]
        public IFormFile Incorporation { get; set; }

        public string IncorporationBase64 { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Tax")]
        public IFormFile Tax { get; set; }

        public string TaxBase64 { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Additional Legal Document")]
        public IFormFile AdditionalLegalDocument { get; set; }

        public string AdditionalLegalDocumentBase64 { get; set; }

        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [Display(Name = "Signed Pdf")]
        public IFormFile SignedPdf { get; set; }

        public string CreatedBy { get; set; }

        public string TransactionId { get; set; }

        public bool PaymentRecordPresent { get; set; }
        public string EsealCertificateStatusPresent { get; set; }
        [Display(Name = "Eseal Certificate Status")]
        public string certificateStatus { get; set; }
        public string certificateStartDate { get; set; }
        public string certificateEndDate { get; set; }

        public bool walletCertificateStatus { get; set; }
    }
}
