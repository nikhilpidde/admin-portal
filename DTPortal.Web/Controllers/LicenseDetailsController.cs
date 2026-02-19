using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using DTPortal.Web.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel.LicenseDetails;

using DTPortal.Core.Domain.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Settings")]
    [Authorize(Roles = "Certificate")]
    [Authorize(Roles = "Certificate Details")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class LicenseDetailsController : Controller
    {
        private readonly ILicenseDetailsService _licenseDetailsService;
        private readonly ISubscriberService _subscriberService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly DataExportService _dataExportService;

        public LicenseDetailsController(ILicenseDetailsService licenseDetailsService,
            ISubscriberService subscriberService,
            IConfiguration configuration,
            IRazorRendererHelper razorRendererHelper,
            DataExportService dataExportService,
            IWebHostEnvironment environment)
        {
            _licenseDetailsService = licenseDetailsService;
            _subscriberService = subscriberService;
            _configuration = configuration;
            _razorRendererHelper = razorRendererHelper;
            _dataExportService = dataExportService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var licenseDetails = _licenseDetailsService.GetLicenseDetailsAsync(_configuration["LicensePath"]);
            var subscribersAndCertificatesCount = await _subscriberService.GetSubscribersAndCertificatesCountAsync();

            LicenseDetailsViewModel viewModel = new LicenseDetailsViewModel
            {
                TotalSubscribersCertificates = String.Format(CultureInfo.InvariantCulture, "{0:N0}", licenseDetails.TotalSubscribersCertificates)
            };

            if (licenseDetails != null && subscribersAndCertificatesCount != null)
            {
                viewModel.SubscribersCertificatesIssued = String.Format(CultureInfo.InvariantCulture, "{0:N0}", subscribersAndCertificatesCount.CertificateCount.TotalCertificates);
                viewModel.SubscribersCertificatesAvailable = String.Format(CultureInfo.InvariantCulture, "{0:N0}", (licenseDetails.TotalCertificates - subscribersAndCertificatesCount.CertificateCount.TotalCertificates));
            }
            else
            {
                viewModel.SubscribersCertificatesIssued = "N/A";
                viewModel.SubscribersCertificatesAvailable = "N/A";
            }

            return View(viewModel);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetPDFBytes([FromBody] LicenseDetailsViewModel licenseDetailsViewModel)
        {
            LicenseDetailsPDFViewModel viewModel = new LicenseDetailsPDFViewModel
            {
                LicenseDetails = licenseDetailsViewModel
            };
            var image = Path.Combine(_environment.WebRootPath, _configuration["PDFLogoPath"]);
            byte[] imageArray = System.IO.File.ReadAllBytes(image);
            string base64Image = Convert.ToBase64String(imageArray);
            viewModel.PdfLogo = "data:image/png;base64, " + base64Image;

            var partialName = "/Views/LicenseDetails/LicenseDetailsPDFView.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, viewModel);
            byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);

            return Json(new { Status = "Success", Title = "Generate PDF", Message = "Successfully Generated PDF bytes", Result = pdfBytes });
        }
    }
}
