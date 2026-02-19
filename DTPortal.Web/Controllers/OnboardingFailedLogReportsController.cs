using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.AdminLogReports;
using DTPortal.Web.ViewModel.OnboardingFailedLogReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DocumentFormat.OpenXml.Bibliography;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Activity Reports")]
    [Authorize(Roles = "Onboarding Failed Log Reports")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class OnboardingFailedLogReportsController : BaseController
    {
        private readonly ILogReportService _logReportService;
        private readonly BackgroundService _backgroundService;
        private readonly ISubscriberService _subscriberService;

        public OnboardingFailedLogReportsController(ILogReportService logReportService,
            BackgroundService backgroundService, ILogClient logClient,
            ISubscriberService subscriberService) : base(logClient)
        {
            _logReportService = logReportService;
            _backgroundService = backgroundService;
            _subscriberService = subscriberService;
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new OnboardingFailedLogReportViewModel());
        }

        [HttpGet]
        //[Route("Reports/Page/{page}")]
        public async Task<IActionResult> ReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                StartDate = "",
                EndDate = "",
                DocumentNumber = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchOnboardingFailedReports"] as string, definition);
            TempData.Keep("SearchOnboardingFailedReports");

            var logReports = await _logReportService.GetOnboardingFailedLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
                        searchDetails.DocumentNumber,
                        page,
                        searchDetails.PerPage);
            if (logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get onboarding failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.OnboardingFailedReports,
                    "Get Onboarding Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            foreach (var logReport in logReports)
            {
                var logMessageArray = logReport.LogMessage.Split('|');
                //if (logMessageArray.Length > 0)
                if(logMessageArray.Length == 4)
                {
                    logReport.DocumentNumber = logMessageArray[0];
                    logReport.SubscriberType = logMessageArray[1];
                    logReport.OnboardingMethod = logMessageArray[2];
                    logReport.Message = logMessageArray[3];
                }
                else
                {
                    logReport.Message = logMessageArray[0];
                }
                logReport.IsChecksumValid = _logReportService.VerifyChecksum(logReport).Success;
            }
            OnboardingFailedLogReportViewModel viewModel = new OnboardingFailedLogReportViewModel
            {
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                DocumentNumber = searchDetails.DocumentNumber,
                PerPage = searchDetails.PerPage,
                Reports = logReports
            };

            // Push the log to Admin Log Server
            logMessage = $"Successfully received onboarding failed reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.OnboardingFailedReports,
                "Get Onboarding Failed Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View("Reports", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public JsonResult ExportOnboardingFailedReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
                StartDate = "",
                EndDate = "",
                DocumentNumber = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchOnboardingFailedReports"] as string, definition);
            TempData.Keep("SearchOnboardingFailedReports");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to export onboarding failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.OnboardingFailedReports,
                    "Export Onboarding Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export Admin Report", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportOnboardingFailedReportToFile(exportType, fullName, email, searchDetails.StartDate, searchDetails.EndDate, searchDetails.DocumentNumber,
                    searchDetails.TotalCount);
            });

            return Json(new { Status = "Success", Title = "Export Admin Report", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberDetails(string identifier)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                return BadRequest();
            }

            var subscriberDetails = await _subscriberService.GetSubscriberOnboardingDetailsAsync(identifier);
            if (subscriberDetails == null)
            {
                return NotFound();
            }
            return PartialView("_SubscriberDetails",subscriberDetails);
        }

        [HttpPost]
        //[Route("Reports")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reports([FromForm] OnboardingFailedLogReportViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View("Reports", viewModel);
            }

            var logReports = await _logReportService.GetOnboardingFailedLogReportAsync(viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.DocumentNumber,
                        perPage: viewModel.PerPage.Value);
            if (logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get onboarding failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.OnboardingFailedReports,
                    "Get Onboarding Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            foreach (var logReport in logReports)
            {
                var logMessageArray = logReport.LogMessage.Split('|');
                //if (logMessageArray.Length > 0)
                if (logMessageArray.Length == 4)
                {
                    logReport.DocumentNumber = logMessageArray[0];
                    logReport.SubscriberType = logMessageArray[1];
                    logReport.OnboardingMethod = logMessageArray[2];
                    logReport.Message = logMessageArray[3];
                }
                else
                {
                    logReport.Message = logMessageArray[0];
                }
                logReport.IsChecksumValid = _logReportService.VerifyChecksum(logReport).Success;
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received onboarding failed reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.OnboardingFailedReports,
                "Get Onboarding Failed Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SearchOnboardingFailedReports"] = JsonConvert.SerializeObject(
                new
                {
                    StartDate = viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                    DocumentNumber = viewModel.DocumentNumber,
                    PerPage = viewModel.PerPage,
                    TotalCount = logReports.TotalCount
                });

            viewModel.Reports = logReports;
            return View("Reports", viewModel);
        }
    }
}
