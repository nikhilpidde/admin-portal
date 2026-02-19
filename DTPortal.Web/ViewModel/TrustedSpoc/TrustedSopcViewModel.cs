using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DTPortal.Web.ViewModel.TrustedSpoc{
    public class TrustedSopcViewModel    
    {

        public int Id { get; set; }

        [Display(Name = "Spoc Name")]
        public string SpocName { get; set; }

        [Required(ErrorMessage = "The Spoc Email field is required.")]
        //[Display(Name = "Spoc MyTrust Email")]        
        public string SpocEmail { get; set; }

        [Display(Name = "Mobile No.")]
        public string MobileNo { get; set; }

        [Display(Name = "Document No.")]    
        public string IdDocumentNo { get; set; }
        public string FullMobileNo { get; set; }
        public string Status { get; set; }
        public bool reInvite { get; set; }

        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization Tin No.")]
        public string OrganizationTin { get; set; }

        [Display(Name = "CEO Tin No.")]
        public string CeoTin { get; set; }


        public string CountryCode { get; set; }
        public List<SelectListItem> CountryCodeOptions { get; set; }
    } 

}
