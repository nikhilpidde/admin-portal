using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.OrganizationBalanceSheet
{
    public class OrganizationBalanceSheetSearchViewModel
    {
        //public OrganizationBalanceSheetSearchViewModel()
        //{
        //    Services = new List<ServiceDefinitionDTO>();
        //}

        //public IEnumerable<ServiceDefinitionDTO> Services { get; set; }

        //[Display(Name = "Service Name")]
        //[Required(ErrorMessage = "Select service name")]
        //public int? ServiceId { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "Enter organization name")]
        public string OrganizationName { get; set; }

        [Required]
        public string OrganizationId { get; set; }

        public IEnumerable<OrganizationBalanceSheetDTO> BalanceSheetDetails { get; set; } 
    }
}
