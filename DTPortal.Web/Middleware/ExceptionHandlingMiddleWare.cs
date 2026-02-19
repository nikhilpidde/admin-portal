using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using DTPortal.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DTPortal.Web.Middleware
{
    public class ExceptionHandlingMiddleWare : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleWare> _logger;

        public ExceptionHandlingMiddleWare(ILogger<ExceptionHandlingMiddleWare> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (NotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            catch (ServiceNotAvailableException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }
            catch (GatewayTimeoutException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
            }
            catch (RequestTimeoutException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
            }
            catch (APIException)
            {
                context.Response.StatusCode = 1001;
            }
        }
    }
}
