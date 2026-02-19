using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCredentialVerifiersController : ControllerBase
    {
        private readonly IQrCredentialVerifiersService _qrCredentialVerifiersService;
        private readonly IConfiguration Configuration;
        public QrCredentialVerifiersController(IQrCredentialVerifiersService credentialVerifiersService,
            IConfiguration configuration)
        {
            _qrCredentialVerifiersService = credentialVerifiersService;
            Configuration = configuration;
        }
        [Route("GetQrCredentialVerifiersList")]
        [HttpGet]
        public async Task<IActionResult> GetQrCredentialVerifiersList()
        {
            var response = await _qrCredentialVerifiersService.GetQrCredentialVerifierDTOsListAsync();
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("GetActiveQrCredentialVerifiersList")]
        [HttpGet]
        public async Task<IActionResult> GetActiveQrCredentialVerifiersList()
        {
            var authHeader = Request.Headers[Configuration["AccessTokenHeaderName"]];
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
            var response = await _qrCredentialVerifiersService.GetActiveQrCredentialVerifiersListAsync(authHeaderVal.Parameter);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("GetQrCredentialVerifiersListByOrganizationId/{orgId}")]
        [HttpGet]
        public async Task<IActionResult> GetQrCredentialVerifiersListByOrganizationId(string orgId)
        {
            var response = await _qrCredentialVerifiersService.GetQrCredentialVerifiersListByOrganizationIdAsync(orgId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("GetActiveQrCredentialVerifiersListByOrganizationId/{orgId}")]
        [HttpGet]
        public async Task<IActionResult> GetActiveQrCredentialVerifiersListByOrganizationId(string orgId)
        {
            var authHeader = Request.Headers[Configuration["AccessTokenHeaderName"]];
            if (string.IsNullOrEmpty(authHeader))
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }

            var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
            if (null == authHeaderVal.Scheme || null == authHeaderVal.Parameter)
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }

            if (!authHeaderVal.Scheme.Equals("bearer",
                 StringComparison.OrdinalIgnoreCase))
            {
                ErrorResponseDTO errResponse = new ErrorResponseDTO();
                errResponse.error = "Invalid Token";
                errResponse.error_description = "Invalid Token";
                return Unauthorized(errResponse);
            }

            var response = await _qrCredentialVerifiersService.GetActiveQrCredentialVerifiersListByOrganizationIdAsync(orgId, authHeaderVal.Parameter);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("GetQrCredentialVerifierById/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetQrCredentialVerifierById(int id)
        {
            var response = await _qrCredentialVerifiersService.GetQrCredentialVerifierByIdAsync(id);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }


        [Route("CreateQrCredentialVerifier")]
        [HttpPost]
        public async Task<IActionResult> CreateQrCredentialVerifier(QrCredentialVerifierDTO qrCredentialVerifierDTO)
        {
            var response = await _qrCredentialVerifiersService.CreateQrCredentialVerifierAsync(qrCredentialVerifierDTO);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("UpdateQrCredentialVerifier")]
        [HttpPost]
        public async Task<IActionResult> UpdateQrCredentialVerifier(QrCredentialVerifierDTO qrCredentialVerifierDTO)
        {
            var response = await _qrCredentialVerifiersService.UpdateQrCredentialVerifierAsync(qrCredentialVerifierDTO);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("GetCredentialVerifierListByIssuerId/{orgId}")]
        [HttpGet]
        public async Task<IActionResult> GetCredentialVerifierListByIssuerId
            (string orgId)
        {
            var response = await _qrCredentialVerifiersService.
                GetCredentialVerifierListByIssuerId(orgId);
            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);
        }

        [Route("ActivateCredential")]
        [HttpPost]
        public async Task<IActionResult> ActivateCredential
            ([FromBody] ActivateCredentialDTO activateCredentialDTO)
        {
            var response = await _qrCredentialVerifiersService.ActivateQrCredentialById
                (activateCredentialDTO.Id);

            var result = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };
            return Ok(result);

        }

        [Route("RejectCredential")]
        [HttpPost]
        public async Task<IActionResult> RejectCredential
            ([FromBody] ActivateCredentialDTO activateCredentialDTO)
        {
            var response = await _qrCredentialVerifiersService.RejectQrCredentialById
                (activateCredentialDTO.Id, activateCredentialDTO.Remarks);

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
