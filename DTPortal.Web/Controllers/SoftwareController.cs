using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Web.Attribute;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Software;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [ServiceFilter(typeof(SessionValidationAttribute))]
    [RequestSizeLimit(500 * 1024 * 1024)] //500MB
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)] //500MB
    public class SoftwareController : Controller
    {
        private readonly ISoftwareService _softwareService;
        private readonly ILogger<SoftwareController> _logger;

        public SoftwareController(ISoftwareService softwareService,
            ILogger<SoftwareController> logger)
        {
            _softwareService = softwareService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> ListNew()
        {
            var softwareList = await _softwareService.GetAllSoftwareListNewAsync();
            if (softwareList == null)
            {
                return NotFound();
            }

            SoftwareListNewViewModel viewModel = new SoftwareListNewViewModel
            {
                SoftwareList = softwareList
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var softwareList = await _softwareService.GetAllSoftwareListAsync();
            if (softwareList == null)
            {
                return NotFound();
            }

            SoftwareListViewModel viewModel = new SoftwareListViewModel
            {
                SoftwareList = softwareList
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PublishSoftware(int id)
        {
            var response = await _softwareService.PublishUnpublishSoftwareAsync(id, "Publish");
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UnpublishSoftware(int id)
        {
            var response = await _softwareService.PublishUnpublishSoftwareAsync(id, "Unpublish");
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public IActionResult UploadSoftware()
        {
            UploadSoftwareViewModel viewModel = new UploadSoftwareViewModel()
            {

            };
            return View("UploadSoftware", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSoftware(UploadSoftwareViewModel viewModel)
        {
            _logger.LogInformation("Upload Software Controller Start");
            UploadSoftwareDTO softwareDTO = new UploadSoftwareDTO
            {
                SoftwareZip = viewModel.SoftwareZip,
              //  Mannual = viewModel.Mannual,
                //SoftwareName = viewModel.SoftwareName,
                SoftwareVersion = viewModel.SoftwareVersion
            };

            if (int.TryParse(viewModel.SoftwareName, out int softwareNameValue))
            {

                if (Enum.IsDefined(typeof(SoftwareName), softwareNameValue))
                {
                    SoftwareName selectedEnum = (SoftwareName)softwareNameValue;
                    softwareDTO.SoftwareName = selectedEnum.ToString();
                }
            }

            var response = await _softwareService.UploadSoftwareAsync(softwareDTO);
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                _logger.LogInformation("Upload Software Controller End");
                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                _logger.LogInformation("Upload Software Controller End");
                return RedirectToAction("List");
            }
        }
    }
}
