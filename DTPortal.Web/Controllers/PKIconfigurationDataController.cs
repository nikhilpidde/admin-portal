using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [AllowAnonymous]
    public class PKIConfigurationDataController : Controller
    {
        private readonly IPKIConfigurationService _pkiConfigurationService;
        private readonly ILogger<PKIConfigurationController> _logger;

        public PKIConfigurationDataController(IPKIConfigurationService pkiConfigurationService,
           ILogger<PKIConfigurationController> logger)
        {
            _pkiConfigurationService = pkiConfigurationService;
            _logger = logger;
        }

        [HttpGet]
        //[Route("ConfigurationData")]
        public async Task<IActionResult> GetConfigurationData()
        {
            var configurationData = await _pkiConfigurationService.GetConfigurationDataAsync("configuration");
            if (configurationData == null)
            {
                return Ok(new APIResponse { Success = false, Message = "Configuration data not found", Result = null });
            }

            return Ok(new APIResponse { Success = true, Message = "Success", Result = configurationData.Value });
        }

        [HttpPost]
        //[Route("GenerateTimestamp")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> GenerateTimestamp()
        {
            try
            {
                byte[] requestBody = null;
                using (var ms = new MemoryStream(2048))
                {
                    await Request.Body.CopyToAsync(ms);
                    requestBody = ms.ToArray();  // returns base64 encoded string JSON result
                }
                var result = _pkiConfigurationService.GenerateTimestamp(requestBody);
                return File(result, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        //[Route("POSDigiTimeStamp")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> POSDigiTimeStamp()
        {
            try
            {
                byte[] requestBody = null;
                using (var ms = new MemoryStream(2048))
                {
                    await Request.Body.CopyToAsync(ms);
                    requestBody = ms.ToArray();  // returns base64 encoded string JSON result
                }
                var result = _pkiConfigurationService.POSDigiTimeStamp(requestBody);
                return File(result, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
