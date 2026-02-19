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
    public class AllPaymentHistoryService : IAllPaymentHistoryService
    {
        private readonly HttpClient _client;
        private readonly ILogger<AllPaymentHistoryService> _logger;

        public AllPaymentHistoryService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AllPaymentHistoryService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<AllPaymentHistoryDTO>> GetAllPaymentHistoryAsync(AllPaymentHistoryDTO allPaymentHistory)
        {
            try
            {
                string json = JsonConvert.SerializeObject(allPaymentHistory,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/payment-history/records", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<AllPaymentHistoryDTO>>(apiResponse.Result.ToString());
                        return paymentHistory;
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


        public async Task<APIResponse> GetOrganizationPaymentHistoryAsync(string orgId)
        {

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/eseal-payments-history?orgId={orgId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryStatusDTO>>(apiResponse.Result.ToString());
                        //paymentHistory = paymentHistory.OrderBy(x => x.CreatedOn);
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return apiResponse;
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
            return new APIResponse() { Success = false, Message = "Something went wrong." };
        }

        public async Task<APIResponse> GetWalletHistoryAsync(string orgId)
        {

            try
            {
               //orgId = "f206b706-4173-4a70-abaf-80244e8d8bd7";
                HttpResponseMessage response = await _client.GetAsync($"api/get/wallet-payments-history?orgId={orgId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryStatusDTO>>(apiResponse.Result.ToString());
                        //paymentHistory = paymentHistory.OrderBy(x => x.CreatedOn);
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return apiResponse;
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
            return new APIResponse() { Success = false, Message = "Something went wrong." };
        }
    }
}
