using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class ProvisionStatusController : Controller
    {
        private readonly IProvisionStatusService _provisionStatusService;
        public ProvisionStatusController(IProvisionStatusService provisionStatusService)
        {
            _provisionStatusService = provisionStatusService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProvisionStatus(string suid,string credentialId)
        {

            var response=await _provisionStatusService.GetProvisionStatus(suid, credentialId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddProvisionStatus([FromBody] ProvisionStatusDTO provisionStatusDTO)
        {
            var response = await _provisionStatusService.AddProvisionStatus(provisionStatusDTO.Suid, provisionStatusDTO.CredentialId,provisionStatusDTO.Status,provisionStatusDTO.DocumentId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }

        [HttpPost]
        public async Task<IActionResult> RevokeProvision([FromBody] ProvisionStatusDTO provisionStatusDTO)
        {

            var response = await _provisionStatusService.RevokeProvision(provisionStatusDTO.CredentialId,provisionStatusDTO.DocumentId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProvision([FromBody] ProvisionStatusDTO provisionStatusDTO)
        {

            var response = await _provisionStatusService.DeleteProvision(provisionStatusDTO.CredentialId,provisionStatusDTO.Suid);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
    }
}
