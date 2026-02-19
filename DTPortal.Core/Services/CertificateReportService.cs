using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Core.Services
{
    public class CertificateReportService : ICertificateReportService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CertificateReportService> _logger;

        public CertificateReportService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CertificateReportService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:ControlledOnboardingServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CertificateReportsDTO>> GetCertificateReportsAsync(string startDate, string endDate)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/subscriber/details/report?startDate=" + startDate + "&endDate=" + endDate);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        if(apiResponse.Result == null)
                        {
                            return new List<CertificateReportsDTO>();
                        }
                        else
                        {
                            var certificateReports = JsonConvert.DeserializeObject<IEnumerable<CertificateReportsDTO>>(apiResponse.Result.ToString());
                            return certificateReports;
                        }
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
