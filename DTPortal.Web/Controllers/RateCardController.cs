using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.RateCard;
using DTPortal.Web.ExtensionMethods;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Price Model")]
    [Authorize(Roles = "Rate Card")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class RateCardController : BaseController
    {
        private readonly IRateCardService _rateCardService;
        private readonly IServiceDefinitionService _serviceDefinitionService;

        public RateCardController(IRateCardService rateCardService,
            IServiceDefinitionService serviceDefinitionService,
            ILogClient logClient) : base(logClient)
        {
            _rateCardService = rateCardService;
            _serviceDefinitionService = serviceDefinitionService;
        }

        [HttpGet]
        //[Route("~/RateCards")]
        public async Task<IActionResult> List()
        {
            string logMessage;

            var rateCards = await _rateCardService.GetAllRateCardsAsync();
            if (rateCards == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get rate cards list";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Get all Rate Cards list", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            RateCardListViewModel viewModel = new RateCardListViewModel
            {
                RateCards = rateCards
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received rate cards list";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                "Get all Rate Cards list", LogMessageType.SUCCESS.ToString(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public IActionResult Add()
        {
            RateCardAddViewModel viewModel = new RateCardAddViewModel();
            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var rateCard = await _rateCardService.GetRateCardAsync(id);
            if (rateCard == null)
            {
                return NotFound();
            }

            RateCardEditViewModel viewModel = new RateCardEditViewModel
            {
                ServiceName = rateCard.ServiceDefinitions.ServiceDisplayName,
                ServiceId = rateCard.ServiceDefinitions.Id,
                //ServiceName = Enum.TryParse<DAESServiceName>(rateCard.ServiceName, out var outServiceName) ? outServiceName : (DAESServiceName?)null,
                Stakeholder = Enum.TryParse<UserType>(rateCard.StakeHolder, out var outServiceFor) ? outServiceFor : (UserType?)null,
                FeePerTransaction = rateCard.Rate,
                RateEffectiveFrom = Convert.ToDateTime(rateCard.RateEffectiveFrom),
                Status = rateCard.Status,
                Tax = rateCard.Tax,
                CreatedBy = rateCard.CreatedBy,
                UpdatedBy = rateCard.UpdatedBy,
                ApprovedBy = rateCard.ApprovedBy
            };

            if(rateCard.RateEffectiveTo != null)
            {
                viewModel.RateEffectiveTo = Convert.ToDateTime(rateCard.RateEffectiveTo);
            }

            return View(viewModel);
        }

        [HttpGet]
        //[Route("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var rateCard = await _rateCardService.GetRateCardAsync(id);
            if (rateCard == null)
            {
                return NotFound();
            }

            RateCardDetailsViewModel viewModel = new RateCardDetailsViewModel
            {
                RateCard = rateCard
            };

            return View("CheckerView", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<JsonResult> GetServiceDefinitions(int value)
        {
            UserType stakeholder = (UserType)value;
            var serviceDefinitions = await _serviceDefinitionService.GetServiceDefinitionsByStakeholderAsync(stakeholder.GetValue());

            List<SelectListItem> listItem = new List<SelectListItem>();

            if(serviceDefinitions != null)
            {
                foreach (var service in serviceDefinitions)
                {
                    listItem.Add(new SelectListItem { Value = service.Id.ToString(), Text = service.ServiceDisplayName });
                }
            }
            return Json(serviceDefinitions);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RateCardAddViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            //if(viewModel.ServiceId != 4 && viewModel.FeePerTransaction == 0)
            //{
            //    AlertViewModel alert = new AlertViewModel { Message = "Fee must be greater than zero for this selected service"};
            //    TempData["Alert"] = JsonConvert.SerializeObject(alert);

            //    return View(viewModel);
            //}

            RateCardDTO rateCard = new RateCardDTO
            {
                StakeHolder = viewModel.Stakeholder.GetValue(),
                Rate = viewModel.FeePerTransaction,
                Tax = viewModel.Tax,
                RateEffectiveFrom = viewModel.RateEffectiveFrom.Value.ToString("yyyy-MM-ddT00:00:00"),
                CreatedBy = UUID,
            };

            rateCard.ServiceDefinitions.Id = viewModel.ServiceId.Value;
            rateCard.ServiceDefinitions.ServiceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == viewModel.ServiceId.Value).Select(x => x.ServiceDisplayName).SingleOrDefault();

            var response = await _rateCardService.AddRateCardAsync(rateCard);
            if (!response.Success)
            {

                // Push the log to Admin Log Server
                logMessage = $"Failed to create rate card for Stakeholder {viewModel.Stakeholder.GetValue()} and Service {rateCard.ServiceDefinitions.ServiceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Create Rate Card", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully created rate card for Stakeholder {viewModel.Stakeholder.GetValue()} and Service {rateCard.ServiceDefinitions.ServiceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Create Rate Card", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        //[Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RateCardEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            //if (viewModel.ServiceId != 4 && viewModel.FeePerTransaction == 0)
            //{
            //    AlertViewModel alert = new AlertViewModel { Message = "Fee must be greater than zero for this service" };
            //    TempData["Alert"] = JsonConvert.SerializeObject(alert);

            //    return View(viewModel);
            //}

            RateCardDTO rateCard = new RateCardDTO
            {
                Id = id,
                StakeHolder = viewModel.Stakeholder.GetValue(),
                Rate = viewModel.FeePerTransaction,
                RateEffectiveFrom = viewModel.RateEffectiveFrom.Value.ToString("yyyy-MM-ddT00:00:00"),
                Status = viewModel.Status,
                Tax = viewModel.Tax,
                CreatedBy = viewModel.CreatedBy,
                UpdatedBy = UUID,
                ApprovedBy = viewModel.ApprovedBy
            };
            rateCard.ServiceDefinitions.Id = viewModel.ServiceId.Value;
            rateCard.ServiceDefinitions.ServiceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == viewModel.ServiceId.Value).Select(x => x.ServiceDisplayName).SingleOrDefault();

            if (viewModel.RateEffectiveTo != null)
            {
                rateCard.RateEffectiveTo = viewModel.RateEffectiveTo.Value.ToString("yyyy-MM-ddT23:59:59");
            }

            ServiceResult response = await _rateCardService.UpdateRateCardAsync(rateCard);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update rate card for Stakeholder {viewModel.Stakeholder.GetValue()} and Service {rateCard.ServiceDefinitions.ServiceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Update Rate Card", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully updated rate card for Stakeholder {viewModel.Stakeholder.GetValue()} and Service {rateCard.ServiceDefinitions.ServiceDisplayName}";

                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.RateCard,
                    "Update Rate Card", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
    }
}
