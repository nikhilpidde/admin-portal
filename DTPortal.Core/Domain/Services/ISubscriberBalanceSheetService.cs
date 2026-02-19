using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ISubscriberBalanceSheetService
    {
        Task<IEnumerable<SubscriberBalanceSheetDTO>> GetSubscriberBalanceSheet(int type, string value);
    }
}
