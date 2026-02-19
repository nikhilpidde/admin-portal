using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel.PrivilegeRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DTPortal.Web.Controllers
{
    [Authorize]
    [Route("PrivilegeRequest")]
    public class PrivilegeRequestController : BaseController
    {
        private readonly IPrivilegeRequestService _privilegeRequestService;
        public PrivilegeRequestController(IPrivilegeRequestService privilegeRequestService, ILogClient logClient) : base(logClient)
        {
            _privilegeRequestService = privilegeRequestService;
        }


        public async Task<IActionResult> Index()
        {
            var response = await _privilegeRequestService.GetAllPrivilegesAsync();
            if (response == null || !response.Success)
            {
                return NotFound();
            }

            var response1 = (List<PreviligeDetails>)response.Resource;
            List<PrivilegeRequestItemViewModel> privilegesList = response1.Select(p => new PrivilegeRequestItemViewModel
            {
                id = p.id,
                organizationId = p.organizationId,
                organizationName = p.organizationName,
                privilege = p.privilege,
                createdBy = p.createdBy,
                status = p.status,
                modifiedBy = p.modifiedBy

            }).ToList();
            return View(privilegesList);
        }

        [HttpPost("ApproveRequest")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var response = await _privilegeRequestService.GetPrivilegeByIdAsync(id);
            if (response == null || !response.Success)
            {
                return NotFound(new { message = "Privilege request not found." });
            }
            var currentPrivilegeRequestModel = (PreviligeDetails)response.Resource;
            UpdatePrivilegeDTO updatePrivilegeModel = new UpdatePrivilegeDTO
            {
                id = currentPrivilegeRequestModel.id,
                orgId = currentPrivilegeRequestModel.organizationId,
                privilege = currentPrivilegeRequestModel.privilege,
                status = "APPROVED",
                adminName = FullName

            };
            var response1 = await _privilegeRequestService.UpdatePrivilegeAsync(updatePrivilegeModel);
            if (response1 == null || !response1.Success)
            {
                return BadRequest(new { message = "Failed to update privilege request." });
            }
            return Ok(new { message = "Privilege request approved successfully." });
        }


        [HttpPost("RejectRequest")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var response = await _privilegeRequestService.GetPrivilegeByIdAsync(id);
            if (response == null || !response.Success)
            {
                return NotFound(new { message = "Privilege request not found." });
            }
            var currentPrivilegeRequestModel = (PreviligeDetails)response.Resource;
            UpdatePrivilegeDTO updatePrivilegeModel = new UpdatePrivilegeDTO
            {
                id = currentPrivilegeRequestModel.id,
                orgId = currentPrivilegeRequestModel.organizationId,
                privilege = currentPrivilegeRequestModel.privilege,
                status = "REJECTED",
                adminName = FullName

            };
            var response1 = await _privilegeRequestService.UpdatePrivilegeAsync(updatePrivilegeModel);
            if (response1 == null || !response1.Success)
            {
                return BadRequest(new { message = "Failed to update privilege request." });
            }
            return Ok(new { message = "Privilege request approved successfully." });
        }


        [HttpPost("SuspendRequest")]
        public async Task<IActionResult> SuspendRequest(int id)
        {
            var response = await _privilegeRequestService.GetPrivilegeByIdAsync(id);
            if (response == null || !response.Success)
            {
                return NotFound(new { message = "Privilege request not found." });
            }
            var currentPrivilegeRequestModel = (PreviligeDetails)response.Resource;
            UpdatePrivilegeDTO updatePrivilegeModel = new UpdatePrivilegeDTO
            {
                id = currentPrivilegeRequestModel.id,
                orgId = currentPrivilegeRequestModel.organizationId,
                privilege = currentPrivilegeRequestModel.privilege,
                status = "SUSPENDED",
                adminName = FullName

            };
            var response1 = await _privilegeRequestService.UpdatePrivilegeAsync(updatePrivilegeModel);
            if (response1 == null || !response1.Success)
            {
                return BadRequest(new { message = "Failed to update privilege request." });
            }
            return Ok(new { message = "Privilege request approved successfully." });
        }




    }
}
