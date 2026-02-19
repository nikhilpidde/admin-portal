using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Newtonsoft.Json.Serialization;
using System.Text;
using DTPortal.Core.Constants;

namespace DTPortal.Core.Services
{
    public class OrganizationPaymentHistoryService : IOrganizationPaymentHistoryService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationPaymentHistoryService> _logger;
        private readonly IMCValidationService _mcValidationService;

        public OrganizationPaymentHistoryService(HttpClient httpClient,
            IConfiguration configuration,
            IMCValidationService mcValidationService,
            ILogger<OrganizationPaymentHistoryService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
            _mcValidationService = mcValidationService;
        }

        public async Task<PaginatedList<OrganizationPaymentHistoryDTO>> GetServiceProviderPaymentHistoryAsync(string uid, int pageIndex = 1, int pageSize = 2)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/payment-history/service-provider/{uid}/{pageIndex}/{pageSize}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryDTO>>(result["data"].ToString());
                        return new PaginatedList<OrganizationPaymentHistoryDTO>(paymentHistory, Convert.ToInt32(result["currentPage"]), pageSize, Convert.ToInt32(result["totalPages"]), Convert.ToInt32(result["totalCount"]));
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

        public async Task<PaginatedList<SubscriberPaymentHistoryDTO>> GetSubscriberPaymentHistoryAsync(string uid, int pageIndex = 1, int pageSize = 2)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-payment-history?suid={uid}");
                //HttpResponseMessage response = await _client.GetAsync($"api/payment-history/subscriber/{uid}/{pageIndex}/{pageSize}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<SubscriberPaymentHistoryDTO>>(result["data"].ToString());
                        return new PaginatedList<SubscriberPaymentHistoryDTO>(paymentHistory, Convert.ToInt32(result["currentPage"]), pageSize, Convert.ToInt32(result["totalPages"]), Convert.ToInt32(result["totalCount"]));
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





        public async Task<IEnumerable<OrganizationPaymentHistoryDTO>> GetOrganizationPaymentHistoryAsync(string data)
        {
            //var list = new List<OrganizationPaymentHistoryDTO>();
            //list.Add(new OrganizationPaymentHistoryDTO
            //{
            //    OrganizationId = "shbjfskjs",
            //    OrganizationName = "org",
            //    PaymentInfo = "[{\"rate\":500,\"discount\":10,\"orgId\":\"91bbbc71-370a-42f7-ad2d-c6327fd1830d\",\"serviceId\":\"1\",\"serviceName\":\"DIGITAL_SIGNATURE\",\"slabId\":\"378\",\"stakeHolder\":\"ORGANIZATION\",\"tax\":5,\"volume\":0},{\"rate\":10000,\"discount\":10,\"orgId\":\"91bbbc71-370a-42f7-ad2d-c6327fd1830d\",\"serviceId\":\"2\",\"serviceName\":\"ESEAL_SIGNATURE\",\"slabId\":\"369\",\"stakeHolder\":\"ORGANIZATION\",\"tax\":12,\"volume\":100}]",
            //    TotalAmountPaid = 3240,
            //    PaymentChannel = "xysd",
            //    TransactionReferenceId = "gkjeigkkerghbk",
            //    InvoiceNumber     = "dbks"
            //});
            //list.Add(new OrganizationPaymentHistoryDTO
            //{
            //    OrganizationId = "shbjfefgssskjs",
            //    OrganizationName = "ordsg",
            //    PaymentInfo = "wegsfssdvsvdsrerhg",
            //    TotalAmountPaid = 3240,
            //    PaymentChannel = "xrvrgerrysd",
            //    TransactionReferenceId = "gkjeigregekkerghbk",
            //    InvoiceNumber = "dbks"
            //});
            //return list;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-org-payment-history?organizationId={data}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryDTO>>(apiResponse.Result.ToString());
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

        public async Task<ServiceResult> AddOrganizationPaymentHistoryAsync(OrganizationPaymentHistoryDTO organizationPaymentHistory, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OrganizationPaymentHistoryActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OrganizationPaymentHistoryActivityId, OperationTypeConstants.Create, organizationPaymentHistory.CreatedBy,
                        JsonConvert.SerializeObject(organizationPaymentHistory));
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

                string json = JsonConvert.SerializeObject(organizationPaymentHistory,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/add-org-payment-history", content);
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

            return new ServiceResult(false, "An error occurred while adding organization payment history. Please try later.");
        }
    }
}
