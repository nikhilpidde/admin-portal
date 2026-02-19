using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class MobileVersionConfigurationService : IMobileVersionConfigurationService
    {
        private readonly HttpClient _client;
        private readonly ILogger<SubscriberService> _logger;
        private readonly string _accessTokenHeaderName;

        public MobileVersionConfigurationService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SubscriberService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:MobileVersionConfigurationServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _accessTokenHeaderName = "Authorization";
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<MobileVersionDTO>> GetAllSupportedMobileVersionsAsync(string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");
                HttpResponseMessage response = await _client.GetAsync("get/onboarding/dataframe?methodname=getAppConfigList");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<MobileVersionDTO>>(apiResponse.Result.ToString());
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

        public async Task<MobileVersionDTO> GetSupportedMobileVersionByIdAsync(int id, string token)
        {
            try
            {
                var mobileVersions = await GetAllSupportedMobileVersionsAsync(token);
                if (mobileVersions == null)
                    return null;

                MobileVersionDTO mobileVersion = mobileVersions.Where(x => x.Id == id).SingleOrDefault();
                if (mobileVersion == null)
                {
                    _logger.LogError($"The requested mobile version id = {id} is not found");
                    return null;
                }

                return mobileVersion;                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

        public async Task<ServiceResult> AddSupportedMobileVersionAsync(MobileVersionDTO mobileVersion, string token)
        {
            try
            {
                var mobileVersions = await GetAllSupportedMobileVersionsAsync(token);
                if (mobileVersions.Any(x => x.OsVersion == mobileVersion.OsVersion))
                {
                    _logger.LogError($"Mobile version {mobileVersion.OsVersion} already exists");
                    return new ServiceResult(false, "Mobile version already exists");
                }

                string json = JsonConvert.SerializeObject(new { ServiceMethod = "addAppConfig", RequestBody = mobileVersion }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");
                HttpResponseMessage response = await _client.PostAsync("post/onboarding/dataframe", content);
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

            return new ServiceResult(false, "An error occurred while creating the Mobile version configuration. Please try later.");
        }

        public async Task<ServiceResult> UpdateSupportedMobileVersionAsync(MobileVersionDTO mobileVersion, string token)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new { ServiceMethod = "addAppConfig", RequestBody = mobileVersion }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");
                HttpResponseMessage response = await _client.PostAsync("post/onboarding/dataframe", content);
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

            return new ServiceResult(false, "An error occurred while updating the the Mobile version configuration. Please try later.");
        }
    }
}
