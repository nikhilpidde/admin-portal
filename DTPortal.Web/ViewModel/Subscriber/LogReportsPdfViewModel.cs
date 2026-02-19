using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Web.Enums;

namespace DTPortal.Web.ViewModel.Subscriber
{
    public class LogReportsPdfViewModel
    {
        public PaginatedList<LogReportDTO> LogReports { get; set; }

        public TransactionType TransactionType { get; set; }

        public string SubscriberFullName { get; set; }
    }
}
