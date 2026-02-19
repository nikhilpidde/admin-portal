using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class OrganizationUsageReportService : IOrganizationUsageReportService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationUsageReportService> _logger;

        public OrganizationUsageReportService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OrganizationUsageReportService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<OrganizationUsageReportDTO>> GetOrganizationUsageReports(string organizationUid, string year)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-org-usage-report?orgId={organizationUid}&year={year}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<OrganizationUsageReportDTO>>(apiResponse.Result.ToString());
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

        public async Task<string> DownloadUsageReport(int reportId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/download-pdf?id={reportId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return JsonConvert.DeserializeObject<string>(apiResponse.Result.ToString());
                        return Convert.ToString(apiResponse.Result);
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

        public async Task<ServiceResult> DownloadCurrentMonthUsageReport(string organizationUid)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-report/{organizationUid}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //return JsonConvert.DeserializeObject<string>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, Convert.ToString(apiResponse.Result));
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, Convert.ToString(apiResponse.Result));
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

            return new ServiceResult(false, "Failed to download the report");
        }
    }
}
