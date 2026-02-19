using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KycServicesController : ControllerBase
    {
        private readonly IKycMethodsService _kycMethodsService;
        private readonly IKycApplicationService _kycApplicationService;
        private readonly IOrganizationKycMethodsService _organizationKycMethodsService;
        public KycServicesController(
            IKycMethodsService kycMethodsService,
            IKycApplicationService kycApplicationService,
            IOrganizationKycMethodsService organizationKycMethodsService)
        {
            _kycMethodsService = kycMethodsService;
            _kycApplicationService = kycApplicationService;
            _organizationKycMethodsService = organizationKycMethodsService;
        }
        string get_unique_string(int string_length)
        {
            const string src = "ABCDEFGHIJKLMNOPQRSTUVWXYSabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < string_length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }

        [Route("GetKycMethodsList")]
        [HttpGet]
        public async Task<IActionResult> GetKycMethodsList()
        {
            var response = await _kycMethodsService.GetKycMethodsListAsync();
            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }

        [Route("GetOrgKycMethodsList/{orgId}")]
        [HttpGet]
        public async Task<IActionResult> GetOrgKycMethodsList(string orgId)
        {
            var response = await _organizationKycMethodsService.GetOrganizationKycMethodsByOrgIdAsync(orgId);
            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }

        [Route("SaveKycApplication")]
        [HttpPost]
        public async Task<IActionResult> SaveKycApplication
            ([FromBody] KycApplicationDTO kycApplicationDTO)
        {
            var client = new Client()
            {
                ClientId = get_unique_string(48),
                ClientSecret = get_unique_string(64),
                ApplicationName = kycApplicationDTO.ApplicationName,
                ApplicationType = "Machine to Machine Application",
                ResponseTypes = "code",
                OrganizationUid = kycApplicationDTO.OrganizationId,
                Type = "OAUTH2",
                EncryptionCert = string.Empty,
                IsKycApplication=true,
                CreatedBy = "egpadmin",
                UpdatedBy = "egpadmin",
            };
            client.ApplicationUrl = "https://localhost/kyc" + client.ClientSecret;
            client.RedirectUri = "https://localhost/kyc" + client.ClientSecret;
            var response = await _kycApplicationService.CreateClientAsync(client);
            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Result
            });
        }

        [Route("UpdateKycApplication")]
        [HttpPost]
        public async Task<IActionResult> UpdateKycApplication
            ([FromBody] KycApplicationDTO kycApplicationDTO)
        {
            var clientInDb = await _kycApplicationService.GetClientAsync(kycApplicationDTO.Id);
            if (clientInDb == null)
            {
                return Ok(new APIResponse()
                {
                    Success = false,
                    Message = "Application not found",
                    Result = null
                });
            }

            clientInDb.Id = kycApplicationDTO.Id;
            clientInDb.ApplicationName = kycApplicationDTO.ApplicationName;
            var response = await _kycApplicationService.UpdateClientAsync(clientInDb);
            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Result
            });
        }

        [Route("GetKycApplicationsListByOrgId")]
        [HttpGet]
        public async Task<IActionResult> GetKycApplicationsListByOrgId
            (string orgId)
        {
            var response = await _kycApplicationService.GetKycClientList();
            List<KycApplicationDTO> kycApplications = new List<KycApplicationDTO>();
            foreach (var client in response)
            {
                if (client.OrganizationUid == orgId)
                {
                    kycApplications.Add(new KycApplicationDTO()
                    {
                        Id = client.Id,
                        ApplicationName = client.ApplicationName,
                        clientId = client.ClientId,
                        clientSecret = client.ClientSecret,
                        OrganizationId = client.OrganizationUid,
                        status = client.Status
                    });
                }
            }
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "KYC Applications retrieved successfully",
                Result = kycApplications
            });

        }

        [Route("GetKycApplicationById")]
        [HttpGet]
        public async Task<IActionResult> GetKycApplicationById(int id)
        {
            var response = await _kycApplicationService.GetClientAsync(id);
            if (response == null)
            {
                return Ok(new APIResponse()
                {
                    Success = false,
                    Message = "KYC Application not found",
                    Result = null
                });
            }
            var kycApplication = new KycApplicationDTO()
            {
                Id = response.Id,
                ApplicationName = response.ApplicationName,
                clientId = response.ClientId,
                clientSecret = response.ClientSecret,
                OrganizationId = response.OrganizationUid,
                status = response.Status
            };
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "KYC Application retrieved successfully",
                Result = kycApplication
            });
        }

        [Route("GetKycOrganizationApplicationsList")]
        [HttpGet]
        public async Task<IActionResult> GetKycOrganizationApplicationsList()
        {
            var response = await _kycApplicationService.GetKycClientList();
            Dictionary<string,List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            foreach (var client in response)
            {
                if (client.OrganizationUid != null && client.IsKycApplication !=null && (bool)client.IsKycApplication)
                {
                    if (!keyValuePairs.ContainsKey(client.OrganizationUid))
                    {
                        keyValuePairs[client.OrganizationUid] = new List<string> { client.ClientId };
                    }
                    else
                    {
                        if (!keyValuePairs[client.OrganizationUid].Contains(client.ClientId))
                        {
                            keyValuePairs[client.OrganizationUid].Add(client.ClientId);
                        }
                    }
                }
            }
            return Ok(new APIResponse()
            {
                Success = true,
                Message = "KYC Applications retrieved successfully",
                Result = keyValuePairs
            });
        }

        [Route("GetOrgKycMethodsListByClientId/{clientId}")]
        [HttpGet]
        public async Task<IActionResult> GetOrgKycMethodsListByClientId(string clientId)
        {
            var response = await _organizationKycMethodsService.GetOrganizationKycMethodsByClientIdAsync(clientId);
            return Ok(new APIResponse()
            {
                Success = response.Success,
                Message = response.Message,
                Result = response.Resource
            });
        }
    }
}
