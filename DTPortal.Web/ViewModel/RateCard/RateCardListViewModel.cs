using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.RateCard
{
    public class RateCardListViewModel
    {
        public IEnumerable<RateCardDTO> RateCards { get; set; }
    }
}
