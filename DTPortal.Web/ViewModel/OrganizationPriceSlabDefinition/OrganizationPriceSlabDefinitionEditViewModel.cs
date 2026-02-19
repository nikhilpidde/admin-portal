using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition
{
    public class OrganizationPriceSlabDefinitionEditViewModel
    {
        public OrganizationPriceSlabDefinitionEditViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }

        [Required(ErrorMessage = "Select a service")]
        [Display(Name = "Service")]
        public int? ServiceId { get; set; }

        [Required(ErrorMessage = "Select Service")]
        [Display(Name = "Service")]
        public string ServiceDisplayName { get; set; }

        [Display(Name = "Organization")]
        public string OrganizationName { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }

        public string OrganizationUID { get; set; }

        public string CreatedBy { get; set; }

        public string Updatedby { get; set; }
    }
}
