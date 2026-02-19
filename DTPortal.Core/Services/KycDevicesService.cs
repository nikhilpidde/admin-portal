using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class KycDevicesService : IKycDevicesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<KycDevicesService> _logger;

        public KycDevicesService(IUnitOfWork unitOfWork, ILogger<KycDevicesService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult> IsKycDeviceActive(string deviceId)
        {
            try
            {
                var kycMethods = await _unitOfWork.KycDevices.GetKycDeviceById(deviceId);
                if(kycMethods == null)
                {
                    return new ServiceResult(false, "KYC device not found");
                }
                if(kycMethods.Status == "ACTIVE")
                {
                    return new ServiceResult(true, "KYC device is active", kycMethods.Status);
                }
                else
                {
                    return new ServiceResult(false, "KYC device is not active");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetKycDeviceStatus(string deviceId)
        {
            try
            {
                var kycDevice = await _unitOfWork.KycDevices.GetKycDeviceById(deviceId);
                if (kycDevice == null)
                {
                    return new ServiceResult(false, "KYC device not found");
                }
                return new ServiceResult(true, "KYC device found", kycDevice.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> RegisterKycDevice(RegisterKycDeviceDTO kycDeviceDto)
        {
            try
            {
                if (kycDeviceDto == null)
                {
                    return new ServiceResult(false, "KYC device data is null");
                }

                var isDeviceRegistered = await _unitOfWork.KycDevices.IsKycDeviceAlreadyRegistered(kycDeviceDto.DeviceId);
                if (isDeviceRegistered == true)
                {
                    return new ServiceResult(true, "Device already registered");
                }

                var kycDevice = new KycDevice
                {
                    DeviceId = kycDeviceDto.DeviceId,
                    OrganizationId = kycDeviceDto.OrganizationId,
                    ClientId = kycDeviceDto.ClientId,
                    Status = "ACTIVE"
                };
                await _unitOfWork.KycDevices.AddAsync(kycDevice);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "KYC device created successfully", kycDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating KYC Device");
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> DeregisterKycDevice(string deviceId)
        {
            try
            {
                var kycDevice = await _unitOfWork.KycDevices.GetKycDeviceById(deviceId);
                if (kycDevice == null)
                {
                    return new ServiceResult(false, "KYC device not found");
                }

                kycDevice.Status = "DEACTIVE";
                _unitOfWork.KycDevices.Update(kycDevice);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Deactivated Successfully", kycDevice.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetAllKycDeviceOfOrganization(string orgId)
        {
            try
            {
                var kycDevice = await _unitOfWork.KycDevices.ListOfkycDeviceByOrganization(orgId);
                if (kycDevice == null)
                {
                    return new ServiceResult(false, "KYC devices data not found");
                }

                var deviceIdList = kycDevice
                                    .Where(d => !string.IsNullOrEmpty(d.DeviceId))
                                    .Select(d => d.DeviceId)
                                    .ToList();

                return new ServiceResult(true, "KYC devices found", deviceIdList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetAllKycDevicesCount()
        {
            try
            {
                var kycDevice = await _unitOfWork.KycDevices.GetAllAsync();
                if (kycDevice == null)
                {
                    return new ServiceResult(false, "KYC devices data not found", 0);
                }

                return new ServiceResult(true, "Count of KYC devices", kycDevice.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message,0);
            }
        }

        public async Task<ServiceResult> GetAllKycDevicesCountByOrganization(string orgId)
        {
            try
            {
                var kycDevice = await _unitOfWork.KycDevices.ListOfkycDeviceByOrganization(orgId);
                if (kycDevice == null)
                {
                    return new ServiceResult(false, "KYC devices data not found", 0);
                }

                return new ServiceResult(true, "Count of KYC devices", kycDevice.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC Device Data");
                return new ServiceResult(false, ex.Message, 0);
            }
        }
    }
}
