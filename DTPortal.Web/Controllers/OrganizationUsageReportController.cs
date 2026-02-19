using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using DTPortal.Web.ViewModel.OrganizationUsageReport;

using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Organization Usage Reports")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class OrganizationUsageReportController : BaseController
    {
        private readonly IOrganizationUsageReportService _organizationUsageReportService;
        private readonly IOrganizationService _organizationService;

        public OrganizationUsageReportController(IOrganizationUsageReportService organizationUsageReportService,
            IOrganizationService organizationService,
        ILogClient logClient) : base(logClient)
        {
            _organizationService = organizationService;
            _organizationUsageReportService = organizationUsageReportService;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetOrganizations(string value)
        {
            var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();
            if (organizationList == null)
            {
                return null;
            }

            return organizationList.Where(x => x.Contains(value, StringComparison.CurrentCultureIgnoreCase)); ;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetOrganizationUsageReport(string organizationUid)
        //{
        //    var organizationUsageReport = await _organizationUsageReportService.GetOrganizationUsageReports(organizationUid);
        //    if (organizationUsageReport == null)
        //    {
        //        return null;
        //    }

        //    return PartialView("_OrganizationUsageReport", organizationUsageReport);
        //}

        [HttpGet]
        public IActionResult Search()
        {
            var viewModel = new OrganizationUsageReportUsageViewModel();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<string> DownloadUsageReport(int reportId)
        {
            return await _organizationUsageReportService.DownloadUsageReport(reportId);
        }

        [HttpGet]
        public async Task<JsonResult> DownloadCurrentMonthOrganizationUsageReport(string organizationUid)
        {
            var response = await _organizationUsageReportService.DownloadCurrentMonthUsageReport(organizationUid);
            if (response.Success)
            {
                return Json(new { Status = response.Success, Message = response.Message, Result = response.Resource });
            }
            else
            {
                return Json(new { Status = response.Success, Message = response.Message, Result = string.Empty });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(OrganizationUsageReportUsageViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(viewModel.OrganizationUid))
                {
                    AlertViewModel alert = new AlertViewModel { Message = "Failed to get organization unique identifier" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                }

                return View(viewModel);
            }

            var organizationUsageReports = await _organizationUsageReportService.GetOrganizationUsageReports(viewModel.OrganizationUid, viewModel.Year);
            if (organizationUsageReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get usage report for organization {viewModel.OrganizationName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationUsageReport,
                    "Get Organization Usage Report", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received usage report for organization {viewModel.OrganizationName}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationUsageReport,
                "Get Organization Usage Report", LogMessageType.SUCCESS.ToString(), logMessage);

            viewModel.OrganizationUsageReports = organizationUsageReports;
            return View(viewModel);
        }
    }
}
