using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.PriceSlabDefinition
{
    public class PriceSlabDefinitionAddViewModel
    {
        public PriceSlabDefinitionAddViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }

        [Required(ErrorMessage = "Select a service")]
        [Display(Name = "Service")]
        public int? ServiceId { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }

        [Required(ErrorMessage = "Select stakeholder")]
        [Display(Name = "Stakeholder")]
        public UserType? Stakeholder { get; set; }
    }
}
