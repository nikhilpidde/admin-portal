using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.SigningServiceReport;

using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class SigningServiceReportController : BaseController
    {
        private readonly ILogReportService _logReportService;
        public SigningServiceReportController(ILogClient logClient,
            ILogReportService logReportService) : base(logClient)
        {
            _logReportService = logReportService;
        }


        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new SigningServiceReportViewModel());
        }

        [HttpPost]
        //[Route("Reports")]
        public async Task<IActionResult> Reports([FromForm] SigningServiceReportViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Reports", viewModel);
            }

            var logReports = await _logReportService.GetSigningServiceLogReportAsync(viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.Identifier,
                        viewModel.ServiceName,
                        viewModel.TransactionType,
                        viewModel.SignatureType,
                        viewModel.ESealUsed,
                        perPage: viewModel.PerPage.Value);
            if (logReports == null)
            {
                SendAdminLog("Electronic Signatures", "Reports", "Get all reports", LogMessageType.FAILURE.GetValue(), "Failed to get reports");
                return NotFound();
            }

            SendAdminLog("Electronic Signatures", "Reports", "Get all reports", LogMessageType.SUCCESS.GetValue(), "Get reports success");

            TempData["SearchDetails"] = JsonConvert.SerializeObject(
                new
                {
                    Identifier = viewModel.Identifier,
                    ServiceName = viewModel.ServiceName,
                    TransactionType = viewModel.TransactionType,
                    SignatureType = viewModel.SignatureType,
                    ESealUsed = viewModel.ESealUsed,
                    StartDate = viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                    PerPage = viewModel.PerPage,
                    TotalCount = logReports.TotalCount
                });
            viewModel.Reports = logReports;
            return View("Reports", viewModel);
        }



        [HttpGet]
        //[Route("Reports/Page/{page}")]
        public async Task<IActionResult> ReportsByPage(int page)
        {
            var definition = new
            {
                Identifier = "",
                ServiceName = "",
                TransactionType = "",
                SignatureType = "",
                ESealUsed = false,
                StartDate = "",
                EndDate = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchDetails"] as string, definition);

            var logReports = await _logReportService.GetSigningServiceLogReportAsync(searchDetails.StartDate,
                         searchDetails.EndDate,
                        searchDetails.Identifier,
                        searchDetails.ServiceName,
                        searchDetails.TransactionType,
                        searchDetails.SignatureType,
                        searchDetails.ESealUsed,
                        page,
                        searchDetails.PerPage);
            if (logReports == null)
            {
                SendAdminLog("Electronic Signatures", "Reports", "Get All Reports", LogMessageType.FAILURE.GetValue(), "Failed to get reports");
                return NotFound();
            }

            SigningServiceReportViewModel viewModel = new SigningServiceReportViewModel
            {
                Identifier = searchDetails.Identifier,
                ServiceName = searchDetails.ServiceName,
                TransactionType = searchDetails.TransactionType,
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                Reports = logReports
            };

            SendAdminLog("Electronic Signatures", "Reports", "Get All Reports", LogMessageType.SUCCESS.GetValue(), "Get reports success");

            TempData["SearchDetails"] = JsonConvert.SerializeObject(searchDetails);
            return View("Reports", viewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        public async Task<IActionResult> Export()
        {
            PaginatedList<LogReportDTO> logReports = null;
            var definition = new
            {
                Identifier = "",
                ServiceName = "",
                TransactionType = "",
                SignatureType = "",
                ESealUsed = false,
                StartDate = "",
                EndDate = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchDetails"] as string, definition);

            TempData.Keep("SearchDetails");

            int downloadPerPage = 100;
            int totalPages = (searchDetails.TotalCount / downloadPerPage) + 1;

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("_Id"),
                                        new DataColumn("Subscriber Identifier"),
                                        new DataColumn("Transaction ID"),
                                        new DataColumn("Service Name"),
                                        new DataColumn("Start Time"),
                                        new DataColumn("End Time"),
                                        new DataColumn("Log Message"),
                                        new DataColumn("Status"),
                                        new DataColumn("Service Provider Name"),
                                        new DataColumn("Service Provider AppName"),
                                        new DataColumn("__V")
                                        });

            logReports = await _logReportService.GetSigningServiceLogReportAsync(searchDetails.StartDate,
                         searchDetails.EndDate,
                        searchDetails.Identifier,
                        searchDetails.ServiceName,
                        searchDetails.TransactionType,
                        searchDetails.SignatureType,
                        searchDetails.ESealUsed,
                        1,
                        perPage: searchDetails.TotalCount);
            if (logReports == null)
            {
                return NotFound();
            }

            foreach (var report in logReports)
            {
                dt.Rows.Add(report._id, report.Identifier, report.TransactionID, report.ServiceName,
                    report.StartTime, report.EndTime, report.LogMessage, report.LogMessageType,
                    report.ServiceProviderName, report.ServiceProviderAppName, report.__v);
            }

            var array = ExportToPDF(dt);
            return File(array, "application/pdf", $"Report_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}.pdf");
        }

        [HttpPost]
        //[Route("[action]")]
        public JsonResult VerifyChecksum([FromBody] LogReportDTO logReport)
        {
            var response = _logReportService.VerifyChecksum(logReport);
            if (!response.Success)
            {
                return Json(new { Status = "Failed", Message = response.Message });
            }
            else
            {
                return Json(new { Status = "Success", Message = response.Message });
            }
        }
    }
}
