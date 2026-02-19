using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ocsp;

namespace DTPortal.Core.Services
{
    public class BannerConfigService : IBannerConfigService
    {
        private readonly HttpClient _client;
        private readonly ILogger<BannerConfigService> _logger;
        public BannerConfigService(HttpClient httpclient,
            IConfiguration configuration,
            ILogger<BannerConfigService> logger)
        {
            _logger = logger;
            _client = httpclient;
            _client.BaseAddress = new Uri(configuration["APIServiceLocations:ConfigurationServiceBaseAddress"]);
        }

        public async Task<List<BannerTextData>> GetLatestBannerTextsAsync(string bannerTextId)
        {
            try
            {
                HttpResponseMessage response =
                    await _client.GetAsync($"api/Banners/GetLatestBannerTexts?bannerTextId={bannerTextId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Request failed with status {response.StatusCode}");
                    return new List<BannerTextData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<APIResponse>(content);

                if (apiResponse == null || !apiResponse.Success)
                {
                    _logger.LogError(apiResponse?.Message ?? "API failed");
                    return new List<BannerTextData>();
                }

                var resultDto =
                    JsonConvert.DeserializeObject<GetLatestBannerTextsResponseDTO>(
                        apiResponse.Result.ToString());

                return resultDto?.BannerTexts ?? new List<BannerTextData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching banner texts");
                return new List<BannerTextData>();
            }
        }



        public async Task<ServiceResult> UpdateBannerTextsAsync(UpdateBannerTextRequestDTO request)
        {
            try
            {
                string json = JsonConvert.SerializeObject(
                    request,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response =
                    await _client.PostAsync("api/Banners/UpdateBannerTexts", content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse =
                        JsonConvert.DeserializeObject<APIResponse>(
                            await response.Content.ReadAsStringAsync());

                    return new ServiceResult(apiResponse.Success, apiResponse.Message);
                }

                _logger.LogError("Banner text update failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new ServiceResult(false, "Update failed");
        }

    }

}
