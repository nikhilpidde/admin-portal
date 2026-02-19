using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IClientService _clientService;
        private readonly ISubscriberService _subscriberService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly ILogger<DashboardService> _logger;
        private readonly IOrganizationService _organizationService;
        public DashboardService(IOrganizationService organizationService,IClientService clientService,
            ISubscriberService subscriberService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DashboardService> logger)
        {
            _configuration = configuration;
            httpClient.BaseAddress = new Uri(_configuration["APIServiceLocations:CentralLogServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            _organizationService= organizationService;
            _clientService = clientService;
            _subscriberService = subscriberService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<CumulativeCountDTO> GetCumulativeCountAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/log-counts");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                        CumulativeCountDTO cumulativeCount = JsonConvert.DeserializeObject<IEnumerable<CumulativeCountDTO>>(result["cumulativeCount"].ToString()).ElementAt(0);
                        SubscribersCountDTO subscribersCount = await _subscriberService.GetSubscribersCountAsync();
                        OrganizationCountDTO organizationsCount = await _organizationService.GetOrganizationStatusCount();
                        if (organizationsCount != null)
                        {
                            cumulativeCount.activeOrganizations = organizationsCount.activeOrganizations;
                            cumulativeCount.inactiveOrganizations=organizationsCount.inactiveOrganizations;
                            cumulativeCount.registeredOrganizations = organizationsCount.registeredOrganizations;
                            cumulativeCount.totalOrganizations=organizationsCount.totalOrganizations;

                        }
                        if (subscribersCount != null)
                        {
                            cumulativeCount.CountOfActiveSubscribers = subscribersCount.ActiveSubscribers;
                            cumulativeCount.CountOfInactiveSubscribers = subscribersCount.InActiveSubscribers;
                            cumulativeCount.CountOfSubscribers = subscribersCount.ActiveSubscribers + subscribersCount.InActiveSubscribers;
                        }
                        else
                        {
                            cumulativeCount.CountOfSubscribers = 0;
                            cumulativeCount.CountOfActiveSubscribers = 0;
                            cumulativeCount.CountOfInactiveSubscribers = 0;
                        }
                        var clientsCount = await _clientService.GetAllClientsCount();
                        if (clientsCount != null)
                        {
                            cumulativeCount.CountOfActiveServiceProviders = clientsCount.Active;
                            cumulativeCount.CountOfInactiveServiceProviders = clientsCount.InActive;
                        }
                        else
                        {
                            cumulativeCount.CountOfActiveServiceProviders = 0;
                            cumulativeCount.CountOfInactiveServiceProviders = 0;
                        }
                        return cumulativeCount;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                        $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

        public async Task<GraphDTO> GetGraphCountAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/statistics/1");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<GraphDTO>(apiResponse.Result.ToString());
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

        public async Task<GraphDTO> GetGraphCountAsync(string serviceProviderName)
        {
            var serviceProviderClient = await _clientService.GetClientByAppNameAsync(serviceProviderName);
            if (serviceProviderClient == null)
            {
                return null;
            }

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/statistics/{serviceProviderClient.ClientId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<GraphDTO>(apiResponse.Result.ToString());
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
