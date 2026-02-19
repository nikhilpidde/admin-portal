using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel.MobileVersionConfiguration;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.ExtensionMethods;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Settings")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class MobileVersionConfigurationController : BaseController
    {
        private readonly IMobileVersionConfigurationService _mobileVersionConfiguration;

        public MobileVersionConfigurationController(IMobileVersionConfigurationService mobileVersionConfiguration,
            ILogClient logClient) : base(logClient)
        {
            _mobileVersionConfiguration = mobileVersionConfiguration;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            string logMessage;

            var mobileVersions = await _mobileVersionConfiguration.GetAllSupportedMobileVersionsAsync(AccessToken);
            if (mobileVersions == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get mobile versions";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                    "Get all mobile versions list", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            MobileVersionConfigurationListViewModel viewModel = new MobileVersionConfigurationListViewModel
            {
                MobileVersions = mobileVersions
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received mobile versions";
            SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                "Get all mobile versions list", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new MobileVersionConfigurationAddViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mobileVersion = await _mobileVersionConfiguration.GetSupportedMobileVersionByIdAsync(id,AccessToken);
            if (mobileVersion == null)
            {
                return NotFound();
            }

            MobileVersionConfigurationEditViewModel viewModel = new MobileVersionConfigurationEditViewModel
            {
                OSVersion = mobileVersion.OsVersion,
                LatestVersion = mobileVersion.LatestVersion,
                MinimumVersion = mobileVersion.MinimumVersion,
                UpdateLink = mobileVersion.UpdateLink
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] MobileVersionConfigurationAddViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string logMessage;

            MobileVersionDTO onboardingTemplate = new MobileVersionDTO
            {
                OsVersion = viewModel.OSVersion,
                LatestVersion = viewModel.LatestVersion,
                MinimumVersion = viewModel.MinimumVersion,
                UpdateLink = viewModel.UpdateLink
            };

            var response = await _mobileVersionConfiguration.AddSupportedMobileVersionAsync(onboardingTemplate, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create mobile version configuration for {viewModel.OSVersion}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                    "Create Mobile Version Configuration", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully created mobile version configuration for {viewModel.OSVersion}";

                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                    "Create Mobile Version Configuration", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] MobileVersionConfigurationEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            MobileVersionDTO requestBodyDTO = new MobileVersionDTO
            {
                Id = id,
                OsVersion = viewModel.OSVersion,
                LatestVersion = viewModel.LatestVersion,
                MinimumVersion = viewModel.MinimumVersion,
                UpdateLink = viewModel.UpdateLink
            };

            var response = await _mobileVersionConfiguration.UpdateSupportedMobileVersionAsync(requestBodyDTO, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update mobile version configuration for {viewModel.OSVersion}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                    "Update Mobile Version Configuration", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully updated mobile version configuration for {viewModel.OSVersion}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MobileVersionConfiguration,
                     "Update Mobile Version Configuration", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
    }
}
