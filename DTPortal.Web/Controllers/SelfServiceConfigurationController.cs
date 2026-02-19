using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel.Beneficiary;
using DTPortal.Web.ViewModel.SelfServiceConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class SelfServiceConfigurationController : BaseController
    {

        private readonly ISelfServiceConfigurationService _selfServiceConfigurationService;
        private readonly ILogger<SelfServiceConfigurationController> _logger;

        public SelfServiceConfigurationController(ILogClient logClient, ILogger<SelfServiceConfigurationController> logger, 
            ISelfServiceConfigurationService selfServiceConfigurationService) : base(logClient)
        {
            _selfServiceConfigurationService = selfServiceConfigurationService;
            _logger = logger;

        }
        public async Task<IActionResult> Index()
        {
            var response = await _selfServiceConfigurationService.GetAllConfigCategories();
            if (response == null)
            {
                return NotFound();
            }
            var OrgCategoryList = (IEnumerable<SelfServiceCategoryDTO>)response.Resource;
            var model = new SelfServiceCategoryViewModel()
            {
                OrgCatogeryFieldList = OrgCategoryList
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryFieldById(int id)
        {

            var response = await _selfServiceConfigurationService.GetCategoryFieldNameById(id);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Message });
            }
            var fields = (OrgCategoryFieldDetailsDTO) response.Resource;
            List<SelfServiceFieldDTO> fieldDetails = fields.organisationFieldDtos;

            return Json(new { success = true, message = response.Message, result = fieldDetails });
            
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] SelfServiceCategoryViewModel model)
        {
            OrgCategoryFieldDetailsDTO dto = new OrgCategoryFieldDetailsDTO();
            dto.OrgCategoryId = model.OrgCategoryId;
            dto.organisationFieldDtos = model.organisationFieldDtos;
            var response = await _selfServiceConfigurationService.UpdateCatogeryFields(dto);
            if (response.Success)
            {
                return Json(new { Success = true, Message = response.Message });
            }
            else
            {
                return Json(new { Success = false, Message = response.Message });
            }
           
        }
    }
}
