using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IOfflinePaymentService
    {
        Task<ServiceResult> GetOfflineCredits(CreditAllocationListDTO creditAllocationDTO, bool makerCheckerFlag = false);

        Task<IEnumerable<CreditAllocationListDTO>> GetAllOfflinePaymentListAsync();

        Task<CreditAllocationListDTO> GetOfflinePaymentDetailsAsync(int id);
    }
}
