using DTPortal.Core.Utilities;
using DTPortal.Web.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class LogoutController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly OpenID openIDHelper;
        
        public LogoutController(IConfiguration configuration, IHttpClientFactory httpClientFactory,IGlobalConfiguration globalConfiguration)
        {
            _configuration = configuration;
            openIDHelper = new OpenID(_configuration, httpClientFactory, globalConfiguration);
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            var state = Guid.NewGuid().ToString("N");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");

        }

        [HttpGet]
        public async Task<IActionResult> CallBack()
        {
            // HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Login");
        }
    }
}
