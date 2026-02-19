using DTPortal.Core.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Serialization;
using System.Text;
using DTPortal.Core.Domain.Models;
using Newtonsoft.Json.Linq;

namespace DTPortal.Core.Services
{
    public class LicenseService : ILicenseService
    {

        private readonly HttpClient _client;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(IMCValidationService mcValidationService, HttpClient httpClient,
            IConfiguration configuration,
            ILogger<LicenseService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:OrganizationOnboardingServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            _client = httpClient;
            _client.Timeout = TimeSpan.FromMinutes(10);
            _logger = logger;
        }

        public async Task<IEnumerable<LicenseDTO>> GetLicenseListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/get/list/licenses");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<LicenseDTO>>(apiResponse.Result.ToString());
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

        public async Task<ServiceResult> GenerateLicenceAsync(string organizatonId, string licenseType, string applicationName)
        {
            try
            {
                JObject values = new JObject();
                values.Add("ouid", organizatonId);
                values.Add("licenseType", licenseType);
                values.Add("applicationType", applicationName);

                string json = JsonConvert.SerializeObject(values,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Add("admin", "WEB");

                _logger.LogInformation("Generate license api call start");
                HttpResponseMessage response = await _client.PostAsync("api/post/generatelicenses", content);
                _logger.LogInformation("Generate license api call end");
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

            return new ServiceResult(false, "An error occurred while generating license. Please try later.");
        } 
    }
}
