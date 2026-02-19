using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DTPortal.Web.ViewModel.Clients;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Models;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.ViewModel;
using DTPortal.Core.Utilities;
using DTPortal.Web.Enums;
using Newtonsoft.Json.Serialization;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Core.Services;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;

namespace DTPortal.Web.Controllers
{
    public class ClientApiController : BaseController
    {
        private readonly IClientService _clientService;
        private readonly IDashboardService _dashboardService;
        private readonly IOrganizationService _organizationService;
        private readonly IConfigurationService _configurationService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<ClientApiController> _logger;
        public ClientApiController(ILogClient logClient,
            IOrganizationService organizationService,
            IClientService clientService,
            IDashboardService dashboardService,
            ISessionService sessionService,
            IConfigurationService configurationService,
            ILogger<ClientApiController> logger) : base(logClient)
        {
            _organizationService = organizationService;
            _clientService = clientService;
            _dashboardService = dashboardService;
            _configurationService = configurationService;
            _sessionService = sessionService;
            _logger= logger;
        }

        async Task<List<SelectListItem>> GetOrganizationList()
        {
            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            var list = new List<SelectListItem>();
            if (result == null)
            {
                return list;
            }
            else
            {
                foreach (var org in result)
                {
                    var orgobj = org.Split(",");
                    list.Add(new SelectListItem { Text = orgobj[0], Value = orgobj[1] });
                }

                return list;
            }
        }

        string getCertificate(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }

            return result.ToString().Replace("\r", "");
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


        [HttpGet]
        public async Task<IActionResult> getList()
        {
            var viewModel = new List<ClientsListViewModel>();
            var ClientsList = await _clientService.ListOAuth2ClientAsync();
            if (ClientsList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider list");
                return Ok(new Reaponse("data not found"));
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.SUCCESS.ToString(), "Get Service Provider list success");

                foreach (var item in ClientsList)
                {
                    viewModel.Add(new ClientsListViewModel
                    {
                        Id = item.Id,
                        ApplicationName = item.ApplicationName,
                        ApplicationType = item.ApplicationType,
                        ApplicationUri = item.ApplicationUrl,
                        ClientID = item.ClientId,
                        State = item.Status,
                        OrgnizationId = item.OrganizationUid
                    });
                }
            }
            return Ok(new Reaponse(viewModel, "data found"));
        }

