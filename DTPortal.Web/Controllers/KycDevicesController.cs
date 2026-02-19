using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KycDevicesController : ControllerBase
    {
        private readonly IKycDevicesService _kycDevicesService;
        public KycDevicesController(IKycDevicesService kycDevicesService)
        {
            _kycDevicesService = kycDevicesService;
        }

        [HttpGet]
        [Route("GetKycDeviceStatus/{deviceId}")]
        public async Task<IActionResult> GetKycDeviceStatus(string deviceId)
        {
            var response = await _kycDevicesService.GetKycDeviceStatus(deviceId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("RegisterKycDevice")]
        public async Task<IActionResult> RegisterKycDevice(RegisterKycDeviceDTO kycDevice)
        {
            var response = await _kycDevicesService.RegisterKycDevice(kycDevice);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }


        [HttpPost]
        [Route("DeregisterKycDevice")]
        public async Task<IActionResult> DeregisterKycDevice(RegisterKycDeviceDTO kycDevice)
        {
            var response = await _kycDevicesService.DeregisterKycDevice(kycDevice.DeviceId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }


        [HttpGet]
        [Route("GetOrganizationKycDevicesList/{orgId}")]
        public async Task<IActionResult> GetOrganizationKycDevicesList(string orgId)
        {
            var response = await _kycDevicesService.GetAllKycDeviceOfOrganization(orgId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetOrganizationKycDevicesCount/{orgId}")]
        public async Task<IActionResult> GetOrganizationKycDevicesCount(string orgId)
        {
            var response = await _kycDevicesService.GetAllKycDevicesCountByOrganization(orgId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }
    }
}
