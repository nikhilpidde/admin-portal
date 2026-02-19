using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.KycApplications
{
    public class KycApplicationsEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Application Name ")]
        [MaxLength(50)]
        public string ApplicationName { get; set; }

        [Required]
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }

        [Required]
        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        public string Status { get; set; }
        [Required]
        [Display(Name = "Organization Name")]
        public string OrganizationId { get; set; }

        public List<SelectListItem> OrganizationList { get; set; }
    }
}
