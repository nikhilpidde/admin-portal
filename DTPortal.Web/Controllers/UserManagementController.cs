using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using DTPortal.Web.ViewModel.UserManagement;

using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using DTPortal.Core.Domain.Services.Communication;
using System.Collections;
using System.Collections.Generic;
using DTPortal.Web.ViewModel.Session;
using System.Linq;
using System;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Utilities;
using Newtonsoft.Json.Serialization;
using DTPortal.Web.Enums;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Users")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class UserManagementController : BaseController
    {
        private readonly IUserManagementService _userService;
        private readonly ISessionService _sessionService;
        private readonly IConfigurationService _configurationService;
        private readonly IConfiguration _configuration;

        public UserManagementController(ILogClient logClient,
            IUserManagementService userService,
            IConfigurationService configurationService,
            ISessionService sessionService,
            IConfiguration configuration) : base(logClient)
        {
            _userService = userService;
            _sessionService = sessionService;
            _configurationService = configurationService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult List()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetJson()
        {
            var totalRecords = 0;
            var users = await _userService.ListUsersAsync();
            if (users == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get all user list", LogMessageType.FAILURE.ToString(), "Fail to get user list");
                return NotFound();
            }
            if (users == null)
            {
                var displayResult = new List<user>();
                totalRecords = 0;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get all user list", LogMessageType.FAILURE.ToString(), "Fail to get user list");
                return Json(new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = displayResult });
            }
            else
            {
                var userlist = new List<user>();
                foreach (var obj in users)
                {
                    userlist.Add(new user
                    {
                        Id = obj.Id,
                        Uuid = obj.Uuid,
                        FullName = obj.FullName,
                        MailId = obj.MailId,
                        Role = obj.Role.DisplayName,
                        Status = obj.Status
                    });
                }
                var displayResult = userlist;
                totalRecords = userlist.Count();
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get all user list", LogMessageType.SUCCESS.ToString(), "Get user list success");
                return Json(new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = displayResult });
            }
        }



        //[HttpGet]
        //public async Task<IActionResult> List1(int? pageNumber)
        //{
        //    var users = await _userService.ListUsersAsync(pageNumber ?? 1, 10);
        //    if (users == null)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get all user list", LogMessageType.FAILURE.ToString(), "Fail to get user list");
        //        return NotFound();
        //    }

        //    var model = new UserManagementListViewModel
        //    {
        //        Users = users
        //    };
        //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get all user list", LogMessageType.SUCCESS.ToString(), "Get user list success");
        //    return View(model);
        //}


        [HttpGet]
        public async Task<IActionResult> New()
        {
            var roleLookups = GetRoleList(await _userService.GetRoleLookupsAsync());
            if (roleLookups == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "get Create new user page", LogMessageType.FAILURE.ToString(), "Fail to get role list");
                return NotFound();
            }

            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "get Create new user page", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }
            var userRoleId = User.FindFirstValue("UserRoleID");

            var roleIdList = _configuration.GetSection("RoleIds").Get<List<int>>();

            if (roleIdList.Count != 0 && !roleIdList.Contains(int.Parse(userRoleId)))
            {
                // Remove roles whose IDs exist in roleIdList
                roleLookups = roleLookups.Where(role => !roleIdList.Contains(int.Parse(role.Value))).ToList();
            }

            var userViewModel = new UserManagementNewViewModel
            {
                Roles = roleLookups,
                EmailDomains = Domainlist
            };

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(UserManagementNewViewModel viewModel)
        {
            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Create new user", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("New", viewModel);
            }
            //if (viewModel.Password != viewModel.ConfPassword)
            //{
            //    ModelState.AddModelError("ConfPassword", "Confirm password not matched");
            //    viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
            //    return View("New", viewModel);
            //}

            var email = viewModel.MailId + "@" + viewModel.EmailDomain;

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){1,7})+)$");
            Match match = regex.Match(email);
            if (!match.Success)
            {
                ModelState.AddModelError("MailId", "Invalid MailId");
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("New", viewModel);
            }

            viewModel.MailId = email;
            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            if (viewModel.MobileNo.StartsWith(mobileCountryCode) || viewModel.MobileNo.StartsWith("256"))
            {
                ModelState.AddModelError("MobileNo", "Write only mobile number without country code");
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("New", viewModel);
            }

            var user = new UserTable()
            {
                Uuid = string.Empty,
                FullName = viewModel.FullName,
                MailId = viewModel.MailId,
                MobileNo = mobileCountryCode + viewModel.MobileNo,
                RoleId = viewModel.RoleId,
                Dob = DateOnly.FromDateTime((DateTime)viewModel.Dob),
                Gender = viewModel.gender,
                AuthScheme = viewModel.AuthScheme,
                CreatedBy = UUID,
                UpdatedBy = UUID
            };

            var response = await _userService.AddUserAsync(user, string.Empty);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Create new user ", LogMessageType.FAILURE.ToString(), "Fail to create user");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Create new user", LogMessageType.SUCCESS.ToString(), "Created user with name " + viewModel.FullName + " successfully");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserManagementEditViewModel viewModel)
        {
            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Update user details", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("Edit", viewModel);
            }

            var email = viewModel.MailId + "@" + viewModel.EmailDomain;

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){1,7})+)$");
            Match match = regex.Match(email);
            if (!match.Success)
            {
                ModelState.AddModelError("MailId", "Invalid MailId");
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("Edit", viewModel);
            }

            viewModel.MailId = email;

            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            if (viewModel.MobileNo.StartsWith(mobileCountryCode) || viewModel.MobileNo.StartsWith("971"))
            {
                ModelState.AddModelError("MobileNo", "Write only mobile number without country code");
                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return View("Edit", viewModel);
            }

            var userInDb = await _userService.GetUserAsync(viewModel.Id);
            if (userInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication,
                    ServiceNameConstants.Users, "Update user  details",
                    LogMessageType.FAILURE.ToString(), 
                    "Fail to get user info of " + viewModel.FullName + " in Update");
                return NotFound();
            }

            userInDb.Id = viewModel.Id;
            userInDb.Uuid = viewModel.Uuid;
            userInDb.FullName = viewModel.FullName;
            userInDb.MailId = viewModel.MailId;
            userInDb.MobileNo = mobileCountryCode + viewModel.MobileNo;
            userInDb.RoleId = viewModel.RoleId;
            userInDb.AuthScheme = viewModel.AuthScheme;
            userInDb.Dob = DateOnly.FromDateTime((DateTime)viewModel.Dob);
            userInDb.Gender = viewModel.gender;
            userInDb.UpdatedBy = UUID;

            var response = await _userService.UpdateUserAsync(userInDb);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Update user  details", LogMessageType.FAILURE.ToString(), "Fail to update user info of  " + viewModel.FullName);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                viewModel.EmailDomains = Domainlist;
                viewModel.Roles = GetRoleList(await _userService.GetRoleLookupsAsync());
                return RedirectToAction("Edit", new { id = viewModel.Id });
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Update user  details", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated user info of  " + viewModel.FullName + " successfully" : "Request for Update user info of " + viewModel.FullName + " has send for approval "));
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
           
            var userInDb = await _userService.GetUserAsync(id);
            if (userInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "View user  details", LogMessageType.FAILURE.ToString(), "Fail to get user info");
                return NotFound();
            }
            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Update user details", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }

            DateOnly dob = (DateOnly)userInDb.Dob;

            var email = userInDb.MailId.Split("@");
            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            var model = new UserManagementEditViewModel
            {
                Id = userInDb.Id,
                Uuid = userInDb.Uuid,
                FullName = userInDb.FullName,
                MailId = email[0],
                EmailDomain = email[1],
                MobileNo = userInDb.MobileNo.Replace("+971", ""),
                Dob = dob.ToDateTime(TimeOnly.Parse("10:00 PM")),
                gender = userInDb.Gender,
                RoleId = userInDb.RoleId.Value,
                AuthScheme = userInDb.AuthScheme,
                Status = userInDb.Status,
                Roles = GetRoleList(await _userService.GetRoleLookupsAsync()),
                EmailDomains = Domainlist
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "View user  details", LogMessageType.SUCCESS.ToString(), "Get user info of " + userInDb.FullName + " successfully");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string uuid)
        {
            try
            {
                var response = await _userService.DeleteUserAsync(id, UUID, false);
                if (response != null && response.Success)
                {

                    await CleareSession(uuid);

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Delete  user  details", LogMessageType.SUCCESS.ToString(), "Delet user info of id = " + id + " success");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Delete user  details", LogMessageType.FAILURE.ToString(), "Fail to delete user info of id = " + id);
                    return null;
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Session(string id)
        {
            var sessionInDb = await _sessionService.GetAllIDPUserSessions(id, 5);
            if (sessionInDb == null || sessionInDb.Success == false)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get User Session", LogMessageType.FAILURE.ToString(), "Fail to get user sessions of id = " + id);
                var model = new UserManagementSessionListViewModel();
                model.UserName = id;
                Alert alert = new Alert { Message = (sessionInDb == null ? "Internal error please contact to admin" : sessionInDb.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(model);
            }
            else
            {
                var model = new UserManagementSessionListViewModel();
                var list = new List<SessionListViewModel>();

                foreach (var ele in sessionInDb.Result)
                {
                    foreach (var clientId in ele.ClientId)
                    {
                        var newClient = new SessionListViewModel()
                        {

                            IpAddress = ele.IpAddress,
                            LastAccessTime = ele.LastAccessTime,
                            LoggedInTime = ele.LoggedInTime,
                            UserId = ele.UserId,
                            GlobalSessionId = ele.GlobalSessionId,
                            ClientId = clientId

                        };

                        list.Add(newClient);
                    }
                }
                model.UserName = sessionInDb.Result[0].FullName;
                model.UserId = sessionInDb.Result[0].UserId;
                model.list = list;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Get User Session", LogMessageType.SUCCESS.ToString(), "Get user sessions of id = " + id + " success");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string id)
        {
            var data = new LogoutUserRequest
            {
                GlobalSession = id
            };

            var response = await _sessionService.LogoutUser(data);
            if (response == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Logout user session", LogMessageType.FAILURE.ToString(), "Fail to logout all SessionFail to logout user sessions of id = " + id + " getting response value null");
                return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
            }
            if (response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Logout user session", LogMessageType.SUCCESS.ToString(), "Logout user sessions of id = " + id + " success");
                return new JsonResult(response);
            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Logout User Session", LogMessageType.FAILURE.ToString(), "Fail to logout user sessions of id = " + id);
            return new JsonResult(response);


        }

        [HttpPost]
        public async Task<IActionResult> LogoutAll(List<string> id)
        {
            foreach (var session in id)
            {
                var data = new LogoutUserRequest
                {
                    GlobalSession = session
                };

                var response = await _sessionService.LogoutUser(data);
                if (response == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "LogoutAll user session", LogMessageType.FAILURE.ToString(), "Fail to logout all user sessions getting response value null");
                    return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
                }
                if (!response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "LogoutAll User Session", LogMessageType.FAILURE.ToString(), "Fail to logout all user sessions");
                    return new JsonResult(response);
                }

            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Users", "LogoutAll User Session", LogMessageType.SUCCESS.ToString(), "Logout all user sessions success");
            return new JsonResult(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SendDeviceRegistrationLink(SendDeviceRegistrationViewModel model)
        {

            //if (DateTime.Compare((DateTime)model.Expiry, DateTime.Now) <= 0)
            //    model.Expiry = null;

            var Responce = await _userService.SendTempDeviceLinkAsync(model.Uuid, null, model.Expiry);
            if (Responce != null && Responce.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Send Device Registration Link to user", LogMessageType.SUCCESS.ToString(), "Resend device registration link to id = " + model.Uuid + " success");
                Alert alert = new Alert { IsSuccess = true, Message = "Device registration link send Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Send Device Registration Link to user", LogMessageType.FAILURE.ToString(), "Fail to resend device registration link to id = " + model.Uuid);
                Alert alert = new Alert { Message = "Fail to resend device registration link" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

            }
            return RedirectToAction("Edit", "UserManagement", new { id = model.id });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPasswoed(int id, string name)
        {
            var Responce = await _userService.AdminResetPassword(id);
            if (Responce == null || !Responce.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Reset user password", LogMessageType.FAILURE.ToString(), "Fail to reset user password of " + name);
                Alert alert = new Alert { Message = "Fail to reset user password" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Reset user password", LogMessageType.SUCCESS.ToString(), "Reset user password of " + name + " success");
                Alert alert = new Alert { IsSuccess = true, Message = "New Password Sent To Mail Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

            }
            return RedirectToAction("Edit", "UserManagement", new { id = id });
        }

        [HttpGet]
        public async Task<string[]> GetUsers(string type, string value)
        {
            List<string> Response = new List<string>();
            if (type == "EMAIL")
                Response = await _userService.SearchUserAsyncByEmail(value);
            else
                Response = await _userService.SearchUserAsyncByPhone(value);

            return Response.ToArray();
        }

        [HttpGet]
        public async Task<ActionResult> SetUserState(int id, string Doaction, string uuid = null)
        {
            UserResponse responce;
            if (Doaction == "Activation")
            {
                responce = await _userService.ActivateUserAsync(id);
            }
            else
            {
                responce = await _userService.DeactivateUserAsync(id);
            }
            if (responce == null || !responce.Success)
            {
                Alert alert = new Alert { Message = "User " + Doaction + " Fail" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Change user state", LogMessageType.FAILURE.ToString(), "User " + Doaction + " Fail");
            }
            else
            {
                if (Doaction != "Activation")
                {
                    await CleareSession(uuid);
                }

                Alert alert = new Alert { IsSuccess = true, Message = "User " + Doaction + " Successful" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Change user state", LogMessageType.SUCCESS.ToString(), "User " + Doaction + " Success");
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public List<SelectListItem> GetRoleList(IEnumerable<RoleLookupItem> role)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (RoleLookupItem i in role)
            {
                list.Add(new SelectListItem { Value = i.Id.ToString(), Text = i.DisplayName });
            }

            return list;
        }

        public async Task<List<SelectListItem>> GetDomainList()
        {

            var AdminPortalconfigInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (AdminPortalconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "get AdminPortal_SSOConfig details", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal_SSOConfig configuration");
                return null;
            }

            List<SelectListItem> list = new List<SelectListItem>();

            foreach (string i in AdminPortalconfigInDB.allowed_domain_users)
            {
                list.Add(new SelectListItem { Value = i, Text = i });
            }

            return list;
        }

        public async Task<bool> CleareSession(string uuid)
        {
            try
            {
                var sessionInDb = await _sessionService.GetAllIDPUserSessions(uuid, 5);
                if (sessionInDb == null || sessionInDb.Success == false)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Cleare User Session", LogMessageType.FAILURE.ToString(), "Fail to get user sessions of id = " + uuid);
                }
                else
                {
                    foreach (var ele in sessionInDb.Result)
                    {
                        var data = new LogoutUserRequest
                        {
                            GlobalSession = ele.GlobalSessionId
                        };

                        var response = await _sessionService.LogoutUser(data);

                    }

                }

                return true;
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Users, "Cleare User Session", LogMessageType.FAILURE.ToString(), "Fail to get user sessions of id = " + uuid + ", Exception  =  " + e.Message);
                return true;
            }
        }
    }
}

