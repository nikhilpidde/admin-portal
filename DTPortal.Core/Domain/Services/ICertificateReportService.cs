using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ICertificateReportService
    {
        Task<IEnumerable<CertificateReportsDTO>> GetCertificateReportsAsync(string startDate, string endDate);
    }
}
