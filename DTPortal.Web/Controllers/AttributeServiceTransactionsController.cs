using DTPortal.Core;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.ExtensionMethods;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.AdminLogReports;
using DTPortal.Web.ViewModel.AttributeServiceTransactions;
using DTPortal.Web.ViewModel.DataPivot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Transactions")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class AttributeServiceTransactionsController : BaseController
    {

        private readonly IAttributeServiceTransactionsService _attributeServiceTransactionsService;
        
        public AttributeServiceTransactionsController(ILogClient logClient, IAttributeServiceTransactionsService attributeServiceTransactionsService) : base(logClient)
        {
            _attributeServiceTransactionsService= attributeServiceTransactionsService;
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<AttributeServiceViewModel>();
            var transactionsList = await _attributeServiceTransactionsService.GetAttributeServiceTransactionsList();
            transactionsList=transactionsList.Reverse().ToList();
            if (transactionsList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AttributeServiceTransactions, "Get all Attribute Service Transactions List", LogMessageType.FAILURE.ToString(), "Fail to get Attribute Service Transactions List");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AttributeServiceTransactions, "Get all Attribute Service Transactions List", LogMessageType.SUCCESS.ToString(), "Get Attribute Service Transactions list success");

                foreach (var item in transactionsList)
                {
                    
                    viewModel.Add(new AttributeServiceViewModel
                    {
                        Id= item.Id,
                        UserId = item.UserId,
                        ClientName = item.ClientName,
                        RequestDate = item.RequestDate,
                        Status = item.Status,
                        RequestProfile = item.RequestProfile

                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            
            var transactionsDetails = await _attributeServiceTransactionsService.GetDetails(id);
            if (transactionsDetails == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AttributeServiceTransactions, "Get all Attribute Service Transaction Details", LogMessageType.FAILURE.ToString(), "Fail to get Attribute Service Transactions Details");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.AttributeServiceTransactions, "Get all Attribute Service Transaction Details", LogMessageType.SUCCESS.ToString(), "Get Attribute Service Transactions Details success");

                AttributeProfileRequest attributeProfileRequest = new AttributeProfileRequest
                {
                    Id = transactionsDetails.attributeProfileRequest.Id,
                    TransactionId = transactionsDetails.attributeProfileRequest.TransactionId,
                    ClientName = transactionsDetails.attributeProfileRequest.ClientName,
                    RequestDate = transactionsDetails.attributeProfileRequest.RequestDate,
                    RequestProfile = transactionsDetails.attributeProfileRequest.RequestProfile,
                    RequestPurpose = transactionsDetails.attributeProfileRequest.RequestPurpose,
                    UserId = transactionsDetails.attributeProfileRequest.UserId

                };

                AttributeProfileConsent attributeProfileConsent = new AttributeProfileConsent
                {
                    ConsentStatus = transactionsDetails.attributeProfileConsent.ConsentStatus,
                    ApprovedProfileAttributes = transactionsDetails.attributeProfileConsent.ApprovedProfileAttributes,
                    RequestedProfileAttributes = transactionsDetails.attributeProfileConsent.RequestedProfileAttributes,
                    ConsentUpdatedDate = transactionsDetails.attributeProfileConsent.ConsentUpdatedDate
                    

                };
                AttributeProfileStatus attributeProfileStatus = new AttributeProfileStatus
                {
                    Status = transactionsDetails.attributeProfileStatus.Status,
                    FailedReason = transactionsDetails.attributeProfileStatus.FailedReason,
                    //DataPivotId = transactionsDetails.attributeProfileStatus.DataPivotId
                };

                AttributeServiceDetailsViewModel viewModel = new AttributeServiceDetailsViewModel
                {
                    attributeProfileRequest = attributeProfileRequest,
                    attributeProfileConsent = attributeProfileConsent,
                    attributeProfileStatus = attributeProfileStatus,

                };
                return PartialView("_TransactionDetails", viewModel);
            }
           
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Reports()
        {
            return View(new AttributeServiceLogReportViewModel());
        }

        [HttpGet]
        //[Route("Reports/Page/{page}")]
        public async Task<IActionResult> ReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                UserId = "",
                UserIdType = "",
                ApplicationName = "",
                Profile = "",
                StartDate = "",
                EndDate = "",
                PerPage = 0,
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAdminReports"] as string, definition);
            TempData.Keep("SearchAdminReports");

            var logReports = await _attributeServiceTransactionsService.GetAttributeLogReportAsync(searchDetails.StartDate,
                        searchDetails.EndDate,
                        searchDetails.UserId,
                        searchDetails.UserIdType,
                        searchDetails.ApplicationName,
                        searchDetails.Profile,
                        searchDetails.PerPage,
                        page);
            if (logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get admin reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                    "Get Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }



            AttributeServiceLogReportViewModel viewModel = new AttributeServiceLogReportViewModel
            {
                UserId = searchDetails.UserId,
                UserIdType = Enum.TryParse<UserIdentifier>(searchDetails.UserIdType, out var outServiceName) ? outServiceName : (UserIdentifier?)null,
                ApplicationName = searchDetails.ApplicationName,
                Profile = searchDetails.Profile,
                StartDate = Convert.ToDateTime(searchDetails.StartDate),
                EndDate = Convert.ToDateTime(searchDetails.EndDate),
                Reports = logReports
            };

            // Push the log to Admin Log Server
            logMessage = $"Successfully received admin reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                "Get Admin Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View("Reports", viewModel);
        }

        //[HttpGet]
        ////[Route("[action]")]
        //public JsonResult ExportAdminReports(string exportType)
        //{
        //    string logMessage;

        //    var definition = new
        //    {
        //        UserName = "",
        //        ModuleName = "",
        //        StartDate = "",
        //        EndDate = "",
        //        PerPage = 0,
        //        TotalCount = 0
        //    };
        //    var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SearchAdminReports"] as string, definition);
        //    TempData.Keep("SearchAdminReports");

        //    if (searchDetails.TotalCount > 1000000)
        //    {
        //        // Push the log to Admin Log Server
        //        logMessage = $"Failed to export admin report";
        //        SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
        //            "Export Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

        //        return Json(new { Status = "Failed", Title = "Export Admin Report", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
        //    }

        //    string fullName = base.FullName;
        //    string email = base.Email;
        //    _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
        //    {
        //        await sender.ExportAdminReportToFile(exportType, fullName, email, searchDetails.StartDate, searchDetails.EndDate,
        //            searchDetails.UserName, searchDetails.ModuleName, searchDetails.TotalCount);
        //    });

        //    return Json(new { Status = "Success", Title = "Export Admin Report", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        //}

        //[HttpGet]
        ////[Route("[action]")]
        //public PartialViewResult ExportTypes()
        //{
        //    return PartialView("_AdminExportTypes");
        //}

        [HttpPost]
        //[Route("Reports")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reports([FromForm] AttributeServiceLogReportViewModel viewModel)
        
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View("Reports", viewModel);
            }
            var userType = viewModel.UserIdType.Value.ToString();
            var logReports = await _attributeServiceTransactionsService.GetAttributeLogReportAsync(viewModel.StartDate.ToString(), viewModel.EndDate.Value.ToString("yyyy-MM-dd 23:59:59"), viewModel.UserId, userType, viewModel.ApplicationName, viewModel.Profile, viewModel.PerPage);
            if(logReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get admin reports";
                SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                    "Get Admin Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            // Push the log to Admin Log Server
            logMessage = $"Successfully received admin reports";
            SendAdminLog(ModuleNameConstants.ActivityReports, ServiceNameConstants.AdminReports,
                "Get Admin Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SearchAdminReports"] = JsonConvert.SerializeObject(
                new
                {
                    UserId = viewModel.UserId,
                    UserIdType = viewModel.UserIdType.GetValue(),
                    ApplicationName = viewModel.ApplicationName,
                    Profile = viewModel.Profile,
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
