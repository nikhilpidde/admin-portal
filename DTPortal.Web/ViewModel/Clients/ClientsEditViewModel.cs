using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Clients
{
    public class ClientsEditViewModel
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Application Type ")]
        public string ApplicationType { get; set; }

        public List<SelectListItem> ApplcationTypeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Native", Text = "Native" },
            new SelectListItem { Value = "Single Page Application", Text = "Single Page Application" },
            new SelectListItem { Value = "Regular Web Application", Text = "Regular Web Application"  },
            new SelectListItem { Value = "Machine to Machine Application", Text = "Machine to Machine Application"  },
        };

        [Required]
        [Display(Name = "Application Name ")]
        [MaxLength(50)]
        public string ApplicationName { get; set; }

       
        [Display(Name = "Application Url")]
        //[Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Required]
        public string ApplicationUri { get; set; }

        [Required]
        //[Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Display(Name = "Redirect Url ")]
        public string RedirectUri { get; set; }

        public string GrantTypes { get; set; }

        [Required(ErrorMessage = "Select atleast one Authorization Flow.")]
        [Display(Name = "Authorization Flow ")]
        public IEnumerable<string> GrantTypesList { get; set; }

        public string Scopes { get; set; }

        [Required(ErrorMessage = "Select atleast one Scope.")]
        [Display(Name = "Scopes ")]
        //public string[] Scopes { get; set; }
        public IEnumerable<string> ScopesList { get; set; }


        // [Url]
        [RegularExpression(@"^(((http(s)?://)((((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|localhost|((((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z]+)+[.])?((?![-_0-9])[A-Za-z0-9]+[-_]?[A-Za-z0-9]+)+([a-zA-Z]{0,30}[-_]?[a-zA-Z]{0,30})?([.][a-zA-Z]{2,30}[-_]?[a-zA-Z]{0,30}){0,10}?))(:([1-9]|[1-5]?[0-9]{2,4}|6[1-4][0-9]{3}|65[1-4][0-9]{2}|655[1-2][0-9]|6553[1-5]))?(([/][a-zA-Z0-9._-]+)*[/]?)?)|((?![-_0-9])([a-zA-Z0-9_-]+)(.[a-zA-Z0-9_-]+)+?)(://)(([a-zA-Z0-9-_/=]+)))$", ErrorMessage = "Invalid Url")]
        [Display(Name = "Logout Url")]
        public string LogoutUri { get; set; }

        [Required]
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        
        public bool WithPkce { get; set; }

        public string State { get; set; }

       // [Required]
        [Display(Name = "Orgnization Name")]
        public string OrganizationId { get; set; }

        public List<SelectListItem> OrganizatioList { get; set; }


        [Display(Name = "Signing Certificate (.crt,.cer only)")]
        public IFormFile Cert { get; set; }
        public string Profiles { get; set; }
        public string Purposes { get; set; }
        //[Display(Name = "Consent Services Required")]
        //public bool ConsentRequired { get; set; }

        public bool IsFileUploaded { get; set; }

        [Required]
        [Display(Name = "Authemtication Schema")]
        public string AuthSchemaId { get; set; }

        public List<SelectListItem> AuthSchemasList { get; set; }
    }
}
