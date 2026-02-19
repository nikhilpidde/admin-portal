using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.TimeBasedAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "TimeBasedAccess")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class TimeBasedAccessController : BaseController
    {
        public readonly ITimeBasedAccessService _timeBasedAccessService;
        private readonly IUserManagementService _userService;

        public TimeBasedAccessController(IUserManagementService userService, ILogClient logClient, ITimeBasedAccessService timeBasedAccessService) : base(logClient)
        {
            _timeBasedAccessService = timeBasedAccessService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var response = await _timeBasedAccessService.ListTimeBasedAccessAsync();
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Get all TimeBasedAccess list", LogMessageType.FAILURE.ToString(), "Fail to get TimeBasedAccess list");
                return NotFound();
            }
            var model = new TimeBasedAccessListViewModel
            {
                List = response.Where(x => x.Status != "DELETED")
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Get all TimeBasedAccess list", LogMessageType.SUCCESS.ToString(), "Get TimeBasedAccess list success");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var roleLookups = GetRoleList(await _userService.GetRoleLookupsAsync());
            if (roleLookups == null)
            {
                return NotFound();
            }
            var model = new TimeBasedAccessNewViewModel
            {
                ApplicableRole = "",
                RolesList = roleLookups
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Save(TimeBasedAccessNewViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                Model.ApplicableRole = (string.IsNullOrEmpty(Model.ApplicableRole) ? "" : Model.ApplicableRole);
                Model.RolesList = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("New", Model);
            }

        
            var data = new TimeBasedAccess
            {
                Name = Model.Name,
                Description = Model.Description,
                ApplicableRoles = Model.ApplicableRole,
                StartDate = (Model.sDate == null ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.FromDateTime((DateTime)Model.sDate)),
                EndDate = (Model.eDate == null ? null : DateOnly.FromDateTime((DateTime)Model.eDate)),
                StartTime =TimeOnly.FromTimeSpan((TimeSpan)Model.sTime),
                EndTime = TimeOnly.FromTimeSpan((TimeSpan)Model.eTime),
                Deny = (Model.isActive == 1 ? true : false),
                AccessDenyTimeZone = "",
                CreatedBy = UUID,
                ModifiedBy = UUID
            };


            var response = await _timeBasedAccessService.CreateTimeBasedAccessAsync(data);
            if (response == null || !response.Success)
            {
                Model.RolesList = GetRoleList(await _userService.GetRoleLookupsAsync());
                Alert alert = new Alert { Message = "Faied to Created Record" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Create TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to create TimeBasedAccess record");
                return View("New", Model);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Create TimeBasedAccess details", LogMessageType.SUCCESS.ToString(), "Created TimeBasedAccess record with name "+ Model.Name+" successfully");
                Alert alert = new Alert { IsSuccess = true, Message = "Record Created Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("Edit", new { id = response.Result.Id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _timeBasedAccessService.GetTimeBasedAccessAsync(id);
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Edit TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to get TimeBasedAccess info");
                return NotFound();
            }

            DateOnly sDate = (DateOnly)response.StartDate;
            DateOnly? eDate = (response.EndDate != null ? (DateOnly)response.EndDate : null);
            TimeOnly sTime = (TimeOnly)response.StartTime;
            TimeOnly eTime = (TimeOnly)response.EndTime;

            DateOnly EDate = default; 
            if (eDate != null)
                EDate = (DateOnly)eDate;

            var model = new TimeBasedAccessEditViewModel
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                ApplicableRole = response.ApplicableRoles,
                sDate = sDate.ToDateTime(TimeOnly.Parse("10:00 PM")),
                eDate = (eDate != null ? EDate.ToDateTime(TimeOnly.Parse("10:00 PM")) : null),
                sTime = sTime.ToTimeSpan(),
                eTime = eTime.ToTimeSpan(),
                isActive = ((bool)response.Deny ? 1 : 0),
                RolesList = GetRoleList(await _userService.GetRoleLookupsAsync())
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Edit TimeBasedAccess details", LogMessageType.SUCCESS.ToString(), "Get TimeBasedAccess info of name = " + response.Name + " success");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(TimeBasedAccessEditViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                Model.ApplicableRole = (string.IsNullOrEmpty(Model.ApplicableRole) ? "" : Model.ApplicableRole);
                Model.RolesList = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("Edit", Model);
            }

            var DbRecord = await _timeBasedAccessService.GetTimeBasedAccessAsync(Model.Id);
            if (DbRecord == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Update TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to get TimeBasedAccess info of name = " + Model.Name);
                return NotFound();
            }


            DbRecord.Name = Model.Name;
            DbRecord.Description = Model.Description;
            DbRecord.ApplicableRoles = Model.ApplicableRole;
            DbRecord.StartDate = (Model.sDate == null ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.FromDateTime((DateTime)Model.sDate));
            DbRecord.EndDate = (Model.eDate != null ? DateOnly.FromDateTime((DateTime)Model.eDate):null);
            DbRecord.StartTime = TimeOnly.FromTimeSpan((TimeSpan)Model.sTime);
            DbRecord.EndTime = TimeOnly.FromTimeSpan((TimeSpan)Model.eTime);
            DbRecord.Deny = (Model.isActive == 1 ? true : false);
            DbRecord.ModifiedBy = UUID;

            var response = await _timeBasedAccessService.UpdateTimeBasedAccessAsync(DbRecord);
            if (response == null || !response.Success)
            {
                Model.RolesList = GetRoleList(await _userService.GetRoleLookupsAsync());
                Alert alert = new Alert { Message = "Faied to Update Record" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Update TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to update TimeBasedAccess record");
                return View("Edit", Model);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Update TimeBasedAccess details", LogMessageType.SUCCESS.ToString(), "Updated TimeBasedAccess Record of name "+Model.Name+" successfuly");
                Alert alert = new Alert { IsSuccess = true, Message = "Record Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("Edit", new { id = response.Result.Id });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _timeBasedAccessService.DeleteTimeBasedAccessAsync(id);
                if (response != null && response.Success)
                {
                    Alert alert = new Alert { IsSuccess = true, Message = (string.IsNullOrEmpty(response.Message)?"Record deleted successfully":response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Delete TimeBasedAccess details", LogMessageType.SUCCESS.ToString(), "Delete timeBasedAccess info success");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert {  Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Delete TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to delete timeBasedAccess info ");
                    return null;
                }
            }catch(Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.TimeBasedAccess, "Delete TimeBasedAccess details", LogMessageType.FAILURE.ToString(), "Fail to delete timeBasedAccess : " + e.Message);
                return StatusCode(500, e);
            }
        }

        public List<SelectListItem> GetRoleList(IEnumerable<RoleLookupItem> role)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (RoleLookupItem i in role)
            {
                list.Add(new SelectListItem { Value = i.Name, Text = i.DisplayName });
            }

            return list;
        }
    }
}
