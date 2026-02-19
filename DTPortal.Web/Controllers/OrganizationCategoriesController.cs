using System.Collections.Generic;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.OrganizationCategories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace DTPortal.Web.Controllers
{
    public class OrganizationCategoriesController : BaseController
    {

        private readonly IOrganizationCategoriesService _organizationCategoriesService;
        private readonly ILogger<OrganizationCategoriesController> _logger;

        public OrganizationCategoriesController(ILogClient logClient, ILogger<OrganizationCategoriesController> logger,
            IOrganizationCategoriesService organizationCategoriesService) : base(logClient)
        {
            _organizationCategoriesService = organizationCategoriesService;
            _logger = logger;

        }
        public async Task<IActionResult> Index()
        {
            var response = await _organizationCategoriesService.GetAllCategories();
            if (response == null)
            {
                return NotFound();
            }
            var OrgCategoryList = (IEnumerable<SelfServiceCategoryDTO>)response.Resource;
            var model = new OrganizationCategoriesListViewModel()
            {
                OrgCatogeryFieldList = OrgCategoryList
            };
            return View(model);
        }


        public IActionResult New()
        {
            var viewModel = new OrganizationCategoriesAddViewModel
            {
                OrgCategoryName = "",
                OrgCategoryId = 0
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {

            var response = await _organizationCategoriesService.GetAllCategoryFields();
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Message });
            }
            var fieldDetails = (List<OrganizationFieldDTO>)response.Resource;

            //List<SelfServiceFieldDTO> fieldDetails = new List<SelfServiceFieldDTO>();

            return Json(new { success = true, message = response.Message, result = fieldDetails });

        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _organizationCategoriesService.GetCategoryFieldNameById(id);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Message });
            }
            var fields = (OrgCategoryFieldDetailsDTO)response.Resource;
            var model = new OrganizationCategoriesEditViewModel
            {
                OrgCategoryName = fields.OrgCategoryName,
                OrgCategoryId = fields.OrgCategoryId
                // You can add OrgCategoryName too if your view model supports it
            };// get basic category data
            return View(model); // returns to Edit.cshtml Razor view
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryFieldById(int id)
        {

            var response = await _organizationCategoriesService.GetCategoryFieldNameById(id);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Message });
            }
            var fields = (OrgCategoryFieldDetailsDTO)response.Resource;
            List<SelfServiceFieldDTO> fieldDetails = fields.organisationFieldDtos;

            return Json(new { success = true, message = response.Message, result = fieldDetails });

        }

        [HttpPost]
        public async Task<IActionResult> SaveCategory([FromBody] OrganizationCategoriesListViewModel model)
        {
            var organizationCategoryAddDto = new OrganizationCategoryAddRequestDTO();
            organizationCategoryAddDto.OrgCategoryName = model.OrgCategoryName;
            List<OrganisationFieldAddDto> organisationFieldAddDtos = new List<OrganisationFieldAddDto>();
            foreach (var item in model.organisationFieldDtos)
            {
                OrganisationFieldAddDto organisationFieldAddDto = new OrganisationFieldAddDto()
                {
                    fieldId = item.fieldId,
                    visibility = item.visibility,
                    mandatory = item.mandatory,
                    fieldName = item.fieldName
                };
                organisationFieldAddDtos.Add(organisationFieldAddDto);
            }
            organizationCategoryAddDto.OrganisationFieldDtos = organisationFieldAddDtos;
            var response = await _organizationCategoriesService.SaveCatogeryFields(organizationCategoryAddDto);
            if (response.Success)
            {
                return Json(new { Success = true, Message = response.Message });
            }
            else
            {
                return Json(new { Success = false, Message = response.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] OrganisationCategoryViewModel model)
        {
            OrgCategoryFieldDetailsDTO dto = new OrgCategoryFieldDetailsDTO();
            dto.OrgCategoryId = model.OrgCategoryId;
            dto.OrgCategoryName = model.OrgCategoryName;
            dto.organisationFieldDtos = model.organisationFieldDtos;
            var response = await _organizationCategoriesService.UpdateCatogeryFields(dto);
            if (response.Success)
            {
                return Json(new { Success = true, Message = response.Message });
            }
            else
            {
                return Json(new { Success = false, Message = response.Message });
            }

        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _organizationCategoriesService.DeleteCategoryAsync(id);
            if (response != null && response.Success)
            {

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Wallet Purpose Configuration", LogMessageType.SUCCESS.ToString(), "Delete Wallet Purpose Configuration successfully");
                return new JsonResult(true);
            }
            else
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                //SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Wallet Purpose Configuration", LogMessageType.FAILURE.ToString(), "Failed to Delete Wallet Purpose Configuration");
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return new JsonResult(false);
            }
        }
    }
}
