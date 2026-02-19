using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.PriceSlabDefinition
{
    public class PriceSlabDefinitionListViewModel
    {
        public PriceSlabDefinitionListViewModel()
        {
            PriceSlabs = new List<PriceSlabViewModel>();
        }

        public IList<PriceSlabViewModel> PriceSlabs { get; set; }
    }

    public class PriceSlabViewModel
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string Stakeholder { get; set; }

        public string Status { get; set; }
    }
}
