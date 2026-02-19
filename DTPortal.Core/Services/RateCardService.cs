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
    public class RateCardService : IRateCardService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly ILogger<RateCardService> _logger;

        public RateCardService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RateCardService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<RateCardDTO>> GetAllRateCardsAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-all-rate-cards");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<RateCardDTO>>(apiResponse.Result.ToString());
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

        public async Task<RateCardDTO> GetRateCardAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-rate-card/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<RateCardDTO>(apiResponse.Result.ToString());
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

        public async Task<bool> IsRateCardExists(int serviceId, string serviceFor)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/rate-card/checkcombination?serviceFor={serviceFor}&serviceId={serviceId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    return Convert.ToBoolean(apiResponse.Result);
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

        public async Task<ServiceResult> AddRateCardAsync(RateCardDTO rateCard, bool makerCheckerFlag = false)
        {
            try
            {
                var isExists = await IsRateCardExists(rateCard.ServiceDefinitions.Id, rateCard.StakeHolder);
                if (isExists == true)
                {
                    _logger.LogError($"Rate Card with combination Service Name ={rateCard.ServiceDefinitions.ServiceDisplayName} and Service For = {rateCard.StakeHolder} already exists");
                    return new ServiceResult(false, "Rate Card already exists");
                }



                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RateCardActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.RateCardActivityId, OperationTypeConstants.Create, rateCard.CreatedBy,
                        JsonConvert.SerializeObject(rateCard));
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



                string json = JsonConvert.SerializeObject(rateCard,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/rate-card/add", content);
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

            return new ServiceResult(false, "An error occurred while creating the rate card. Please try later.");
        }

        public async Task<ServiceResult> UpdateRateCardAsync(RateCardDTO rateCard, bool makerCheckerFlag = false)
        {
            try
            {
                var rateCardInDb = await GetRateCardAsync(rateCard.Id);
                if (rateCardInDb == null)
                {
                    return new ServiceResult(false, "Rate Card doesn't exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RateCardActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.RateCardActivityId, OperationTypeConstants.Update, rateCard.UpdatedBy,
                        JsonConvert.SerializeObject(rateCard));
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

                string json = JsonConvert.SerializeObject(rateCard,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/rate-card/update", content);
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

            return new ServiceResult(false, "An error occurred while updating the rate card. Please try later.");
        }

        public async Task<ServiceResult> EnableRateCardAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var rateCard = await GetRateCardAsync(id);
                if (rateCard == null)
                {
                    return new ServiceResult(false, "Rate Card doesn't exists");
                }

                //rateCard.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RateCardActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.RateCardActivityId, OperationTypeConstants.Enable, uuid,
                        JsonConvert.SerializeObject(rateCard));
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

                HttpResponseMessage response = await _client.PostAsync($"api/rate-card/status?id={id}", content);
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
            return new ServiceResult(false, "An error occurred while enabling the rate card. Please try later.");
        }

        public async Task<ServiceResult> DisableRateCardAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var rateCard = await GetRateCardAsync(id);
                if (rateCard == null)
                {
                    return new ServiceResult(false, "Rate Card doesn't exists");
                }

                //rateCard.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RateCardActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.RateCardActivityId, OperationTypeConstants.Disable, uuid,
                        JsonConvert.SerializeObject(rateCard));
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

                HttpResponseMessage response = await _client.PostAsync($"api/rate-card/status?id={id}", content);
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
            return new ServiceResult(false, "An error occurred while disabling the rate card. Please try later.");
        }

        public async Task<ServiceResult> DeleteRateCardAsync(int id, string uuid, bool makerCheckerFlag = false)
        {
            try
            {
                var rateCard = await GetRateCardAsync(id);
                if (rateCard == null)
                {
                    return new ServiceResult(false, "Rate Card doesn't exists");
                }

                //rateCard.UpdatedBy = uuid;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.RateCardActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.RateCardActivityId, OperationTypeConstants.Delete, uuid,
                        JsonConvert.SerializeObject(rateCard));
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

                HttpResponseMessage response = await _client.PostAsync($"api/rate-card/delete?id={id}", content);
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

            return new ServiceResult(false, "An error occurred while deleting rate card. Please try later.");
        }
    }
}
