using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class OrganizationDetailsViewModel : BaseESealRegistrationViewModel
    {
        public int OrganizationId { get; set; }

        public string OrganizationUID { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "Please enter valid Organization name")]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization Email")]
        [Required(ErrorMessage = "Invalid email address")]
        public string OrganizationEmail { get; set; }

        [Display(Name = "Email Domain")]
        public string EmailDomain { get; set; }

        [Display(Name = "Unique Registered No")]
        [Required(ErrorMessage = "Please enter unique registered number")]
        public string UniqueRegdNo { get; set; }

        [Display(Name = "Tax Number (GST)")]
        [Required(ErrorMessage = "Please enter tax number(GST)")]
        public string TaxNo { get; set; }

        [Display(Name = "Corporate Office Address (Building, Area)")]
        [Required(ErrorMessage = "Address is required")]
        public string CorporateOfficeAddress1 { get; set; }

        [Display(Name = "Corporate Office Address (City, Country)")]
        [Required(ErrorMessage = "Address is required")]
        public string CorporateOfficeAddress2 { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please select country")]
        public string Country { get; set; }

        [Display(Name = "Pincode")]
        [Required(ErrorMessage = "Please enter pincode")]
        [MaxLength(6, ErrorMessage = "Must be a maximum of 6 characters")]
        public string Pincode { get; set; }

        [Required]
        public IList<OrganizationUser> OrganizationUsersList { get; set; }

        public IList<string> DirectorsEmailList { get; set; }

        public IList<SignatureTemplatesDTO> TemplateList { get; set; }

        //[Display(Name = "SPOC MyTrust Email")]
        [Required(ErrorMessage = "Invalid email address")]
        [DataType(DataType.EmailAddress)]
        public string SpocUgpassEmail { get; set; }

        [Display(Name = "Agent Url")]
        public string AgentUrl { get; set; }

        public bool EnablePostPaidOption { get; set; }


        [Display(Name = "Select signature template")]
        [Required(ErrorMessage = "Select signature template")]
        public int SignatureTemplate { get; set; }

        [Display(Name = "Select ESeal Template")]
        //[Required(ErrorMessage = "Select ESeal Template")]
        public int ESealTemplate { get; set; }

        public string AuthorizedLetterForSignatories { get; set; }

        public string ESealImage { get; set; }

        public string Incorporation { get; set; }

        public string Tax { get; set; }

        public string AdditionalLegalDocument { get; set; }

        public string SignedPdf { get; set; }

        public string Status { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }
}
