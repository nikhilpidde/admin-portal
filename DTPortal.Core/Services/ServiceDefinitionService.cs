using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using System.Linq;

namespace DTPortal.Core.Services
{
    public class ServiceDefinitionService : IServiceDefinitionService
    {
        private readonly HttpClient _client;
        private readonly IMCValidationService _mcValidationService;
        private readonly ILogger<ServiceDefinitionService> _logger;

        public ServiceDefinitionService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServiceDefinitionService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceDefinitionDTO>> GetServiceDefinitionsAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-all-services");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var serviceDefinitions = JsonConvert.DeserializeObject<IEnumerable<ServiceDefinitionDTO>>(apiResponse.Result.ToString());
                        return serviceDefinitions.Where(x => x.Status.ToLower() == "active");
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
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

        public async Task<IEnumerable<ServiceDefinitionDTO>> GetServiceDefinitionsByStakeholderAsync(string stakeholder)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-services-by-stakeholder?stakeHolder={stakeholder}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var serviceDefinitions = JsonConvert.DeserializeObject<IEnumerable<ServiceDefinitionDTO>>(apiResponse.Result.ToString());
                        return serviceDefinitions.Where(x => x.Status.ToLower() == "active");
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
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

        public async Task<ServiceDefinitionDTO> GetServiceDefinitionsAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-service-by-id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var serviceDefinition = JsonConvert.DeserializeObject<ServiceDefinitionDTO>(apiResponse.Result.ToString());
                        return serviceDefinition.Status.ToLower() == "active" ? serviceDefinition : null;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
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
    }
}
