using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly HttpClient _client;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<HealthCheckService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:HealthCheckServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.Timeout = TimeSpan.FromMinutes(2);
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceHealthDTO>> GetServiceCheckAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("healthcheck-details");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<ServiceHealthDTO>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

        public async Task<ServiceHealthDTO> GetPKITimestampServiceCheckAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("APIStatus/PKITimeStamp");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ServiceHealthDTO>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new RequestTimeoutException();
            }
            catch (GatewayTimeoutException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new GatewayTimeoutException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<ServiceHealthHistory>> GetServiceHealthHistoryAsync(string serviceName)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/servicehistory/{serviceName}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        JObject jObject = (JObject)JToken.FromObject(apiResponse.Result);
                        return JsonConvert.DeserializeObject<IEnumerable<ServiceHealthHistory>>(jObject["serviceHistory"].ToString());
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new RequestTimeoutException();
            }
            catch (GatewayTimeoutException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new GatewayTimeoutException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }
    }
}
