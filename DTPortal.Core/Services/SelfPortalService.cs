using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class SelfPortalService : ISelfPortalService
    {
        private readonly ILogger<SelfPortalService> _logger;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IMCValidationService _mcValidationService;
        public SelfPortalService(IConfiguration configuration,
            HttpClient httpClient,
            ILogger<SelfPortalService> logger,
            IMCValidationService mcValidationService)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;
            _logger = logger;
            _configuration = configuration;
            _mcValidationService = mcValidationService;
        }

        public async Task<IEnumerable<SelfOrganizationNewDTO>> GetAllSelfServiceOrganizationListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/all-organizations");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<SelfOrganizationNewDTO>>(apiResponse.Result.ToString());
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
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
        public async Task<SelfOrganizationNewDTO> GetSelfServiceNewOrganizationDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation("Get self service organization details api call start");
                //_client.BaseAddress = new Uri(_configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
                HttpResponseMessage response = await _client.GetAsync($"get/organisation/by/id/{id}");
                _logger.LogInformation("Get self service organization details api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var organization = JsonConvert.DeserializeObject<SelfOrganizationNewDTO>(apiResponse.Result.ToString());
                        return organization;
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
        public async Task<ServiceResult> ApproveOrganizationNewAsync(SelfOrganizationNewDTO organizationDTO, bool makerCheckerFlag = false)
        {
            try
            {
                _logger.LogInformation("ApproveOrganizationAsync start");

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingApprovalRequestActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingApprovalRequestActivityId, OperationTypeConstants.Approve, organizationDTO.CreatedBy,
                        JsonConvert.SerializeObject(organizationDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }
                _client.DefaultRequestHeaders.Remove("SpocOfficialEmail");
                _client.DefaultRequestHeaders.Add(
                    "SpocOfficialEmail",
                    organizationDTO.SpocOfficialEmail ?? string.Empty);
                string json = JsonConvert.SerializeObject(organizationDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Approve Organization api call start");
                var url = $"change/org/status/by/id/APPROVED/{organizationDTO.OrgDetailsId}";

                HttpResponseMessage response = await _client.PostAsync(url, content);
                // HttpResponseMessage response = await _client.PostAsync("api/public/change/org/status/by/id/APPROVED/{organizationDTO.OrgDetailsId}", content);
                _logger.LogInformation("Approve Organization api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        _logger.LogInformation("ApproveOrganizationAsync end");
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

            _logger.LogInformation("ApproveOrganizationAsync end");
            return new ServiceResult(false, "An error occurred while approving the organization. Please try later.");
        }

        public async Task<ServiceResult> RejectOrganizationNewAsync(SelfOrganizationNewDTO organizationDTO, bool makerCheckerFlag = false)
        {
            try
            {
                _logger.LogInformation("RejectOrganizationAsync start");

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingApprovalRequestActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingApprovalRequestActivityId, OperationTypeConstants.Reject, organizationDTO.CreatedBy,
                        JsonConvert.SerializeObject(organizationDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }


                string json = JsonConvert.SerializeObject(organizationDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Reject Organization api call start");
                var url = $"change/org/status/by/id/REJECTED/{organizationDTO.OrgDetailsId}";

                HttpResponseMessage response = await _client.PostAsync(url, content);
                // HttpResponseMessage response = await _client.PostAsync("api/public/change/org/status/by/id/REJECTED/{organizationDTO.OrgDetailsId}", content);
                _logger.LogInformation("Reject Organization api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        _logger.LogInformation("RejectOrganizationAsync end");
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

            _logger.LogInformation("RejectOrganizationAsync end");
            return new ServiceResult(false, "An error occurred while rejecting the organization. Please try later.");
        }
        public async Task<SelfServiceOrganizationDTO> GetSelfServiceOrganizationDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation("Get self service organization details api call start");
                //_client.BaseAddress = new Uri(_configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
                HttpResponseMessage response = await _client.GetAsync($"get/formById/after/submit/{id}");
                _logger.LogInformation("Get self service organization details api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var organization = JsonConvert.DeserializeObject<SelfServiceOrganizationDTO>(apiResponse.Result.ToString());
                        return organization;
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

        public async Task<ServiceResult> ApproveOrganizationAsync(SelfServiceOrganizationDTO organizationDTO, bool makerCheckerFlag = false)
        {
            try
            {
                _logger.LogInformation("ApproveOrganizationAsync start");

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingApprovalRequestActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingApprovalRequestActivityId, OperationTypeConstants.Approve, organizationDTO.CreatedBy,
                        JsonConvert.SerializeObject(organizationDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                _client.DefaultRequestHeaders.Add("adminugpassemail", organizationDTO.AdminUgpassEmail);

                string json = JsonConvert.SerializeObject(organizationDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Approve Organization api call start");
                HttpResponseMessage response = await _client.PostAsync("post/approve/organization", content);
                _logger.LogInformation("Approve Organization api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("ApproveOrganizationAsync end");
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        _logger.LogInformation("ApproveOrganizationAsync end");
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

            _logger.LogInformation("ApproveOrganizationAsync end");
            return new ServiceResult(false, "An error occurred while approving the organization. Please try later.");
        }

        public async Task<ServiceResult> RejectOrganizationAsync(SelfServiceOrganizationDTO organizationDTO, bool makerCheckerFlag = false)
        {
            try
            {
                _logger.LogInformation("RejectOrganizationAsync start");

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingApprovalRequestActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingApprovalRequestActivityId, OperationTypeConstants.Reject, organizationDTO.CreatedBy,
                        JsonConvert.SerializeObject(organizationDTO));
                    if (!isRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(false, isRequired.Message);
                    }
                    if (isRequired.Result)
                    {
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }


                string json = JsonConvert.SerializeObject(organizationDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Reject Organization api call start");
                HttpResponseMessage response = await _client.PostAsync("api/post/reject/organization", content);
                _logger.LogInformation("Reject Organization api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("RejectOrganizationAsync end");
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        _logger.LogInformation("RejectOrganizationAsync end");
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

            _logger.LogInformation("RejectOrganizationAsync end");
            return new ServiceResult(false, "An error occurred while rejecting the organization. Please try later.");
        }

        public async Task<ServiceResult> GetRejectedReasonAsync()
        {
            try
            {
                _logger.LogInformation("Get reject reason api call start");
                //_client.BaseAddress = new Uri(_configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
                HttpResponseMessage response = await _client.GetAsync($"get/orgRejectedReason");
                _logger.LogInformation("Get reject reason api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var list = JsonConvert.DeserializeObject<IList<string>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, "Reject reason received Successfully", list);
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

        public async Task<BusinessRequirementsDTO> GetQuestionsAsync(int id)
        {
            try
            {
                //_logger.LogInformation("Get reject reason api call start");
                HttpResponseMessage response = await _client.GetAsync($"GetAll/Answers/" + id);
                //_logger.LogInformation("Get reject reason api call end");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var list = JsonConvert.DeserializeObject<BusinessRequirementsDTO>(apiResponse.Result.ToString());
                        return list;
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

        public async Task<ServiceResult> RecommendSoftwareAsync(RecommendedSoftwareDTO recommendedSoftwareDTO)
        {
            try
            {
                _logger.LogInformation("RecommendSoftwareAsync start");

                string json = JsonConvert.SerializeObject(recommendedSoftwareDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Recommend software api call start");
                HttpResponseMessage response = await _client.PostAsync("post/save/recommended/software", content);
                _logger.LogInformation("Recommend software api call end");
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

            _logger.LogInformation("RecommendSoftwareAsync end");
            return new ServiceResult(false, "An error occurred. Please try later.");
        }
    }
}
