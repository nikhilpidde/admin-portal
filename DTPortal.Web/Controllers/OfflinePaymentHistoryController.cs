using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.OrganizationPaymentHistory;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Web.ViewModel.OfflinePayment;
using System.Reflection.Metadata.Ecma335;
using DTPortal.Web.ViewModel;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Offline Payment History")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class OfflinePaymentHistoryController : BaseController
    {
        private readonly IOfflinePaymentService _offlinePaymentService;

        public OfflinePaymentHistoryController(ILogClient logClient, 
            IOfflinePaymentService offlinePaymentService) : base(logClient)
        {
            _offlinePaymentService = offlinePaymentService;
        }

        public async Task<IActionResult> List()
        {
            var offlinePaymentList = await _offlinePaymentService.GetAllOfflinePaymentListAsync();

            if (offlinePaymentList == null)
            {
                return NotFound();
            }
            OfflinePaymentListViewModel model = new OfflinePaymentListViewModel
            {
                 CreditAllocations = offlinePaymentList
            };

            return View(model);
            //return View();
        }

        public IActionResult AddPayment()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OfflinePaymentDetails(int id)
        {
            var offlinepaymentdetails = await _offlinePaymentService.GetOfflinePaymentDetailsAsync(id);
            if (offlinepaymentdetails == null) 
            {
                return NotFound();
            }
            OfflinePaymentViewModel model = new OfflinePaymentViewModel
            {
                //CreditAllocations = offlinepaymentdetails
                Id = offlinepaymentdetails.Id,
                OrgName = offlinepaymentdetails.OrgName,
                OrgId = offlinepaymentdetails.OrgId,
                AmountReceived = offlinepaymentdetails.AmountReceived,
                TransactionRefId = offlinepaymentdetails.TransactionRefId,
                InvoiceNo = offlinepaymentdetails.InvoiceNo,
                PaymentChannel= offlinepaymentdetails.PaymentChannel,
                OnlinePaymentGateway= offlinepaymentdetails.OnlinePaymentGateway,
                OnlinePaymentGatewayReferenceNo = offlinepaymentdetails.OnlinePaymentGatewayReferenceNo,
                TotalSigningCredits = offlinepaymentdetails.TotalSigningCredits,    
                TotalEsealCredits = offlinepaymentdetails.TotalEsealCredits,
                TotalUserSubscriptionCredits = offlinepaymentdetails.OnboardingCredits
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] OfflinePaymentViewModel viewModel)
        {
            string logMessage;
            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(viewModel.OrgId))
                {
                    AlertViewModel alert = new AlertViewModel { Message = "Failed to get organization unique identifier" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);                  
                }
                return View("AddPayment",viewModel);
            }
            if(viewModel.TotalEsealCredits <=0 && viewModel.TotalSigningCredits<=0 && viewModel.TotalUserSubscriptionCredits<=0)
            {
                AlertViewModel aalert = new AlertViewModel { Message = "One of the EsealCredits,Signature credits,subscription credits must be greater than zero" };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);
                return View("AddPayment", viewModel);
            }
            CreditAllocationListDTO creditAllocation = new CreditAllocationListDTO()
            {
                OrgName = viewModel.OrgName,
                OrgId = viewModel.OrgId,
                AmountReceived = viewModel.AmountReceived,
                TransactionRefId = viewModel.TransactionRefId,
                InvoiceNo = viewModel.InvoiceNo,
                PaymentChannel = viewModel.PaymentChannel,
                OnlinePaymentGateway = viewModel.OnlinePaymentGateway,
                OnlinePaymentGatewayReferenceNo = viewModel.OnlinePaymentGatewayReferenceNo,
                TotalSigningCredits = viewModel.TotalSigningCredits,
                TotalEsealCredits   = viewModel.TotalEsealCredits,
                OnboardingCredits = viewModel.TotalUserSubscriptionCredits,
                CreatedBy =UUID,
                //CreatedOn = viewModel.CreatedOn,
                //AllocationStatus = viewModel.AllocationStatus,
            };

            var response = await _offlinePaymentService.GetOfflineCredits(creditAllocation);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to add payment history for organization {viewModel.OrgName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                    "Add Organization Payment History", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel aalert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);

                //return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = response.Message });
                return View("AddPayment", viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for add payment history for organization {viewModel.OrgName} has sent for approval";
                else
                    logMessage = $"Successfully added payment history for organization {viewModel.OrgName}";

                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                  "Add Organization Payment History", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
                
            }
        }
        
    }
}
