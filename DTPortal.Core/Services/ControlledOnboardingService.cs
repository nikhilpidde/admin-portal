using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using DTPortal.Core.Utilities;

namespace DTPortal.Core.Services
{
    public class ControlledOnboardingService : IControlledOnboardingService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ControlledOnboardingService> _logger;
        private readonly IMCValidationService _mcValidationService;

        public ControlledOnboardingService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ControlledOnboardingService> logger, 
            IMCValidationService mcValidationService)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:ControlledOnboardingServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;

            _logger = logger;
            _mcValidationService = mcValidationService;
        }

        public async Task<ServiceResult> AddTrustedUsersAsync(ControlledOnboardingDTO userList, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.ControlledOnboardingActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.ControlledOnboardingActivityId, OperationTypeConstants.Create, userList.CreatedBy,
                        JsonConvert.SerializeObject(userList));
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

                string json = JsonConvert.SerializeObject(userList,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/post/trusted/user", content);
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

                        if (apiResponse.Result != null)
                        {
                            return new ServiceResult(false, apiResponse.Message, apiResponse.Result);
                        }

                        return new ServiceResult(false, apiResponse.Message, apiResponse.Result);
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

            return new ServiceResult(false, "An error occurred while adding trusted users. Please try later.");
        }

        public async Task<TrustedSpocEmailDTO> GetTrustedUserByEmail(string email)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/subscriber/details/emailId/{email}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        TrustedSpocEmailDTO dto = JsonConvert.DeserializeObject<TrustedSpocEmailDTO>(apiResponse.Result.ToString());
                        return dto;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
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
