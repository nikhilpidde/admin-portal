using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Services
{
    public class ServiceHealthStatusService : IServiceHealthStatusService
    {
        private readonly ILogger<ServiceHealthStatusService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SSOConfig ssoConfig;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceHealthStatusService(IUnitOfWork unitOfWork,
            ILogger<ServiceHealthStatusService> logger,
            IGlobalConfiguration globalConfiguration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _globalConfiguration = globalConfiguration;
            _httpClientFactory = httpClientFactory;

            // Get SSO Configuration
            ssoConfig = _globalConfiguration.GetSSOConfiguration();
            if (null == ssoConfig)
            {
                _logger.LogError("Get SSO Configuration failed in service health");
                throw new NullReferenceException();
            }
        }


        // Get service status
        private async Task<StatusResponse> GetConnectionStatus(string serviceUrl)
        {
            StatusResponse response = new StatusResponse();

            if (string.IsNullOrEmpty(serviceUrl))
            {
                _logger.LogError("GenerateSignature failed:Invalid Parameter");
                response.status = false;
                response.message = "Invalid Parameter";
                return response;
            }

            try
            {
                //using (var client = new HttpClient())
                //{
                HttpClient client = _httpClientFactory.CreateClient();

                // Assign the base address
                client.BaseAddress = new Uri(serviceUrl);

                // Set Request Timeout
                client.Timeout = TimeSpan.FromSeconds(5);

                string requestUri = null;

                // Call the webservice with get method
                var result = await client.GetAsync(requestUri,
                    HttpCompletionOption.ResponseHeadersRead);

                // Check the status code
                if (result.IsSuccessStatusCode)
                {
                    if (serviceUrl != ssoConfig.service_urls.idp)
                    {
                        // Read the response
                        response = await result.Content.ReadFromJsonAsync
                            <StatusResponse>();
                    }
                    else
                    {
                        response.status = true;
                    }

                }
                else
                {
                    _logger.LogError($"GetServiceStatus failed returned status code : {0}",
                        result.StatusCode);
                }
                //}
            }
            catch (TimeoutException error)
            {
                _logger.LogError("GetServiceStatus failed: {0}",
                    error.Message);

                response.status = false;
                response.message = "E_CONNECTION_TIMEOUT";
                return response;
            }
            catch (Exception error)
            {
                _logger.LogError("GetServiceStatus failed: {0}",
                error.Message);

                // Exception occured if it not able to connect to service
                if (error.HResult == -2147467259)
                {
                    response.status = false;
                    response.message = "E_CONNECTION_FAILED";
                    return response;
                }
            }

            return response;
        }

        // GetServiceStatus
        public async Task<ServiceHealthStatusResponse> GetServiceStatus()
        {
            var result = new ServiceHealthStatusResponse();

            var response = await GetConnectionStatus(ssoConfig.service_urls.idp);
            if (response.status == true)
            {
                var IDPServiceResponse = new ServiceResponse();
                IDPServiceResponse.ServiceName = "Identity Provider";
                IDPServiceResponse.Status = true;
                result.serviceResponses.Add(IDPServiceResponse);
            }
            else
            {
                var IDPServiceResponse = new ServiceResponse();
                IDPServiceResponse.ServiceName = "Identity Provider";
                IDPServiceResponse.Status = false;
                result.serviceResponses.Add(IDPServiceResponse);
            }

            response = await GetConnectionStatus(ssoConfig.service_urls.pki);
            if (response.status == false)
            {
                if (null != response.message)
                {
                    if (response.message.Contains("E_TRANSACTION_HANDLER_NOT_RUNNING"))
                    {
                        var PKIServiceResponse = new ServiceResponse();
                        PKIServiceResponse.ServiceName = "Registration Authority";
                        PKIServiceResponse.Status = true;
                        result.serviceResponses.Add(PKIServiceResponse);

                        var RAServiceResponse = new ServiceResponse();
                        RAServiceResponse.ServiceName = "PKI";
                        RAServiceResponse.Status = false;
                        result.serviceResponses.Add(RAServiceResponse);
                    }

                    if (response.message.Contains("E_CONNECTION_TIMEOUT") ||
                        response.message.Contains("E_CONNECTION_FAILED"))
                    {

                        var PKIServiceResponse = new ServiceResponse();
                        PKIServiceResponse.ServiceName = "Registration Authority";
                        PKIServiceResponse.Status = false;
                        result.serviceResponses.Add(PKIServiceResponse);

                        var RAServiceResponse = new ServiceResponse();
                        RAServiceResponse.ServiceName = "PKI";
                        RAServiceResponse.Status = false;
                        result.serviceResponses.Add(RAServiceResponse);
                    }
                }
                else
                {
                    var PKIServiceResponse = new ServiceResponse();
                    PKIServiceResponse.ServiceName = "Registration Authority";
                    PKIServiceResponse.Status = false;
                    result.serviceResponses.Add(PKIServiceResponse);

                    var RAServiceResponse = new ServiceResponse();
                    RAServiceResponse.ServiceName = "PKI";
                    RAServiceResponse.Status = false;
                    result.serviceResponses.Add(RAServiceResponse);
                }
            }
            else if (response.status == true)
            {
                var PKIServiceResponse = new ServiceResponse();
                PKIServiceResponse.ServiceName = "Registration Authority";
                PKIServiceResponse.Status = true;
                result.serviceResponses.Add(PKIServiceResponse);

                var RAServiceResponse = new ServiceResponse();
                RAServiceResponse.ServiceName = "PKI";
                RAServiceResponse.Status = true;
                result.serviceResponses.Add(RAServiceResponse);
            }

            return result;
        }
    }
}
