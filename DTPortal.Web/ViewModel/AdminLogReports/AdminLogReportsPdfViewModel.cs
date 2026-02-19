using DTPortal.Core;
using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.AdminLogReports
{
    public class AdminLogReportsPdfViewModel
    {
        public PaginatedList<AdminLogReportDTO> AdminLogReports { get; set; }
    }
}
