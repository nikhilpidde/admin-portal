using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IOrganizationUsageReportService
    {
        Task<IEnumerable<OrganizationUsageReportDTO>> GetOrganizationUsageReports(string organizationUid, string year);

        Task<string> DownloadUsageReport(int reportId);

        Task<ServiceResult> DownloadCurrentMonthUsageReport(string organizationUid);
    }
}
