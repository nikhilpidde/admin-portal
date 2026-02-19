using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.AdminLogReports;
using DTPortal.Web.ViewModel.AuthenticationFailedLogReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using DTPortal.Web.ExtensionMethods;
using DocumentFormat.OpenXml.Bibliography;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Activity Reports")]
    [Authorize(Roles = "Authentication Failed Log Reports")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class AuthenticationFailedLogReportsController : BaseController
    {
        private readonly ILogReportService _logReportService;
        private readonly IClientService _clientService;
        private readonly BackgroundService _backgroundService;
        private readonly ISubscriberService _subscriberService;

        public AuthenticationFailedLogReportsController(ILogReportService logReportService,
            IClientService clientService,
            BackgroundService backgroundService, ILogClient logClient,
            ISubscriberService subscriberService) : base(logClient)
        {
            _logReportService = logReportService;
            _clientService = clientService;
            _backgroundService = backgroundService;
            _subscriberService = subscriberService;
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new AuthenticationFailedLogViewModel());
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
				TransactionType = "AUTHENTICATON",
				TransactionStatus = "FAILED",
				PerPage = 0,
				TotalCount = 0
			};
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAuthenticationFailedReports"] as string, definition);
            TempData.Keep("SearchAuthenticationFailedReports");

            var logReports = await _logReportService.GetAuthenticationFailedLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
						page,
                        searchDetails.PerPage);
            if (logReports == null)
            {
                
                logMessage = $"Failed to get Authentication failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AuthenticationFailedReports,
                    "Get Authentication Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            var clients = await _clientService.EnumClientIds();

            if (clients != null)
            {
                foreach (var logReport in logReports)
                {
                    // Get Service Provider name by client id
                    if (logReport.ServiceProviderAppName != null)
                        logReport.ServiceProviderAppName
                            = clients.TryGetValue(logReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                    else
                        logReport.ServiceProviderAppName = "N/A";
                }
            }
            else
            {
                foreach (var logReport in logReports)
                {
                    logReport.ServiceProviderAppName = "N/A";
                }
            }

            //        foreach (var logReport in logReports)
            //        {
            //            var logMessageArray = logReport.LogMessage.Split('|');
            //            if (logMessageArray.Length > 0)
            //            {
            //	//logReport.DocumentNumber = logMessageArray[0];
            //	logReport.TransactionType = logMessageArray[0];
            //	logReport.ServiceName = logMessageArray[1];
            //	logReport.LogMessage = logMessageArray[2];
            //}
            //            logReport.IsChecksumValid = _logReportService.VerifyChecksum(logReport).Success;
            //        }
            AuthenticationFailedLogViewModel viewModel = new AuthenticationFailedLogViewModel
            {
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                PerPage = searchDetails.PerPage,
                Reports = logReports
            };

            
            logMessage = $"Successfully received Authentication failed reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AuthenticationFailedReports,
                "Get Authentication Failed Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View("Reports", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public JsonResult ExportAuthenticationFailedReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
				StartDate = "",
				EndDate = "",
				TransactionStatus = "FAILED",
				TransactionType = "AUTHENTICATON",
				PerPage = 0,
				TotalCount = 0
			};
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAuthenticationFailedReports"] as string, definition);
            TempData.Keep("SearchAuthenticationFailedReports");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Authentication Log Server
                logMessage = $"Failed to export Authentication failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AuthenticationFailedReports,
                    "Export Authentication Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export Authentication Report", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportAuthenticationFailedReportToFile(exportType, fullName, email, searchDetails.StartDate, searchDetails.EndDate, //searchDetails.DocumentNumber,
					searchDetails.TotalCount);
            });

            return Json(new { Status = "Success", Title = "Export Authentication Report", Message = "Your request has been processed successfully. Please check your email to download the reports" });
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
            return PartialView("_Subscriber", subscriberDetails);
        }

        [HttpPost]
        //[Route("Reports")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reports([FromForm] AuthenticationFailedLogViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View("Reports", viewModel);
                //return View("~/Views/AuthenticationFailedLogsReports/Reports.cshtml", viewModel);
            }

            var logReports = await _logReportService.GetAuthenticationFailedLogReportAsync(
                viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        perPage: viewModel.PerPage.Value);
            if (logReports == null)
            {
               
                logMessage = $"Failed to get Authentication failed reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AuthenticationFailedReports,
                    "Get Authentication Failed Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            var clients = await _clientService.EnumClientIds();

            if (clients != null)
            {
                foreach (var logReport in logReports)
                {
                    // Get Service Provider name by client id
                    if (logReport.ServiceProviderAppName != null)
                        logReport.ServiceProviderAppName
                            = clients.TryGetValue(logReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                    else
                        logReport.ServiceProviderAppName = "N/A";
                }
            }
            else
            {
                foreach (var logReport in logReports)
                {
                    logReport.ServiceProviderAppName = "N/A";
                }
            }

            logMessage = $"Successfully received Authentication failed reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AuthenticationFailedReports,
                "Get Authentication Failed Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SearchAuthenticationFailedReports"] = JsonConvert.SerializeObject(
                new
                {
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
