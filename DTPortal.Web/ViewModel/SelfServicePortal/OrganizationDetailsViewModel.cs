using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class OrganizationDetailsViewModel
    {
        public int OrgOnboardingFormsId { get; set; }
        public string OrgUid { get; set; }

        [Display(Name = "Organization Registration ID Number")]
        public string OrgRegIdNumber { get; set; }

        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization Contact Number")]
        public string OrgOfficialContactNumber { get; set; }

        [Display(Name = "Corporate Office Address (Building, Street)")]
        public string CorporateOfficeAddress1 { get; set; }

        [Display(Name = "Corporate Office Address (Area, City)")]
        public string CorporateOfficeAddress2 { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Post Address")]
        public string Pincode { get; set; }

        [Display(Name = "Organization Website URL")]
        public string OrgWebUrl { get; set; }

        [Display(Name = "Organization Tin Number")]
        public string OrgTanTaxNumber { get; set; }

        [Display(Name = "User Certificate")]
        public string UrsbCertificate { get; set; }

        [Display(Name = "Approval Letter")]
        public string ApprovalLetter { get; set; }

        [Display(Name = "Form Status")]
        public string ObFormStatus { get; set; }

        [Display(Name = "Status")]
        public string OrgApprovalStatus { get; set; }

        [Display(Name = "OTP Verification")]
        public string OtpVerification { get; set; }

        [Display(Name = "Rejected Reason")]
        public string RejectedReason { get; set; }

        [Display(Name = "Sign Approve by Staff")]
        public string SignApprByBrmStaff { get; set; }

        [Display(Name = "Organization Category")]
        public string OrgCategory { get; set; }

        [Display(Name = "SPOC Suid")]
        public string SpocSuid { get; set; }

        [Display(Name = "Created On")]
        public string CreatedOn { get; set; }

        [Display(Name = "Updated On")]
        public string UpdatedOn { get; set; }


        [Display(Name = "Auditor Name")]
        public string FinancialAuditorName { get; set; }
        
        //[Display(Name = "Auditor MyTrust Email")]
        public string AuditorUgPassEmail { get; set; }
        [Display(Name = "Auditor ID Document Number")]
        public string FinancialAuditorNin { get; set; }
        [Display(Name = "Auditor ID Document Number")]
        public string FinancialAuditorIdDocNumber { get; set; }
        [Display(Name = "Auditor License Number")]
        public string FinancialAuditorLicenseNum { get; set; }
        //public string createdOn { get; set; }
        //public string updatedOn { get; set; }
        public int OrgFinancialAuditorDetailsId { get; set; }

        public int OrgSpocDetailsId { get; set; }
        [Display(Name = "SPOC Official Email")]
        public string SpocOfficeEmail { get; set; }
        [Display(Name = "SPOC Name")]
        public string SpocName { get; set; }
        
        //[Display(Name = "SPOC MyTrust Email")]
        public string SpocUgpassEmail { get; set; }
        
        //[Display(Name = "SPOC MyTrust Mobile Number")]
        public string SpocUgpassMobNum { get; set; }
        [Display(Name = "SPOC Id Document Number")]
        public string SpocIdDocNo { get; set; }
        //[Display(Name = "SPOC Id Document Number")]
        //public string SpocPassport { get; set; }
        [Display(Name = "SPOC Social Security Number")]
        public string SpocSocialSecurityNum { get; set; }
        [Display(Name = "SPOC TIN Number")]
        public string SpocTaxNum { get; set; }
        public string SpocOtpVerfyStatus { get; set; }
        public bool SpocFaceMatchStatus { get; set; }
        public string orgUid { get; set; }
        public string spocSuid { get; set; }
        public string SpocFaceCaptured { get; set; }
        public string SpocRAFaceCaptured { get; set; }
        public string SpocFaceFromUgpass { get; set; }
        public int OrgCeoDetailsiId { get; set; }
        [Display(Name = "CEO Tax Number")]
        public string CeoPanTaxNum { get; set; }

        [Display(Name = "CEO Name")]
        public string CeoName { get; set; }

        //[Display(Name = "CEO MyTrust Email")]
        public string CeoEmail { get; set; }
        [Display(Name = "Auditor TIN Number ")]
        public string financialAuditorTinNumber { get; set; }
        [Display(Name = "CEO ID Document Number")]
        public string ceoIdDocumentNumber { get; set; }
        [Display(Name = "Auditor Ura Pdf")]
        public string auditorUraPdf { get; set; }
        [Display(Name = "Spoc Ura Pdf")]
        public string spocUraPdf { get; set; }
        [Display(Name = "CEO Ura Pdf")]
        public string ceoUraPdf { get; set; }
        [Display(Name = "Org Ura Pdf")]
        public string orgUraPdf { get; set; }


    }
}
