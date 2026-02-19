using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition
{
    public class OrganizationPriceSlabDefinitionAddViewModel
    {
        public OrganizationPriceSlabDefinitionAddViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }
        
        [Required]
        public string OrganizationUid { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "Enter organization name")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Select a service")]
        [Display(Name = "Service")]
        public int? ServiceId { get; set; }

        public IEnumerable<ServiceDefinitionDTO> ServiceNames { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }
    }
}
