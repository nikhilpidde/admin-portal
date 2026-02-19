using DocumentFormat.OpenXml.Drawing.Charts;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel.SigningCredits;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Core.DTOs;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using DTPortal.Core.Domain.Services.Communication;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Exclusive Apps")]
    public class SigningCreditsController : BaseController
    {
        private readonly ILogger<SigningCreditsController> _logger;
        private readonly IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        private readonly ISigningCreditsService _signingCreditsService;
        public SigningCreditsController(ILogClient logClient,
            ILogger<SigningCreditsController> logger,
            IClientService clientService,
            IOrganizationService organizationService,
            ISigningCreditsService signingCreditsService) : base(logClient)
        {
            _logger = logger;
            _clientService = clientService;
            _organizationService = organizationService;
            _signingCreditsService = signingCreditsService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<Dictionary<string, string>> GetApplicationsList(string value)
        {
            var response = await _clientService.GetClientsByName(value);

            if (response == null)
            {
                return null;
            }
            return response;
        }
        public IActionResult GetApplicationDetails(string value)
        {
            return View();
        }
        public async Task<IActionResult> AddBucket()
        {
            var model = new AddBucketViewModel();

            var clientsList = await _clientService.GetApplicationsList();
            var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();

            if (organizationList == null)
            {
                return NotFound();
            }

            var list = new List<SelectListItem>();

            if (organizationList == null)
            {
                return NotFound();
            }

            foreach (var item in organizationList)
            {
                var org = item.Split(',');

                list.Add(new SelectListItem { Text = org[0], Value = org[1] });
            }

            model.clientsList = new List<SelectListItem>();
            model.organizationList = list;
            return View("_AddBucket", model);
        }
        public async Task<IActionResult> Save(AddBucketViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {

                var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();

                if (organizationList == null)
                {
                    return NotFound();
                }

                var list = new List<SelectListItem>();

                if (organizationList == null)
                {
                    return NotFound();
                }

                foreach (var item in organizationList)
                {
                    var org = item.Split(',');

                    list.Add(new SelectListItem { Text = org[0], Value = org[1] });
                }

                if(ViewModel.OrganizationId != null && ViewModel.OrganizationId != "")
                {
                    ViewModel.clientsList=await _clientService.GetApplicationsListByOuid(ViewModel.OrganizationId);
                }


                ViewModel.organizationList = list;

                return View("_AddBucket", ViewModel);
            }
            var saveBucketDTO = new SaveBucketConfigDTO()
            {
                orgId = ViewModel.OrganizationId,
                appId = ViewModel.clientId,
                label = ViewModel.label,
                orgName = ViewModel.OrganizationName,
                bucketClosingMessage = ViewModel.label + " " + "CLOSED",
                additionalDs = 1,
                additionalEds = 0
            };
            var response = await _signingCreditsService.SaveBucket(saveBucketDTO,UUID);
            if (response == null || !response.Success)
            {
                Alert alert1 = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };

                TempData["Alert"] = JsonConvert.SerializeObject(alert1);

                var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();

                if (organizationList == null)
                {
                    return NotFound();
                }

                var list = new List<SelectListItem>();

                if (organizationList == null)
                {
                    return NotFound();
                }

                foreach (var item in organizationList)
                {
                    var org = item.Split(',');

                    list.Add(new SelectListItem { Text = org[0], Value = org[1] });
                }

                if (ViewModel.OrganizationId != null && ViewModel.OrganizationId != "")
                {
                    ViewModel.clientsList = await _clientService.GetApplicationsListByOuid(ViewModel.OrganizationId);
                }

                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.ExclusiveApps, "Create new Exclusive App", LogMessageType.FAILURE.ToString(), "Create Exclusive App failed");
                ViewModel.organizationList = list;

                return View("_AddBucket", ViewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.ExclusiveApps, "Create new Exclusive App", LogMessageType.SUCCESS.ToString(), "Create Exclusive App success");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            return RedirectToAction("List");
        }
        public async Task<IActionResult> List()
        {
            var bucketList = new List<BucketListViewModel>();

            var buckets = await _signingCreditsService.GetBucketList();

            var response = await _clientService.GetClientsByName("");

            var bucketConfigList = (List<BucketListDTO>)buckets.Resource;

            foreach (var bucket in bucketConfigList)
            {
                string AppName = "";

                if (response.ContainsKey(bucket.AppId))
                {
                    AppName = response[bucket.AppId];
                }

                var bucketListViewModel = new BucketListViewModel()
                {
                    Id = bucket.Id,
                    ApplicationName = AppName,
                    OrganizationName = bucket.OrgName,
                    CreatedOn = bucket.CreatedOn.Date,
                    Status = bucket.Status,
                    Label = bucket.Label
                };

                bucketList.Add(bucketListViewModel);
            }
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.ExclusiveApps, "Get Exclusive App List Success", LogMessageType.SUCCESS.ToString(), "Get Exclusive App List success");
            return View(bucketList);
        }
        public async Task<IActionResult> GetDataList(string Id, string appName, string Label)
        {
            var BucketDataList = new List<BucketDataListViewModel>();

            var bucketListResponse = await _signingCreditsService.BucketHistoryList(Id);

            if (bucketListResponse == null || !bucketListResponse.Success)
            {
                return NotFound();
            }

            var historyList = (List<BucketDetailsDTO>)bucketListResponse.Resource;

            foreach (var bucketDetails in historyList)
            {
                var bucketdataListviewModel = new BucketDataListViewModel()
                {
                    Id = bucketDetails.id,
                    documentId = bucketDetails.bucketId,
                    totalDigital = bucketDetails.totalDS,
                    totalEseal = bucketDetails.totalEDS,
                    createdOn = bucketDetails.createdOn,
                    status = bucketDetails.status
                };
                BucketDataList.Add(bucketdataListviewModel);
            }
            BucketHistoryListViewModel model = new BucketHistoryListViewModel()
            {
                appName = appName,
                bucketHistoryList = BucketDataList,
                Label = Label
            };
            return View(model);
        }

        public async Task<IActionResult> GetApplications(string ordId)
        {
            var clientsList = await _clientService.GetApplicationsListByOrgId(ordId);

            return Json(clientsList);
        }

        public async Task<IActionResult> Edit(string Id)
        {
            var bucketConfigDetails = await _signingCreditsService.GetBucketConfigById(Id);

            if (bucketConfigDetails == null || !bucketConfigDetails.Success)
            {
                return NotFound();
            }

            var bucketConfig = (BucketListDTO)bucketConfigDetails.Resource;
            var clientName = "";

            var clientinDb = await _clientService.GetClientByClientIdAsync(bucketConfig.AppId);

            if (clientinDb != null)
            {
                clientName = clientinDb.ApplicationName;
            }

            var model = new UpdateBucketViewModel()
            {
                Id = bucketConfig.Id,
                OrganizationName = bucketConfig.OrgName,
                closingMessage = bucketConfig.BucketClosingMessage,
                AdditionalDs = bucketConfig.AdditionalDs,
                AdditionalEDs = bucketConfig.AdditionalEds,
                label = bucketConfig.Label,
                status = bucketConfig.Status,
                ApplicationName = clientName,
                OrgId = bucketConfig.OrgId,
                appId = bucketConfig.AppId
            };
            return View(model);
        }
        public async Task<IActionResult> Update(UpdateBucketViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", ViewModel);
            }
            var model = new UpdateBucketConfigDTO()
            {
                id = ViewModel.Id,
                orgId = ViewModel.OrgId,
                appId = ViewModel.appId,
                orgName = ViewModel.OrganizationName,
                bucketClosingMessage = ViewModel.label + " " + "CLOSED",
                additionalDs = 1,
                additionalEds = 0,
                label = ViewModel.label,
                status = ViewModel.status
            };
            var response = await _signingCreditsService.UpdateBucket(model,UUID);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.ExclusiveApps, "Update Exclusive App", LogMessageType.FAILURE.ToString(), "Update Exclusive App failed");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.ExclusiveApps, "Update Exclusive App", LogMessageType.SUCCESS.ToString(), "Update Exclusive App success");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }

            return RedirectToAction("List");
        }

        public async Task<IActionResult> SetBucketStatus(string id, string Doaction)
        {
            var bucketConfigDetails = await _signingCreditsService.GetBucketConfigById(id);

            if (bucketConfigDetails == null || !bucketConfigDetails.Success)
            {
                return NotFound();
            }

            var bucketConfig = (BucketListDTO)bucketConfigDetails.Resource;
            var clientName = "";

            var clientinDb = await _clientService.GetClientByClientIdAsync(bucketConfig.AppId);

            if (clientinDb != null)
            {
                clientName = clientinDb.ApplicationName;
            }

            var model = new UpdateBucketConfigDTO()
            {
                id = bucketConfig.Id,
                orgId = bucketConfig.OrgId,
                appId = bucketConfig.AppId,
                orgName = bucketConfig.OrgName,
                bucketClosingMessage = bucketConfig.BucketClosingMessage,
                additionalDs = bucketConfig.AdditionalDs,
                additionalEds = bucketConfig.AdditionalEds,
                label = bucketConfig.Label,
                status = Doaction
            };
            var response = await _signingCreditsService.UpdateBucket(model,UUID);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            return RedirectToAction("List");
        }
        public async Task<IActionResult> BucketDetails(string Id, string appName, string Label)
        {
            var bucketDetailsResult = await _signingCreditsService.GetBucketDetailsById(Id);
            if (bucketDetailsResult == null || !bucketDetailsResult.Success)
            {
                return NotFound();
            }
            var bucketDetails = (BucketDetailsDTO)bucketDetailsResult.Resource;
            var viewModel = new BucketDetailsViewModel()
            {
                Id = bucketDetails.id,
                BucketId = bucketDetails.bucketId,
                BucketConfigurationId = bucketDetails.orgBucketConfig.id,
                CreatedOn = bucketDetails.createdOn,
                UpdatedOn = bucketDetails.updatedOn,
                LastSignatoryId = bucketDetails.closedBy,
                PaymentReceived = bucketDetails.paymentReceived ? "true" : "false",
                TotalDigitalSignatures = bucketDetails.totalDS,
                TotalESeal = bucketDetails.totalEDS,
                PaymentReceivedOn = bucketDetails.closedOn,
                SponsorId = bucketDetails.sponsorId,
                Status = bucketDetails.status,
                AdditionalDSRemaining = bucketDetails.remainingDSAfterPayment,
                AdditionalEDSRemaining = bucketDetails.remainingEDSAfterPayment,
                appName = appName,
                Label = Label
            };
            return View(viewModel);
        }
    }
}
