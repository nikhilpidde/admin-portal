using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.KycApplications
{
    public class KycApplicationsNewViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Application Name ")]
        [MaxLength(50)]
        public string ApplicationName { get; set; }
        [Required]

        [Display(Name = "Organization Name")]
        public string OrganizationId { get; set; }

        public List<SelectListItem> OrganizationList { get; set; }
    }
}
