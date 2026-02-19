using System;
using System.Net;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.AdminLogReports;

using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Activity Reports")]
    [Authorize(Roles = "Admin Log Reports")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class AdminLogReportsController : BaseController
    {
        private readonly ILogReportService _logReportService;
        private readonly BackgroundService _backgroundService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AdminLogReportsController> _logger;
        private readonly IConfiguration _configuration;

        public AdminLogReportsController(IEmailSender emailSender,
            ILogReportService logReportService,
            BackgroundService backgroundService,
            IConfiguration configuration,
            ILogger<AdminLogReportsController> logger,
            ILogClient logClient) : base(logClient)
        {
            _emailSender = emailSender;
            _logReportService = logReportService;
            _backgroundService = backgroundService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new AdminLogReportViewModel());
        }

        [HttpGet]
        //[Route("Reports/Page/{page}")]
        public async Task<IActionResult> ReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                UserName = "",
                ModuleName = "",
                StartDate = "",
                EndDate = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAdminReports"] as string, definition);
            TempData.Keep("SearchAdminReports");

            var logReports = await _logReportService.GetAdminLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
                        searchDetails.UserName,
                        searchDetails.ModuleName,
                        page,
                        searchDetails.PerPage);
            if (logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get admin reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                    "Get Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            foreach (var logReport in logReports)
            {
                logReport.IsChecksumValid = _logReportService.VerifyChecksum(logReport).Success;
            }
            AdminLogReportViewModel viewModel = new AdminLogReportViewModel
            {
                UserName = searchDetails.UserName,
                ModuleName = Enum.TryParse<ModuleName>(searchDetails.ModuleName, out var outServiceName) ? outServiceName : (ModuleName?)null,
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                PerPage = searchDetails.PerPage,
                Reports = logReports
            };

            // Push the log to Admin Log Server
            logMessage = $"Successfully received admin reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                "Get Admin Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View("Reports", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public JsonResult ExportAdminReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
                UserName = "",
                ModuleName = "",
                StartDate = "",
                EndDate = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAdminReports"] as string, definition);
            TempData.Keep("SearchAdminReports");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to export admin report";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                    "Export Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export Admin Report", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportAdminReportToFile(exportType, fullName, email, searchDetails.StartDate, searchDetails.EndDate, 
                    searchDetails.UserName, searchDetails.ModuleName, searchDetails.TotalCount);
            });

            return Json(new { Status = "Success", Title = "Export Admin Report", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        }

        [HttpGet]
        //[Route("[action]")]
        public PartialViewResult ExportTypes()
        {
            return PartialView("_AdminExportTypes");
        }

        [HttpPost]
        //[Route("Reports")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reports([FromForm] AdminLogReportViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View("Reports", viewModel);
            }

            var logReports = await _logReportService.GetAdminLogReportAsync(viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.UserName,
                        viewModel.ModuleName.GetValue(),
                        perPage: viewModel.PerPage.Value);
            if (logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get admin reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                    "Get Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            foreach (var logReport in logReports)
            {
                logReport.IsChecksumValid = _logReportService.VerifyChecksum(logReport).Success;
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received admin reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                "Get Admin Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SearchAdminReports"] = JsonConvert.SerializeObject(
                new
                {
                    UserName = viewModel.UserName,
                    ModuleName = viewModel.ModuleName.GetValue(),
                    StartDate = viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                    PerPage = viewModel.PerPage,
                    TotalCount = logReports.TotalCount
                });

            viewModel.Reports = logReports;
            return View("Reports", viewModel);
        }
    }
}
