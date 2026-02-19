using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class OfflinePaymentService : IOfflinePaymentService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IMCValidationService _mcValidationService;
        public OfflinePaymentService(HttpClient httpclient,
            IConfiguration configuration,
            IMCValidationService mCValidationService,
            ILogger<ConfigurationService> logger)
        {
            _mcValidationService = mCValidationService;
            _logger = logger;
            _client = httpclient;
            _client.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
        }


        public async Task<ServiceResult> GetOfflineCredits(CreditAllocationListDTO creditAllocationDTO, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OfflinePaymentHistoryActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OfflinePaymentHistoryActivityId, OperationTypeConstants.Create, creditAllocationDTO.CreatedBy,
                        JsonConvert.SerializeObject(creditAllocationDTO));
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
                string json = JsonConvert.SerializeObject(creditAllocationDTO,
                   new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/manual-credit-allocation", content);
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

        public async Task<IEnumerable<CreditAllocationListDTO>> GetAllOfflinePaymentListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/manual-credit-allocation/records");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<CreditAllocationListDTO>>(apiResponse.Result.ToString());
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

        public async Task<CreditAllocationListDTO> GetOfflinePaymentDetailsAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/manual-payment-record/by/id/{id}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var offPayment = JsonConvert.DeserializeObject<CreditAllocationListDTO>(apiResponse.Result.ToString());
                        return offPayment;

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
