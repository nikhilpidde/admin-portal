using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class SigningCreditsService: ISigningCreditsService
    {
        private readonly HttpClient _client;
        private readonly IMCValidationService _mcValidationService;
        private readonly ILogger<OrganizationService> _logger;
        public SigningCreditsService(HttpClient httpClient,
            IConfiguration configuration,
            IMCValidationService mcValidationService,
            ILogger<OrganizationService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:BucketConfigurationBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;
            _mcValidationService = mcValidationService;
            _client.Timeout = TimeSpan.FromMinutes(10);
            _logger = logger;
        }
        public async Task<ServiceResult> GetBucketList()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/bucket-config-list/by/ouid?ouid=null");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var jsonArray = JArray.Parse(apiResponse.Result.ToString());
                        List<BucketListDTO> bucketList = jsonArray.ToObject<List<BucketListDTO>>();
                        return new ServiceResult(true, "get bucket Configuration List Success", bucketList);
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
        public async Task<ServiceResult> GetBucketDetailsById(string Id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/bucket-details/id?id={Id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var jsonArray = JArray.Parse(apiResponse.Result.ToString());
                        //List<BucketListDTO> bucketList = jsonArray.ToObject<List<BucketListDTO>>();
                        var bucketDetails=JsonConvert.DeserializeObject<BucketDetailsDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, "get bucket Configuration List Success", bucketDetails);
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
        public async Task<ServiceResult> SaveBucket(SaveBucketConfigDTO saveBucketConfigDTO,string createdBy, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.ExclusiveAppsActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.ExclusiveAppsActivityId, OperationTypeConstants.Create, createdBy,
                        JsonConvert.SerializeObject(saveBucketConfigDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }
                string json = JsonConvert.SerializeObject(saveBucketConfigDTO);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"api/post/add/org-bucket-config", content).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return new ServiceResult(true, apiResponse.Message, apiResponse.Result);

                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, apiResponse.Result);
                    }
                }
                _logger.LogError(response.ToString());
                return new ServiceResult(false, "Internal Error", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return new ServiceResult(false,ex.Message , null);
            }
        }
        public async Task<ServiceResult> UpdateBucket(UpdateBucketConfigDTO updateBucketConfigDTO,string createdBy, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.ExclusiveAppsActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.ExclusiveAppsActivityId, OperationTypeConstants.Update, createdBy,
                        JsonConvert.SerializeObject(updateBucketConfigDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }
                string json = JsonConvert.SerializeObject(updateBucketConfigDTO);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _client.PostAsync($"api/update/bucket-config", content).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return new ServiceResult(true, apiResponse.Message, apiResponse.Result);

                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, apiResponse.Result);
                    }
                }
                _logger.LogError(response.ToString());
                return new ServiceResult(false, "Internal Error", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return new ServiceResult(false, ex.Message, null);
            }
        }
        public async Task<ServiceResult> GetBucketConfigById(string Id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/bucket-config/by/id?id={Id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var jsonArray = JArray.Parse(apiResponse.Result.ToString());
                        //List<BucketListDTO> bucketList = jsonArray.ToObject<List<BucketListDTO>>();
                        var bucketDetails = JsonConvert.DeserializeObject<BucketListDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, "get bucket Configuration List Success", bucketDetails);
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
        public async Task<ServiceResult> BucketHistoryList(string Id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/bucket-history/by/bucket-config-id?bucketConfigId={Id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var jsonArray = JArray.Parse(apiResponse.Result.ToString());
                        List<BucketDetailsDTO> bucketList = jsonArray.ToObject<List<BucketDetailsDTO>>();
                        return new ServiceResult(true, "get bucket History List Success", bucketList);
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
    }
}
