using Microsoft.AspNetCore.Mvc;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Purposes;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Twilio.Rest.Api.V2010.Account;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Purposes")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class PurposeController : BaseController
    {
        private readonly IPurposeService _purposeService;
        public PurposeController(ILogClient logClient,IPurposeService purposeService) : base(logClient)
        {
            _purposeService = purposeService;
        }
        [HttpGet]
        public IActionResult New()
        {
            var ViewModel = new PurposeNewViewModel();
            return View(ViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var ViewModel = new List<PurposeListViewModel>();

            var PurposeList = await _purposeService.GetPurposeListAsync();

            if(PurposeList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Get all Purposes Configuration List", LogMessageType.FAILURE.ToString(), "Fail to get Purposes Configuration list");
                return NotFound();
            }

            foreach(var purpose in PurposeList)
            {
                var PurposeListViewModel = new PurposeListViewModel
                {
                    Id = purpose.Id,
                    Name = purpose.Name,
                    DisplayName = purpose.DisplayName,
                    Description = purpose.Description,
                    UserConsent = purpose.UserConsentRequired
                };
                ViewModel.Add(PurposeListViewModel);
            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Get all Purposes Configuration List", LogMessageType.SUCCESS.ToString(), "Get Purposes Configuration list success");
            return View(ViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var purposeinDb = await _purposeService.GetPurposeAsync(id);

            if (purposeinDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "View Purposes Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get Purposes Configuration details");
                return NotFound();
            }
            var ViewModel = new PurposeEditViewModel
            {
                Id = purposeinDb.Id,
                Name = purposeinDb.Name,
                DisplayName = purposeinDb.DisplayName,
                Description = purposeinDb.Description,
                UserConsent = purposeinDb.UserConsentRequired,
                Status= purposeinDb.Status
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "View Purposes Configuration details", LogMessageType.SUCCESS.ToString(), "Get Purposes Configuration details of " + purposeinDb.DisplayName + " successfully ");
            return View(ViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Save(PurposeNewViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("New",ViewModel);
            }
            var purpose = new Purpose()
            {
                Name = ViewModel.Name,
                DisplayName = ViewModel.DisplayName,
                Description = ViewModel.Description,
                UserConsentRequired = ViewModel.UserConsent,
                CreatedBy=UUID,
                UpdatedBy=UUID
            };
            var response = await _purposeService.CreatePurposeAsync(purpose);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Create new Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to create Purposes Configuration");
                return View("New", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Create new Purposes Configuration", LogMessageType.SUCCESS.ToString(), "Created New Purposes Configuration with name " + ViewModel.DisplayName + " Successfully");
                return RedirectToAction("List");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Update(PurposeEditViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit",ViewModel);
            }
            var purposeInDb=await _purposeService.GetPurposeAsync(ViewModel.Id);
            if (purposeInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to get Purposes Configuration details");
                return View("Edit",ViewModel);
            }
            purposeInDb.Id = ViewModel.Id;
            purposeInDb.Name = ViewModel.Name;
            purposeInDb.DisplayName = ViewModel.DisplayName;
            purposeInDb.Description = ViewModel.Description;
            purposeInDb.UserConsentRequired= ViewModel.UserConsent;
            purposeInDb.UpdatedBy = UUID;
            var response=await _purposeService.UpdatePurposeAsync(purposeInDb);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Update Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to update Purposes Configuration details of  name " + ViewModel.DisplayName);
                return View("Edit", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Update Purposes Configuration", LogMessageType.SUCCESS.ToString(),"Updated Purposes Configuration details of  name " + ViewModel.DisplayName + " successfully");
                return RedirectToAction("List");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _purposeService.DeletePurposeAsync(id,UUID);
            if (response != null && response.Success)
            {

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Purpose Configuration", LogMessageType.SUCCESS.ToString(), "Delete Purpose Configuration successfully");
                return new JsonResult(true);
            }
            else
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Purpose Configuration", LogMessageType.FAILURE.ToString(), "Failed to Delete Purpose Configuration");
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return new JsonResult(false);
            }
        }
    }
}
