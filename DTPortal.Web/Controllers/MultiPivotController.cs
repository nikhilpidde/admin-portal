using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultiPivotController : BaseController
    {
        private readonly IDataPivotService _dataPivotService;
        private readonly IScopeService _scopeService;
        private readonly IPurposeService _purposeService;
        private readonly IUserClaimService _userClaimService;
        private readonly IAttributeServiceTransactionsService _attributeServiceTransactionsService;
        private readonly ICategoryService _categoryService;
        public MultiPivotController(ILogClient logClient,
            IPurposeService purposeService,
            IDataPivotService dataPivotService,
            IScopeService scopeService,
            IUserClaimService userClaimService,
            IAttributeServiceTransactionsService attributeServiceTransactionsService,
            ICategoryService categoryService) : base(logClient)
        {
            _purposeService = purposeService;
            _dataPivotService = dataPivotService;
            _scopeService = scopeService;
            _userClaimService = userClaimService;
            _attributeServiceTransactionsService = attributeServiceTransactionsService;
            _categoryService = categoryService;
        }
        [Route("AddDataPivot")]
        [HttpPost]
        public async Task<IActionResult> AddDataPivot([FromBody] DataPivot dataPivot)
        {
            APIResponse response = new APIResponse();
            if (dataPivot == null)
            {
                return BadRequest();
            }
            var response1 = await _dataPivotService.CreatePivotDataAsync(dataPivot);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
                return Ok(response);
            }
            response.Success = response1.Success;
            response.Message = response1.Message;
            return Ok(response);
        }
        [Route("UpdateDataPivot")]
        [HttpPost]
        public async Task<IActionResult> UpdateDataPivot([FromBody] DataPivot dataPivot)
        {
            APIResponse response = new APIResponse();
            if (dataPivot == null)
            {
                return BadRequest();
            }
            var response1 = await _dataPivotService.UpdatePivotDataAsync(dataPivot);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
                return Ok(response);
            }
            response.Success = response1.Success;
            response.Message = response1.Message;
            return Ok(response);
        }
        [Route("GetDataPivotList/{OrgId}")]
        [HttpGet]
        public async Task<IActionResult> GetDataPivotList(string OrgId)
        {
            APIResponse response = new APIResponse();
            try
            {
                if (OrgId == null)
                {
                    return BadRequest();
                }
                var response1 = await _dataPivotService.GetAllPivotDataByOrgIdAsync(OrgId);
                if (response1 == null)
                {
                    response.Success = false;
                    response.Message = "Internal Error";
                    return Ok(response);
                }
                response.Success = true;
                response.Message = "get List Success";
                response.Result = response1;
                return Ok(response);
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "Internal Error";
                return Ok(response);
            }
        }
        [Route("EditDataPivot/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditDataPivot(int id)
        {
            APIResponse response = new APIResponse();
            try
            {
                var response1 = await _dataPivotService.GetPivotAsync(id);
                if (response1 == null)
                {
                    response.Success = false;
                    response.Message = "Internal Error";
                    return Ok(response);
                }
                response.Success = true;
                response.Message = "Get DataPivot Success";
                response.Result = response1;
                return Ok(response);
            }
            catch (Exception )
            {
                response.Success = false;
                response.Message = "Internal Error";
                return Ok(response);
            }
        }
        [Route("AddProfile")]
        [HttpPost]
        public async Task<IActionResult> AddProfile([FromBody] Scope scope)
        {
            APIResponse response = new APIResponse();
            var response1 = await _scopeService.CreateScopeAsync(scope);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("UpdateProfile")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] Scope scope)
        {
            APIResponse response = new APIResponse();
            var response1 = await _scopeService.UpdateScopeAsync(scope);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("GetProfileList")]
        [HttpGet]
        public async Task<IActionResult> GetProfileList()
        {
            APIResponse response = new APIResponse();
            var response1 = await _scopeService.ListScopeAsync();
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get List Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("EditProfile/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditProfile(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _scopeService.GetScopeAsync(id);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get List Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("AddPurpose")]
        [HttpPost]
        public async Task<IActionResult> AddPurpose([FromBody] Purpose purpose)
        {
            APIResponse response = new APIResponse();
            var response1 = await _purposeService.CreatePurposeAsync(purpose);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
        [Route("UpdatePurpose")]
        [HttpPost]
        public async Task<IActionResult> UpdatePurpose([FromBody] Purpose purpose)
        {
            APIResponse response = new APIResponse();
            var response1 = await _purposeService.UpdatePurposeAsync(purpose);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
        [Route("GetPurposeList")]
        [HttpGet]
        public async Task<IActionResult> GetPurposeList()
        {
            APIResponse response = new APIResponse();
            var response1 = await _purposeService.GetPurposeListAsync();
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "Get List Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("EditPurpose/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditPurpose(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _purposeService.GetPurposeAsync(id);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "Get Purpose Success";
                response.Result = response1;
            }
            return Ok(response);
        }

        [Route("AddAttribute")]
        [HttpPost]
        public async Task<IActionResult> AddAttribute([FromBody] UserClaim userClaim)
        {
            APIResponse response = new APIResponse();
            var response1 = await _userClaimService.CreateUserClaimAsync(userClaim);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = response1.Message;
            }
            return Ok(response);
        }
        [Route("UpdateAttribute")]
        [HttpPost]
        public async Task<IActionResult> UpdateAttribute([FromBody] UserClaim userClaim)
        {
            APIResponse response = new APIResponse();
            var response1 = await _userClaimService.UpdateUserClaimAsync(userClaim);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = response1.Message;
            }
            return Ok(response);
        }
        [Route("GetAttributeList")]
        [HttpGet]
        public async Task<IActionResult> GetAttributeList()
        {
            APIResponse response = new APIResponse();
            var response1 = await _userClaimService.ListUserClaimAsync();
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get List Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("EditAttribute/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditAttribute(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _userClaimService.GetUserClaimAsync(id);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get List Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("GetTransactionList/{OrgId}")]
        [HttpGet]
        public async Task<IActionResult> GetTransactionList(string OrgId)
        {
            APIResponse response = new APIResponse();
            var response1=await _attributeServiceTransactionsService.GetAttributeServiceTransactionsListByOrgId(OrgId);
            if(response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get List Success";
                response.Result= response1;
            }
            return Ok(response);
        }
        [Route("GetTransactionDetails/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetTransactionDetails(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _attributeServiceTransactionsService.GetDetails(id);
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = true;
                response.Message = "get Details Success";
                response.Result = response1;
            }
            return Ok(response);
        }
        [Route("DeleteDataPivot/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeleteDataPivot(int id)
        {
            APIResponse response=new APIResponse();
            var response1 = await _dataPivotService.DeleteDatapivotAsync(id, "");
            if(response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message= response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
        [Route("DeleteProfile/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _scopeService.DeleteScopeAsync(id, "");
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
        [Route("DeleteAttribute/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _userClaimService.DeleteUserClaimAsync(id, "");
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
        [Route("DeletePurpose/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeletePurpose(int id)
        {
            APIResponse response = new APIResponse();
            var response1 = await _purposeService.DeletePurposeAsync(id, "");
            if (response1 == null)
            {
                response.Success = false;
                response.Message = "Internal Error";
            }
            else
            {
                response.Success = response1.Success;
                response.Message = response1.Message;
                response.Result = response1.Result;
            }
            return Ok(response);
        }
    }

}
