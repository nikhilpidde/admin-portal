using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IAllPaymentHistoryService
    {
        Task<IEnumerable<AllPaymentHistoryDTO>> GetAllPaymentHistoryAsync(AllPaymentHistoryDTO allPaymentHistory);
        Task<APIResponse> GetOrganizationPaymentHistoryAsync(string orgId);

        Task<APIResponse> GetWalletHistoryAsync(string orgId);
    }
}
