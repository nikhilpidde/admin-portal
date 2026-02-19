using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IKycDevicesService
    {
        Task<ServiceResult> GetKycDeviceStatus(string deviceId);
        Task<ServiceResult> RegisterKycDevice(RegisterKycDeviceDTO kycDeviceDto);
        Task<ServiceResult> IsKycDeviceActive(string deviceId);
        Task<ServiceResult> DeregisterKycDevice(string deviceId);
        Task<ServiceResult> GetAllKycDeviceOfOrganization(string orgId);
        Task<ServiceResult> GetAllKycDevicesCount();
        Task<ServiceResult> GetAllKycDevicesCountByOrganization(string orgId);
    }
}
