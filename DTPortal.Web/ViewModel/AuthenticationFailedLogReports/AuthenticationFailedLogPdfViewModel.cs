using DTPortal.Core.DTOs;
using DTPortal.Core;

namespace DTPortal.Web.ViewModel.AuthenticationFailedLogReports
{
    public class AuthenticationFailedLogPdfViewModel
    {
        public PaginatedList<LogReportDTO> AuthenticationFailedLogReports { get; set; }
    }
}
