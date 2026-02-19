using DTPortal.Core.DTOs;
using DTPortal.Core;

namespace DTPortal.Web.ViewModel.SigningFailedLogReports
{
    public class SigningFailedLogReportsPdfViewModel
    {
        public PaginatedList<LogReportDTO> SigningFailedLogReports { get; set; }
    }
}
