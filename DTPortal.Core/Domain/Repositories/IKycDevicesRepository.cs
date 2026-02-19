using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IKycDevicesRepository : IGenericRepository<KycDevice>
    {
        Task<IEnumerable<KycDevice>> ListOfkycDeviceByOrganization(string orgId);
        Task<KycDevice> GetKycDeviceById(string deviceId);
        Task<bool> IsKycDeviceAlreadyRegistered(string deviceId);
    }
}
