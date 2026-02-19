using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel.ControlledOnboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Controlled Onboarding")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class ControlledOnboardingController : BaseController
    {
        private readonly IControlledOnboardingService _controlledOnboardingService;
        private IWebHostEnvironment _environment;
        public ControlledOnboardingController(ILogClient logClient,
            IControlledOnboardingService controlledOnboardingService,
            IWebHostEnvironment environment) : base(logClient)
        {
            _controlledOnboardingService = controlledOnboardingService;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View(new ControlledOnboardingSearchViewModel());
        }

        [HttpGet]
        public IActionResult DulicateEmailList(string data)
        {

            DuplicateEmailViewModel duplicateEmailViewModel = new DuplicateEmailViewModel();
            duplicateEmailViewModel.EmailList = JsonConvert.DeserializeObject<IList<string>>(data.Replace("\r\n",""));
            return PartialView("_duplicateEmails",duplicateEmailViewModel);
        }
        [HttpGet]
        public IActionResult DownloadCSV()
        {
            // Get the path to the CSV file on the server
            string csvFilePath = Path.Combine(_environment.WebRootPath, "Samples/SampleControlledOnboarding.csv");

            // Check if the file exists
            if (!System.IO.File.Exists(csvFilePath))
            {
                return NotFound(); // or return appropriate error response
            }

            // Set the response content type and headers
            string contentType = "text/csv";
            string fileName = Path.GetFileName(csvFilePath);
            return PhysicalFile(csvFilePath, contentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> AddTrustedUsers([FromBody]ControlledOnboardingIndexViewModel dataList)
        {
            string logMessage;

            if(dataList.UserList.Count == 0)
            {
                return Json(new { Status = "Failed", Title = "Add Trusted Users", Message = "User list cannot be null" });
            }

            foreach(var data in dataList.UserList)
            {
                if (string.IsNullOrEmpty(data.Name))
                {
                    return Json(new { Status = "Failed", Title = "Add Trusted Users", Message = "Name cannot be null" });
                }
                if (string.IsNullOrEmpty(data.Email))
                {
                    return Json(new { Status = "Failed", Title = "Add Trusted Users", Message = "Email cannot be null" });
                }
                if (string.IsNullOrEmpty(data.MobileNo))
                {
                    return Json(new { Status = "Failed", Title = "Add Trusted Users", Message = "Mobile number cannot be null" });
                }
            }

            ControlledOnboardingDTO controlledOnboarding = new ControlledOnboardingDTO()
            {
                Emails = dataList.UserList,
                CreatedBy = UUID
            };

            var response = await _controlledOnboardingService.AddTrustedUsersAsync(controlledOnboarding);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to add trusted users";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                    "Add Trusted Users", LogMessageType.FAILURE.ToString(), logMessage);

                if(response.Resource != null)
                {
                    DuplicateEmailViewModel duplicateEmailViewModel = new DuplicateEmailViewModel();
                    duplicateEmailViewModel.EmailList = JsonConvert.DeserializeObject<IList<string>>(response.Resource.ToString());
                    return PartialView("_duplicateEmails", duplicateEmailViewModel);
                }

                return Json(new { Status = "Failed", Title = "Add Trusted Users", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for add trusted users has sent for approval";
                else
                    logMessage = $"Successfully added trusted users";

                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                  "Add Trusted Users", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Add Trusted Users", Message = response.Message });
            }
        }
    }
}
