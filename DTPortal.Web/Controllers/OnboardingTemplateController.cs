using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.OnboardingTemplate;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Registration Authority")]
    [Authorize(Roles = "Template Management")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class OnboardingTemplateController : BaseController
    {
        private readonly IOnboardingTemplateService _onboardingTemplatesService;
        private readonly ILogReportService _reportsService;

        public OnboardingTemplateController(IOnboardingTemplateService onboardingTemplatesService,
            ILogReportService reportsService,
            ILogClient logClient) : base(logClient)
        {
            _onboardingTemplatesService = onboardingTemplatesService;
            _reportsService = reportsService;
        }

        [HttpGet]
        //[Route("~/OnboardingTemplates")]
        public async Task<IActionResult> List()
        {
            string logMessage;
            var templates = await _onboardingTemplatesService.GetAllTemplatesAsync(AccessToken);
            if (templates == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get Onboarding Template list";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Get all Onboarding Template list", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            OnbaordingTemplateListViewModel viewModel = new OnbaordingTemplateListViewModel
            {
                Templates = templates
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received Onboarding Template list";
            SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                "Get all Onboarding Template list", LogMessageType.SUCCESS.ToString(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        //[Route("Add")]
        public async Task<IActionResult> Add()
        {
            OnboardingTemplateAddViewModel viewModel = new OnboardingTemplateAddViewModel
            {
                OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken),
                OnboardingSteps = await GetOnboardingSteps()
            };

            return View(viewModel);
        }

        [HttpGet]
        //[Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _onboardingTemplatesService.GetTemplateAsync(id, AccessToken);
            if (template == null)
            {
                return NotFound();
            }

            OnboardingTemplateEditViewModel viewModel = new OnboardingTemplateEditViewModel
            {
                TemplateName = template.TemplateName,
                MethodName = template.TemplateMethod,
                PublishedStatus = template.PublishedStatus,
                OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken),
                OnboardingSteps = await GetOnboardingSteps(template.Steps),
                SelectedOnboardingSteps = template.Steps.ToList(),
                CreatedBy = template.CreatedBy
            };

            return View(viewModel);
        }

        [HttpGet]
        //[Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var template = await _onboardingTemplatesService.GetTemplateAsync(id, AccessToken);
            if (template == null)
            {
                return NotFound();
            }

            OnboardingTemplateDetailsViewModel viewModel = new OnboardingTemplateDetailsViewModel
            {
                OnboardingTemplate = template
            };

            return View("View", viewModel);
        }

        [HttpPost]
        //[Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] OnboardingTemplateAddViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }

            string logMessage;

            if (viewModel.SelectedOnboardingSteps.Count <= 0)
            {
                AlertViewModel alert = new AlertViewModel { Message = "Please select atleast one Onboarding Step to proceed." };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }

            IList<OnboardingStepDTO> onboardingSteps = new List<OnboardingStepDTO>();
            foreach (var step in viewModel.SelectedOnboardingSteps)
            {
                onboardingSteps.Add(new OnboardingStepDTO
                {
                    OnboardingStepId = step.OnboardingStepId,
                    OnboardingStep = step.OnboardingStep,
                    OnboardingStepDisplayName = step.OnboardingStepDisplayName,
                    OnboardingStepThreshold = step.OnboardingStepThreshold,
                    AndriodTFliteThreshold = step.AndriodTFliteThreshold,
                    AndriodDTTThreshold = step.AndriodDTTThreshold,
                    IosTFliteThreshold = step.IosTFliteThreshold,
                    IosDTTThreshold = step.IosDTTThreshold,
                    IntegrationUrl = step.IntegrationUrl
                });
            }

            OnboardingTemplateDTO onboardingTemplate = new OnboardingTemplateDTO
            {
                //TemplateName = viewModel.TemplateName,
                TemplateName = viewModel.TemplateName?.Trim(),
                TemplateMethod = viewModel.MethodName,
                CreatedBy = UUID,
                Steps = onboardingSteps
            };

            var response = await _onboardingTemplatesService.AddTemplateAsync(onboardingTemplate, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create template with name {viewModel.TemplateName} using method {viewModel.MethodName}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Create Template", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for add template {viewModel.TemplateName} has sent for approval";
                else
                    logMessage = $"Successfully created template with name {viewModel.TemplateName} using method {viewModel.MethodName}";

                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                  "Create Template", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] OnboardingTemplateEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }

            IList<OnboardingStepDTO> onboardingSteps = new List<OnboardingStepDTO>();
            foreach (var step in viewModel.SelectedOnboardingSteps)
            {
                onboardingSteps.Add(new OnboardingStepDTO
                {
                    OnboardingStepId = step.OnboardingStepId,
                    OnboardingStep = step.OnboardingStep,
                    OnboardingStepDisplayName = step.OnboardingStepDisplayName,
                    OnboardingStepThreshold = step.OnboardingStepThreshold,
                    AndriodTFliteThreshold = step.AndriodTFliteThreshold,
                    AndriodDTTThreshold = step.AndriodDTTThreshold,
                    IosTFliteThreshold = step.IosTFliteThreshold,
                    IosDTTThreshold = step.IosDTTThreshold,
                    IntegrationUrl = step.IntegrationUrl
                });
            }

            OnboardingTemplateDTO onboardingTemplate = new OnboardingTemplateDTO
            {
                TemplateId = id,
                TemplateName = viewModel.TemplateName,
                TemplateMethod = viewModel.MethodName,
                Steps = onboardingSteps,
                CreatedBy = viewModel.CreatedBy,
                UpdatedBy = UUID
            };

            if (viewModel.SelectedOnboardingSteps.Count <= 0)
            {
                AlertViewModel alert = new AlertViewModel { Message = "Please select atleast one Onboarding Step to proceed." };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }

            ServiceResult response = await _onboardingTemplatesService.UpdateTemplateAsync(onboardingTemplate, AccessToken);
            if (!response.Success)
            {
                logMessage = $"Failed to update template {viewModel.TemplateName}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Update Template", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                viewModel.OnboardingMethods = await _onboardingTemplatesService.GetAllMethodsAsync(AccessToken);
                viewModel.OnboardingSteps = await GetOnboardingSteps();
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for update template {viewModel.TemplateName} has sent for approval";
                else
                {
                    logMessage = $"Template {viewModel.TemplateName} updated successfully";
                }

                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Update Template", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Publish(int id)
        {
            string logMessage;

            var response = await _onboardingTemplatesService.PublishTemplateAsync(id, UUID, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to publish template";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Publish Template", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Publish Template", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for publish template has sent for approval";
                else
                {
                    var template = await _onboardingTemplatesService.GetTemplateAsync(id, AccessToken);
                    logMessage = $"Template {template.TemplateName} published successfully";
                }
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                   "Publish Template", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Publish Template", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Unpublish(int id)
        {
            string logMessage;

            var response = await _onboardingTemplatesService.UnPublishTemplateAsync(id, UUID, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to unpublish template";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Unpublish Template", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Unpublish Template", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for unpublish template has sent for approval";
                else
                {
                    var template = await _onboardingTemplatesService.GetTemplateAsync(id, AccessToken);
                    logMessage = $"Template {template.TemplateName} unpublished successfully";
                }
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                   "Unpublish Template", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Unpublish Template", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            string logMessage;

            var response = await _onboardingTemplatesService.DeleteTemplateAsync(id, UUID, AccessToken);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to delete template";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                    "Delete Template", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Delete Template", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for delete template has sent for approval";
                else
                    logMessage = $"Successfully deleted template";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.TemplateManagement,
                   "Delete Template", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Delete Template", Message = response.Message });
            }
        }

        [NonAction]
        private async Task<List<OnboardingStepListItem>> GetOnboardingSteps(IEnumerable<OnboardingStepDTO> onboardingSteps = null)
        {
            List<OnboardingStepListItem> onboardingStepsItems = new List<OnboardingStepListItem>();
            var allSteps = await _onboardingTemplatesService.GetAllStepsAsync(AccessToken);

            if (onboardingSteps != null)
            {
                foreach (var step in allSteps)
                {
                    var onboardingStep = onboardingSteps.SingleOrDefault(x => x.OnboardingStepId == step.OnboardingStepId);

                    if (onboardingStep != null)
                    {
                        onboardingStepsItems.Add(new OnboardingStepListItem
                        {
                            Id = onboardingStep.OnboardingStepId,
                            DisplayName = onboardingStep.OnboardingStep,
                            IsSelected = true
                        });
                    }
                    else
                    {
                        onboardingStepsItems.Add(new OnboardingStepListItem
                        {
                            Id = step.OnboardingStepId,
                            DisplayName = step.OnboardingStepDisplayName,
                            IsSelected = false
                        });
                    }
                }
            }
            else
            {
                if (allSteps != null)
                {
                    foreach (var step in allSteps)
                    {
                        onboardingStepsItems.Add(new OnboardingStepListItem()
                        {
                            Id = step.OnboardingStepId,
                            DisplayName = step.OnboardingStepDisplayName,
                            IsSelected = false
                        });
                    }
                }
            }

            return onboardingStepsItems;
        }
    }
}
