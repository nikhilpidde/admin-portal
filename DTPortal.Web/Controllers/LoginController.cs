using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.Helper;
using DTPortal.Web.Models;
using DTPortal.Web.ViewModel.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DTPortal.Web.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IRoleManagementService _roleActivityService;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationService _configurationService;
        private readonly IUserConsoleService _userConsoleService;
        private readonly ILogger<LoginController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IGlobalConfiguration _globalConfiguration;
        private readonly OpenID openIDHelper;

        public LoginController(IConfigurationService configurationService,
            ILogger<LoginController> logger,
            ILogClient logClient,
            IUserConsoleService userConsoleService,
            IConfiguration configuration,
            IGlobalConfiguration globalConfiguration,
            IRoleManagementService roleActivityService,
            IHttpClientFactory httpClientFactory) : base(logClient)
        {
            _logger = logger;
            _configuration = configuration;
            _configurationService = configurationService;
            _userConsoleService = userConsoleService;
            _roleActivityService = roleActivityService;
            _httpClientFactory = httpClientFactory;
            openIDHelper = new OpenID(_configuration, _httpClientFactory, globalConfiguration);
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    _logger.LogInformation("Login  : User has valid session");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation("Login  : User has invalid session redirect to login");
                    TempData.Clear();

                    var state = Guid.NewGuid().ToString("N");
                    var nonce = Guid.NewGuid().ToString("N");

                    HttpContext.Session.SetString("Nonce", nonce);
                    HttpContext.Session.SetString("state", state);

                    return View();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Login  Exception :  {0}", e.Message);
                _logger.LogError(e, "Login  Exception ");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login ", LogMessageType.SUCCESS.ToString(), "Login  : " + e.Message);
                ViewBag.error = "Something Went wrong";
                ViewBag.error_description = e.Message;
                return View("Errorp");
            }
        }
        [HttpGet]
        public async Task<IActionResult> CallBack()
        {
            try
            {

                if (!string.IsNullOrEmpty(Request.Query["error"]) && !string.IsNullOrEmpty(Request.Query["error_description"]))
                {
                    ViewBag.error = Request.Query["error"].ToString();
                    ViewBag.error_description = Request.Query["error_description"].ToString();
                    return View("Errorp");
                }

                /*
                //get state value from session 
                var Cstate = HttpContext.Session.GetString("state");
                if (string.IsNullOrEmpty(Cstate))
                {
                    ViewBag.error = "Something went wrong ";
                    ViewBag.error_description = "state value not found";
                    return View("Errorp");
                }
                //get state value from idp responce
                string Rstate = Request.Query["state"].ToString();
                if (string.IsNullOrEmpty(Rstate))
                {
                    ViewBag.error = "Invalid responce";
                    ViewBag.error_description = "The state value is empty string or null";
                    return View("Errorp");
                }

                //validate state value of idp responce is matched with your 
                // state value which is send in login request
                if (!string.Equals(Cstate, Rstate))
                {
                    ViewBag.error = "invalid state";
                    ViewBag.error_description = "The state value is not matched";
                    return View("Errorp");
                }
                */
                string code = Request.Query["code"].ToString();
                if (string.IsNullOrEmpty(code))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The code value is empty string or null";
                    return View("Errorp");
                }

                JObject TokenResponse = openIDHelper.GetAccessToken(code).Result;
                if (TokenResponse.ContainsKey("error") && TokenResponse.ContainsKey("error_description"))
                {
                    ViewBag.error = TokenResponse["error"].ToString();
                    ViewBag.error_description = TokenResponse["error_description"].ToString();
                    return View("Errorp");
                }


                _logger.LogInformation("Login Callback  : get access_token from idp successfully");

                UserSessionObj user = new UserSessionObj();

                var isOpenId = _configuration.GetValue<bool>("OpenId_Connect");

                var ID_Token = "";
                if (isOpenId)
                {
                    ID_Token = TokenResponse["id_token"].ToString();
                    if (string.IsNullOrEmpty(ID_Token))
                    {
                        ViewBag.error = "Invalid code";
                        ViewBag.error_description = "The ID_Token value is empty string or null";
                        return View("Errorp");
                    }
                }

                var accessToken = TokenResponse["access_token"].ToString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    ViewBag.error = "Invalid code";
                    ViewBag.error_description = "The code value is empty string or null";
                    return View("Errorp");
                }

                if (isOpenId == true)
                {
                    //code for openid connect

                    //validate id_token and get cliam values from  id_token
                    ClaimsPrincipal userObj = openIDHelper.ValidateIdentityToken(ID_Token);
                    if (userObj == null)
                    {
                        ViewBag.error = "Something went wrong ";
                        ViewBag.error_description = "Claim Object getting null value";
                        return View("Errorp");
                    }
                    _logger.LogInformation("Login Callback  : get userinfo from id_token successfully");

                    //get nonce value from session which is send from idp login url
                    var Nonce = HttpContext.Session.GetString("Nonce");
                    if (string.IsNullOrEmpty(Nonce))
                    {
                        ViewBag.error = "Something went wrong ";
                        ViewBag.error_description = "Nonce value not found";
                        return View("Errorp");
                    }

                    //validate nonce value is matched with our nonce value
                    //which is send from login url
                    var nonce = userObj.FindFirst("nonce")?.Value ?? "";
                    if (!string.Equals(nonce, Nonce)) throw new Exception("invalid nonce");

                    var daesClaim = userObj.FindFirst("daes_claims")?.Value ?? "";
                    UserObj userdata = JsonConvert.DeserializeObject<UserObj>(daesClaim);
                    userdata.sub = userObj.FindFirst("sub")?.Value ?? "";

                    user.Uuid = userdata.suid;
                    user.fullname = userdata.name;
                    user.dob = userdata.birthdate;
                    user.mailId = userdata.email;
                    user.mobileNo = userdata.phone;
                    user.sub = int.Parse(userdata.sub);
                }
                else
                {
                    //code for oauth
                    JObject userObj = openIDHelper.GetUserInfo(accessToken).Result;
                    if (userObj.ContainsKey("error") && userObj.ContainsKey("error_description"))
                    {
                        ViewBag.error = userObj["error"].ToString();
                        ViewBag.error_description = userObj["error_description"].ToString();
                        return View("Errorp");
                    }
                    _logger.LogInformation("Login Callback  : get userinfo from idp successfully");

                    user.Uuid = userObj["uuid"].ToString();
                    user.fullname = userObj["name"].ToString();
                    user.dob = userObj["birthdate"].ToString();
                    user.mailId = userObj["email"].ToString();
                    user.mobileNo = userObj["phone_number"].ToString();
                    user.sub = int.Parse(userObj["sub"].ToString());
                }

                var userInDb = await _userConsoleService.GetUserAsync(user.sub);
                if (userInDb == null)
                {
                    _logger.LogError("Login Callback : get user details failed");
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user info");
                    return NotFound();
                }
                _logger.LogInformation("Login Callback  : get user details successfully");
                var roleInDb = await _roleActivityService.GetRoleAsync((int)userInDb.RoleId);
                if (roleInDb == null)
                {
                    _logger.LogError("Login Callback : get user role details failed");
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user role info");
                    return NotFound();
                }
                _logger.LogInformation("Login Callback  : get user role details successfully");
                var rolesList = GetAccessibleModuleList(roleInDb.Id, roleInDb.RoleActivities).Result;
                var isUserChecker = IsCheckerRole(roleInDb.Id, roleInDb.RoleActivities).Result;

                var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
                var isKycAdmin = false;
                if (!string.IsNullOrEmpty(kycAdminRoleId))
                {
                    if (userInDb.RoleId.ToString() == kycAdminRoleId)
                    {
                        rolesList.Add(new Claim(ClaimTypes.Role, "KYC Admin"));
                        isKycAdmin = true;
                    }
                }

                var identity = new ClaimsIdentity(new[] {
                    new Claim("Access_Token", accessToken),
                    new Claim(ClaimTypes.Name, user.fullname),
                    new Claim(ClaimTypes.NameIdentifier, user.Uuid),
                    new Claim("LastLoginTime",userInDb.LastLoginTime.ToString().Replace("/","-")),
                    new Claim(ClaimTypes.Email,user.mailId),
                    new Claim("UserRoleID",userInDb.RoleId.ToString()),
                    new Claim(ClaimTypes.UserData,user.sub.ToString()),
                    new Claim("IsUserChecker",isUserChecker.ToString()),
                    new Claim("UserStatus",userInDb.Status.ToString()),
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                if (isOpenId)
                {
                    identity.AddClaim(new Claim("ID_Token", ID_Token));
                }

                if (rolesList.Count > 0)
                    identity.AddClaims(rolesList);

                var principal = new ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties();
                properties.IsPersistent = true;
                properties.AllowRefresh = false;

                //get session timeout from AdminPortal_SSOConfig db
                var configInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
                if (configInDB == null)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal SSO configuration");
                    return NotFound();
                }
                int SessionTimeOut = configInDB.session_timeout;
                if (0 == SessionTimeOut)
                    SessionTimeOut = 30;

                properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(SessionTimeOut));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

                _logger.LogInformation("Login Callback  :  Login successfully");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "User login successfully", null, user.fullname);

                if (userInDb.Status == "NEW")
                {
                    return RedirectToAction("ChangePassword", "UserDashboard", new { id = user.sub, isFirstLogin = true });
                }
                else if (userInDb.Status == "CHANGE_PASSWORD")
                {
                    return RedirectToAction("SetSecurityQuestion", "UserDashboard");
                }
                else if (userInDb.Status == "SET_FIDO2")
                {
                    return RedirectToAction("RegisterFido2", "Registration", new { id = user.sub });
                }
                else if (isKycAdmin == true)
                {
                    return RedirectToAction("Index", "KycDashboard");
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Login Callback Exception :  {0}", e.Message);
                _logger.LogError(e, "Login Callback Exception ");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "Login Callback : " + e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong!";
                return View("Errorp");
            }
        }

        [NonAction]
        private async Task<List<Claim>> GetAccessibleModuleList(int roleId, IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                _logger.LogError("Login Callback : get all activity list failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var activityListItems = new List<Claim>();

            var roleIdList = _configuration.GetSection("RoleIds").Get<List<int>>();

            if (roleIdList.Count != 0 && !roleIdList.Contains(roleId))
            {
                activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();
            }

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    var ActivityName = roleActivities.Any(x => x.ActivityId == activity.Id && (bool)x.IsEnabled) ? activity.DisplayName : "";
                    if (!String.IsNullOrEmpty(ActivityName))
                    {
                        if (ActivityName == "Reports")
                        {
                            ActivityName = ActivityName + "_" + activity.Id;
                            activityListItems.Add(new Claim(ClaimTypes.Role, ActivityName));
                        }
                        else
                        {
                            activityListItems.Add(new Claim(ClaimTypes.Role, ActivityName));
                        }
                    }
                    var isChecker = roleActivities.Any(x => x.ActivityId == activity.Id && (bool)x.IsEnabled && x.IsChecker == true);
                    if (activity.McSupported && isChecker)
                    {
                        var CheckerActivityName = activity.DisplayName + " Checker";
                        activityListItems.Add(new Claim(ClaimTypes.Role, CheckerActivityName));
                    }
                    if (!String.IsNullOrEmpty(ActivityName))
                    {
                        var Name = "";
                        var isParentExsist = activityLookupItems.Any(x => x.Id == activity.ParentId);
                        if (isParentExsist)
                            Name = activityLookupItems.First(x => x.Id == activity.ParentId).DisplayName;

                        var isTitleAdded = activityListItems.Any(x => x.Value == Name);
                        if (!isTitleAdded && Name != "")
                            activityListItems.Add(new Claim(ClaimTypes.Role, Name));
                    }
                }
            }
            else
            {

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Get Accessible Module List", LogMessageType.FAILURE.ToString(), "Fail to get activity info");
            }


            return activityListItems;
        }

        [NonAction]
        private async Task<bool> IsCheckerRole(int roleId, IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                _logger.LogError("Login Callback : get all activity list failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var IsCheckerRole = false;
            var TotalCheckerActivity = 0;

            var roleIdList = _configuration.GetSection("RoleIds").Get<List<int>>();

            if (roleIdList.Count != 0 && !roleIdList.Contains(roleId))
            {
                activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();
            }

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {

                    var isChecker = roleActivities.Any(x => x.ActivityId == activity.Id && x.IsChecker == true);
                    if (activity.McSupported && isChecker)
                    {
                        TotalCheckerActivity = TotalCheckerActivity + 1;
                    }

                }
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Get User Role Type", LogMessageType.FAILURE.ToString(), "Fail to get activity info");
            }

            if (TotalCheckerActivity != 0)
                IsCheckerRole = true;

            return IsCheckerRole;
        }
        public IActionResult ForgotPassword(string id, int userType)
        {
            return RedirectToAction("ValidateSecurityQuestion", "UserDashboard", new { id = id, type = userType });
        }
        //public IActionResult ForgotPassword(string id, int UserType)
        //{
        //    try
        //    {
        //        //_logger.LogDebug("--> ForgotPassword");
        //        //var ForgotPasswordUrl = string.Format(_configuration["ForgotPasswordUrl"],
        //        //                                    id, UserType);
        //        //_logger.LogDebug("<-- ForgotPassword");
        //        //return Redirect(ForgotPasswordUrl);
        //        return RedirectToAction("ValidateSecurityQuestion", "UserDashboard", new { id = id, type = UserType });
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("SendPushNotification: {0}", e.Message);
        //        _logger.LogDebug("<--SendPushNotification");
        //        return StatusCode(500, "Internal Server Error");
        //    }
        //}

        [HttpPost]
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            // 1. Set the language cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = false }
            );

            // 2. Check if a valid Return URL exists and is local (security practice)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> AuthenticatUser([FromBody] AuthenticateUserViewModel Model)
        {
            var verifyPasswordResponse = await _userConsoleService.VerifyUserPassword(Model.UserEmail, Model.AuthenticationData);

            if (!verifyPasswordResponse.Success)
            {
                var Res1 = new
                {
                    Success = false,
                    Message = verifyPasswordResponse.Message
                };
                return Ok(Res1);
            }

            var userInDb = (UserTable)verifyPasswordResponse.Resource;

            var user = new UserSessionObj();

            if (userInDb == null)
            {
                _logger.LogError("Login Callback : get user details failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user info");
                return NotFound();
            }

            user.Uuid = userInDb.Uuid;
            user.fullname = userInDb.FullName;
            user.mailId = userInDb.MailId;
            user.sub = (int)userInDb.Id;
            user.mobileNo = userInDb.MobileNo;

            _logger.LogInformation("Login Callback  : get user details successfully");

            var roleInDb = await _roleActivityService.GetRoleAsync((int)userInDb.RoleId);

            if (roleInDb == null)
            {
                _logger.LogError("Login Callback : get user role details failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user role info");
                return NotFound();
            }
            _logger.LogInformation("Login Callback  : get user role details successfully");
            var rolesList = GetAccessibleModuleList(roleInDb.Id, roleInDb.RoleActivities).Result;
            var isUserChecker = IsCheckerRole(roleInDb.Id, roleInDb.RoleActivities).Result;

            //var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
            //var isKycAdmin = false;
            //if (!string.IsNullOrEmpty(kycAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == kycAdminRoleId)
            //    {
            //        rolesList.Add(new Claim(ClaimTypes.Role, "KYC Admin"));
            //        isKycAdmin = true;
            //    }
            //}

            var adminRoleId = _configuration.GetValue<string>("AdminRoleId");
            if (!string.IsNullOrEmpty(adminRoleId))
            {
                if (userInDb.RoleId.ToString() == adminRoleId)
                {
                    rolesList.Add(new Claim(ClaimTypes.Role, "Digital ID Admin"));
                    rolesList.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
            }

            var checker = "false";

            var adminCheckerRoleId = _configuration.GetValue<string>("AdminCheckerRoleId");
            if (!string.IsNullOrEmpty(adminCheckerRoleId))
            {
                if (userInDb.RoleId.ToString() == adminCheckerRoleId)
                {
                    rolesList.Add(new Claim(ClaimTypes.Role, "Digital ID Admin Checker"));
                    rolesList.Add(new Claim(ClaimTypes.Role, "Admin Checker"));
                    checker = "true";
                }
            }



            //var commercialAdminRoleId = _configuration.GetValue<string>("CommercialAdminRoleId");
            //var isCommercialAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminRoleId)
            //    {
            //        isCommercialAdmin = true;
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Commercial Admin"));
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Finance Admin"));
            //    }
            //}


            //var commercialAdminCheckerRoleId = _configuration.GetValue<string>("CommercialAdminCheckerRoleId");
            //var isCommercialCheckerAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminCheckerRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminCheckerRoleId)
            //    {
            //        isCommercialCheckerAdmin = true;
            //        checker = "true";
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Commercial Admin Checker"));
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Finance Admin Checker"));
            //    }
            //}

            JObject TokenResponse = openIDHelper.GetAccessToken().Result;
            if (TokenResponse.ContainsKey("error") && TokenResponse.ContainsKey("error_description"))
            {
                ViewBag.error = TokenResponse["error"].ToString();
                ViewBag.error_description = TokenResponse["error_description"].ToString();
                return View("Errorp");
            }

            var accessToken = TokenResponse["access_token"].ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                ViewBag.error = "Invalid code";
                ViewBag.error_description = "The code value is empty string or null";
                return View("Errorp");
            }



            var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.fullname),
                    new Claim("Access_Token", accessToken),
                    new Claim(ClaimTypes.NameIdentifier, user.Uuid),
                    new Claim("LastLoginTime",userInDb.LastLoginTime.ToString().Replace("/","-")),
                    new Claim(ClaimTypes.Email,user.mailId),
                    new Claim("UserRoleID",userInDb.RoleId.ToString()),
                    new Claim(ClaimTypes.UserData,user.sub.ToString()),
                    new Claim("IsUserChecker",isUserChecker.ToString()),
                    new Claim("UserStatus",userInDb.Status.ToString()),
                    new Claim("IsChecker",checker)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

            if (rolesList.Count > 0)
                identity.AddClaims(rolesList);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties();
            properties.IsPersistent = true;
            properties.AllowRefresh = false;

            var configInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (configInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal SSO configuration");
                return NotFound();
            }
            int SessionTimeOut = configInDB.session_timeout;
            if (0 == SessionTimeOut)
                SessionTimeOut = 30;

            properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(SessionTimeOut));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            _logger.LogInformation("Login Callback  :  Login successfully");
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "User login successfully", null, user.fullname);

            var Res = new
            {
                Success = true,
                Message = "Authentication Successful"
            };

            return Ok(Res);
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("Authenticate : User has invalid session redirect to login");
                return RedirectToAction("Index", "Login");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

            var userInDb = await _userConsoleService.GetUserAsync(int.Parse(userId));

            //var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
            //var isKycAdmin = false;
            //if (!string.IsNullOrEmpty(kycAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == kycAdminRoleId)
            //    {
            //        isKycAdmin = true;
            //    }
            //}

            var adminRoleId = _configuration.GetValue<string>("AdminRoleId");
            //var isAdmin = false;
            if (!string.IsNullOrEmpty(adminRoleId))
            {
                if (userInDb.RoleId.ToString() == adminRoleId)
                {
                    //isAdmin = true;
                }
            }

            //var checker = "false";

            var adminCheckerRoleId = _configuration.GetValue<string>("AdminCheckerRoleId");
            //var isAdminChecker = false;
            if (!string.IsNullOrEmpty(adminCheckerRoleId))
            {
                if (userInDb.RoleId.ToString() == adminCheckerRoleId)
                {
                    //isAdminChecker = true;
                    //checker = "true";
                }
            }

            //var commercialAdminRoleId = _configuration.GetValue<string>("CommercialAdminRoleId");
            //var isCommercialAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminRoleId)
            //    {
            //        isCommercialAdmin = true;
            //    }
            //}


            //var commercialAdminCheckerRoleId = _configuration.GetValue<string>("CommercialAdminCheckerRoleId");
            //var isCommercialCheckerAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminCheckerRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminCheckerRoleId)
            //    {
            //        isCommercialCheckerAdmin = true;
            //        checker = "true";
            //    }
            //}

            if (userInDb.Status == "NEW")
            {
                return RedirectToAction("ChangePassword", "UserDashboard", new { id = userInDb.Id, isFirstLogin = true });
            }
            else if (userInDb.Status == "CHANGE_PASSWORD")
            {
                return RedirectToAction("SetSecurityQuestion", "UserDashboard");
            }
            else if (userInDb.Status == "SET_FIDO2")
            {
                return RedirectToAction("RegisterFido2", "Registration", new { id = userInDb.Id });
            }
            //else if (isKycAdmin)
            //{
            //    return RedirectToAction("Index", "KycAdminDashboard");
            //}
            //else if (isCommercialAdmin)
            //{
            //    return RedirectToAction("Index", "CommercialisationDashboard");
            //}
            //else if (isCommercialCheckerAdmin)
            //{
            //    return RedirectToAction("Index", "CommercialisationDashboard");
            //}
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
