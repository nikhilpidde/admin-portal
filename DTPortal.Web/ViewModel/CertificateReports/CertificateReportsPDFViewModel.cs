using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.CertificateReports
{
    public class CertificateReportsPDFViewModel
    {
        public IEnumerable<CertificateReportsDTO> CertificateReports { get; set; }
    }
}
