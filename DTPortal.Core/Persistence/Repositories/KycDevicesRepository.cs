using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class KycDevicesRepository : GenericRepository<KycDevice, idp_dtplatformContext>, IKycDevicesRepository
    {
        private readonly ILogger _logger;
        public KycDevicesRepository(idp_dtplatformContext context, ILogger logger) :
            base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<KycDevice>> ListOfkycDeviceByOrganization(string orgId)
        {
            try
            {
                return await Context.KycDevices
                    .Where(u => u.Status == "ACTIVE" && u.OrganizationId == orgId).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("ListOfkycDeviceByOrganization::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<KycDevice> GetKycDeviceById(string deviceId)
        {
            try
            {
                return await Context.KycDevices
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            }
            catch (Exception error)
            {
                _logger.LogError("GetKycDeviceById::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<bool> IsKycDeviceAlreadyRegistered(string deviceId)
        {
            try
            {
                return await Context.KycDevices
                    .AnyAsync(d => d.DeviceId == deviceId);
            }
            catch (Exception error)
            {
                _logger.LogError("GetKycDeviceById::Database exception: {0}", error);
                return false;
            }
        }
    }
}
