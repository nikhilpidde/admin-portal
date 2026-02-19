using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.Reports;

using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class ReportsController : BaseController
    {
        private readonly ILogReportService _logReportService;

        public ReportsController(ILogReportService logReportService,
            ILogClient logClient) : base(logClient)
        {
            _logReportService = logReportService;
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new ReportsSearchViewModel());
        }

        [HttpPost]
        //[Route("[action]")]
        public async Task<IActionResult> Reports([FromForm] ReportsSearchViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            PaginatedList<LogReportDTO> logReports = null;
            switch (viewModel.ReportType)
            {
                case "RA":
                    logReports = await _logReportService.GetRegistrationAuthorityLogReportAsync(viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.Identifier,
                        viewModel.ServiceName,
                        viewModel.TransactionType.GetValue(),
                        viewModel.SignatureType.GetValue(),
                        viewModel.ESealUsed,
                        perPage: viewModel.PerPage.Value);
                    if (logReports == null)
                    {
                        return NotFound();
                    }
                    break;

                case "Onboarding":
                    logReports = await _logReportService.GetOnboardingLogReportAsync(viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                        viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.Identifier,
                        viewModel.ServiceName,
                        viewModel.TransactionType.GetValue(),
                        perPage: viewModel.PerPage.Value);
                    if (logReports == null)
                    {
                        return NotFound();
                    }
                    break;
            }

            TempData["SearchDetails"] = JsonConvert.SerializeObject(
                new
                {
                    ReportType = viewModel.ReportType,
                    Identifier = viewModel.Identifier,
                    ServiceName = viewModel.ServiceName,
                    TransactionType = viewModel.TransactionType.GetValue(),
                    SignatureType = viewModel.SignatureType.GetValue(),
                    ESealUsed = viewModel.ESealUsed,
                    StartDate = viewModel.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                    PerPage = viewModel.PerPage,
                    TotalCount = logReports.TotalCount
                });

            viewModel.Reports = logReports;
            return View(viewModel);
        }

        [HttpGet]
        //[Route("Reports/Page/{page}")]
        public async Task<IActionResult> ReportsByPage(int page)
        {
            PaginatedList<LogReportDTO> logReports = null;
            var definition = new
            {
                ReportType = "",
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

            switch (searchDetails.ReportType)
            {
                case "RA":
                    logReports = await _logReportService.GetRegistrationAuthorityLogReportAsync(searchDetails.StartDate,
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
                        return NotFound();
                    }
                    break;

                case "Onboarding":
                    logReports = await _logReportService.GetOnboardingLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
                        searchDetails.Identifier,
                        searchDetails.ServiceName,
                        searchDetails.TransactionType,
                        page,
                        searchDetails.PerPage);
                    if (logReports == null)
                    {
                        return NotFound();
                    }
                    break;
            }

            ReportsSearchViewModel viewModel = new ReportsSearchViewModel
            {
                ReportType = searchDetails.ReportType,
                Identifier = searchDetails.Identifier,
                ServiceName = searchDetails.ServiceName,
                TransactionType = Enum.TryParse<TransactionType>(searchDetails.TransactionType, out var outTransactionType) ? outTransactionType : null,
                SignatureType = Enum.TryParse<SignatureType>(searchDetails.SignatureType, out var outSignatureType) ? outSignatureType : null,
                ESealUsed = searchDetails.ESealUsed,
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                PerPage = searchDetails.PerPage,
                Reports = logReports
            };

            TempData["SearchDetails"] = JsonConvert.SerializeObject(searchDetails);
            return View("Reports", viewModel);
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

        [HttpPost]
        //[Route("[action]")]
        public async Task<IActionResult> Export()
        {
            PaginatedList<LogReportDTO> logReports = null;
            var definition = new
            {
                ReportType = "",
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
            
            switch (searchDetails.ReportType)
            {
                case "RA":
                    logReports = await _logReportService.GetRegistrationAuthorityLogReportAsync(searchDetails.StartDate,
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
                    break;

                case "Onboarding":
                    logReports = await _logReportService.GetOnboardingLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
                        searchDetails.Identifier,
                        searchDetails.ServiceName,
                        searchDetails.TransactionType,
                        1,
                        perPage: searchDetails.TotalCount);
                    if (logReports == null)
                    {
                        return NotFound();
                    }
                    break;
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
    }
}
