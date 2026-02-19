using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.AllPaymentHistory
{
    public class AllPaymentHistoryPdfViewModel
    {
        public IEnumerable<AllPaymentHistoryDTO> AllPaymentHistory { get; set; }
    }
}
