using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel.Purposes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel.WalletPurpose;
using DTPortal.Core.Services;
using DTPortal.Web.Enums;
using DTPortal.Core.Domain.Models;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class WalletPurposeController : BaseController
    {

        private readonly IWalletPurposeService _walletPurposeService;
        public WalletPurposeController(ILogClient logClient, IWalletPurposeService walletPurposeService) : base(logClient)
        {
            _walletPurposeService = walletPurposeService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var ViewModel = new List<WalletPurposeListViewModel>();


            var PurposeList = await _walletPurposeService.GetPurposeListAsync();

            if (PurposeList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Get all Purposes Configuration List", LogMessageType.FAILURE.ToString(), "Fail to get Purposes Configuration list");
                return NotFound();
            }

            foreach (var purpose in PurposeList)
            {
                var PurposeListViewModel = new WalletPurposeListViewModel
                {
                    Id = purpose.Id,
                    Name = purpose.Name,
                    DisplayName = purpose.DisplayName,
                    CreatedDate=purpose.CreatedDate,
                    Status = purpose.Status,
                   
                };
                ViewModel.Add(PurposeListViewModel);
            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Get all Purposes Configuration List", LogMessageType.SUCCESS.ToString(), "Get Purposes Configuration list success");
            return View(ViewModel);
        }

        [HttpGet]
        public IActionResult New()
        {
            var ViewModel = new WalletPurposeNewViewModel();
            return View(ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(WalletPurposeNewViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("New", ViewModel);
            }
            var walletPurpose = new WalletPurpose()
            {
                Name = ViewModel.Name,
                DisplayName = ViewModel.DisplayName,
                Description = ViewModel.Description,
                CreatedBy = UUID,
                UpdatedBy = UUID
            };
            var response = await _walletPurposeService.CreatePurposeAsync(walletPurpose);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Create new Wallet Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to create Purposes Configuration");
                return View("New", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Create new Wallet Purposes Configuration", LogMessageType.SUCCESS.ToString(), "Created New Purposes Configuration with name " + ViewModel.DisplayName + " Successfully");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var purposeinDb = await _walletPurposeService.GetPurposeAsync(id);

            if (purposeinDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "View Wallet Purposes Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get Wallet Purposes Configuration details");
                return NotFound();
            }
            var ViewModel = new WalletPurposeEditViewModel
            {
                Id = purposeinDb.Id,
                Name = purposeinDb.Name,
                DisplayName = purposeinDb.DisplayName,
                Description = purposeinDb.Description,
                Status = purposeinDb.Status
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "View Wallet Purposes Configuration details", LogMessageType.SUCCESS.ToString(), "Get Wallet Purposes Configuration details of " + purposeinDb.DisplayName + " successfully ");
            return View(ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(WalletPurposeEditViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", ViewModel);
            }
            var purposeInDb = await _walletPurposeService.GetPurposeAsync(ViewModel.Id);
            if (purposeInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Wallet Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to get Wallet Purposes Configuration details");
                return View("Edit", ViewModel);
            }
            purposeInDb.Id = ViewModel.Id;
            purposeInDb.Name = ViewModel.Name;
            purposeInDb.DisplayName = ViewModel.DisplayName;
            purposeInDb.Description = ViewModel.Description;
           
            purposeInDb.UpdatedBy = UUID;
            var response = await _walletPurposeService.UpdatePurposeAsync(purposeInDb);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Update Wallet Purposes Configuration", LogMessageType.FAILURE.ToString(), "Fail to update Wallet Purposes Configuration details of  name " + ViewModel.DisplayName);
                return View("Edit", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Update Wallet Purposes Configuration", LogMessageType.SUCCESS.ToString(), "Updated Wallet Purposes Configuration details of  name " + ViewModel.DisplayName + " successfully");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _walletPurposeService.DeletePurposeAsync(id, UUID);
            if (response != null && response.Success)
            {

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Wallet Purpose Configuration", LogMessageType.SUCCESS.ToString(), "Delete Wallet Purpose Configuration successfully");
                return new JsonResult(true);
            }
            else
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.PurposesConfiguration, "Delete Wallet Purpose Configuration", LogMessageType.FAILURE.ToString(), "Failed to Delete Wallet Purpose Configuration");
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return new JsonResult(false);
            }
        }
    }
}
