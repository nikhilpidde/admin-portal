using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

using DTPortal.Web.Utilities;

namespace DTPortal.Web.Controllers
{
    [AllowAnonymous]
    public class FileManagerController : Controller
    {
        private readonly DataExportService _dataExportService;
        private readonly IConfiguration _configuration;

        public FileManagerController(DataExportService dataExportService,
            IConfiguration configuration)
        {
            _dataExportService = dataExportService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Download([FromQuery(Name = "value")] string value)
        {
            string fileName = value.Split('=')[1];
            var stream = new MemoryStream(_dataExportService.DownloadFile(fileName, Convert.ToInt32(_configuration["ReportExpirationHours"])));
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }
        [HttpGet]
        public IActionResult DownloadCsv([FromQuery(Name = "value")] string value)
        {
            string fileName = value;
            var stream = new MemoryStream(_dataExportService.DownloadFile(fileName, Convert.ToInt32(_configuration["ReportExpirationHours"])));
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }
    }
}
