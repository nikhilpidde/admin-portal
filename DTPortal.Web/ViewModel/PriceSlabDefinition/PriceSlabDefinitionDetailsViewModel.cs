using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.PriceSlabDefinition
{
    public class PriceSlabDefinitionDetailsViewModel
    {
        public PriceSlabDefinitionDetailsViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }

        public string ServiceDisplayName { get; set; }

        public string Stakeholder { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }

        public string CreatedBy { get; set; }

        public string Updatedby { get; set; }
    }
}
