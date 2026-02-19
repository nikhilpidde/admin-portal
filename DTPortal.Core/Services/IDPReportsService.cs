using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class IDPReportsService : IIDPReportsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IDPReportsService> _logger;

        public IDPReportsService(IConfiguration configuration,
            ILogger<IDPReportsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaginatedList<LogReportDTO>> GetReportsAsync(IDPSearchReportsDTO searchReportsDTO, int page = 1)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:IDPLogServiceBaseAddress"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    string json = JsonConvert.SerializeObject(searchReportsDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"api/audit-logs/{page}", content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                        if (apiResponse.Success)
                        {
                            JObject result = (JObject)JToken.FromObject(apiResponse.Result);
                            var logs = JsonConvert.DeserializeObject<IEnumerable<LogReportDTO>>(result["data"].ToString());
                            //return new PaginatedList<LogReportDTO>(logs, Convert.ToInt32(logsArray["pages"]), Convert.ToInt32(logsArray["current"]));
                            return new PaginatedList<LogReportDTO>(logs, Convert.ToInt32(result["currentPage"]), 10, Convert.ToInt32(result["totalPages"]), Convert.ToInt32(result["totalCount"]));
                        }
                        else
                        {
                            // Log the error
                            string error = apiResponse.Message;

                        }
                    }
                    else
                    {
                        // Log the error
                    }
                }
                catch (Exception error)
                {
                    // Log the error
                    _logger.LogError("GetReportsAsync Failed: {0}",
                        error.Message);
                }
            }

            return null;
        }
        public bool VerifyChecksum(LogReportDTO logReport)
        {
            bool result = false;
            try
            {
                result = PKIMethods.Instance.VerifyChecksum(
                    JsonConvert.SerializeObject(logReport, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            }
            catch (PKIException ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return result;
        }
    }
}
