using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using DTPortal.Core.Domain.Services.Communication;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog;
using DTPortal.Core.Constants;
using System.Collections.Specialized;

namespace DTPortal.Core.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BeneficiaryService> _logger;
        private readonly IMCValidationService _mcValidationService;
        // readonly ConfigurationService _configurationService;
        public BeneficiaryService(HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BeneficiaryService> logger,
            IMCValidationService mcValidationService)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:BeneficiaryPrivilegesBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _client = httpClient;

            _configuration = configuration;
            _logger = logger;
            _mcValidationService = mcValidationService;
        }

        public async Task<ServiceResult> GetBeneficiaryPrivilegesAsync() 
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/active/beneficiary-privileges");
                _logger.LogInformation("Get Get Beneficiary Privileges Async api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation(apiResponse.Message);
                        var result = JsonConvert.DeserializeObject<List<BeneficiaryPrivilegeDTO>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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
                    return new ServiceResult(false, "Internal Error");
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        public async Task<ServiceResult> GetAllBeneficiariesListByOrgIdAsync(string orgId)
        {
            try
            {

                //_logger.LogInformation("Get All Subscription Verification List by Org Id api call start");
                HttpResponseMessage response = await _client.GetAsync($"api/get/all/beneficiaries/by/sponsor-id/{orgId}");
                _logger.LogInformation("Get All Subscription Verification List by Org Id api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation(apiResponse.Message);
                        var result = JsonConvert.DeserializeObject<IEnumerable<SponsorBeneficiaryDTO>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, result);
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
                    return new ServiceResult(false,"Internal Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
                //return new ServiceResult(ex.Message);
                return null;
            }

        }

        public async Task<ServiceResult> AddBeneficiaryAsync(BeneficiaryAddDTO beneficiaryAddDTO,string createdBy, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.BeneficiaryId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.BeneficiaryId, OperationTypeConstants.Create, createdBy,
                        JsonConvert.SerializeObject(beneficiaryAddDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                string json = JsonConvert.SerializeObject(beneficiaryAddDTO, 
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync("api/add/beneficiaries",content);
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
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public async Task<ServiceResult> GetBeneficiaryDetailsById(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/beneficiary/by/id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var details = JsonConvert.DeserializeObject<BeneficiaryEditDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true,apiResponse.Message, details);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, null);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                    return new ServiceResult(false, "Internal Error", null);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, "Internal Error", null);

            }
        }

        public async Task<ServiceResult> EditBeneficiaryAsync(BeneficiaryUpdateDTO beneficiaryDTO,string createdBy, bool makerCheckerFlag = false)
        {
            try
            {
                // string json = JsonConvert.SerializeObject(beneficiaryDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.BeneficiaryId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.BeneficiaryId, OperationTypeConstants.Update, createdBy,
                        JsonConvert.SerializeObject(beneficiaryDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }
                string json = JsonConvert.SerializeObject(beneficiaryDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/update/beneficiaries", content);
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
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
            }
            return null;
        }

        public async Task<ServiceResult> AddMultipleBeneficiariesAsync(IList<BeneficiaryAddDTO> beneficiaries ,string createdBy, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.BeneficiaryId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.BeneficiaryId, OperationTypeConstants.CreateMany, createdBy,
                        JsonConvert.SerializeObject(beneficiaries));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                _logger.LogInformation("AddMultipleBeneficiariesAsync start");
                string json = JsonConvert.SerializeObject(beneficiaries,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClient client1 = new HttpClient();

                client1.BaseAddress = new Uri(_configuration["APIServiceLocations:OrganizationOnboardingServiceBaseAddress"]);
                _logger.LogInformation("api call start");
                //HttpResponseMessage response = await _client.PostAsync("api/add/multiple/beneficiaries", content);
                var response = await client1.PostAsync($"api/add/multiple/beneficiaries", content);
                _logger.LogInformation(" api call end");
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

            _logger.LogInformation(" AddMultipleBeneficiariesAsync end");
            return new ServiceResult(false, "An error occurred while adding Beneficiaries. Please try later.");
        }


    }

}
