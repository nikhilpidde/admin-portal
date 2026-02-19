using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Clients;
using DTPortal.Web.ViewModel.EConsent;
using DTPortal.Web.ViewModel.PriceSlabDefinition;
using DTPortal.Web.ViewModel.RoleManagement;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Roles")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class EConsentClientController : BaseController
    {
        private readonly IEConsentService _eConsentService;
        private readonly IConfigurationService _configurationService;
        private readonly IOrganizationService _organizationService;
        private readonly IScopeService _scopeService;
        private readonly IPurposeService _purposeService;
        public EConsentClientController(ILogClient logClient,
            IEConsentService eConsentService,
            IConfigurationService configurationService,
            IOrganizationService organizationService,
            IScopeService scopeService,
            IPurposeService purposeService) : base(logClient)
        {
            _eConsentService = eConsentService;
            _configurationService = configurationService;
            _organizationService = organizationService;
            _scopeService = scopeService;
            _purposeService = purposeService;
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

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<EConsentClientListViewModel>();
            var result = await _eConsentService.GetConsentListAsync();
            if (result == null || !result.Success)
            {
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider list");
                return NotFound();
            }
            else
            {
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.SUCCESS.ToString(), "Get Service Provider list success");
                var ClientsList = (IEnumerable<EConsentClient>)result.Resource;
                foreach (var item in ClientsList)
                {
                    viewModel.Add(new EConsentClientListViewModel
                    {
                        Id = item.Id,
                        //ApplicationName = item.ApplicationName,
                        //OrganizationUid = item.OrganizationUid,
                        Status = item.Status
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

            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    return NotFound();
            //}

            var purposeList = await _purposeService.GetPurposesListAsync();
            if (purposeList == null)
            {
                return NotFound();
            }

            var consentViewModel = new EConsentClientNewViewModel
            {
                ScopesList = scope,
                Scopes = "",
                OrganizationList = orgList,
                Purposes = "",
                PurposesList = purposeList
            };

            return View(consentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(EConsentClientNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var scope = await _scopeService.GetScopesListAsync();
                if (scope == null)
                {
                    return NotFound();
                }

                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var purposeList = await _purposeService.GetPurposesListAsync();
                if (purposeList == null)
                {
                    return NotFound();
                }

                viewModel.ScopesList = scope;
                viewModel.OrganizationList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.PurposesList = purposeList;
                viewModel.Purposes = viewModel.Purposes != null ? viewModel.Purposes : "";

                return View("New", viewModel);
            }

            //if (viewModel.Cert == null)
            //{
            //    ModelState.AddModelError("Cert", "required signing certificate");
            //    return View("New", viewModel);
            //}
            if (viewModel.PublicKeyCert != null && viewModel.PublicKeyCert.ContentType != "application/x-x509-ca-cert")
            {
                var scope = await _scopeService.GetScopesListAsync();
                if (scope == null)
                {
                    return NotFound();
                }

                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var purposeList = await _purposeService.GetPurposesListAsync();
                if (purposeList == null)
                {
                    return NotFound();
                }

                viewModel.ScopesList = scope;
                viewModel.OrganizationList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.PurposesList = purposeList;
                viewModel.Purposes = viewModel.Purposes != null ? viewModel.Purposes : "";

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("New", viewModel);
            }

            var eConsentDTO = new EConsentDTO()
            {
                ApplicationName = viewModel.ApplicationName,
                Scopes = viewModel.Scopes,
                UpdatedBy = UUID,
                PublicKeyCert = viewModel.PublicKeyCert,
                Purposes = viewModel.Purposes,
                OrganizationUid = viewModel.OrganizationUid
            };

            var response = await _eConsentService.CreateConsentAsync(eConsentDTO);
            if (response == null || !response.Success)
            {
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.FAILURE.ToString(), "Fail to create Service Provider");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                var scope = await _configurationService.GetAllScopes();
                if (scope == null)
                {
                    return NotFound();
                }

                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    return NotFound();
                //}

                var purposeList = await _purposeService.GetPurposesListAsync();
                if (purposeList == null)
                {
                    return NotFound();
                }

                viewModel.ScopesList = scope;
                viewModel.Scopes = "";
                viewModel.OrganizationList = orgList;
                viewModel.PurposesList = purposeList;
                viewModel.Purposes = "";

                return View("New", viewModel);
            }
            else
            {
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.SUCCESS.ToString(), "Created New Service Provider with application name " + viewModel.ApplicationName + " Successfully");

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
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");
                return NotFound();
            }

            var orgList = await GetOrganizationList();
            //if (orgList.Count == 0)
            //{
            //    return NotFound();
            //}

            var purposeList = await _purposeService.GetPurposesListAsync();
            if (purposeList == null)
            {
                return NotFound();
            }

            var res = await _eConsentService.GetConsentbyIdAsync(id);
            if (res == null || !res.Success)
            {
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider client details", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider details");
                return NotFound();
            }

            var consentInDb = (EConsentClient)res.Resource;

            var consentEditViewModel = new EConsentClientEditViewModel
            {
                Id = consentInDb.Id,
                //ClientId = consentInDb.ClientId,
                //ClientSecret = consentInDb.ClientSecret,
                //ApplicationName = consentInDb.ApplicationName,
                Scopes = consentInDb.Scopes,
                ScopesList = scope,
                Status = consentInDb.Status,
                //IsFileUploaded = !String.IsNullOrEmpty(consentInDb.PublicKeyCert),
                Purposes = consentInDb.Purposes,
                PurposesList = purposeList,
                //OrganizationId = consentInDb.OrganizationUid,
                OrganizationList = orgList
            };

            //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.SUCCESS.ToString(), "Get Service Provider details of " + clientInDb.ApplicationName + " successfully ");

            return View(consentEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EConsentClientEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var scope = await _scopeService.GetScopesListAsync();
                if (scope == null)
                {
                    //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");

                    return NotFound();
                }

                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
                //    return NotFound();
                //}

                var purposeList = await _purposeService.GetPurposesListAsync();
                if (purposeList == null)
                {
                    return NotFound();
                }

                viewModel.ScopesList = scope;
                viewModel.OrganizationList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.PurposesList = purposeList;
                viewModel.Purposes = viewModel.Purposes != null ? viewModel.Purposes : "";

                return View("Edit", viewModel);
            }

            if (viewModel.PublicKeyCert != null && viewModel.PublicKeyCert.ContentType != "application/x-x509-ca-cert")
            {
                var scope = await _scopeService.GetScopesListAsync();
                if (scope == null)
                {
                    //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Scopes in Service Provider view");

                    return NotFound();
                }

                var orgList = await GetOrganizationList();
                //if (orgList.Count == 0)
                //{
                //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Organization list in Service Provider view");
                //    return NotFound();
                //}

                var purposeList = await _purposeService.GetPurposesListAsync();
                if (purposeList == null)
                {
                    return NotFound();
                }

                viewModel.ScopesList = scope;
                viewModel.OrganizationList = orgList;
                viewModel.Scopes = viewModel.Scopes != null ? viewModel.Scopes : "";
                viewModel.PurposesList = purposeList;
                viewModel.Purposes = viewModel.Purposes != null ? viewModel.Purposes : "";

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("Edit", viewModel);
            }

            var eConsentDTO = new EConsentDTO()
            {
                Id = viewModel.Id,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret,
                ApplicationName = viewModel.ApplicationName,
                Scopes = viewModel.Scopes,
                UpdatedBy = UUID,
                PublicKeyCert = viewModel.PublicKeyCert,
                Purposes = viewModel.Purposes,
                OrganizationUid = viewModel.OrganizationId
            };

            //var response = await _eConsentService.UpdateConsentAsync(eConsentDTO);
            //if (response == null || !response.Success)
            //{
            //    //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to update Service Provider details of application name " + viewModel.ApplicationName);

            //    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
            //    TempData["Alert"] = JsonConvert.SerializeObject(alert);

            //    var scope = await _configurationService.GetAllScopes();
            //    if (scope == null)
            //    {
            //        return NotFound();
            //    }

            //    var orgList = await GetOrganizationList();
            //    //if (orgList.Count == 0)
            //    //{
            //    //    return NotFound();
            //    //}

            //    var purposeList = await _purposeService.GetPurposesListAsync();
            //    if (purposeList == null)
            //    {
            //        return NotFound();
            //    }

            //    viewModel.ScopesList = scope;
            //    viewModel.Scopes = "";
            //    viewModel.OrganizationList = orgList;
            //    viewModel.PurposesList = purposeList;
            //    viewModel.Purposes = "";
            //    return View("Edit", viewModel);
            //}
            //else
            //{
            //    //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Service Provider details of application name " + viewModel.ApplicationName + " successfully" : "Request for Update Service Provider details of application name " + viewModel.ApplicationName + " has send for approval "));

            //    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
            //    TempData["Alert"] = JsonConvert.SerializeObject(alert);

            //    return RedirectToAction("List");
            //}
            return View("Edit", viewModel);
        }
    }
}