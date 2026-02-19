using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.UserClaims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Attributes")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class UserClaimsController : BaseController
    {
        private readonly IUserClaimService _userClaimService;

        public UserClaimsController(ILogClient logClient, IUserClaimService userClaimService) : base(logClient)
        {
            _userClaimService = userClaimService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<UserClaimsListViewModel>();

            var CliamList = await _userClaimService.ListUserClaimAsync();
            if (CliamList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Get all UserClaims Configuration List", LogMessageType.FAILURE.ToString(), "Fail to get UserClaims Configuration list");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Get all UserClaims Configuration List", LogMessageType.SUCCESS.ToString(), "Get UserClaims Configuration list success");

                foreach (var item in CliamList)
                {
                    viewModel.Add(new UserClaimsListViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DisplayName = item.DisplayName,
                        Description = item.Description,
                        DefaultClaim = item.DefaultClaim,
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var UserCliamInDb = await _userClaimService.GetUserClaimAsync(id);
            if (UserCliamInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "View UserClaims Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get UserClaims Configuration details");
                return NotFound();
            }

            var userClaimEditViewModel = new UserClaimsEditViewModel
            {
                Id = UserCliamInDb.Id,
                Name = UserCliamInDb.Name,
                DisplayName = UserCliamInDb.DisplayName,
                Description = UserCliamInDb.Description,
                UserConsent = UserCliamInDb.UserConsent,
                DefaultClaim = UserCliamInDb.DefaultClaim,
                Metadata = UserCliamInDb.MetadataPublish,

            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "View UserClaims Configuration details", LogMessageType.SUCCESS.ToString(), "Get UserClaims Configuration details of " + UserCliamInDb.DisplayName + " successfully ");

            return View(userClaimEditViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Save(UserClaimsNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("New", viewModel);
            }

            var userclaim = new UserClaim()
            {
                Name = viewModel.Name,
                DisplayName = viewModel.DisplayName,
                Description = viewModel.Description,
                UserConsent = viewModel.UserConsent,
                DefaultClaim = viewModel.DefaultClaim,
                MetadataPublish = viewModel.Metadata,
                CreatedBy = UUID,
                UpdatedBy = UUID
            };

            var response = await _userClaimService.CreateUserClaimAsync(userclaim);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Create new UserClaims Configuration", LogMessageType.FAILURE.ToString(), "Fail to create UserClaims Configuration");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Create new UserClaims Configuration", LogMessageType.SUCCESS.ToString(), "Created New UserClaims Configuration with name " + viewModel.DisplayName + " Successfully");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Update(UserClaimsEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", viewModel);
            }



            var UserCliamInDb = await _userClaimService.GetUserClaimAsync(viewModel.Id);
            if (UserCliamInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Update UserClaims Configuration", LogMessageType.FAILURE.ToString(), "Fail to get UserClaims Configuration details");
                ModelState.AddModelError(string.Empty, "UserClaims Configuration not found");

                return View("Edit", viewModel);
            }


            UserCliamInDb.Id = viewModel.Id;
            UserCliamInDb.Name = viewModel.Name;
            UserCliamInDb.DisplayName = viewModel.DisplayName;
            UserCliamInDb.Description = viewModel.Description;
            UserCliamInDb.UserConsent = viewModel.UserConsent;
            UserCliamInDb.DefaultClaim = viewModel.DefaultClaim;
            UserCliamInDb.MetadataPublish = viewModel.Metadata;
            UserCliamInDb.UpdatedBy = UUID;

            var response = await _userClaimService.UpdateUserClaimAsync(UserCliamInDb);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Update UserClaims Configuration", LogMessageType.FAILURE.ToString(), "Fail to update UserClaims Configuration details of  name " + viewModel.DisplayName);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Update UserClaims Configuration", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated UserClaims Configuration details of  name " + viewModel.DisplayName + " successfully" : "Request for Update UserClaims Configuration details of application name " + viewModel.DisplayName + " has send for approval "));

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _userClaimService.DeleteUserClaimAsync(id, UUID);
                if (response != null || response.Success)
                {

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Delete UserClaims Configuration", LogMessageType.SUCCESS.ToString(), "Delete UserClaims Configuration successfully");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Delete UserClaims Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete UserClaims Configuration");
                    return new JsonResult(true);
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserClaimsConfiguration, "Delete UserClaims Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete UserClaims Configuration " + e.Message);
                return StatusCode(500, e);
            }
        }
    }
}
