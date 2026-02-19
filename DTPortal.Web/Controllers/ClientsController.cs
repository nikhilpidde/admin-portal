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
using DTPortal.Web.ViewModel.Subscriber;
using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Web.ViewModel.AuthnPolicies;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Service Provider")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class ClientsController : BaseController
    {
        private readonly IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        private readonly IConfigurationService _configurationService;
        private readonly ISessionService _sessionService;
        private readonly IEConsentService _econsentService;
        private readonly IAuthSchemeSevice _authSchemeSevice;
        private readonly IScopeService _scopeService;
        public ClientsController(ILogClient logClient,
            IOrganizationService organizationService,
            IClientService clientService,
            ISessionService sessionService,
            IConfigurationService configurationService,
            IEConsentService eConsentService,
            IScopeService scopeService,
            IAuthSchemeSevice authSchemeSevice) : base(logClient)
        {
            _organizationService = organizationService;
            _clientService = clientService;
            _configurationService = configurationService;
            _sessionService = sessionService;
            _econsentService = eConsentService;
            _authSchemeSevice = authSchemeSevice;
            _scopeService = scopeService;
        }

        async Task<List<SelectListItem>> GetOrganizationList()
        {
            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            var list =new List<SelectListItem>();
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

        async Task<List<SelectListItem>> GetAuthSchemasList()
        {
            var result = await _authSchemeSevice.ListAuthSchemesAsync();
            var list = new List<SelectListItem>();
            if (result == null)
            {
                return list;
            }
            else
            {
                list.Add(new SelectListItem { Text = "DEFAULT", Value = "0" });
                foreach (var org in result)
                {
                    list.Add(new SelectListItem { Text = org.DisplayName, Value = org.Id.ToString() });
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
        public async Task<IActionResult> List()
        {
            var viewModel = new List<ClientsListViewModel>();
            var ClientsList = await _clientService.ListOAuth2ClientAsync();
            if (ClientsList == null)
            {
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider list");
                return NotFound();
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
                        CreatedDate = item.CreatedDate
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var scope = await _scopeService.GetScopesListAsync();
            if (scope == null)
            {
                return NotFound();
            }
            var grant = await _configurationService.GetAllGrantTypes();
            if (grant == null)
            {
                return NotFound();
            }
            var orgList =await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    return NotFound();
            //}

            var authlist = await GetAuthSchemasList();

            var clientViewModel = new ClientsNewViewModel
            {
                GrantTypesList = grant,
                ScopesList = scope,
                Scopes = "",
                GrantTypes = "",
                OrganizatioList = orgList,
                AuthSchemasList = authlist
            };
            clientViewModel.ApplcationTypeList.ToList();
            clientViewModel.Scopes = "";
            clientViewModel.Purposes = "";
            return View(clientViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ClientsNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    return NotFound();
                }
                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.GrantTypes = viewModel.GrantTypes != null ? viewModel.GrantTypes : "";
                viewModel.OrganizatioList = orgList;
                viewModel.AuthSchemasList = authlist;

                return View("New", viewModel);
            }

            //if (viewModel.Cert == null)
            //{
            //    ModelState.AddModelError("Cert", "required signing certificate");
            //    return View("New", viewModel);
            //}
            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {
                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    return NotFound();
                }
                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}
                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.GrantTypes = viewModel.GrantTypes != null ? viewModel.GrantTypes : "";
                viewModel.OrganizatioList = orgList;
                viewModel.AuthSchemasList = authlist;

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("New", viewModel);
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
                //WithPkce = viewModel.WithPkce,
                ResponseTypes = responce,
                OrganizationUid = (viewModel.OrganizationId != null ? viewModel.OrganizationId : ""),
                Type = "OAUTH2",
                PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) :""),
                EncryptionCert = string.Empty,
                CreatedBy = UUID,
                UpdatedBy = UUID,
                AuthScheme = int.Parse(viewModel.AuthSchemaId)
            };
            if (!string.IsNullOrEmpty(viewModel.Profiles) || !string.IsNullOrEmpty(viewModel.Purposes))
            {
                var eConsentClient = new EConsentClient()
                {
                    Scopes = viewModel.Profiles,
                    Purposes = viewModel.Purposes,
                    CreatedBy = UUID,
                    CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    //ModifiedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    Status = "ACTIVE"
                };
                client.EConsentClients.Add(eConsentClient);
            }

            var response = await _clientService.CreateClientAsync(client);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.FAILURE.ToString(), "Fail to create Service Provider");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    return NotFound();
                }


                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.Scopes = "";
                viewModel.GrantTypes = "";
                viewModel.OrganizatioList = orgList;
                viewModel.AuthSchemasList = authlist;

                return View("New", viewModel);
            }
            else
            {
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.SUCCESS.ToString(), "Created New Service Provider with application name " + viewModel.ApplicationName+" Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var scope = await _scopeService.GetScopesListAsync();
            if (scope == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");
                return NotFound();
            }
            var grant = await _configurationService.GetAllGrantTypes();
            if (grant == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get grant types in Service Provider view");
                return NotFound();
            }

            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
            //     return NotFound();
            //}

            var authlist = await GetAuthSchemasList();

            var clientInDb = await _clientService.GetClientAsync(id);
            if (clientInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider client details", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider details");
                return NotFound();
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
                IsFileUploaded = !String.IsNullOrEmpty(clientInDb.PublicKeyCert),
                AuthSchemaId = clientInDb.AuthScheme.ToString(),
                AuthSchemasList = authlist
            };
            clientsEditViewModel.ApplcationTypeList.ToList();

            var response = await _econsentService.GetConsentbyClientIdAsync(id);

            if (response != null && response.Success)
            {
                var ConsentDetails = (EConsentClient)response.Resource;
                clientsEditViewModel.Profiles = ConsentDetails.Scopes;
                clientsEditViewModel.Purposes=ConsentDetails.Purposes;
            }
            SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.SUCCESS.ToString(), "Get Service Provider details of "+ clientInDb.ApplicationName + " successfully ");

            return View(clientsEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ClientsEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");

                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get grant type in Service Provider view");
                    return NotFound();
                }
                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
                //    return NotFound();
                //}

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.OrganizatioList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.GrantTypes = viewModel.GrantTypes != null ? viewModel.GrantTypes : "";
                viewModel.AuthSchemasList = authlist;

                return View("Edit", viewModel);
            }

            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {
                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");

                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get grant type in Service Provider view");
                    return NotFound();
                }
                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
                //    return NotFound();
                //}

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.OrganizatioList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.GrantTypes = viewModel.GrantTypes != null ? viewModel.GrantTypes : "";
                viewModel.AuthSchemasList = authlist;

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("Edit", viewModel);
            }

            var clientInDb = await _clientService.GetClientAsync(viewModel.Id);
            if (clientInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider details");
                ModelState.AddModelError(string.Empty, "Service Provider not found");

                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    return NotFound();
                }
                var orgList = await GetOrganizationList();

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.Scopes = "";
                viewModel.GrantTypes = "";
                viewModel.OrganizatioList = orgList;
                viewModel.AuthSchemasList = authlist;

                return View("Edit", viewModel);
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
            clientInDb.UpdatedBy = UUID;
            //clientInDb.WithPkce = viewModel.WithPkce;
            clientInDb.PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) : clientInDb.PublicKeyCert);
            clientInDb.AuthScheme = int.Parse(viewModel.AuthSchemaId);

            var response1 = await _econsentService.GetConsentbyClientIdAsync(viewModel.Id);

            if (response1 != null && response1.Success)
            {
                var ConsentDetails = (EConsentClient)response1.Resource;
                ConsentDetails.Scopes = viewModel.Profiles;
                ConsentDetails.Purposes = viewModel.Purposes;
                ConsentDetails.UpdatedBy = UUID;
                ConsentDetails.ModifiedDate =
                DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                ConsentDetails.Status = "ACTIVE";
                clientInDb.EConsentClients.Add(ConsentDetails);
            }
            else
            {
                if (!string.IsNullOrEmpty(viewModel.Profiles) || !string.IsNullOrEmpty(viewModel.Purposes))
                {
                    var eConsentClient = new EConsentClient()
                    {
                        Scopes = viewModel.Profiles,
                        Purposes = viewModel.Purposes,
                        CreatedBy = UUID,
                        CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        Status = "ACTIVE"
                    };
                    clientInDb.EConsentClients.Add(eConsentClient);
                }
            }
            var response = await _clientService.UpdateClientAsync(clientInDb,null);
            if (response == null || !response.Success)
            {
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to update Service Provider details of application name " + viewModel.ApplicationName);

                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }
                var grant = await _configurationService.GetAllGrantTypes();
                if (grant == null)
                {
                    return NotFound();
                }


                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var authlist = await GetAuthSchemasList();

                viewModel.GrantTypesList = grant;
                viewModel.ScopesList = scope;
                viewModel.Scopes = "";
                viewModel.GrantTypes = "";
                viewModel.OrganizatioList = orgList;
                viewModel.AuthSchemasList = authlist;
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Service Provider details of application name " + viewModel.ApplicationName + " successfully" : "Request for Update Service Provider details of application name " + viewModel.ApplicationName + " has send for approval ") );

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _clientService.DeleteClientAsync(id, UUID);
                if (response == null || !response.Success)
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Delete Service Provider", LogMessageType.FAILURE.ToString(), "Fail to delete Service Provider");
                    return null;
                }
                else
                {
                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Delete Service Provider", LogMessageType.SUCCESS.ToString(), "Delete Service Provider successfully");
                    return new JsonResult(true);
                }
            }catch(Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Session(string id, string name)
        {
            var sessionInDb = await _sessionService.GetAllClientSessions(id);
            if (sessionInDb == null || sessionInDb.Success == false)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Session Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get client session in Service Provider");
                var model = new ClientsSessionListViewModel();
                return View(model); ;
            }
            else
            {
                var model = new ClientsSessionListViewModel
                {
                    ClientName = name,
                    session = sessionInDb.GlobalSessions
                };

                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Session Service Provider", LogMessageType.SUCCESS.ToString(), "Get client session in Service Provider success");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string id)
        {
            var data = new LogoutUserRequest
            {
                GlobalSession = id
            };

            var response = await _sessionService.LogoutUser(data);
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Logout Service Provider", LogMessageType.FAILURE.ToString(), "Fail to logout all session in Service Provider getting response value null");
                return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
            }
            if (response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Logout Service Provider", LogMessageType.FAILURE.ToString(), "Fail to logout session of id(" + id + ") in Service Provider");
                return new JsonResult(response);
            }

            SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Logout Service Provider", LogMessageType.SUCCESS.ToString(), "Logout session of id(" + id + ") in Service Provider success");
            return new JsonResult(response);


        }

        [HttpPost]
        public async Task<IActionResult> LogoutAll(List<string> id)
        {
            foreach (var session in id)
            {
                var data = new LogoutUserRequest
                {
                    GlobalSession = session
                };

                var response = await _sessionService.LogoutUser(data);
                if(response == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "LogoutAll Service Provider", LogMessageType.FAILURE.ToString(), "Fail to logout all session in Service Provider getting response value null");
                    return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
                }
                if (!response.Success)
                {
                    SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "LogoutAll Service Provider", LogMessageType.FAILURE.ToString(), "Fail to logout all session in Service Provider");
                    return new JsonResult(response);
                }

            }

            SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "LogoutAll Service Provider", LogMessageType.SUCCESS.ToString(), "Logout all session in Service Provider success");
            return new JsonResult(new { success = true });
        }

        [HttpGet]
        public async Task<ActionResult> SetClientState(int id, string Doaction)
        {
            ClientResponse responce;
            if (Doaction == "Activation")
            {
                responce = await _clientService.ActivateClientAsync(id);
            }
            else
            {
                responce = await _clientService.DeActivateClientAsync(id);
            }
            if (responce == null || !responce.Success)
            {
                Alert alert = new Alert { Message = "Service Provider " + Doaction + " Fail" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "SetUserState Service Provider", LogMessageType.FAILURE.ToString(), "Service Provider " + Doaction + " Fail");
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = "Service Provider " + Doaction + " Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "SetUserState Service Provider", LogMessageType.SUCCESS.ToString(), "Service Provider " + Doaction + " Success");
            }
            return RedirectToAction("Edit", new { id = id });
        }
        [HttpPost]
        public async Task<IActionResult> GetClientProfiles(string Profiles,string Purposes)
        {
            var eConsentClient = await _econsentService.GetEConsentClientDetailsAsync(Profiles, Purposes);
            if (eConsentClient == null || !eConsentClient.Success)
            {
                return NotFound();
            }

            var request = (ClientConsentRequestModel)eConsentClient.Resource;

            var viewModel = new ClientConsentViewModel
            {
                PurposesList = request.Purposes,
                ProfilesList = request.Profiles,
            };
            return PartialView("_ClientConsent", viewModel);
        }
        [HttpGet]
        public async Task<string[]> GetClients(string value)
        {
            var clientsList = await _clientService.GetClientNamesAsync(value);

            return clientsList;
        }
    }
}
