using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.OrganizationBalanceSheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    //[Authorize(Roles = "Price Model")]
    [Authorize(Roles = "Organization Balance Sheet")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class OrganizationBalanceSheetController : BaseController
    {
        private readonly IServiceDefinitionService _serviceDefinitionService;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationBalanceSheetService _balanceSheetService;
        public OrganizationBalanceSheetController(IServiceDefinitionService serviceDefinitionService,
            IOrganizationService organizationService,
            IOrganizationBalanceSheetService balanceSheetService,
            ILogClient logClient) : base(logClient)
        {
            _serviceDefinitionService = serviceDefinitionService;
            _organizationService = organizationService;
            _balanceSheetService = balanceSheetService;
        }

        [HttpGet]
        public IActionResult SearchBalanceSheet()
        {
            OrganizationBalanceSheetSearchViewModel viewModel = new OrganizationBalanceSheetSearchViewModel();
            //viewModel.Services = await _serviceDefinitionService.GetServiceDefinitionsAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<string[]> GetOrganizations(string value)
        {
            var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();
            if (organizationList == null)
            {
                return null;
            }

            return organizationList;
        }

        [HttpPost]
        public async Task<IActionResult> SearchBalanceSheet([FromForm] OrganizationBalanceSheetSearchViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(viewModel.OrganizationId))
                {
                    return Json(new { Status = "Failed", Title = "Get Organization Balance Sheet", Message = "Failed to get organization unique identifier" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Get Organization Balance Sheet", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            OrganizationBalanceSheetDTO balanceSheet = new OrganizationBalanceSheetDTO();
            balanceSheet.OrganizationId = viewModel.OrganizationId;

            var apiResult = await _balanceSheetService.GetBalanceSheetDetailsAsync(balanceSheet);
            if (apiResult == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get balance sheet for organization {viewModel.OrganizationName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationBalanceSheet,
                    "Get Organization Balance Sheet", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            if(!apiResult.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = apiResult.Message};
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { Status = "Failed", Title = "Get Organization Balance Sheet", Message = apiResult.Message});
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received balance sheet for organization {viewModel.OrganizationName}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationBalanceSheet,
                "Get Organization Balance Sheet", LogMessageType.SUCCESS.ToString(), logMessage);

            var organizationBalanceSheets = JsonConvert.DeserializeObject<IList<OrganizationBalanceSheetDTO>>(apiResult.Resource.ToString()); ;

            return PartialView("_BalanceSheetDetails", organizationBalanceSheets);
        }
    }
}
