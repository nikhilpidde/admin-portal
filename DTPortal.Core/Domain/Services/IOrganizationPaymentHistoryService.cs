using System.Collections.Generic;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IOrganizationPaymentHistoryService
    {
        Task<PaginatedList<OrganizationPaymentHistoryDTO>> GetServiceProviderPaymentHistoryAsync(string uid, int page = 1, int count = 2);

        Task<PaginatedList<SubscriberPaymentHistoryDTO>> GetSubscriberPaymentHistoryAsync(string uid, int page = 1, int count = 2);

        
        
        Task<IEnumerable<OrganizationPaymentHistoryDTO>> GetOrganizationPaymentHistoryAsync(string data);

        Task<ServiceResult> AddOrganizationPaymentHistoryAsync(OrganizationPaymentHistoryDTO organizationPaymentHistory, bool makerCheckerFlag = false);
    }
}
