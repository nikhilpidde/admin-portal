using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using DTPortal.Web.ViewModel.UserDashboard;
using DTPortal.Core.Domain.Services;
using Microsoft.Extensions.Logging;
using static DTPortal.Common.CommonResponse;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DTPortal.Web.Attribute
{
    public class SessionValidationAttribute : ActionFilterAttribute, IResultFilter
    {
        private readonly ISessionService _sessionService;
        private readonly IUserManagementService _userService;
        private readonly ILogger<SessionValidationAttribute> _logger;
        public SessionValidationAttribute(ISessionService sessionService, ILogger<SessionValidationAttribute> logger, IUserManagementService userService)
        {
            _userService = userService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<Response> validateAccessToken(string AccessToken, bool isDasboardUrl)
        {
            try
            {
                _logger.LogInformation("-->validateAccessToken");
                Response response;

                if (isDasboardUrl)
                    response = await _sessionService.ValidateAccessTokenSession(AccessToken);
                else
                    response = await _sessionService.ValidateAccessToken(AccessToken);

                if (response == null)
                {
                    _logger.LogInformation("validateAccessToken : validate accessToken fail getting null response");

                    return new Response { Success = false };
                }
                _logger.LogInformation("validateAccessToken Response = {0}", JsonConvert.SerializeObject(response));
                _logger.LogInformation("<--validateAccessToken");
                return response;
            }
            catch (Exception e)
            {
                _logger.LogInformation("validateAccessToken Exception :{0}", e.Message);
                _logger.LogInformation("<--validateAccessToken");
                return new Response { Success = false };
            }
        }

        public async Task<string> GetUserStatus(int id)
        {
            try
            {
                var UserStatus = await _userService.GetUserStatusAsync(id);
                if (string.IsNullOrEmpty(UserStatus))
                {
                    _logger.LogInformation("GetUserStatus : getuser state null");
                    return "ACTIVE";
                }
                return UserStatus;
            }
            catch (Exception e)
            {
                _logger.LogInformation("GetUserStatus Exception :{0}", e.Message);
                return "ACTIVE";
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _logger.LogInformation("--> Custom Attribute :: OnActionExecuting");

            bool isAjax = filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var RequestPath = filterContext.HttpContext.Request.Path.Value;
            bool isDasboardUrl = RequestPath.StartsWith("/Dashboard");

            var isAuthenticated = filterContext.HttpContext.User?.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                _logger.LogInformation("Custom Attribute::OnActionExecuting :- Access Token not found");
                filterContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (isAjax)
                {
                    filterContext.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                                { "Controller", "Login" },
                                { "Action", "Index" }
                   });
                }
            }
            else
            {
                var status = filterContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserStatus").Value;
                if (status != "ACTIVE")
                {
                    var id = int.Parse(filterContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);
                    var UserStatus = GetUserStatus(id).Result;
                    if (UserStatus == "NEW")
                    {
                        filterContext.Result = new RedirectToRouteResult(
                       new RouteValueDictionary {
                                { "Controller", "UserDashboard" },
                                { "Action", "ChangePassword" },
                                {"id", id},
                                {"isFirstLogin", true}
                           });
                    }
                    else if (UserStatus == "CHANGE_PASSWORD")
                    {
                        filterContext.Result = new RedirectToRouteResult(
                       new RouteValueDictionary {
                                { "Controller", "UserDashboard" },
                                { "Action", "SetSecurityQuestion" }
                           });
                    }
                    else if (UserStatus == "SET_FIDO2")
                    {
                        filterContext.Result = new RedirectToRouteResult(
                      new RouteValueDictionary {
                                { "Controller", "Registration" },
                                { "Action", "RegisterFido2" },
                                { "id", id }
                          });
                    }
                    else
                    {

                    }
                }
            }
            base.OnActionExecuting(filterContext);

        }
    }
}
