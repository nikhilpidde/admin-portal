using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.Beneficiary
{
    public class BeneficiaryEditViewModel
    {
            public int id { get; set; }

        public string orgName { get; set; }

        public int orgContactsEmailId { get; set; }

           // [Display(Name = "Beneficiary Office Email")]
            public string EmployeeEmail { get; set; }

            public string OrganizationUid { get; set; }


           /// [Display(Name = "Designation")]
            public string Designation { get; set; }

           // [Display(Name = "Signature Photo(Accepted max 20kb)")]
            public string SignaturePhoto { get; set; }

           /// [Display(Name = "Beneficiary MyTrust Email")]
            public string UgpassEmail { get; set; }


           [Display(Name = "Passport Number")]
            public string PassportNumber { get; set; }
            [MinLength(9)]
            [MaxLength(10)]
             [Display(Name = "National Id Number")]
            public string NationalIdNumber { get; set; }
        
            [Display(Name = "Mobile Number")]
            public string MobileNumber { get; set; }

            //[Display(Name = "Sponsor Type")]
            public string SponsorType { get; set; }

            //[Display(Name = "Beneficiary Type")]
            public string BeneficiaryType { get; set; }
            public string SponsorExternalId { get; set; }

            public string SubscriberUid { get; set; }
            public string BeneficiaryDigitalId { get; set; }
            public string BeneficiaryName { get; set; }
            public string Status { get; set; }
            public bool ConsentAcquired { get; set; }
            public string SponsorDigitalId { get; set; }
            public string CountryCode { get; set; }

            public IList<BeneficiaryPrivilegeDTO> Service { get; set; } = new List<BeneficiaryPrivilegeDTO>();

            public IList<BeneficiaryValidityDto> ServicePrevilage { get; set; } = new List<BeneficiaryValidityDto>();
            public List<int> ServiceIds { get; set; }
        }
    
}
