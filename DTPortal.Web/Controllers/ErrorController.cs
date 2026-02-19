using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DTPortal.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewResult view;
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 401:
                    ViewBag.ErrorMessage = "You are not authorized to access this resource";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $" and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("AccessDenied");
                    break;

                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $" and QueryString = {statusCodeResult?.OriginalQueryString}");

                    if (statusCodeResult.OriginalPath.Contains("/FileManager/Download"))
                    {
                        view = View("FileNotFound");
                    }
                    else
                    {
                        view = View("NotFound");
                    }
                    break;

                case 408:
                    ViewBag.ErrorMessage = "The request is timed out";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $" and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("RequestTimedOut");
                    break;

                case 500:
                    ViewBag.ErrorMessage = "Something went wrong. Please try later.";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $" and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("Error");
                    break;

                case 503:
                    ViewBag.ErrorMessage = "Service is unavailable currently, please try after sometime";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $"and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("ServiceUnavailable");
                    break;

                case 504:
                    ViewBag.ErrorMessage = "Gateway Timeout";
                    _logger.LogError($"{statusCode} Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $"and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("GatewayTimeout");
                    break;

                default:
                    _logger.LogError($"Unknown Error Occurred. Path = {statusCodeResult?.OriginalPath}" +
                        $" and QueryString = {statusCodeResult?.OriginalQueryString}");

                    view = View("DefaultError");
                    break;
            }
            return view;
        }

        [Route("Error")]
        public IActionResult Error()
        {
            // Log the exception
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError($"The path {exceptionDetails?.Path} threw an exception" +
                $" {exceptionDetails?.Error}");
            ViewBag.ErrorMessage = exceptionDetails;
            return View();
        }
    }
}
