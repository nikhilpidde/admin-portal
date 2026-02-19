using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel.Dashboard;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using System;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IClientService _clientService;
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogReportService _reportsService;

        public DashboardController(IDashboardService dashboardService,
            IClientService clientService,
            IHealthCheckService healthCheckService,
            ILogReportService reportsService)
        {
            _dashboardService = dashboardService;
            _clientService = clientService;
            _healthCheckService = healthCheckService;
            _reportsService = reportsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCumulativeCount()
        {
            var cumulativeCount = await _dashboardService.GetCumulativeCountAsync();
            return Json(cumulativeCount);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetAdminTimeLine()
        {
            var logs = await _reportsService.GetAdminLogReportAsync();
            return PartialView("_AdminTimeline", logs);
        }

        [HttpGet]
        public async Task<string[]> GetServiceProviderNames(string request)
        {
            return await _clientService.GetAllClientAppNames(request);
        }

        [HttpGet]
        public async Task<JsonResult> GetCompleteGraphDetails()
        {
            var graphDetails = await _dashboardService.GetGraphCountAsync();
            if (graphDetails == null)
                return Json(new { Status = "Failed", Message = "Failed to get data" });
            else
                return Json(new { Status = "Success", Message = "Successfully received data", Data = graphDetails });
        }

        [HttpGet]
        public async Task<PartialViewResult> GetServiceHealth()
        {
            var serviceHealthCheck = await _healthCheckService.GetServiceCheckAsync();
            return PartialView("_ServiceHealth", serviceHealthCheck);
        }

        [HttpGet]
        public async Task<IActionResult> GetPKITimestampServiceHealth()
        {
            var serviceHealthCheck = await _healthCheckService.GetPKITimestampServiceCheckAsync();
            if (serviceHealthCheck == null)
            {
                return NotFound();
            }

            return PartialView("_PKITimestampServiceHealth", serviceHealthCheck);
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceHealthHistory(string serviceName, string displayName)
        {
            var serviceHealthHistory = await _healthCheckService.GetServiceHealthHistoryAsync(serviceName);
            if (serviceHealthHistory == null)
            {
                return NotFound();
            }

            ServiceHealthHistoryViewModel viewModel = new ServiceHealthHistoryViewModel()
            {
                ServiceName = serviceName,
                DisplayName = displayName,
                ServiceHealthHistory = serviceHealthHistory
            };

            return PartialView("_ServiceHealthHistory", viewModel);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceHealthHistory>>> GetFilteredServiceHealthHistory(string serviceName, string startDate, string endDate)
        {
            var serviceHealthHistory = await _healthCheckService.GetServiceHealthHistoryAsync(serviceName);
            if (serviceHealthHistory == null)
            {
                return NotFound();
            }

            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                serviceHealthHistory = serviceHealthHistory.Where(x => Convert.ToDateTime(x.DateTime) >= Convert.ToDateTime(startDate)
                                                    && Convert.ToDateTime(x.DateTime) <= Convert.ToDateTime(endDate))
                                           .ToList();
            }

            return PartialView("_FilteredServiceHealthHistory", serviceHealthHistory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetServiceProviderGraphDetails(string serviceProviderName)
        {
            var graphDetails = await _dashboardService.GetGraphCountAsync(serviceProviderName);
            if (graphDetails == null)
                return Json(new { Status = "Failed", Message = "Failed to get data" });
            else
                return Json(new { Status = "Success", Message = "Successfully received data", Data = graphDetails });
        }
    }
}
