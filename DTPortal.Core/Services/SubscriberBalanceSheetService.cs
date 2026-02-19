using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class SubscriberBalanceSheetService : ISubscriberBalanceSheetService
    {
        private readonly HttpClient _client;
        private readonly ILogger<SubscriberBalanceSheetService> _logger;

        public SubscriberBalanceSheetService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SubscriberBalanceSheetService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }
        public async Task<IEnumerable<SubscriberBalanceSheetDTO>> GetSubscriberBalanceSheet(int type, string value)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-bal-sheet-sub?type={type}&value={value}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IList<SubscriberBalanceSheetDTO>>(apiResponse.Result.ToString());
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
