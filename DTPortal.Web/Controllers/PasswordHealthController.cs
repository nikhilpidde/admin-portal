using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.PasswordHealth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Password Policy")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class PasswordHealthController : BaseController
    {
        private readonly IPasswordPolicyService _passwordPolicyService;

        public PasswordHealthController(ILogClient logClient, IPasswordPolicyService passwordPolicyService) : base(logClient)
        {
            _passwordPolicyService = passwordPolicyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var PasswordDetails = await _passwordPolicyService.GetPasswordPolicyCriteria(1);
            if (PasswordDetails == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PasswordHealth, "Get Password health criteria", LogMessageType.FAILURE.ToString(), "Fail to get Password health criteria");
                return NotFound();
            }

            var model = new PasswordHealthViewModel()
            {
                Id = PasswordDetails.Id,
                enforcepwdhistory = (PasswordDetails.PasswordHistory != 0 ? 1 : 0),
                PasswordHistory = PasswordDetails.PasswordHistory,
                //MinimumPwdAge = PasswordDetails.MinimumPwdAge,
                //MaximumPwdAge = PasswordDetails.MaximumPwdAge,
                MinimumPwdLength = PasswordDetails.MinimumPwdLength,
                MaximumPwdLength = PasswordDetails.MaximumPwdLength,
                PwdContains = PasswordDetails.PwdContains,
                //IsReversibleEncryption = PasswordDetails.IsReversibleEncryption,
                //BadPwdCount = PasswordDetails.BadPwdCount
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PasswordHealth, "Get Password health criteria", LogMessageType.SUCCESS.ToString(), "Get Password policy details successfully");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PasswordHealthViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", viewModel);
            }
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);
            var PasswordDetails = await _passwordPolicyService.GetPasswordPolicyCriteria(viewModel.Id);
            if (PasswordDetails == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PasswordHealth, "Update Password health criteria", LogMessageType.FAILURE.ToString(), "Fail to get Password health criteria in Update");
                return NotFound();
            }

            PasswordDetails.PasswordHistory = viewModel.PasswordHistory;
            //PasswordDetails.MinimumPwdAge = viewModel.MinimumPwdAge;
            //PasswordDetails.MaximumPwdAge = viewModel.MaximumPwdAge;
            PasswordDetails.MinimumPwdLength = viewModel.MinimumPwdLength;
            PasswordDetails.MaximumPwdLength = viewModel.MaximumPwdLength;
            PasswordDetails.PwdContains = viewModel.PwdContains;
            //PasswordDetails.IsReversibleEncryption = viewModel.IsReversibleEncryption;
            //PasswordDetails.BadPwdCount = viewModel.BadPwdCount;
            PasswordDetails.UpdatedBy = id;

            var response = await _passwordPolicyService.UpdatePasswordPolicyCriteria(PasswordDetails);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PasswordHealth, "Update Password policy", LogMessageType.FAILURE.ToString(), "Fail to Update Password policy");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Index", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Update Password policy", LogMessageType.SUCCESS.ToString(), "Updated Password policy successfully");
                Alert alert = new Alert { IsSuccess = true, Message = (string.IsNullOrEmpty(response.Message) ? "Update Password policy successfully" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Index", viewModel);
            }

        }
    }
}
