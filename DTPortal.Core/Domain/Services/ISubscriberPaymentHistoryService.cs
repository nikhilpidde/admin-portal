using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ISubscriberPaymentHistoryService
    {
        Task<IEnumerable<SubscriberPaymentHistoryDTO>> GetSubscriberPaymentHistoryAsync(int type, string value);
    }
}
