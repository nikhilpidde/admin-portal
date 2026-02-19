using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using DTPortal.Core.DTOs;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class PackageService : IPackageService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly ILogger<PackageService> _logger;

        public PackageService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PackageService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<PackageDTO>> GetAllPackagesAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/package/list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<PackageDTO>>(apiResponse.Result.ToString());
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

        public async Task<PackageDTO> GetPackageAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/package/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<PackageDTO>(apiResponse.Result.ToString());
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

        public async Task<bool> IsPackageExists(string packageCode)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/package/isexists?packageCode={packageCode}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    return apiResponse.Success;
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
            return false;
        }

        public async Task<ServiceResult> AddPackageAsync(PackageDTO package, bool makerCheckerFlag = false)
        {
            try
            {
                var isExists = await IsPackageExists(package.PackageCode);
                if (isExists == true)
                {
                    _logger.LogError($"Package with Package Code ={package.PackageCode} already exists");
                    return new ServiceResult(false, "Package already exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PackageActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PackageActivityId, OperationTypeConstants.Create, package.CreatedBy,
                        JsonConvert.SerializeObject(package));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                string json = JsonConvert.SerializeObject(package, 
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/package/add", content);
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

            return new ServiceResult(false, "An error occurred while creating the package. Please try later.");
        }

        public async Task<ServiceResult> EnablePackageAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var package = await GetPackageAsync(id);
                if (package == null)
                {
                    return new ServiceResult(false, "Package doesn't exists");
                }

                package.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PackageActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PackageActivityId, OperationTypeConstants.Enable, uuid,
                        JsonConvert.SerializeObject(package));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                string json = JsonConvert.SerializeObject(new { Enable = true, UpdatedBy = uuid },
                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/package/status?id={id}", content);
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
            return new ServiceResult(false, "An error occurred while enabling the package. Please try later.");
        }

        public async Task<ServiceResult> DisablePackageAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var package = await GetPackageAsync(id);
                if (package == null)
                {
                    return new ServiceResult(false, "Package doesn't exists");
                }

                package.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PackageActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PackageActivityId, OperationTypeConstants.Disable, uuid,
                        JsonConvert.SerializeObject(package));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                string json = JsonConvert.SerializeObject(new { Enable = false, UpdatedBy = uuid },
                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/package/status?id={id}", content);
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
            return new ServiceResult(false, "An error occurred while disabling the package. Please try later.");
        }

        public async Task<ServiceResult> DeletePackageAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var package = await GetPackageAsync(id);
                if (package == null)
                {
                    return new ServiceResult(false, "Package doesn't exists");
                }

                package.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PackageActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PackageActivityId, OperationTypeConstants.Delete, uuid,
                        JsonConvert.SerializeObject(package));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                string json = JsonConvert.SerializeObject(new { UpdatedBy = uuid },
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync($"api/package/delete?id={id}", content);
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

            return new ServiceResult(false, "An error occurred while deleting package. Please try later.");
        }
    }
}