        [HttpGet]
        public async Task<IActionResult> getConfig()
        {
            var scope = await _configurationService.GetAllScopes();
            if (scope == null)
            {
                return Ok(new Reaponse("fali to get scopes"));
            }
            var grant = await _configurationService.GetAllGrantTypes();
            if (grant == null)
            {
                return Ok(new Reaponse("fali to get granttype"));
            }
            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    return NotFound();
            //}

            var clientViewModel = new ClientsNewViewModel
            {
                GrantTypesList = grant,
                ScopesList = scope,
                Scopes = "",
                GrantTypes = "",
                OrganizatioList = orgList
            };
            clientViewModel.ApplcationTypeList.ToList();

            return Ok(new Reaponse(clientViewModel, "Get Config successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> SaveClient([FromBody] ClientsNewViewModel viewModel)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(viewModel));
            if (!ModelState.IsValid)
            {
                return Ok(new Reaponse(ModelState.Values.SelectMany(v => v.Errors).ToList().ToString()));
            }

            //if (viewModel.Cert == null)
            //{
            //    ModelState.AddModelError("Cert", "required signing certificate");
            //    return View("New", viewModel);
            //}
            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {
                return Ok(new Reaponse("invalid signing certificate"));
            }

            var responce = "";
            if (viewModel.GrantTypes.Contains("authorization_code") || viewModel.GrantTypes.Contains("authorization_code_with_pkce"))
            {
                responce = "code";
            }
            if (viewModel.GrantTypes.Contains("implicit"))
            {
                responce = (responce == "") ? responce + " token" : "token";
            }

            var client = new Client()
            {
                ClientId = get_unique_string(48),
                ClientSecret = get_unique_string(64),
                ApplicationName = viewModel.ApplicationName,
                ApplicationType = viewModel.ApplicationType,
                ApplicationUrl = viewModel.ApplicationUri,
                RedirectUri = viewModel.RedirectUri,
                GrantTypes = viewModel.GrantTypes,
                Scopes = viewModel.Scopes,
                LogoutUri = viewModel.LogoutUri,
                ResponseTypes = responce,
                OrganizationUid = (viewModel.OrganizationId != null ? viewModel.OrganizationId : ""),
                Type = "OAUTH2",
                PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) : ""),
                EncryptionCert = string.Empty,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            var response = await _clientService.CreateClientAsync(client);
            if (response == null || !response.Success)
            {
                var error = (response == null ? "Internal error please contact to admin" : response.Message);
                return Ok(new Reaponse(error));
            }
            else
            {
                return Ok(new Reaponse(client, "Data added success"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditClient(int id)
        {
            var scope = await _configurationService.GetAllScopes();
            if (scope == null)
            {
                return Ok(new Reaponse("fail to get scope"));
            }
            var grant = await _configurationService.GetAllGrantTypes();
            if (grant == null)
            {
                return Ok(new Reaponse("fail to get scope"));
            }

            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
            //     return NotFound();
            //}

            var clientInDb = await _clientService.GetClientAsync(id);
            if (clientInDb == null)
            {
                return Ok(new Reaponse("fail to get client data"));
            }

            var clientsEditViewModel = new ClientsEditViewModel
            {
                Id = clientInDb.Id,
                ClientId = clientInDb.ClientId,
                ClientSecret = clientInDb.ClientSecret,
                ApplicationType = clientInDb.ApplicationType,
                ApplicationName = clientInDb.ApplicationName,
                ApplicationUri = clientInDb.ApplicationUrl,
                RedirectUri = clientInDb.RedirectUri,
                LogoutUri = clientInDb.LogoutUri,
                GrantTypes = clientInDb.GrantTypes,
                Scopes = clientInDb.Scopes,
                GrantTypesList = grant,
                ScopesList = scope,
                State = clientInDb.Status,
                OrganizationId = clientInDb.OrganizationUid,
                OrganizatioList = orgList,
                IsFileUploaded = !String.IsNullOrEmpty(clientInDb.PublicKeyCert)
            };
            clientsEditViewModel.ApplcationTypeList.ToList();

            return Ok(new Reaponse(clientsEditViewModel, " get client data"));
        }


        [HttpGet]
        public async Task<IActionResult> GetClientByClientID(string clientId)
        {
            var scope = await _configurationService.GetAllScopes();
            if (scope == null)
            {
                return Ok(new Reaponse("fail to get scope"));
            }
            var grant = await _configurationService.GetAllGrantTypes();
            if (grant == null)
            {
                return Ok(new Reaponse("fail to get scope"));
            }

            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
            //     return NotFound();
            //}

            var clientInDb = await _clientService.GetClientByClientIdAsync(clientId);
            if (clientInDb == null)
            {
                return Ok(new Reaponse("fail to get client data"));
            }

            var clientsEditViewModel = new ClientsEditViewModel
            {
                Id = clientInDb.Id,
                ClientId = clientInDb.ClientId,
                ClientSecret = clientInDb.ClientSecret,
                ApplicationType = clientInDb.ApplicationType,
                ApplicationName = clientInDb.ApplicationName,
                ApplicationUri = clientInDb.ApplicationUrl,
                RedirectUri = clientInDb.RedirectUri,
                LogoutUri = clientInDb.LogoutUri,
                GrantTypes = clientInDb.GrantTypes,
                Scopes = clientInDb.Scopes,
                //WithPkce = (bool)clientInDb.WithPkce,
                GrantTypesList = grant,
                ScopesList = scope,
                State = clientInDb.Status,
                OrganizationId = clientInDb.OrganizationUid,
                OrganizatioList = orgList,
                IsFileUploaded = !String.IsNullOrEmpty(clientInDb.PublicKeyCert)
            };
            clientsEditViewModel.ApplcationTypeList.ToList();

            return Ok(new Reaponse(clientsEditViewModel, " get client data"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateClient([FromBody] ClientsEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new Reaponse(ModelState.Values.SelectMany(v => v.Errors).ToList().ToString()));
            }

            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {
                return Ok(new Reaponse("invalid signing certificate"));
            }

            var clientInDb = await _clientService.GetClientAsync(viewModel.Id);
            if (clientInDb == null)
            {
                return Ok(new Reaponse("client details not found"));
            }

            var responce = "";
            if (viewModel.GrantTypes.Contains("authorization_code") || viewModel.GrantTypes.Contains("authorization_code_with_pkce"))
            {
                responce = "code";
            }
            if (viewModel.GrantTypes.Contains("implicit"))
            {
                responce = (responce == "") ? responce + " token" : "token";
            }
            clientInDb.Id = viewModel.Id;
            clientInDb.ClientId = viewModel.ClientId;
            clientInDb.ClientSecret = viewModel.ClientSecret;
            clientInDb.ApplicationName = viewModel.ApplicationName;
            clientInDb.ApplicationType = viewModel.ApplicationType;
            clientInDb.ApplicationUrl = viewModel.ApplicationUri;
            clientInDb.RedirectUri = viewModel.RedirectUri;
            clientInDb.GrantTypes = viewModel.GrantTypes;
            clientInDb.Scopes = viewModel.Scopes;
            clientInDb.ResponseTypes = responce;
            clientInDb.LogoutUri = viewModel.LogoutUri;
            clientInDb.OrganizationUid = (viewModel.OrganizationId != null ? viewModel.OrganizationId : "");
            clientInDb.UpdatedBy = "system";
            //clientInDb.WithPkce = viewModel.WithPkce;
            clientInDb.PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) : clientInDb.PublicKeyCert);
            var response = await _clientService.UpdateClientAsync(clientInDb, null);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to update Service Provider details of application name " + viewModel.ApplicationName);

                var Message = (response == null ? "Internal error please contact to admin" : response.Message);
                return Ok(new Reaponse(Message));
            }
            else
            {
                return Ok(new Reaponse(clientInDb, "data update successfully"));
            }
        }

        [HttpGet]
        public async Task<string[]> GetServiceProviderNames(string request)
        {
            return await _clientService.GetAllClientAppNames(request);
        }

        [HttpPost]
        public async Task<JsonResult> GetServiceProviderGraphDetails(string serviceProviderName)
        {
            var graphDetails = await _dashboardService.GetGraphCountAsync(serviceProviderName);
            if (graphDetails == null)
                return Json(new { Status = "Failed", Message = "Failed to get data" });
            else
                return Json(new { Status = "Success", Message = "Successfully received data", Data = graphDetails });
        }

        [HttpGet]
        public async Task<IActionResult> getclientsListByOrgUID(string orgUID)
        {
            var viewModel = new List<ClientsListViewModel>();
            var ClientsList = await _clientService.ListClientByOrgUidAsync(orgUID);
            
            return Ok(new Reaponse(ClientsList, "data found"));
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string clientId)
        {
            try
            {
                var response = await _clientService.DeleteClientByClientId(clientId);
                if (response == null || !response.Success)
                {
                    return Ok(new Reaponse("Failed to Delete Application"));
                }
                else
                {
                    return Ok(new Reaponse(null, "Successfully Deactivated Application"));
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
    }
}
