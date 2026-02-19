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

namespace DTPortal.Web.Controllers
{
    //[Authorize(Roles = "Price Model")]
    [Authorize(Roles = "Payment History")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class OrganizationPaymentHistoryController : BaseController
    {
        private readonly IOrganizationPaymentHistoryService _paymentHistoryService;
        private readonly IOrganizationService _organizationService;
        private readonly IServiceDefinitionService _serviceDefinitionService;

        public OrganizationPaymentHistoryController(IOrganizationPaymentHistoryService paymentHistoryService,
            ILogClient logClient,
            IOrganizationService organizationService,
            IServiceDefinitionService serviceDefinitionService) : base(logClient)
        {
            _paymentHistoryService = paymentHistoryService;
            _organizationService = organizationService;
            _serviceDefinitionService = serviceDefinitionService;
        }

        [HttpGet]
        public IActionResult OrganizationPaymentHistory()
        {
            return View(new OrganizationPaymentHistoryViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizationPaymentHistory(string organizationName, string organizationUid)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(organizationUid))
                {
                    return Json(new { Status = "Failed", Title = "Get Organization Payment History", Message = "Failed to get organization unique identifier" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Get Organization Payment History", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var organizationPaymentHistory = await _paymentHistoryService.GetOrganizationPaymentHistoryAsync(organizationUid);
            if (organizationPaymentHistory == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get payment history for organization {organizationName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                    "Get Organization Payment History", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received payment history for organization {organizationName}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                "Get Organization Payment History", LogMessageType.SUCCESS.ToString(), logMessage);

            return PartialView("_OrganizationPaymentHistory", organizationPaymentHistory);
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

        [HttpGet]
        public IActionResult Add()
        {
            return PartialView("_AddOrganizationPaymentHistory", new AddOrganizationPaymentHistoryViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentInfo(string paymentInfo)
        {
            var viewModel = JsonConvert.DeserializeObject<IList<OrganizationPaymentInfoViewModel>>(paymentInfo);
            foreach (var item in viewModel)
            {
                item.ServiceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == Convert.ToInt32(item.ServiceId)).Select(x => x.ServiceDisplayName).SingleOrDefault();
            }

            

            return PartialView("_PaymentInfo",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Add([FromForm] AddOrganizationPaymentHistoryViewModel viewModel)
        {
            string logMessage;
            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(viewModel.OrganizationId))
                {
                    return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = "Failed to get organization unique identifier" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            OrganizationPaymentHistoryDTO organizationPaymentHistory = new OrganizationPaymentHistoryDTO
            {
                OrganizationId = viewModel.OrganizationId,
                OrganizationName = viewModel.OrganizationName,
                PaymentInfo = viewModel.PaymentInfo,
                PaymentChannel = viewModel.PaymentChannel.GetValue(),
                TotalAmountPaid = viewModel.TotalAmountPaid,
                TransactionReferenceId = viewModel.TransactionReferenceId,
                InvoiceNumber = viewModel.InvoiceNumber,
                CreatedBy = UUID
            };

            var response = await _paymentHistoryService.AddOrganizationPaymentHistoryAsync(organizationPaymentHistory);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to add payment history for organization {viewModel.OrganizationName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                    "Add Organization Payment History", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for add payment history for organization {viewModel.OrganizationName} has sent for approval";
                else
                    logMessage = $"Successfully added payment history for organization {viewModel.OrganizationName}";

                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                  "Add Organization Payment History", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Add Organization Payment History", Message = response.Message });
            }
        }
    }
}
