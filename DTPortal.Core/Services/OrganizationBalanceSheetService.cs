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
    public class OrganizationBalanceSheetService : IOrganizationBalanceSheetService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationBalanceSheetService> _logger;

        public OrganizationBalanceSheetService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OrganizationBalanceSheetService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;
            _logger = logger;
        }

        public async Task<ServiceResult> GetBalanceSheetDetailsAsync(OrganizationBalanceSheetDTO balanceSheet)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-bal-sheet-org?orgId={balanceSheet.OrganizationId}");
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

            return null;
        }
    }
}
