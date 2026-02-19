using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Consent;
using DTPortal.Web.ExtensionMethods;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Settings")]
    [Authorize(Roles = "Consent")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class ConsentController : BaseController
    {
        private readonly IConsentService _applicationConsentService;

        public ConsentController(IConsentService applicationConsentService,
            ILogClient logClient) : base(logClient)
        {
            _applicationConsentService = applicationConsentService;
        }

        [HttpGet]
        //[Route("~/Consents")]
        public async Task<IActionResult> List()
        {
            string logMessage;

            var consents = await _applicationConsentService.GetAllConsentsAsync(AccessToken);
            if (consents == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get Consent list";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Get all Consent list", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            ConsentListViewModel viewModel = new ConsentListViewModel
            {
                Consents = consents
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received Consent list";
            SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                "Get all Consent list", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Add()
        {
            return View(new ConsentAddViewModel());
        }

        [HttpGet]
        //[Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var consent = await _applicationConsentService.GetConsentAsync(id,AccessToken);
            if (consent == null)
            {
                return NotFound();
            }

            ConsentEditViewModel viewModel = new ConsentEditViewModel
            {
                Consent = consent.Consent,
                PrivacyConsent = consent.PrivacyConsent,
                ConsentType = consent.ConsentType.ToEnum<ConsentType>(),
                ConsentRequired = consent.ConsentRequired
            };

            return View(viewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] ConsentAddViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            ConsentRequestBodyDTO consentDTO = new ConsentRequestBodyDTO
            {
                Consent = viewModel.Consent,
                PrivacyConsent = viewModel.PrivacyConsent,
                ConsentType = viewModel.ConsentType.GetValue(),
                ConsentRequired = viewModel.ConsentRequired,
                DataPrivacy = viewModel.DataPrivacy,
                TermsAndConditions = viewModel.TermsAndConditions
            };

            var response = await _applicationConsentService.AddConsentAsync(consentDTO, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create consent for {viewModel.ConsentType.GetValue()}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Create Consent", LogMessageType.FAILURE.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully created consent for {viewModel.ConsentType.GetValue()}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Create Consent", LogMessageType.SUCCESS.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] ConsentEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            ConsentRequestBodyDTO requestBodyDTO = new ConsentRequestBodyDTO
            {
                ConsentId = id,
                Consent = viewModel.Consent,
                PrivacyConsent = viewModel.PrivacyConsent,
                ConsentType = viewModel.ConsentType.GetValue(),
                DataPrivacy = viewModel.DataPrivacy,
                TermsAndConditions = viewModel.TermsAndConditions,
                ConsentRequired = viewModel.ConsentRequired
            };

            var response = await _applicationConsentService.UpdateConsentAsync(requestBodyDTO, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update consent for {viewModel.ConsentType}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Update Consent", LogMessageType.FAILURE.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully updated consent for {viewModel.ConsentType.GetValue()}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Update Consent", LogMessageType.SUCCESS.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EnableConsent(int id)
        {
            string logMessage;

            var response = await _applicationConsentService.EnableConsentAsync(id, AccessToken);
            if (!response.Success)
            {
                string consentType = string.Empty;
                var consent = await _applicationConsentService.GetConsentAsync(id, AccessToken);
                if (consent != null)
                {
                    consentType = consent.ConsentType;
                }

                // Push the log to Admin Log Server
                logMessage = $"Failed to enable consent for {consentType}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Enable Consent", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Enable Consent", Message = response.Message });
            }
            else
            {
                string consentType = string.Empty;
                var consent = await _applicationConsentService.GetConsentAsync(id, AccessToken);
                if (consent != null)
                {
                    consentType = consent.ConsentType;
                }

                // Push the log to Admin Log Server
                logMessage = $"Successfully enabled consent for {consentType}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Enable Consent", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Enable Consent", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisableConsent(int id)
        {
            string logMessage;

            var response = await _applicationConsentService.DisableConsentAsync(id, AccessToken);
            if (!response.Success)
            {
                string consentType = string.Empty;
                var consent = await _applicationConsentService.GetConsentAsync(id, AccessToken);
                if (consent != null)
                {
                    consentType = consent.ConsentType;
                }

                // Push the log to Admin Log Server
                logMessage = $"Failed to disable consent for {consentType}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Disable Consent", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Disable Consent", Message = response.Message });
            }
            else
            {
                string consentType = string.Empty;
                var consent = await _applicationConsentService.GetConsentAsync(id, AccessToken);
                if (consent != null)
                {
                    consentType = consent.ConsentType;
                }

                // Push the log to Admin Log Server
                logMessage = $"Successfully disabled consent for {consentType}";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Consent,
                    "Disable Consent", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Disable Consent", Message = response.Message });
            }
        }
    }
}
