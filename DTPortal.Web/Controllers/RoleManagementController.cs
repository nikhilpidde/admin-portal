using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.RoleManagement;

using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Utilities;
using Newtonsoft.Json.Serialization;
using DTPortal.Web.Enums;
using System.Security.Claims;
using System;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Enums;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Roles")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class RoleManagementController : BaseController
    {
        private readonly IRoleManagementService _roleActivityService;

        public RoleManagementController(ILogClient logClient, IRoleManagementService roleActivityService) : base(logClient)
        {
            _roleActivityService = roleActivityService;
        }



        [HttpGet]
        public async Task<IActionResult> List()
        {
            var roleLookupItems = await _roleActivityService.GetRoleLookupItemsAsync();
            if (roleLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Get all role list", LogMessageType.FAILURE.ToString(), "Fail to get role list");
                return NotFound();
            }

            var viewModel = new RoleManagementListViewModel()
            {
                RoleLookupItems = roleLookupItems
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Get all role list", LogMessageType.SUCCESS.ToString(), "Get role list success");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            try
            {
                var activities = JsonConvert.DeserializeObject(await GetActivitiesList());
                if (activities == null)
                {
                    return NotFound();
                }
                var CheckerActivitie = await GetCkeckerList();
                if (CheckerActivitie == null)
                {
                    return NotFound();
                }
                var viewModel = new RoleManagementNewViewModel
                {
                    Activities = activities,
                    CheckerActivitie = CheckerActivitie
                };

                return View(viewModel);
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.FAILURE.ToString(), e.Message);
                Alert alert = new Alert { Message = "Internal Error please contact to admin" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var roleInDb = await _roleActivityService.GetRoleAsync(id);
                if (roleInDb == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "View Role details", LogMessageType.FAILURE.ToString(), "Fail to get role info");
                    return NotFound();
                }

                var viewModel = new RoleManagementNewViewModel
                {
                    Id = roleInDb.Id,
                    RoleName = roleInDb.Name,
                    DisplayName = roleInDb.DisplayName,
                    Description = roleInDb.Description,
                    CheckerActivitie = await GetCkeckerList(roleInDb.RoleActivities),
                    Activities = JsonConvert.DeserializeObject(await GetActivitiesList(roleInDb.RoleActivities)),
                    Status = roleInDb.Status
                };
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "View Role details", LogMessageType.SUCCESS.ToString(), "View role info of role name " + roleInDb.Name + " success");

                return View(viewModel);
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "View Role details", LogMessageType.FAILURE.ToString(), e.Message);
                Alert alert = new Alert { Message = "Internal Error please contact to admin" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(RoleManagementNewViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("New", viewModel);
                }

                var activityArry = viewModel.Activitie.Split(",");

                var selectedActivities = viewModel.Activitie.Split(",")
                    .Where(x => !x.StartsWith("IsChecker_"))
                   .Select(x => new { activityId = int.Parse(x), isChecker = activityArry.Contains("IsChecker_" + x) })
                   .ToDictionary(x => x.activityId, x => x.isChecker);

                //var selectedActivities = viewModel.Activities
                //    .Where(x => x.IsSelected)
                //    .Select(x => new { activityId = x.Id, isChecker = x.IsChecker != null && x.IsChecker.IsSelected })
                //    .ToDictionary(x => x.activityId, x => x.isChecker);


                var role = new Role
                {
                    Name = viewModel.RoleName,
                    DisplayName = viewModel.DisplayName,
                    Description = viewModel.Description,
                    CreatedBy = UUID,
                    UpdatedBy = UUID
                };

                var response = await _roleActivityService.AddRoleAsync(role, selectedActivities);
                if (response == null || !response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.FAILURE.ToString(), "fail to create role");

                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);

                    var Activity = JsonConvert.DeserializeObject(await GetActivitiesList());
                    if (Activity == null)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.FAILURE.ToString(), "fail to get activity in role create");
                        return NotFound();
                    }
                    viewModel.Activities = Activity;

                    var CheckerActivitie = await GetCkeckerList();
                    if (CheckerActivitie == null)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.FAILURE.ToString(), "fail to get checker activity in role create");
                        return NotFound();
                    }

                    viewModel.CheckerActivitie = CheckerActivitie;
                    return View("New", viewModel);
                }
                else
                {

                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.SUCCESS.ToString(), "Created new role with name "+viewModel.DisplayName +" successfully");

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("List");
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Create new role", LogMessageType.FAILURE.ToString(), e.Message);
                Alert alert = new Alert { Message = "Internal Error please contact to admin" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(RoleManagementNewViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Edit", viewModel);
                }

                var roleInDb = await _roleActivityService.GetRoleAsync(viewModel.Id);
                if (roleInDb == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.FAILURE.ToString(), "Fail to get  role info of " + viewModel.RoleName);
                    return NotFound();
                }

                roleInDb.Name = viewModel.RoleName;
                roleInDb.DisplayName = viewModel.DisplayName;
                roleInDb.Description = viewModel.Description;
                roleInDb.UpdatedBy = UUID;

                var activityArry = viewModel.Activitie.Split(",");

                var selectedActivities = viewModel.Activitie.Split(",")
                    .Where(x => !x.StartsWith("IsChecker_"))
                   .Select(x => new { activityId = int.Parse(x), isChecker = activityArry.Contains("IsChecker_" + x) })
                   .ToDictionary(x => x.activityId, x => x.isChecker);

                var response = await _roleActivityService.UpdateRoleAsync(roleInDb, selectedActivities);
                if (response == null || !response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.FAILURE.ToString(), "Fail to update role info of " + viewModel.RoleName);

                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    var Activity = JsonConvert.DeserializeObject(await GetActivitiesList());
                    if (Activity == null)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.FAILURE.ToString(), "fail to get activity in role update");
                        return NotFound();
                    }
                    viewModel.Activities = Activity;

                    var CheckerActivitie = await GetCkeckerList();
                    if (CheckerActivitie == null)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.FAILURE.ToString(), "fail to get checker activity in role update");
                        return NotFound();
                    }
                    viewModel.CheckerActivitie = CheckerActivitie;
                    return View("Edit", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated role info of  " + viewModel.RoleName + " successfully" : "Request for Update role info of " + viewModel.RoleName + " has send for approval "));

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);

                    return RedirectToAction("List");
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Update role details", LogMessageType.FAILURE.ToString(), e.Message);
                Alert alert = new Alert { Message = "Internal Error please contact to admin" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _roleActivityService.DeleteRoleAsync(id, UUID, false);
                if (response != null && response.Success)
                {
                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Delete role details", LogMessageType.SUCCESS.ToString(), "Delete role of id(" + id + ") success");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Delete role details", LogMessageType.FAILURE.ToString(), "fail to delete role of id(" + id + ")");
                    return null;
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet]
        public async Task<ActionResult> SetRoleState(int id, string Doaction)
        {
            RoleResponse responce;
            if (Doaction == "Activation")
            {
                responce = await _roleActivityService.ActivateRoleAsync(id);
            }
            else
            {
                responce = await _roleActivityService.DeActivateRoleAsync(id);
            }
            if (responce == null || !responce.Success)
            {
                Alert alert = new Alert { Message = "Roles " + Doaction + " Fail" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Change Roles state", LogMessageType.FAILURE.ToString(), "Role " + Doaction + " Fail");
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = "Role " + Doaction + " Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Change Roles state", LogMessageType.SUCCESS.ToString(), "Role " + Doaction + " Success");
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public async Task<string> GetActivitiesList(IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Get all activity list", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }

            activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();
            var nodes = new List<ActivityTreeItem>();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetState(roleActivities, activity.Id)) //Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id) })
                    });
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetState(null, -1))//Json(new {selected = false})
                    });
                }
            }

            var data = new JsonResult(nodes);
            return JsonConvert.SerializeObject(data.Value);
        }

        public async Task<List<CheckerListItem>> GetCkeckerList(IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Roles, "Get all activity list", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<CheckerListItem>();

            activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerState(roleActivities, activity.Id)// Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id && x.IsChecker == true) })
                        }); ;
                    }
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerState(roleActivities, activity.Id)//Json(new {selected = false})
                        });
                    }
                }
            }

            return nodes;
        }

        [NonAction]
        private string GetState(IEnumerable<RoleActivity> roleActivity = null, int parentId = -1)
        {
            JsonResult data;
            if (roleActivity != null)
            {
                data = Json(new { selected = roleActivity.Any(x => x.ActivityId == parentId && (bool)x.IsEnabled) });
            }
            else
            {
                data = Json(new { selected = false });
            }
            return JsonConvert.SerializeObject(data.Value);
        }

        [NonAction]
        private bool GetCheckerState(IEnumerable<RoleActivity> roleActivity = null, int parentId = -1)
        {
            bool data;
            if (roleActivity != null)
                data = roleActivity.Any(x => x.ActivityId == parentId && x.IsChecker);
            else
                data = false;

            return data;
        }


    }
}
