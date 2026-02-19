using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    public class CredentialController : Controller
    {
        private readonly ICredentialService _credentialService;
        private readonly IConfiguration _configuration;
        public CredentialController(ICredentialService credentialService,
            IConfiguration configuration)
        {
            _credentialService = credentialService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetCredentialList()
        {
            var response = await _credentialService.GetCredentialList();

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
            var response = await _credentialService.GetActiveCredentialList(authHeaderVal.Parameter);

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
            var response = await _credentialService.GetCredentialListByOrgId(orgUid);

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
            var response = await _credentialService.GetCredentialById(Id);

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
            var response = await _credentialService.GetCredentialByUid(Id);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetCredentialOfferByUid(string Id)
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
            var response = await _credentialService.GetCredentialOfferByUid(Id,authHeaderVal.Parameter);

            var apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCredential([FromBody] CredentialDTO credentialDto)
        {

            var response = await _credentialService.CreateCredentialAsync(credentialDto);

            APIResponse apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            };

            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCredential([FromBody] CredentialDTO credentialDto)
        {
            var response = await _credentialService.UpdateCredential(credentialDto);
            APIResponse apiResponse = new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = null
            };
            return Ok(apiResponse);
        }
        [HttpPost]
        public async Task<IActionResult> TestCredential([FromBody] TestCredentialRequest testCredentialRequest)
        {
            var response = await _credentialService.TestCredential(testCredentialRequest.UserId, testCredentialRequest.CredentialId);

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
            var response = await _credentialService.ActivateCredential(credentialId);

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
            var response = await _credentialService.GetCredentialDetails(credentialId);

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
            var response = await _credentialService.GetCredentialNameIdListAsync(credentialId);

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
            var response = await _credentialService.GetVerifiableCredentialList(orgId);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public IActionResult GetAuthSchemeList()
        {
            var authSchemeList = _configuration.GetSection("auth_schemes_supported").Get<string[]>();

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var authScheme in authSchemeList)
            {
                dict[authScheme] = authScheme;
            }
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "Get Auth Scheme List Success",
                Result = dict
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendToApproval([FromBody] ApprovalRequest approvalRequest)
        {
            var response = await _credentialService.SendToApproval(approvalRequest.credentialId, approvalRequest.signedDocument);

            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
        [HttpGet]
        public IActionResult GetAttributes()
        {
            string[] attributes = new string[] { "fullName", "dateOfBirth", "gender", "nationality", "mobileNumber", "email", "address", "idDocNumber", "pidIssueDate", "pidExpiryDate", "photo", "pidDocument", "cardNumber" };

            string[] displayNames = new string[] { "Full Name", "Date Of Birth", "Gender", "Nationality", "Mobile Number", "Email", "Address", "Id Document Number", "PID Issue Date", "PID Expiry Date", "Photo", "PID Document","Card Number" };

            int[] type = new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 1, 1, 4};
            List<DataAttributesDTO> dataAttributesDTOs = new List<DataAttributesDTO>();
            for (int i= 0;i < attributes.Length; i++){
                dataAttributesDTOs.Add(new DataAttributesDTO()
                {
                    attribute = attributes[i],
                    dataType = type[i],
                    displayName = displayNames[i],
                });
            }
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "Get Attributes Success",
                Result = dataAttributesDTOs
            });
        }
    }
}
