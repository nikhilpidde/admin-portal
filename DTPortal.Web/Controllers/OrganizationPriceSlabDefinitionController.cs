using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.Constants;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Organization Price Slab")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class OrganizationPriceSlabDefinitionController : BaseController
    {
        private readonly IOrganizationPriceSlabDefinitionService _orgPriceSlabDefinitionService;
        private readonly IServiceDefinitionService _serviceDefinitionService;
        private readonly IOrganizationService _organizationService;

        public OrganizationPriceSlabDefinitionController(IOrganizationPriceSlabDefinitionService orgPriceSlabDefinitionService,
            IServiceDefinitionService serviceDefinitionService,
            IOrganizationService organizationService,
            ILogClient logClient) : base(logClient)
        {
            _orgPriceSlabDefinitionService = orgPriceSlabDefinitionService;
            _serviceDefinitionService = serviceDefinitionService;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            string logMessage;

            var priceSlabDefinitions = await _orgPriceSlabDefinitionService.GetAllPriceSlabDefinitionsAsync();
            if (priceSlabDefinitions == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get organization price slabs list";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                    "Get all Organization Price Slabs list", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            OrganizationPriceSlabDefinitionListViewModel viewModel = new OrganizationPriceSlabDefinitionListViewModel();
            var list = priceSlabDefinitions.DistinctBy(
                x => new { x.OrganizationUid, x.ServiceDefinitions.Id })
                .Select(y => new { y.ServiceDefinitions.Id, y.ServiceDefinitions.ServiceDisplayName, y.OrganizationUid, y.ServiceDefinitions.Status })
                .ToList();

            var organizations = await GetOrganizationList();

            for (int i = 0; i < list.Count; i++)
            {
                viewModel.PriceSlabs.Add(
                    new PriceSlabViewModel
                    {
                        ServiceId = list[i].Id,
                        ServiceName = list[i].ServiceDisplayName,
                        OrganizationUid = list[i].OrganizationUid,
                        OrganizationName = organizations.Where(x => x.Value == list[i].OrganizationUid).Select(x => x.Text).SingleOrDefault(),
                        Status = list[i].Status
                    });
            }
            logMessage = "Successfully received organization price slabs list";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                "Get all Organization Price Slabs list", LogMessageType.SUCCESS.ToString(), logMessage);

            return View("List", viewModel);
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
        public async Task<IActionResult> Add()
        {
            OrganizationPriceSlabDefinitionAddViewModel viewModel = new OrganizationPriceSlabDefinitionAddViewModel();
            viewModel.ServiceNames = (await _serviceDefinitionService.GetServiceDefinitionsByStakeholderAsync(UserType.ORGANIZATION.GetValue())).Where(x => x.PricingSlabApplicable == true);

            return View("Add", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(OrganizationPriceSlabDefinitionAddViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(viewModel.OrganizationUid))
                {
                    AlertViewModel alert = new AlertViewModel { Message = "Failed to get organization unique identifier" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                }

                viewModel.ServiceNames = (await _serviceDefinitionService.GetServiceDefinitionsByStakeholderAsync(UserType.ORGANIZATION.GetValue())).Where(x => x.PricingSlabApplicable == true);
                return View(viewModel);
            }

            IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions = new List<OrganizationPriceSlabDefinitionDTO>();
            string organizationName = viewModel.OrganizationName;
            string serviceDisplayName = (await _serviceDefinitionService.GetServiceDefinitionsAsync()).Where(x => x.Id == viewModel.ServiceId.Value).Select(x => x.ServiceDisplayName).SingleOrDefault();
            for (int i = 0; i < viewModel.DiscountVolumeRanges.Count; i++)
            {
                OrganizationPriceSlabDefinitionDTO priceSlab = new OrganizationPriceSlabDefinitionDTO
                {
                    VolumeRangeFrom = viewModel.DiscountVolumeRanges[i].VolumeRangeFrom,
                    VolumeRangeTo = viewModel.DiscountVolumeRanges[i].VolumeRangeTo,
                    Discount = viewModel.DiscountVolumeRanges[i].Discount,
                    OrganizationUid = viewModel.OrganizationUid
                };
                priceSlab.ServiceDefinitions.Id = viewModel.ServiceId.Value;
                priceSlab.ServiceDefinitions.ServiceDisplayName = serviceDisplayName;
                priceSlab.OrganizationName = organizationName;
                priceSlab.CreatedBy = UUID;
                priceSlabDefinitions.Add(priceSlab);
            }

            var response = await _orgPriceSlabDefinitionService.AddPriceSlabDefinitionAsync(priceSlabDefinitions);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create price slab for Organization {organizationName} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                    "Create Organization Price Slab", LogMessageType.FAILURE.ToString(), logMessage);

                viewModel.ServiceNames = (await _serviceDefinitionService.GetServiceDefinitionsByStakeholderAsync(UserType.ORGANIZATION.GetValue())).Where(x => x.PricingSlabApplicable == true);
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully created price slab for Organization {organizationName} and service {serviceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                    "Create Organization Price Slab", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int serviceId, string organizationUid)
        {
            var priceSlabs = await _orgPriceSlabDefinitionService.GetPriceSlabDefinitionAsync(serviceId, organizationUid);
            if (priceSlabs == null)
            {
                return NotFound();
            }

            OrganizationPriceSlabDefinitionEditViewModel viewModel = new OrganizationPriceSlabDefinitionEditViewModel();
            if (priceSlabs.Count() > 0)
            {
                OrganizationPriceSlabDefinitionDTO priceSlab = priceSlabs[0];
                viewModel.ServiceId = priceSlab.ServiceDefinitions.Id;
                viewModel.ServiceDisplayName = priceSlab.ServiceDefinitions.ServiceDisplayName;
                viewModel.OrganizationUID = priceSlab.OrganizationUid;
                viewModel.OrganizationName = (await GetOrganizationList()).Where(x => x.Value == viewModel.OrganizationUID).Select(x => x.Text).FirstOrDefault(); ;
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
        public async Task<IActionResult> Edit(int id, OrganizationPriceSlabDefinitionEditViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
               return View(viewModel);
            }

            IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions = new List<OrganizationPriceSlabDefinitionDTO>();
            for (int i = 0; i < viewModel.DiscountVolumeRanges.Count; i++)
            {
                OrganizationPriceSlabDefinitionDTO priceSlab = new OrganizationPriceSlabDefinitionDTO
                {
                    VolumeRangeFrom = viewModel.DiscountVolumeRanges[i].VolumeRangeFrom,
                    VolumeRangeTo = viewModel.DiscountVolumeRanges[i].VolumeRangeTo,
                    Discount = viewModel.DiscountVolumeRanges[i].Discount,
                    OrganizationUid = viewModel.OrganizationUID
                };
                priceSlab.ServiceDefinitions.Id = viewModel.ServiceId.Value;
                priceSlab.ServiceDefinitions.ServiceDisplayName = viewModel.ServiceDisplayName;
                priceSlab.OrganizationName = viewModel.OrganizationName;
                priceSlab.UpdatedBy = UUID;
                priceSlabDefinitions.Add(priceSlab);
            }

            var response = await _orgPriceSlabDefinitionService.UpdatePriceSlabDefinitionAsync(priceSlabDefinitions);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update price slab for Organization {viewModel.OrganizationName} and service {viewModel.ServiceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                    "Update Organization Price Slab", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                logMessage = $"Successfully updated price slab for Organization {viewModel.OrganizationName} and service {viewModel.ServiceDisplayName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPriceSlab,
                    "Update Organization Price Slab", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        private async Task<IList<SelectListItem>> GetOrganizationList()
        {
            IList<SelectListItem> list = new List<SelectListItem>();

            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            if (result == null)
            {
                return list;
            }
            else
            {
                foreach (var org in result)
                {
                    var orgobj = org.Split(",");
                    list.Add(new SelectListItem { Text = orgobj[0], Value = orgobj[1] });
                }

                return list;
            }
        }
    }
}
