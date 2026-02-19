using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.Constants;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.PriceSlabDefinition;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Generic Price Slab")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class PriceSlabDefinitionController : BaseController
    {
        private readonly IPriceSlabDefinitionService _priceSlabDefinitionService;
        private readonly IServiceDefinitionService _serviceDefinitionService;

        public PriceSlabDefinitionController(IPriceSlabDefinitionService priceSlabDefinitionService,
            IServiceDefinitionService serviceDefinitionService,
            ILogClient logClient) : base(logClient)
        {
            _priceSlabDefinitionService = priceSlabDefinitionService;
            _serviceDefinitionService = serviceDefinitionService;
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            string logMessage;

            var priceSlabDefinitions = await _priceSlabDefinitionService.GetAllPriceSlabDefinitionsAsync();
            if (priceSlabDefinitions == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get generic price slabs list";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                    "Get all Generic Price Slabs list", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            PriceSlabDefinitionListViewModel viewModel = new PriceSlabDefinitionListViewModel();
            var list = priceSlabDefinitions.DistinctBy(x => new { x.Stakeholder, x.ServiceDefinitions.Id }).Select(y => new { y.ServiceDefinitions.Id, y.ServiceDefinitions.ServiceDisplayName, y.Stakeholder, y.ServiceDefinitions.Status }).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                viewModel.PriceSlabs.Add(new PriceSlabViewModel { ServiceId = list[i].Id, ServiceName = list[i].ServiceDisplayName, Stakeholder = list[i].Stakeholder, Status = list[i].Status });
            }

            // Push the log to Admin Log Server
            logMessage = "Successfully received generic price slabs list";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                "Get all Generic Price Slabs list", LogMessageType.SUCCESS.ToString(), logMessage);

            return View("List", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<JsonResult> GetServiceDefinitions(int value)
        {
            UserType stakeholder = (UserType)value;
            var serviceDefinitions = (await _serviceDefinitionService.GetServiceDefinitionsByStakeholderAsync(stakeholder.GetValue())).Where(x => x.PricingSlabApplicable == true);

            List<SelectListItem> listItem = new List<SelectListItem>();

            if (serviceDefinitions != null)
            {
                foreach (var service in serviceDefinitions)
                {
                    listItem.Add(new SelectListItem { Value = service.Id.ToString(), Text = service.ServiceDisplayName });
                }
            }
            return Json(serviceDefinitions);
        }

        [HttpGet]
        public IActionResult Add()
        {
            PriceSlabDefinitionAddViewModel viewModel = new PriceSlabDefinitionAddViewModel();

            return View("Add", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int serviceId, string stakeholder)
        {
            var priceSlabs = await _priceSlabDefinitionService.GetPriceSlabDefinitionAsync(serviceId, stakeholder);
            if (priceSlabs == null)
            {
                return NotFound();
            }

            PriceSlabDefinitionEditViewModel viewModel = new PriceSlabDefinitionEditViewModel();
            if (priceSlabs.Count() > 0)
            {
                PriceSlabDefinitionDTO priceSlab = priceSlabs[0];
                viewModel.ServiceId = priceSlab.ServiceDefinitions.Id;
                viewModel.ServiceDisplayName = priceSlab.ServiceDefinitions.ServiceDisplayName;
                viewModel.Stakeholder = priceSlab.Stakeholder;
                viewModel.CreatedBy = priceSlab.CreatedBy;

                for (int i = 0; i < priceSlabs.Count(); i++)
                {
                    viewModel.DiscountVolumeRanges.Add(new DiscountVolumeRangeDTO
                    {
                        Id = priceSlabs[i].Id,
                        VolumeRangeFrom = priceSlabs[i].VolumeRangeFrom,
                        VolumeRangeTo = priceSlabs[i].VolumeRangeTo,
                        Discount = priceSlabs[i].Discount
                    });
                }
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PriceSlabDefinitionAddViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            IList<PriceSlabDefinitionDTO> priceSlabDefinitions = new List<PriceSlabDefinitionDTO>();
            string serviceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == viewModel.ServiceId.Value).Select(x => x.ServiceDisplayName).SingleOrDefault();
            for (int i = 0; i < viewModel.DiscountVolumeRanges.Count; i++)
            {
                PriceSlabDefinitionDTO priceSlab = new PriceSlabDefinitionDTO
                {
                    VolumeRangeFrom = viewModel.DiscountVolumeRanges[i].VolumeRangeFrom,
                    VolumeRangeTo = viewModel.DiscountVolumeRanges[i].VolumeRangeTo,
                    Discount = viewModel.DiscountVolumeRanges[i].Discount,
                    Stakeholder = viewModel.Stakeholder.GetValue()
                };
                priceSlab.ServiceDefinitions.Id = viewModel.ServiceId.Value;
                priceSlab.ServiceDefinitions.ServiceDisplayName = serviceDisplayName;
                priceSlab.CreatedBy = UUID;
                priceSlabDefinitions.Add(priceSlab);
            }

            var response = await _priceSlabDefinitionService.AddPriceSlabDefinitionAsync(priceSlabDefinitions);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create generic price slab for Stakeholder {viewModel.Stakeholder.GetValue()} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                    "Create Generic Price Slab", LogMessageType.FAILURE.ToString(), logMessage);

               AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully created generic price slab for Stakeholder {viewModel.Stakeholder.GetValue()} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                    "Create Generic Price Slab", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PriceSlabDefinitionEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            IList<PriceSlabDefinitionDTO> priceSlabDefinitions = new List<PriceSlabDefinitionDTO>();
            string serviceDisplayName = viewModel.ServiceDisplayName;
            for (int i = 0; i < viewModel.DiscountVolumeRanges.Count; i++)
            {
                PriceSlabDefinitionDTO priceSlab = new PriceSlabDefinitionDTO
                {
                    VolumeRangeFrom = viewModel.DiscountVolumeRanges[i].VolumeRangeFrom,
                    VolumeRangeTo = viewModel.DiscountVolumeRanges[i].VolumeRangeTo,
                    Discount = viewModel.DiscountVolumeRanges[i].Discount,
                    Id = viewModel.DiscountVolumeRanges[i].Id,
                    Stakeholder = viewModel.Stakeholder
                };
                priceSlab.ServiceDefinitions.Id = viewModel.ServiceId.Value;
                priceSlab.ServiceDefinitions.ServiceDisplayName = serviceDisplayName;
                priceSlab.UpdatedBy = UUID;
                priceSlabDefinitions.Add(priceSlab);
            }

            var response = await _priceSlabDefinitionService.UpdatePriceSlabDefinitionAsync(priceSlabDefinitions);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update generic price slab for Stakeholder {viewModel.Stakeholder} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                    "Update Generic Price Slab", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully updated generic price slab for Stakeholder {viewModel.Stakeholder} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.GenericPriceSlab,
                    "Update Generic Price Slab", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
    }
}
