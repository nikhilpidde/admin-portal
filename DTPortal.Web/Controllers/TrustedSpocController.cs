using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.TrustedSpoc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Trusted Spoc")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class TrustedSpocController : BaseController
    {
        private readonly ITrustedSpocService _trustedSpocService;
        private readonly IControlledOnboardingService _controlledOnboardingService;
        private readonly IConfiguration _configuration;
        public TrustedSpocController(ILogClient logClient,
            ITrustedSpocService trustedSpocService,
            IConfiguration configuration,
            IControlledOnboardingService controlledOnboardingService) : base(logClient)
        {
            _trustedSpocService = trustedSpocService;
            _configuration = configuration;
            _controlledOnboardingService = controlledOnboardingService;
        }

        public async Task<IActionResult> List()
        {
            var trustedUserList = await _trustedSpocService.GetTrustedSpocList();
            if (trustedUserList == null)
            {
                return NotFound();
            }
            var ViewModel = new TrustedUserNewList()
            {
                TrustedSpocList = trustedUserList
            };
            return View(ViewModel);
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddSpoc([FromBody] TrustedSpocModel viewModel)
        {
            if (viewModel == null)
                return BadRequest("Request body is null");

            var requestDto = new TrustedUserRequestDTO
            {
                Name = viewModel.name,
                Email = viewModel.email,
                MobileNumber = viewModel.mobileNumber
            };

            var response = await _trustedSpocService.AddTrustedSpocAsync(requestDto);

            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> AddSpocOld([FromForm] TrustedSopcViewModel viewModel)
        {
            string logMessage;

            TrustedSpocAddDTO trustedSpocAddDTO = new TrustedSpocAddDTO()
            {
                SpocName = viewModel.SpocName,
                SpocEmail = viewModel.SpocEmail,
                MobileNo = viewModel.FullMobileNo,
                IdDocumentNo = viewModel.IdDocumentNo,
                OrganizationName = viewModel.OrganizationName
            };

            //TrustedSpocAddDTO trustedSpocAddDTO = new TrustedSpocAddDTO()
            //{
            //    SpocName = viewModel.SpocName,
            //    SpocEmail = viewModel.SpocEmail,
            //    MobileNo = viewModel.FullMobileNo,
            //    IdDocumentNo = viewModel.IdDocumentNo,
            //    CeoTin = viewModel.CeoTin,
            //    OrganizationName = viewModel.OrganizationName,
            //    OrganizationTin = viewModel.OrganizationTin
            //};

            var response = await _trustedSpocService.AddTrustedSpocAsync1(trustedSpocAddDTO);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to Trusted Spoc ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.TrustedSpoc,
                    "Add trusted spoc ", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel aalert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);

                //return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = response.Message });
                return RedirectToAction("List");
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for adding trusted spoc has sent for approval";
                else
                    logMessage = $"Successfully Trusted Spoc";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.TrustedSpoc,
                  "Add trusted spoc", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");

            }
            //return View();
        }

        public async Task<IActionResult> ViewSpoc(int id)
        {
            var response = await _trustedSpocService.GetSpocDetailsByIDasync(id);
            if (response == null)
            {
                return NotFound();
            }

            string countryCode = string.Empty;
            string mobileNumber = response.MobileNumber;
            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            if (mobileNumber.StartsWith(mobileCountryCode))
            {
                countryCode = mobileCountryCode;
                mobileNumber = mobileNumber.Substring(4);
            }
            else if (mobileNumber.StartsWith("+91"))
            {
                countryCode = "+91";
                mobileNumber = mobileNumber.Substring(3);
            }

            var countryCodeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = mobileCountryCode, Text = mobileCountryCode, Selected = countryCode == mobileCountryCode },
                new SelectListItem { Value = "+91", Text = "+91", Selected = countryCode == "+91" }
            };

            TrustedSopcViewModel model = new TrustedSopcViewModel()
            {
                Id = response.Id,
                SpocEmail = response.SpocEmail,
                SpocName = response.SpocName,
                MobileNo = mobileNumber,
                IdDocumentNo = response.IdDocumentNo,
                CountryCode = countryCode,
                CountryCodeOptions = countryCodeOptions,
                Status = response.Status,
                reInvite = response.ReInvite,
            };

            return View(model);
        }


        public async Task<IActionResult> SuspendSpoc(int id)
        {
            var response = await _trustedSpocService.SuspendSpoc(id);
            if (!response.Success)
            {
                AlertViewModel aalert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);
                return RedirectToAction("List");
                // return Json(new { success = false, message = response == null ? "Internal error please contact to admin" : response.Message });
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
                //return Json(new { success = true, message = response.Message });
            }
            //return null;
        }

        public async Task<IActionResult> ActiveSpoc(int id)
        {
            var response = await _trustedSpocService.RemoveSuspensionSpoc(id);
            if (!response.Success)
            {
                AlertViewModel aalert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);
                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");

            }
            //return null;
        }

        public async Task<IActionResult> ReInviteSpoc(int id)
        {
            var response = await _trustedSpocService.ReInviteSpoc(id);
            if (!response.Success)
            {
                AlertViewModel aalert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(aalert);
                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");

            }
            //return null;
        }

        public async Task<IActionResult> GetDetailsbyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is missing.");
            }

            var result = await _controlledOnboardingService.GetTrustedUserByEmail(email);

            string mobileNumber = result.MobileNumber;
            string countryCode = string.Empty;
            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            if (mobileNumber.StartsWith(mobileCountryCode))
            {
                countryCode = mobileCountryCode;
                mobileNumber = mobileNumber.Substring(4);
            }
            else if (mobileNumber.StartsWith("+91"))
            {
                countryCode = "+91";
                mobileNumber = mobileNumber.Substring(3);
            }

            var countryCodeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = mobileCountryCode, Text = mobileCountryCode, Selected = countryCode == mobileCountryCode },
                new SelectListItem { Value = "+91", Text = "+91", Selected = countryCode == "+91" }
            };

            var responseObject = new 
            {
                documentid = result.IdDocNumber,
                name = result.FullName,
                code = countryCode,
                number = mobileNumber,
            };

            if (result != null)
            {
                return Json(new { success = true, message = "Success", responseObject });
            }
            else
            {
                return Json(new { success = false, message = "Failed to retrieve details.", result = (TrustedSpocEmailDTO)null });
            }            
        }
    }
}
