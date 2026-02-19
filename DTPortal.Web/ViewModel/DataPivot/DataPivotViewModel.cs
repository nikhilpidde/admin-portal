using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.DataPivot
{
    public class DataPivotViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is Required")]

        public string Name { get; set; }
        [Required(ErrorMessage = "Display Name is Required")]
        public string DisplayName { get; set; }
        public string OrgnizationId { get; set; }

        public string Scopes {  get; set; }

        public List<SelectListItem> OrganizatioList { get; set; }

        public List<SelectListItem> ScopesList { get; set; }

        public List<SelectListItem> CategoryList { get; set; }

        public string Category { get; set; }

        [Required]
        [Display(Name = "Authentication scheme ")]
        public string AuthScheme { get; set; }

        public List<SelectListItem> AuthSchemeList { get; set; } 

        public string AttributeConfiguration { get; set; }

        [Display(Name = "Signing Certificate (.crt,.cer only)")]
        public IFormFile Cert { get; set; }

        public bool IsFileUploaded { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Status { get; set; }

        [Required(ErrorMessage = "Service url is Required")]
        public string Serviceurl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public IFormFile DataPivotLogo { get; set; }

        public string ResizedDataPivotLogo { get; set; }
        
        public string AllowedSubscriberTypes { get; set; }
        public string DataPivotImage { get; set; }


    }
    public class ServiceConfiguration
    {
        public string Serviceurl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }


    }
}
