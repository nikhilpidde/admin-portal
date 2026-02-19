using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel.SubscriberBalanceSheet;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Subscriber Balance Sheet")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class SubscriberBalanceSheetController : BaseController
    {
        private readonly ISubscriberBalanceSheetService _subscriberBalanceSheetService;
        private readonly ISubscriberService _subscriberService;
        public SubscriberBalanceSheetController(ISubscriberBalanceSheetService subscriberBalanceSheetService,
            ISubscriberService subscriberService,
            ILogClient logClient) : base(logClient)
        {
            _subscriberBalanceSheetService = subscriberBalanceSheetService;
            _subscriberService = subscriberService;
        }

        [HttpGet]
        public IActionResult SearchBalanceSheet()
        {
            return View(new SubscriberBalanceSheetViewModel());
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<string[]> GetSubscribers(int type, string value)
        {
            return await _subscriberService.GetSubscribersNamesAysnc(type, value);
        }

        [HttpPost]
        public async Task<IActionResult> SearchBalanceSheet([FromForm] SubscriberBalanceSheetViewModel viewModel)
        {
            string logMessage;

            var balanceSheetDetails = await _subscriberBalanceSheetService.GetSubscriberBalanceSheet((int)viewModel.IdentifierType, viewModel.IdentifierValue);
            if (balanceSheetDetails == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get balance sheet for subscriber with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.SubscriberBalanceSheet,
                    "Get Subscriber Balance Sheet", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received balance sheet for subscriber with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.SubscriberBalanceSheet,
                "Get Subscriber Balance Sheet", LogMessageType.SUCCESS.ToString(), logMessage);

            viewModel.BalanceSheetDetails = balanceSheetDetails;
            return View(viewModel);
        }
    }
}
