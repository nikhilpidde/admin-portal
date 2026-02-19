using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.PriceSlabDefinition
{
    public class PriceSlabDefinitionEditViewModel
    {
        public PriceSlabDefinitionEditViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }

        [Required(ErrorMessage = "Select a service")]
        [Display(Name = "Service")]
        public int? ServiceId { get; set; }

        [Required(ErrorMessage = "Select Service")]
        [Display(Name = "Service")]
        public string ServiceDisplayName { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }

        [Required(ErrorMessage = "Select stakeholder")]
        [Display(Name = "Stakeholder")]
        public string Stakeholder { get; set; }

        public string CreatedBy { get; set; }

        public string Updatedby { get; set; }
    }
}
