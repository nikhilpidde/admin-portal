using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.EConsent
{
    public class EConsentClientEditViewModel
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ApplicationName { get; set; }

        public string Scopes { get; set; }

        [Required(ErrorMessage = "Select atleast one Scope.")]
        [Display(Name = "Scopes ")]
        public IEnumerable<string> ScopesList { get; set; }

        [Display(Name = "Signing Certificate (.crt,.cer only)")]
        public IFormFile PublicKeyCert { get; set; }

        public bool IsFileUploaded { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public string Status { get; set; }

        public string Purposes { get; set; }
        [Required(ErrorMessage = "Select atleast one Purpose.")]
        [Display(Name = "Purposes ")]
        public IEnumerable<string> PurposesList { get; set; }

        [Display(Name = "Orgnization Name")]
        public string OrganizationId { get; set; }

        public List<SelectListItem> OrganizationList { get; set; }
    }
}
