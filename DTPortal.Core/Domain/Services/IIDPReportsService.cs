using System.Threading.Tasks;

using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IIDPReportsService
    {
        Task<PaginatedList<LogReportDTO>> GetReportsAsync(IDPSearchReportsDTO searchReportsDTO, int page = 1);
        bool VerifyChecksum(LogReportDTO logReport);
    }
}
