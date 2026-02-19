using DTPortal.Core.Domain.Services;
using DTPortal.Web.Attribute;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.CertificateReports;
using DTPortal.Web.ViewModel.LicenseDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Settings")]
    [Authorize(Roles = "Certificate")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class CertificateReportsController : Controller
    {
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly DataExportService _dataExportService;
        private readonly ICertificateReportService _certificateReportService;

        public CertificateReportsController(IRazorRendererHelper razorRendererHelper,
            DataExportService dataExportService,
            ICertificateReportService certificateReportService)
        {
            _razorRendererHelper = razorRendererHelper;
            _dataExportService = dataExportService;
            _certificateReportService = certificateReportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPDFBytes(CertificateReportsViewModel viewModel)
        {

            var certificateReports = await _certificateReportService.GetCertificateReportsAsync(viewModel.StartDate.ToString("yyyy:MM:dd 00:00:00"),viewModel.EndDate.ToString("yyyy:MM:dd 00:00:00"));
            if(certificateReports == null)
            {
                return Json(new { Status = "Failed", Title = "Download Certificate Reports", Message = "Something went wrong" });
            }
            else if(certificateReports.Count() == 0)
            {
                return Json(new { Status = "Failed", Title = "Download Certificate Reports", Message = "No records found" });
            }

            CertificateReportsPDFViewModel pdfViewModel = new CertificateReportsPDFViewModel
            {
                CertificateReports = certificateReports
            };

            var partialName = "/Views/CertificateReports/CertificateReportsPDFView.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, pdfViewModel);
            byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);

            return Json(new { Status = "Success", Title = "Download Certificate Reports", Message = "Successfully downloaded certificate eports", Result = pdfBytes });
        }
    }
}
