using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.KycDashboard
{
    public class IdValidationDashboardModal
    {
        public List<IdValidationResponseDTO> Reports { get; set; } = new List<IdValidationResponseDTO>();

        public KycSummaryDTO KycSummary { get; set; }
    }

}
