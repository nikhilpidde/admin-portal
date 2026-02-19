using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DTPortal.Core.Services
{
    public class PrivilegeRequestService : IPrivilegeRequestService
    {

        private readonly HttpClient _client;
        private readonly ILogger<PrivilegeRequestService> _logger;

        public PrivilegeRequestService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PrivilegeRequestService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PrivilegeRequestBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }


        public async Task<ServiceResult> GetAllUniquePreviligesAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/privileges");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return new ServiceResult(true,apiResponse.Message,apiResponse.Result);
                        var result = JsonConvert.DeserializeObject<List<string>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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


        public async Task<ServiceResult> GetAllPrivilegesAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/all/privileges");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return new ServiceResult(true,apiResponse.Message,apiResponse.Result);
                        var result = JsonConvert.DeserializeObject<List<PreviligeDetails>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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

        public async Task<ServiceResult> GetPrivilegeByIdAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/privilege/by/id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var result = JsonConvert.DeserializeObject<PreviligeDetails>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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


        public async Task<ServiceResult> UpdatePrivilegeAsync(UpdatePrivilegeDTO updatePrivilegeModel)
        {
            try
            {
                string json = JsonConvert.SerializeObject(updatePrivilegeModel,
                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync("api/update/organization/privilege", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var result = JsonConvert.DeserializeObject<PreviligeDetails>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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


        public async Task<ServiceResult> GetPrivilegesByOrganizationIdAsync(string organizationId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/privilege/by/orgId/{organizationId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return new ServiceResult(true,apiResponse.Message,apiResponse.Result);
                        var result = JsonConvert.DeserializeObject<OrganizationPrivilegeDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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


        public async Task<ServiceResult> UpdateOrganizationPrivilegesAsync(UpdateOrganizationPrivilegesDTO updateOrgPrivilegesModel)
        {
            try
            {
                string json = JsonConvert.SerializeObject(updateOrgPrivilegesModel,
                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync("api/update/organization/privileges", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return new ServiceResult(true,apiResponse.Message,apiResponse.Result);
                        List<PreviligeDetails> result = null;
                        if (apiResponse.Result != null)
                        {
                            result = JsonConvert.DeserializeObject<List<PreviligeDetails>>(apiResponse.Result.ToString());
                        }

                        return new ServiceResult(true, apiResponse.Message, result);
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
