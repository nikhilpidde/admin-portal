using DTPortal.Core.Domain.Services;
using DTPortal.Core.Services;
using DTPortal.Web.ViewModel.OrganizationPaymentHistory;
using DTPortal.Web.ViewModel.SubscriberPaymentHistory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Subscriber Payment History")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class SubscriberPaymentHistoryController : BaseController
    {
        private readonly ISubscriberPaymentHistoryService _subscriberPaymentHistoryService;
        private readonly ISubscriberService _subscriberService;
        private readonly IServiceDefinitionService _serviceDefinitionService;
        public SubscriberPaymentHistoryController(ISubscriberPaymentHistoryService subscriberPaymentHistoryService,
            ISubscriberService subscriberService,
            IServiceDefinitionService serviceDefinitionService,
            ILogClient logClient) : base(logClient)
        {
            _subscriberPaymentHistoryService = subscriberPaymentHistoryService;
            _subscriberService = subscriberService;
            _serviceDefinitionService = serviceDefinitionService;
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View(new SubscriberPaymentHistoryViewModel());
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<string[]> GetSubscribers(int type, string value)
        {
            return await _subscriberService.GetSubscribersNamesAysnc(type, value);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentInfo(string paymentInfo)
        {
            var viewModel = JsonConvert.DeserializeObject<IList<SubscriberPaymentInfoViewModel>>(paymentInfo);
            foreach (var item in viewModel)
            {
                item.ServiceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == Convert.ToInt32(item.ServiceId)).Select(x => x.ServiceDisplayName).SingleOrDefault();
            }

            return PartialView("_PaymentInfo", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(SubscriberPaymentHistoryViewModel viewModel)
        {
            string logMessage;

            var subscriberPaymentHistory = await _subscriberPaymentHistoryService.GetSubscriberPaymentHistoryAsync((int)viewModel.IdentifierType, viewModel.IdentifierValue);
            if (subscriberPaymentHistory == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get payment history for subscriber with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.SubscriberPaymentHistory,
                    "Get Subscriber Payment History", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received payment history for subscriber with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.SubscriberPaymentHistory,
                "Get Subscriber Payment History", LogMessageType.SUCCESS.ToString(), logMessage);

            viewModel.SubscriberPaymentHistory = subscriberPaymentHistory;
            return View(viewModel);
        }
    }
}
