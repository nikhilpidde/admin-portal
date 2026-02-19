using System.Threading.Tasks;

using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IAccountBalanceService
    {
        Task<ServiceProviderAccountBalanceDTO> GetServiceProviderAccountBalanceAsync(string serviceProviderUID);

        Task<SubscriberAccountBalanceDTO> GetSubscriberAccountBalanceAsync(string subscriberUID);
    }
}
