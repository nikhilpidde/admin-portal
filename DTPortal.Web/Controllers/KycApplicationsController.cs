using DocumentFormat.OpenXml.Drawing.Diagrams;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Clients;
using DTPortal.Web.ViewModel.KycApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class KycApplicationsController : BaseController
    {
        private readonly IKycApplicationService _kycApplicationService;
        private readonly IOrganizationService _organizationService;
        public KycApplicationsController(
            ILogClient logClient,
            IKycApplicationService kycApplicationService,
            IOrganizationService organizationService
            )
            :base(logClient)
        {
            _kycApplicationService = kycApplicationService;
            _organizationService = organizationService;
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
            var viewModel = new List<KycApplicationsListViewModel>();
            var ClientsList = await _kycApplicationService.GetKycClientList();
            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            if (ClientsList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider list");
                return View(viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Get all Service Provider List", LogMessageType.SUCCESS.ToString(), "Get Service Provider list success");

                foreach (var item in ClientsList)
                {
                    var KycApplication = new KycApplicationsListViewModel()
                    {
                        Id = item.Id,
                        ApplicationName = item.ApplicationName,
                        Status = item.Status
                    };
                    foreach (var org in result)
                    {
                        var orgobj = org.Split(",");
                        if (orgobj[1] == item.OrganizationUid)
                        {
                            KycApplication.OrganizationName = orgobj[0];
                            break;
                        }
                    }
                    viewModel.Add(KycApplication);
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var orgList = await GetOrganizationList();

            var clientViewModel = new KycApplicationsNewViewModel
            {
                OrganizationList = orgList
            };
            return View(clientViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(KycApplicationsNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var orgList = await GetOrganizationList();
                viewModel.OrganizationList = orgList;

                return View("New", viewModel);
            }

            var responce = "code";
            var client = new Client()
            {
                ClientId = get_unique_string(48),
                ClientSecret = get_unique_string(64),
                ApplicationName = viewModel.ApplicationName,
                ApplicationType = "Machine to Machine Application",
                ResponseTypes = responce,
                OrganizationUid = (viewModel.OrganizationId != null ? viewModel.OrganizationId : ""),
                Type = "OAUTH2",
                EncryptionCert = string.Empty,
                CreatedBy = UUID,
                UpdatedBy = UUID,
                IsKycApplication=true
            };
            client.ApplicationUrl = "https://localhost/kyc" + client.ClientSecret;
            client.RedirectUri = "https://localhost/kyc" + client.ClientSecret;
            var response = await _kycApplicationService.CreateClientAsync(client);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.FAILURE.ToString(), "Fail to create Service Provider");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                var orgList = await GetOrganizationList();
                viewModel.OrganizationList = orgList;

                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Create new Service Provider", LogMessageType.SUCCESS.ToString(), "Created New Service Provider with application name " + viewModel.ApplicationName + " Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var orgList = await GetOrganizationList();

            var clientInDb = await _kycApplicationService.GetClientAsync(id);
            if (clientInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider client details", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider details");
                return NotFound();
            }

            var clientsEditViewModel = new KycApplicationsEditViewModel
            {
                Id = clientInDb.Id,
                ClientId = clientInDb.ClientId,
                ClientSecret = clientInDb.ClientSecret,
                ApplicationName = clientInDb.ApplicationName,
                Status = clientInDb.Status,
                OrganizationId = clientInDb.OrganizationUid,
                OrganizationList = orgList,
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "View Service Provider details", LogMessageType.SUCCESS.ToString(), "Get Service Provider details of " + clientInDb.ApplicationName + " successfully ");

            return View(clientsEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(KycApplicationsEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var orgList = await GetOrganizationList();

                viewModel.OrganizationList = orgList;

                return View("Edit", viewModel);
            }

            var clientInDb = await _kycApplicationService.GetClientAsync(viewModel.Id);
            if (clientInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to get Service Provider details");
                ModelState.AddModelError(string.Empty, "Service Provider not found");

                var orgList = await GetOrganizationList();

                viewModel.OrganizationList = orgList;

                return View("Edit", viewModel);
            }

            clientInDb.Id = viewModel.Id;
            clientInDb.ClientId = viewModel.ClientId;
            clientInDb.ClientSecret = viewModel.ClientSecret;
            clientInDb.ApplicationName = viewModel.ApplicationName;
            clientInDb.OrganizationUid = (viewModel.OrganizationId != null ? viewModel.OrganizationId : "");
            clientInDb.UpdatedBy = UUID;
            var response = await _kycApplicationService.UpdateClientAsync(clientInDb);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.FAILURE.ToString(), "Fail to update Service Provider details of application name " + viewModel.ApplicationName);

                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                var orgList = await GetOrganizationList();
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.OAuth2_OpenID, "Update Service Provider", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Service Provider details of application name " + viewModel.ApplicationName + " successfully" : "Request for Update Service Provider details of application name " + viewModel.ApplicationName + " has send for approval "));

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

    }
}
