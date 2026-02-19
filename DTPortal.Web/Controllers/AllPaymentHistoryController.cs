using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.ExtensionMethods;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.AllPaymentHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "All Payment History")]
    [ServiceFilter(typeof(SessionValidationAttribute))]

    public class AllPaymentHistoryController : Controller
    {
        private readonly DataExportService _dataExportService;
        private readonly IAllPaymentHistoryService _allPaymentHistoryService;
        private readonly IRazorRendererHelper _razorRendererHelper;

        public AllPaymentHistoryController(DataExportService dataExportService,
            ILogClient logClient,
            IAllPaymentHistoryService allPaymentHistoryService,
            IRazorRendererHelper razorRendererHelper)
        {
            _dataExportService = dataExportService;
            _allPaymentHistoryService = allPaymentHistoryService;
            _razorRendererHelper = razorRendererHelper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> ExportAllPaymentHistory(AllPaymentHistoryViewModel viewModel)
        //{
        //    DataTable dt = new DataTable("Grid");
        //    dt.Columns.AddRange(new DataColumn[] {
        //                                new DataColumn("DateTime"),
        //                                new DataColumn("Transaction Reference Id"),
        //                                new DataColumn("Aggregator Acknowledgement Id"),
        //                                new DataColumn("Payment Status"),
        //                                new DataColumn("Total Amount"),
        //                                new DataColumn("Mobile Number"),
        //                                });

        //    AllPaymentHistoryDTO allPaymentHistory = new AllPaymentHistoryDTO
        //    {
        //        Timestamp = viewModel.PaymentHistoryDate.Value.ToString("yyyy-MM-dd"),
        //        Status = viewModel.Status.GetValue(),
        //    };

        //    var getAllPaymentHistory = await _allPaymentHistoryService.GetAllPaymentHistoryAsync(allPaymentHistory);

        //    if (getAllPaymentHistory == null)
        //    {
        //        return NotFound();
        //    }
        //    else if(getAllPaymentHistory.Count() == 0)
        //    {
        //        AlertViewModel alert = new AlertViewModel { Message = "No records found" };
        //        TempData["Alert"] = JsonConvert.SerializeObject(alert);
        //        return RedirectToAction("Index");
        //    }

        //    foreach (var paymentHistory in getAllPaymentHistory)
        //    {
        //        dt.Rows.Add(paymentHistory.CreatedOn,paymentHistory.TransactionReferenceId, paymentHistory.AggregatorAcknowledgementId,
        //            paymentHistory.PaymentStatus, paymentHistory.TotalAmount, paymentHistory.EncryptedMobileNumber);
        //    }

        //    StringBuilder sbData = new StringBuilder();

        //    // Only return Null if there is no structure.
        //    if (dt.Columns.Count == 0)
        //        return null;
        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (var col in dt.Columns)
        //        {
        //            if (col == null)
        //                sbData.Append(",");
        //            else
        //                sbData.Append("\"" + col.ToString().Replace("\"", "\"\"") + "\",");
        //        }

        //        sbData.Replace(",", System.Environment.NewLine, sbData.Length - 1, 1);

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            foreach (var column in dr.ItemArray)
        //            {
        //                if (column == null)
        //                    sbData.Append(",");
        //                else
        //                    sbData.Append("\"" + column.ToString().Replace("\"", "\"\"") + "\",");
        //            }
        //            sbData.Replace(",", System.Environment.NewLine, sbData.Length - 1, 1);
        //        }
        //    }

        //    return File(Encoding.UTF8.GetBytes(sbData.ToString()), "text/csv", viewModel.PaymentHistoryDate.Value.ToString("dd-MM-yyyy") + ".csv");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ExportAllPaymentHistory(AllPaymentHistoryViewModel viewModel)
        {
            if(viewModel.PaymentHistoryDate == null)
            {
                return Json(new { Status = "Failed", Title = "Export All Payments history", Message = "Please select date" });
            }

            if(viewModel.Status == 0)
            {
                return Json(new { Status = "Failed", Title = "Export All Payments history", Message = "Please select status" });
            }

            AllPaymentHistoryDTO allPaymentHistory = new AllPaymentHistoryDTO
            {
                Timestamp = viewModel.PaymentHistoryDate.Value.ToString("yyyy-MM-dd"),
                Status = viewModel.Status.GetValue(),
            };

            var getAllPaymentHistory = await _allPaymentHistoryService.GetAllPaymentHistoryAsync(allPaymentHistory);

            if (getAllPaymentHistory == null)
            {
                return Json(new { Status = "Failed", Title = "Export All Payments history", Message = "Not found" });
            }
            else if (getAllPaymentHistory.Count() == 0)
            {
                //AlertViewModel alert = new AlertViewModel { Message = "No records found" };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { Status = "Failed", Title = "Export All Payments history", Message = "No records found" });
            }

            AllPaymentHistoryPdfViewModel pdfViewModel = new AllPaymentHistoryPdfViewModel();
            pdfViewModel.AllPaymentHistory = getAllPaymentHistory;

            var partialName = "/Views/AllPaymentHistory/AllPaymentHistoryPdfView.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, pdfViewModel);
            byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);

            return Json(new { Status = "Success", Title = "Export All Payments history", Message = "Successfully Generated PDF bytes", Result = pdfBytes });
        }
    }
}
