using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition
{
    public class OrganizationPriceSlabDefinitionDetailsViewModel
    {
        public OrganizationPriceSlabDefinitionDetailsViewModel()
        {
            DiscountVolumeRanges = new List<DiscountVolumeRangeDTO>();
        }
        public string ServiceDisplayName { get; set; }

        public string OrganizationName { get; set; }

        public IList<DiscountVolumeRangeDTO> DiscountVolumeRanges { get; set; }

        public string CreatedBy { get; set; }

        public string Updatedby { get; set; }
    }
}
