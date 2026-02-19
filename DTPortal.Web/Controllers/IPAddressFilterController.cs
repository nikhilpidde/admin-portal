using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.IPAddressFilter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "IPAddressFilter")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class IPAddressFilterController : BaseController
    {
        public readonly IIPBasedAccessService _IpBasedAccessService;
        private readonly IUserManagementService _userService;

        public IPAddressFilterController(IUserManagementService userService, ILogClient logClient, IIPBasedAccessService IpBasedAccessService) : base(logClient)
        {
            _IpBasedAccessService = IpBasedAccessService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var response = await _IpBasedAccessService.ListIPBasedAccessAsync();
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Get all IPAddressFilter list", LogMessageType.FAILURE.ToString(), "Fail to get IPAddressFilter list");
                return NotFound();
            }
            var model = new IPAddressFilterListViewModel
            {
                List = response
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Get all IPAddressFilter list", LogMessageType.SUCCESS.ToString(), "Get IPAddressFilter list success");
            return View(model);
        }
/*
        [HttpGet]
        public async Task<IActionResult> New()
        {
            var model = new IPAddressFilterNewViewModel();
            return View(model);
        }

*/
        [HttpPost]
        public async Task<IActionResult> Save(IPAddressFilterNewViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                return View("New", Model);
            }

            if (Model.IpType == "SINGLE_IP")
            {
                if (Model.Permission == "1")
                    Model.IpType = "ALLOWED_SINGLE_IP";
                else
                    Model.IpType = "DENIED_SINGLE_IP";
            }
            else
            {
                if (Model.Permission == "1")
                    Model.IpType = "ALLOWED_MASKED_IP";
                else
                    Model.IpType = "DENIED_MASKED_IP";
            }

          
            var data = new IpBasedAccess
            {
                Id = 0,
                Name = Model.Name,
                Description = Model.Description,
                Type = Model.IpType,
                Ip = Model.IpAddress,
                Permission = (Model.Permission == "1" ? true : false),
                CreatedBy = UUID,
                ModifiedBy = UUID
            };


            var response = await _IpBasedAccessService.CreateIPBasedAccessAsync(data);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Create New IPAddressFilter record", LogMessageType.FAILURE.ToString(), "Fail to create IPAddressFilter record");
                return View("New", Model);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Create New IPAddressFilter record", LogMessageType.SUCCESS.ToString(), "Created IPAddressFilter record successfully" );
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
                // return RedirectToAction("Edit", new { id = response.Result.Id });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _IpBasedAccessService.GetIPBasedAccessAsync(id);
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "View IPAddressFilter details", LogMessageType.FAILURE.ToString(), "Fail to get IPAddressFilter info ");
                return NotFound();
            }
            var model = new IPAddressFilterEditViewModel
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                IpAddress = response.Ip,
                IpType = (response.Type.Contains("SINGLE_IP") ? "SINGLE_IP" : "MASKED_IP"),
                Permission = ((bool)response.Permission ? "1" : "0")
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "View IPAddressFilter details", LogMessageType.SUCCESS.ToString(), "Get IPAddressFilter info of " + response.Name + " success" );
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(IPAddressFilterEditViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", Model);
            }

            var DbRecord = await _IpBasedAccessService.GetIPBasedAccessAsync(Model.Id);
            if (DbRecord == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Update IPAddressFilter details", LogMessageType.FAILURE.ToString(), "Fail to get IPAddressFilter info of " + Model.Name);
                return NotFound();
            }

            //if (Model.IpType == "SINGLE_IP")
            //{
            //    if (Model.Permission == "1")
            //        Model.IpType = "ALLOWED_SINGLE_IP";
            //    else
            //        Model.IpType = "DENIED_SINGLE_IP";
            //}
            //else
            //{
            //    if (Model.Permission == "1")
            //        Model.IpType = "ALLOWED_MASKED_IP";
            //    else
            //        Model.IpType = "DENIED_MASKED_IP";
            //}

            DbRecord.Description = Model.Description;
            DbRecord.Ip = Model.IpAddress;
           // DbRecord.Type = Model.IpType;
           // DbRecord.Permission = (Model.Permission == "1" ? true : false);
            DbRecord.ModifiedBy = UUID;

            var response = await _IpBasedAccessService.UpdateIPBasedAccessAsync(DbRecord);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = "Faied to Update Record" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Update IPAddressFilter details", LogMessageType.FAILURE.ToString(), "Fail to update IPAddressFilter record of "+ Model.Name);
                return View("Edit", Model);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Update IPAddressFilter details", LogMessageType.SUCCESS.ToString(), "Updated IPAddressFilter Record of "+ Model.Name+" successfully");
                Alert alert = new Alert { IsSuccess = true, Message = "Record Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("Edit", new { id = response.Result.Id });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _IpBasedAccessService.DeleteIPBasedAccessAsync(id);
                if (response != null && response.Success)
                {
                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Delete IPAddressFilter details", LogMessageType.SUCCESS.ToString(), "Delete IPAddressFilter info of id = " + id + " success");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert {  Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.IPAddressFilter, "Delete IPAddressFilter details", LogMessageType.FAILURE.ToString(), "Fail to delete IPAddressFilter info of id = " + id);
                    return null;
                }
            }catch(Exception e)
            {
                return StatusCode(500, e);
            }
        }

    }
}
