using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class AgentService : IAgentService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly ILogger<AgentService> _logger;

        public AgentService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AgentService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:AssistedOnboardingBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<AgentListDTO>> GetAllOnboardingAgent()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get-all/onboarding-agents");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<AgentListDTO>>(apiResponse.Result.ToString());
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
                _logger.LogError(ex.Message);
            }
            return null;
        }


        public async Task<ServiceResult> AddOnboardingAgent(List<AgentDTO> agent)
        {
            try
            {
                string json = JsonConvert.SerializeObject(agent,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("add/onboarding-agents", content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message);
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
                _logger.LogError(ex.Message);
            }

            return null;
        }

        public async Task<ServiceResult> AgentStatus(int id)
        {
            try
            {
                string json = JsonConvert.SerializeObject(id,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"toggle/status/by/id?id={id}", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                        if (apiResponse.Success)
                        {
                            // return JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());  
                            var status = JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());
                            return new ServiceResult(true, apiResponse.Message);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError("GetTemplateDetailsAsync Exception :  {0}", ex.Message);
            }

            return null;
        }
        public async Task<AgentListDTO> GetAgentById(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"getAgent/By/id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                        if (apiResponse.Success)
                        {
                            return JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError("GetTemplateDetailsAsync Exception :  {0}", ex.Message);
            }
            return null;
        }

        public async Task<ServiceResult> DeleteAgent(int id)
        {
            try
            {
                string json = JsonConvert.SerializeObject(id,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"delete/Agent/by/id/{id}", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        // return JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());  
                        // var status = JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        return new ServiceResult(false, apiResponse.Message);
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
                _logger.LogError("GetTemplateDetailsAsync Exception :  {0}", ex.Message);
            }

            return null;
        }
    }
}
