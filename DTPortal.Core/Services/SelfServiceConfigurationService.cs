using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class SelfServiceConfigurationService : ISelfServiceConfigurationService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SelfServiceConfigurationService> _logger;

        public SelfServiceConfigurationService(HttpClient httpClient, IConfiguration configuration, ILogger<SelfServiceConfigurationService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:SelfServiceConfigurationBaseAddress"]);           
            _client = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResult> GetAllConfigCategories()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/all/categories");
                _logger.LogInformation("Get all categories list api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation(apiResponse.Message);
                        var result = JsonConvert.DeserializeObject<List<SelfServiceCategoryDTO>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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
                    return new ServiceResult(false, "Internal Error");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, ex.Message);

            }
        }

        public async Task<ServiceResult> GetCategoryFieldNameById(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/category-fields/by/id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var details = JsonConvert.DeserializeObject<OrgCategoryFieldDetailsDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, details);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, null);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                    return new ServiceResult(false, "Internal Error", null);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, "Internal Error", null);

            }
        }

        public async Task<ServiceResult> UpdateCatogeryFields(OrgCategoryFieldDetailsDTO fieldsDto)
        {
            try
            {
                _logger.LogInformation(fieldsDto.ToString());
                string json = JsonConvert.SerializeObject(fieldsDto,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInformation(json);
                HttpResponseMessage response = await _client.PostAsync("add/rules/category", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
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
                _logger.LogError(ex, ex.Message);
            }

            return new ServiceResult(false, "An error occurred while updating Category fields. Please try later.");
        }
        public async Task<ServiceResult> GetCategoryByOrganizationId(string organizationId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/org-category/category-id/{organizationId}");
                _logger.LogInformation("Get Category By OrganizationId api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation(apiResponse.Message);
                        var result = JsonConvert.DeserializeObject<OrganizationCategoryDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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
                    return new ServiceResult(false, "Internal Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, ex.Message);
            }
        }

    }
}
