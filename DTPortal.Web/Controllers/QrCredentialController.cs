using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    public class QrCredentialController : Controller
    {
        private readonly IQrCredentialService _qrCredentialService;
        private readonly IConfiguration _configuration;
        public QrCredentialController(IQrCredentialService qrCredentialService,
            IConfiguration configuration)
        {
            _qrCredentialService = qrCredentialService;
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialList()
        {
            var response = await _qrCredentialService.GetCredentialList();

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetActiveCredentialList()
        {
            var response = await _qrCredentialService.GetActiveCredentialList();

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialListByOrgUid(string orgUid)
        {
            var response = await _qrCredentialService.GetCredentialListByOrgId(orgUid);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialById(int Id)
        {
            var response = await _qrCredentialService.GetCredentialById(Id);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialByUid(string Id)
        {
            var response = await _qrCredentialService.GetCredentialByUid(Id);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetQrCredentialOfferByUid(string Id)
        {
            var authHeader = Request.Headers[_configuration["AccessTokenHeaderName"]];
            if (string.IsNullOrEmpty(authHeader))
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }

            // Parse the authorization header
            var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
            if (null == authHeaderVal.Scheme || null == authHeaderVal.Parameter)
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }

            // Check the authorization is of Bearer type
            if (!authHeaderVal.Scheme.Equals("bearer",
                 StringComparison.OrdinalIgnoreCase))
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }
            var response = await _qrCredentialService.GetCredentialOfferByUid(Id, authHeaderVal.Parameter);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCredential([FromBody] QrCredentialDTO credentialDto)
        {

            var response = await _qrCredentialService.CreateCredentialAsync(credentialDto);

            APIResponse apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCredential([FromBody] QrCredentialDTO credentialDto)
        {
            var response = await _qrCredentialService.UpdateCredential(credentialDto);
            APIResponse apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = null
            };
            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> TestCredential([FromBody] QrTestCredentialRequest testCredentialRequest)
        {
            var response = await _qrCredentialService.TestCredential(testCredentialRequest);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public async Task<IActionResult> ActivateCredential(string credentialId)
        {
            var response = await _qrCredentialService.ActivateCredential(credentialId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialDetails(string credentialId)
        {
            var response = await _qrCredentialService.GetCredentialDetails(credentialId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialNameIdList(string credentialId)
        {
            var response = await _qrCredentialService.GetCredentialNameIdListAsync(credentialId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetVerifiableCredentialList(string orgId)
        {
            var response = await _qrCredentialService.GetVerifiableCredentialList(orgId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpPost]
        public async Task<IActionResult> SendToApproval([FromBody] ApprovalRequest approvalRequest)
        {
            var response = await _qrCredentialService.SendToApproval(approvalRequest.credentialId, approvalRequest.signedDocument);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
    }
}
