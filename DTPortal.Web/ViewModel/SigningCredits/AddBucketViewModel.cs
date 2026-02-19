using DTPortal.Core.Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SigningCredits
{
    public class AddBucketViewModel
    {
        [Display(Name = "Organization Name")]
        [Required]
        public string OrganizationId { get; set; }
        [Display(Name = "Application Name")]
        [Required]
        public string clientId { get; set; }
        [Display(Name = "Label")]
        [Required]
        public string label { get; set; }
        public string closingMessage { get; set; }
        public int AdditionalDs { get; set; }
        public int AdditionalEDs { get; set; }
        public string OrganizationName { get; set; }

        public List<SelectListItem> clientsList { get; set; }
        public List<SelectListItem> organizationList { get; set; }
    }
}