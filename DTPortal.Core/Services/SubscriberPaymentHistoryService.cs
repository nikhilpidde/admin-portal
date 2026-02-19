using DTPortal.Core.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace DTPortal.Core.Services
{
    public class SubscriberPaymentHistoryService : ISubscriberPaymentHistoryService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationPaymentHistoryService> _logger;

        public SubscriberPaymentHistoryService(HttpClient httpClient,
            IConfiguration configuration,
            IMCValidationService mcValidationService,
            ILogger<OrganizationPaymentHistoryService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<SubscriberPaymentHistoryDTO>> GetSubscriberPaymentHistoryAsync(int type, string value)
        {
            //var list = new List<SubscriberPaymentHistoryDTO>();
            //list.Add(new SubscriberPaymentHistoryDTO
            //{
            //    PaymentInfo = "[{\"rate\":502130,\"discount\":10,\"orgId\":\"91bbbc71-370a-42f7-ad2d-c6327fd1830d\",\"serviceId\":\"1\",\"serviceName\":\"DIGITAL_SIGNATURE\",\"slabId\":\"378\",\"stakeHolder\":\"ORGANIZATION\",\"tax\":5,\"volume\":0},{\"rate\":10000,\"discount\":10,\"orgId\":\"91bbbc71-370a-42f7-ad2d-c6327fd1830d\",\"serviceId\":\"2\",\"serviceName\":\"ESEAL_SIGNATURE\",\"slabId\":\"369\",\"stakeHolder\":\"ORGANIZATION\",\"tax\":12,\"volume\":100}]",
            //    TotalAmountPaid = 3240,
            //    PaymentChannel = "xysd",
            //    TransactionReferenceId = "gkjeigkkerghbk",
            //    PaymentRefNumber = "fshjhb"
            //});
            //list.Add(new SubscriberPaymentHistoryDTO
            //{
            //    PaymentInfo = "geljkrghreg",
            //    TotalAmountPaid = 320,
            //    PaymentChannel = "xyfbfdthsd",
            //    TransactionReferenceId = "gkjeithdtgghbk",
            //    PaymentRefNumber = "fshgy"
            //});
            //return list;

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-sub-pay-history?type={type}&value={value}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                        var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<SubscriberPaymentHistoryDTO>>(apiResponse.Result.ToString());
                        var subscriberPaymentHistory = paymentHistory.OrderByDescending(x => x.CreatedOn);
                        return subscriberPaymentHistory;
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
