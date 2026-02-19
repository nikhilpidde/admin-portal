using DTPortal.Core.Domain.Services;
using DTPortal.Core.Services;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.License;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    public class LicenseController : Controller
    {
        private readonly ILicenseService _licenseService;
        public LicenseController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var licenseList = await _licenseService.GetLicenseListAsync();
            if(licenseList == null)
            {
                return NotFound();
            }

            LicenseListViewModel viewModel = new LicenseListViewModel()
            {
                LicenseList = licenseList
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateLicense(string orgId, string licenseType, string applicationName)
        {
            var response = await _licenseService.GenerateLicenceAsync(orgId, licenseType, applicationName);
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
    }
}
