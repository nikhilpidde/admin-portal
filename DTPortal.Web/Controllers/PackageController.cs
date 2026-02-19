using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.Package;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Price Model")]
    [Authorize(Roles = "Package")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class PackageController : BaseController
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService,
            ILogClient logClient) : base(logClient)
        {
            _packageService = packageService;
        }

        [HttpGet]
        //[Route("~/Packages")]
        public async Task<IActionResult> List()
        {
            string logMessage;

            var packages = await _packageService.GetAllPackagesAsync();
            if (packages == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get Package list";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Get all Package list", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            PackageListViewModel viewModel = new PackageListViewModel
            {
                Packages = packages
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received Package list";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                "Get all Package list", LogMessageType.SUCCESS.ToString(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Add()
        {
            return View(new PackageAddViewModel());
        }

        [HttpGet]
        //[Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var package = await _packageService.GetPackageAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            PackageEditViewModel viewModel = new PackageEditViewModel
            {
                PackageCode = package.PackageCode,
                PackageDescription = package.PackageDescription,
                ServiceFor = Enum.TryParse<UserType>(package.ServiceFor, out var outServiceFor) ? outServiceFor : (UserType?)null,
                TotalSigningTransactions = package.TotalSigningTransactions,
                TotalESealTransactions = package.TotalESealTransactions,
                DiscounOnSigningTransactions = package.DiscountOnSigningTransactions,
                DiscounOnESealTransactions = package.DiscountOnESealTransactions,
                TaxPercentage = package.TaxPercentage,
                Status = package.Status,
                CreatedBy = package.CreatedBy,
                UpdatedBy = package.UpdatedBy,
                ApprovedBy = package.ApprovedBy
            };

            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var package = await _packageService.GetPackageAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            PackageDetailsViewModel viewModel = new PackageDetailsViewModel
            {
                Package = package
            };

            return View("CheckerView", viewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(PackageAddViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            PackageDTO package = new PackageDTO
            {
                PackageCode = viewModel.PackageCode,
                PackageDescription = viewModel.PackageDescription,
                ServiceFor = viewModel.ServiceFor.GetValue(),
                TotalSigningTransactions = viewModel.TotalSigningTransactions,
                TotalESealTransactions = viewModel.TotalESealTransactions,
                DiscountOnSigningTransactions = viewModel.DiscounOnSigningTransactions,
                DiscountOnESealTransactions = viewModel.DiscounOnESealTransactions,
                TaxPercentage = viewModel.TaxPercentage,
                CreatedBy = UUID
            };

            var response = await _packageService.AddPackageAsync(package);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create package with Code {viewModel.PackageCode} for Service {viewModel.ServiceFor.GetValue()}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.Package,
                    "Create Package", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully created package with Code {viewModel.PackageCode} for Service {viewModel.ServiceFor.GetValue()}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.Package,
                    "Create Package", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PackageEditViewModel viewModel, string action)
        {
            string logMessage;
            string actionType;
            string activityName;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            ServiceResult response;
            switch (action)
            {
                case "Enable":
                    actionType = "enable";
                    activityName = "Update Package";
                    response = await _packageService.EnablePackageAsync(id, UUID);
                    break;

                case "Disable":
                    actionType = "disable";
                    activityName = "Disable Package";
                    response = await _packageService.DisablePackageAsync(id, UUID);
                    break;

                case "Delete":
                    actionType = "delete";
                    activityName = "Delete Package";
                    response = await _packageService.DeletePackageAsync(id, UUID);
                    break;

                default:
                    actionType = "unknown action";
                    activityName = "Package Activity";
                    response = new ServiceResult(false, "Unknown action performed");
                    break;
            }

            if (!response.Success)
            {
                // Push the log to Admin Log Server
                if (actionType == "unknown action")
                {
                    logMessage = response.Message;
                }
                else
                {
                    logMessage = $"Failed to {actionType} package with Code {viewModel.PackageCode} for Service {viewModel.ServiceFor.GetValue()}";
                }
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.Package,
                    activityName, LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully {actionType}d package with Code {viewModel.PackageCode} for Service {viewModel.ServiceFor.GetValue()}";

                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.Package,
                    activityName, LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
    }
}
