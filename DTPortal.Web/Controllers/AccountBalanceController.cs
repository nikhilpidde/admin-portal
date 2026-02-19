using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.AccountBalance;

using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Price Model")]
    [Authorize(Roles = "Account Balance")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class AccountBalanceController : BaseController
    {
        private readonly IAccountBalanceService _accountBalanceService;

        public AccountBalanceController(IAccountBalanceService accountBalanceService,
            ILogClient logClient) : base(logClient)
        {
            _accountBalanceService = accountBalanceService;
        }

        [HttpGet]
        //[Route("Search")]
        public IActionResult Search()
        {
            return View(new AccountBalanceSearchViewModel());
        }

        [HttpPost]
        //[Route("Search")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromForm] AccountBalanceSearchViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            object accountBalanceDetails;
            if (viewModel.IdentifierType.GetValue() == UserType.ORGANIZATION.GetValue())
            {
                accountBalanceDetails = await _accountBalanceService.GetServiceProviderAccountBalanceAsync(viewModel.UID);
            }
            else
            {
                accountBalanceDetails = await _accountBalanceService.GetSubscriberAccountBalanceAsync(viewModel.UID);
            }


            if (accountBalanceDetails == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get account balance details for {viewModel.IdentifierType.GetValue()} with UID {viewModel.UID}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.AccountBalance,
                    "Get Account Balance Details", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received account balance details for {viewModel.IdentifierType.GetValue()} with UID {viewModel.UID}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.AccountBalance,
                "Get Account Balance Details", LogMessageType.SUCCESS.ToString(), logMessage);
            viewModel.AccountBalanceDetails = accountBalanceDetails;
            return View(viewModel);
        }
    }
}
