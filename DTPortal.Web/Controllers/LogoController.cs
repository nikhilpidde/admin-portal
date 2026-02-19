using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Logo;

using DTPortal.Core.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Settings")]
    [Authorize(Roles = "Update Logo")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class LogoController : BaseController
    {
        private readonly ILogoService _logoService;

        public LogoController(ILogoService logoService,
            ILogClient logClient) : base(logClient)
        {
            _logoService = logoService;
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<JsonResult> GetLogoImage()
        {
            var settings = await _logoService.GetLogoPrimary();
            if (settings != null)
            {
                return Json(Encoding.ASCII.GetString(settings.Value));
            }

            return Json(null);
        }

        [HttpGet]
        //[Route("[action]")]
        [Authorize(Roles = "Settings")]
        [Authorize(Roles = "Update Logo")]
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Settings")]
        [Authorize(Roles = "Update Logo")]
        public async Task<IActionResult> Update(LogoUpdateViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.LogoPrimary == null || viewModel.LogoPrimary.Length == 0)
            {
                AlertViewModel alert = new AlertViewModel { Message = "Uploaded file is empty or null" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }

            string base64Image;
            using (var stream = new MemoryStream())
            {
                viewModel.LogoPrimary.CopyTo(stream);
                var fileBytes = stream.ToArray();
                base64Image = Convert.ToBase64String(fileBytes);
            }

            if (!String.IsNullOrEmpty(base64Image))
            {
                base64Image = "data:image/svg+xml;base64," + base64Image;
                var response = await _logoService.UpdateLogoPrimary(base64Image, UUID);
                if (!response.Success)
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Failed to update logo";
                    SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Logo,
                        "Update Logo", LogMessageType.FAILURE.GetValue(), logMessage);

                    AlertViewModel alert = new AlertViewModel { Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                }
                else
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Logo updated successfully";
                    SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.Logo,
                        "Update Logo", LogMessageType.SUCCESS.GetValue(), logMessage);

                    AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                }
            }

            return View(viewModel);
        }
    }
}
