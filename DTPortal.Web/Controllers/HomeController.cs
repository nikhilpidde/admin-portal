using DTPortal.Core.Domain.Services;
using DTPortal.Core.Services;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class HomeController : Controller
    {
        private readonly ILogoService _logoService;
        private readonly ILicenseDetailsService _licenseDetailsService;
        private readonly ISubscriberService _subscriberService;
        private readonly IConfiguration _configuration;
        public HomeController(ILogoService logoService,
            ILicenseDetailsService licenseDetailsService,
            ISubscriberService subscriberService,
            IConfiguration configuration
            ) 
        { 
            _logoService = logoService;
            _licenseDetailsService = licenseDetailsService;
            _subscriberService = subscriberService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("KYC Admin"))
                return RedirectToAction("Index", "KycDashboard");

            return RedirectToAction("Index", "Login");
        }
        [HttpPost]
        public IActionResult SetTheme(string data)
        {
            CookieOptions cookie = new CookieOptions();
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Append("theme", data, cookie);
            return Ok();
        }
        [HttpGet]
        public async Task<JsonResult> GetLogoImage()
        {
            var settings = await _logoService.GetLogoPrimary();
            if (settings != null)
            {
                return Json(Encoding.ASCII.GetString(settings.Value));
            }

            return Json(null);
        }
        [HttpGet]
        public async Task<string> GetAvailableLicenses()
        {
            var licenseDetails = _licenseDetailsService.GetLicenseDetailsAsync(_configuration["LicensePath"]);
            var subscribersAndCertificatesCount = await _subscriberService.GetSubscribersAndCertificatesCountAsync();

            if (licenseDetails != null && subscribersAndCertificatesCount != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0:N0}", licenseDetails.TotalSubscribersCertificates - subscribersAndCertificatesCount.CertificateCount.TotalCertificates);
            }
            else
            {
                return "N/A";
            }
        }
    }
}
