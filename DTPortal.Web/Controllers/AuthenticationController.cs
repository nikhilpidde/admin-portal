using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.IDP.DTOs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration Configuration;
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(ILogger<AuthenticationController> logger,
            IConfiguration configuration,
            IAuthenticationService authenticationService)
        {
            _logger = logger;
            Configuration = configuration;
            _authenticationService = authenticationService;
        }

        [Route("token")]
        [EnableCors("AllowedOrigins")]
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> GetAccessToken
            ([FromForm] GetAccessTokenRequest request)
        {
            _logger.LogDebug("--->GetAccessToken");

            // For Staging/Development
            _logger.LogDebug("GetAccessToken Request Data:: {0}", JsonConvert.SerializeObject(request));

            IDP.DTOs.ErrorResponseDTO errResponse = new IDP.DTOs.ErrorResponseDTO();
            var credential = string.Empty;
            var type = string.Empty;

            if (null == request)
            {
                _logger.LogError("Invalid input");
                errResponse.error = "invalid_input";
                errResponse.error_description = "Invalid input";
                return Ok(errResponse);
            }

            if (request.client_assertion_type == null)
            {
                // Check the value of authorization header
                var authHeader = Request.Headers[Configuration["AccessTokenHeaderName"]];
                if (string.IsNullOrEmpty(authHeader))
                {
                    _logger.LogInformation("NO Authorization header received");
                    errResponse.error = "invalid_client";
                    errResponse.error_description = "Invalid Authorization header";
                    return Unauthorized(errResponse);
                }

                // Parse the authorization header
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                if (null == authHeaderVal.Scheme || null == authHeaderVal.Parameter)
                {
                    errResponse.error = "invalid_client";
                    errResponse.error_description = "Invalid Authorization header";
                    return Unauthorized(errResponse);
                }

                if (authHeaderVal.Scheme.Contains("Basic"))
                {
                    credential = authHeaderVal.Parameter;
                    type = "client_secret_basic";
                }
            }
            if (request.client_assertion_type != null)
            {
                if (request.client_assertion_type.Contains
                    ("urn:ietf:params:oauth:client-assertion-type:jwt-bearer"))
                {
                    credential = request.client_assertion;
                    type = "private_key_jwt";
                }
            }
            var result = await _authenticationService.GetAccessToken(
                request, credential, type);
            if (!result.Success)
            {
                // Failure
                errResponse.error = result.error;
                errResponse.error_description = result.error_description;
                return Ok(errResponse);
            }
            else
            {
                if (null != result.scopes && result.scopes.Contains("openid"))
                {
                    if (string.IsNullOrEmpty(result.refresh_token))
                    {
                        // Success
                        var successResponse = new AccessTokenOpenIdResponseDTO();
                        successResponse.access_token = result.access_token;
                        successResponse.expires_in = result.expires_in;
                        successResponse.scopes = result.scopes;
                        successResponse.token_type = result.token_type;
                        successResponse.id_token = result.id_token;

                        return Ok(successResponse);
                    }
                    else
                    {
                        var successResponse = new AccessTokenOpenIdRefreshTokenDTO();
                        successResponse.access_token = result.access_token;
                        successResponse.expires_in = result.expires_in;
                        successResponse.scopes = result.scopes;
                        successResponse.token_type = result.token_type;
                        successResponse.id_token = result.id_token;
                        successResponse.refresh_token = result.refresh_token;

                        return Ok(successResponse);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(result.refresh_token))
                    {
                        // Success
                        var successResponse = new AccessTokenOAuthResponseDTO();
                        successResponse.access_token = result.access_token;
                        successResponse.expires_in = result.expires_in;
                        successResponse.scopes = result.scopes;
                        successResponse.token_type = result.token_type;
                        return Ok(successResponse);
                    }
                    else
                    {
                        // Success
                        var successResponse = new AccessTokenOAuthRefreshTokenDTO();
                        successResponse.access_token = result.access_token;
                        successResponse.expires_in = result.expires_in;
                        successResponse.scopes = result.scopes;
                        successResponse.token_type = result.token_type;
                        successResponse.refresh_token = result.refresh_token;
                        return Ok(successResponse);
                    }
                }
            }
        }
    }
}
