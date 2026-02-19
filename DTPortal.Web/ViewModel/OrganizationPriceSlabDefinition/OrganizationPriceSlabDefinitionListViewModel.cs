using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition
{
    public class OrganizationPriceSlabDefinitionListViewModel
    {
        public OrganizationPriceSlabDefinitionListViewModel()
        {
            PriceSlabs = new List<PriceSlabViewModel>();
        }

        public IList<PriceSlabViewModel> PriceSlabs { get; set; }
    }

    public class PriceSlabViewModel
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string OrganizationUid { get; set; }

        public string OrganizationName { get; set; }

        public string Status { get; set; }
    }
}
