using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DTPortal.Core.Services
{
    public class OnboardingTemplateService : IOnboardingTemplateService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly ILogger<OnboardingTemplateService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _accessTokenHeaderName;

        public OnboardingTemplateService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OnboardingTemplateService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:OnboardingServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _configuration=configuration;
            _accessTokenHeaderName = "Authorization";
            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<OnboardingTemplateDTO>> GetAllTemplatesAsync(string token)
        {
            try
            {
                _logger.LogInformation("Starting GetAllTemplatesAsync method execution.");

                // Ensure header management is clean
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _logger.LogInformation("Access token header already exists. Removing old header before adding a new one.");
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _logger.LogInformation("Adding new access token header.");
                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                string requestUri = $"get/onboarding/dataframe?methodname=getTemplates";
                _logger.LogInformation("Sending GET request to URI: {RequestUri}", requestUri);

                HttpResponseMessage response = await _client.GetAsync(requestUri);
                _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Response content successfully read. Deserializing APIResponse.");

                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("APIResponse indicates success. Deserializing OnboardingTemplateDTO list.");
                        var templates = JsonConvert.DeserializeObject<IEnumerable<OnboardingTemplateDTO>>(apiResponse.Result.ToString());
                        _logger.LogInformation("Successfully deserialized {Count} templates.", templates?.Count() ?? 0);

                        return templates;
                    }
                    else
                    {
                        _logger.LogError("APIResponse returned unsuccessful. Message: {Message}", apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError("The request with URI={RequestUri} failed with status code={StatusCode}",
                        response.RequestMessage.RequestUri, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetAllTemplatesAsync: {Message}", ex.Message);
            }

            _logger.LogInformation("GetAllTemplatesAsync method execution completed with no data returned.");
            return null;
        }


        public async Task<IEnumerable<OnboardingMethodDTO>> GetAllMethodsAsync(string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe?methodname=getMethods");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<OnboardingMethodDTO>>(apiResponse.Result.ToString());
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

        public async Task<IEnumerable<OnboardingStepDTO>> GetAllStepsAsync(string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe?methodname=getSteps");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<OnboardingStepDTO>>(apiResponse.Result.ToString());
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

        public async Task<OnboardingTemplateDTO> GetTemplateAsync(int id, string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?id={id}&methodname=getTemplateByID");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<OnboardingTemplateDTO>(apiResponse.Result.ToString());
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

        public async Task<bool> IsTemplateExists(string templateName, string methodName, string token)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    ServiceMethod = "isTemplateExist",
                    RequestBody = new { TemplateName = templateName, MethodName = methodName }
                }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.PostAsync("post/onboarding/dataframe", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    return apiResponse.Success;
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
            return false;
        }

        public async Task<ServiceResult> AddTemplateAsync(OnboardingTemplateDTO onboardingTemplate, string token, bool makerCheckerFlag = false)
        {
            try
            {
                var isExists = await IsTemplateExists(onboardingTemplate.TemplateName, onboardingTemplate.TemplateMethod,token);
                if (isExists == true)
                {
                    _logger.LogError($"Template with combination Templae Name ={onboardingTemplate.TemplateName} and Method Name = {onboardingTemplate.TemplateMethod} already exists");
                    return new ServiceResult(false, "Template already exists");
                }

                OnboardingCreateTemplateDTO createTemplateDTO = new OnboardingCreateTemplateDTO
                {
                    ServiceMethod = "addTemplate",
                    RequestBody = onboardingTemplate
                };

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingTemplateActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingTemplateActivityId, OperationTypeConstants.Create, onboardingTemplate.CreatedBy,
                        JsonConvert.SerializeObject(onboardingTemplate));
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

                string json = JsonConvert.SerializeObject(createTemplateDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.PostAsync("post/onboarding/dataframe", content);
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

            return new ServiceResult(false, "An error occurred while creating the template. Please try later.");
        }

        public async Task<ServiceResult> UpdateTemplateAsync(OnboardingTemplateDTO onboardingTemplate, string token, bool makerCheckerFlag = false)
        {
            try
            {
                var template = await GetTemplateAsync(onboardingTemplate.TemplateId,token);
                if (template == null)
                {
                    return new ServiceResult(false, "Template doesn't exists");
                }

                OnboardingCreateTemplateDTO createTemplateDTO = new OnboardingCreateTemplateDTO
                {
                    ServiceMethod = "addTemplate",
                    RequestBody = onboardingTemplate
                };

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingTemplateActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check Approval is required for the operation
                    var isRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingTemplateActivityId, OperationTypeConstants.Update, onboardingTemplate.UpdatedBy,
                        JsonConvert.SerializeObject(onboardingTemplate));
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

                string json = JsonConvert.SerializeObject(createTemplateDTO,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.PostAsync("post/onboarding/dataframe", content);
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

            return new ServiceResult(false, "An error occurred while updating the template. Please try later.");
        }

        public async Task<ServiceResult> PublishTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false)
        {
            try
            {
                var template = await GetTemplateAsync(id,token);
                if (template == null)
                {
                    return new ServiceResult(false, "Template doesn't exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingTemplateActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingTemplateActivityId, OperationTypeConstants.Publish, uuid,
                        JsonConvert.SerializeObject(template));
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

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?methodname=publishTemplateByID&id={id}");
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
            return new ServiceResult(false, "An error occurred while publishing the template. Please try later.");
        }

        public async Task<ServiceResult> UnPublishTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false)
        {
            try
            {
                var template = await GetTemplateAsync(id,token);
                if (template == null)
                {
                    return new ServiceResult(false, "Template doesn't exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingTemplateActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingTemplateActivityId, OperationTypeConstants.Unpublish, uuid,
                        JsonConvert.SerializeObject(template));
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

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?methodname=unPublishTemplateByID&id={id}");
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
            return new ServiceResult(false, "An error occurred while unpublishing the template. Please try later.");
        }

        public async Task<ServiceResult> DeleteTemplateAsync(int id, string uuid, string token, bool makerCheckerFlag = false)
        {
            try
            {
                var template = await GetTemplateAsync(id,token);
                if (template == null)
                {
                    return new ServiceResult(false, "Template doesn't exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingTemplateActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OnboardingTemplateActivityId, OperationTypeConstants.Delete, uuid,
                        JsonConvert.SerializeObject(template));
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

                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?id={id}&methodname=deleteTemplateByID");
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

            return new ServiceResult(false, "An error occurred while creating the template. Please try later.");
        }
    }
}
