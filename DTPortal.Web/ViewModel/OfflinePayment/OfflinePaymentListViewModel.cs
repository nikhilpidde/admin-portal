using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OfflinePayment
{
    public class OfflinePaymentListViewModel
    {
        public IEnumerable<CreditAllocationListDTO> CreditAllocations { get; set; } = new List<CreditAllocationListDTO>();
    }
}
