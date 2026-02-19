using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using System.Net.Http.Headers;
using System.IO;

namespace DTPortal.Core.Services
{
    public class ConsentService : IConsentService
    {

        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConsentService> _logger;
        private readonly string _accessTokenHeaderName;

        public ConsentService(HttpClient httpClient,
           IConfiguration configuration,
           ILogger<ConsentService> logger)
        {
            _logger = logger;
            _configuration = configuration;

            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:OnboardingServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _accessTokenHeaderName = "Authorization";
            _client = httpClient;
        }

        public async Task<IEnumerable<ConsentDTO>> GetAllConsentsAsync(string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");
                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe?methodname=getConsentList");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<ConsentDTO>>(apiResponse.Result.ToString());
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

        public async Task<ConsentDTO> GetConsentAsync(int consentId, string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");
                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?id={consentId}&methodname=getConsentById");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<ConsentDTO>(apiResponse.Result.ToString());
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

        public async Task<ServiceResult> AddConsentAsync(ConsentRequestBodyDTO requestBodyDTO, string token)
        {
            try
            {
                //var consents = await GetAllConsentsAsync();
                //if (consents.Any(x => x.ConsentType == requestBodyDTO.ConsentType))
                //{
                //    _logger.LogError($"Consent type {requestBodyDTO.ConsentType} already exists");
                //    return new ServiceResult(false, "Consent type already exists");
                //}

                CreateConsentDTO createConsentDTO = new CreateConsentDTO
                {
                    ConsentId = requestBodyDTO.ConsentId,
                    ConsentType = requestBodyDTO.ConsentType,
                    ConsentRequired = requestBodyDTO.ConsentRequired,
                    Consent = requestBodyDTO.Consent,
                    PrivacyConsent = requestBodyDTO.PrivacyConsent

                };

                var url = _configuration.GetValue<string>("APIServiceLocations:ConsentServiceBaseAddress") + "api/add/consent/files";

                HttpResponseMessage response;

                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    if (requestBodyDTO.DataPrivacy != null)
                    {
                        StreamContent fileStreamContent = new StreamContent(requestBodyDTO.DataPrivacy.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        multipartFormContent.Add(fileStreamContent, name: "dataPrivacy", fileName: requestBodyDTO.DataPrivacy.FileName);
                    }
                    else
                    {
                        multipartFormContent.Add(new StreamContent(new MemoryStream()), name: "dataPrivacy", fileName: "empty.html");
                    }

                    if (requestBodyDTO.TermsAndConditions != null)
                    {
                        StreamContent fileStreamContent1 = new StreamContent(requestBodyDTO.TermsAndConditions.OpenReadStream());
                        fileStreamContent1.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        multipartFormContent.Add(fileStreamContent1, name: "termsAndConditions", fileName: requestBodyDTO.TermsAndConditions.FileName);
                    }
                    else
                    {
                        multipartFormContent.Add(new StreamContent(new MemoryStream()), name: "termsAndConditions", fileName: "empty.html");
                    }

                    multipartFormContent.Add(new StringContent(JsonConvert.SerializeObject(createConsentDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })), "model");
                    if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                    {
                        _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                    }

                    _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                    response = await _client.PostAsync(url, multipartFormContent);
                }
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

            return new ServiceResult(false, "An error occurred while creating the consent. Please try later.");
        }

        public async Task<ServiceResult> UpdateConsentAsync(ConsentRequestBodyDTO requestBodyDTO, string token)
        {
            try
            {
                CreateConsentDTO createConsentDTO = new CreateConsentDTO
                {
                    ConsentId = requestBodyDTO.ConsentId,
                    ConsentType = requestBodyDTO.ConsentType,
                    ConsentRequired = requestBodyDTO.ConsentRequired,
                    Consent = requestBodyDTO.Consent,
                    PrivacyConsent = requestBodyDTO.PrivacyConsent
                };

                var url = _configuration.GetValue<string>("APIServiceLocations:ConsentServiceBaseAddress") + "api/add/consent/files";

                HttpResponseMessage response;

                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    if (requestBodyDTO.DataPrivacy != null)
                    {
                        StreamContent fileStreamContent = new StreamContent(requestBodyDTO.DataPrivacy.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        multipartFormContent.Add(fileStreamContent, name: "dataPrivacy", fileName: requestBodyDTO.DataPrivacy.FileName);
                    }
                    else
                    {
                        multipartFormContent.Add(new StreamContent(new MemoryStream()), name: "dataPrivacy", fileName: "empty.html");
                    }

                    if (requestBodyDTO.TermsAndConditions != null)
                    {
                        StreamContent fileStreamContent1 = new StreamContent(requestBodyDTO.TermsAndConditions.OpenReadStream());
                        fileStreamContent1.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        multipartFormContent.Add(fileStreamContent1, name: "termsAndConditions", fileName: requestBodyDTO.TermsAndConditions.FileName);
                    }
                    else
                    {
                        multipartFormContent.Add(new StreamContent(new MemoryStream()), name: "termsAndConditions", fileName: "empty.html");
                    }

                    multipartFormContent.Add(new StringContent(JsonConvert.SerializeObject(createConsentDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })), "model");

                    if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                    {
                        _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                    }

                    _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                    response = await _client.PostAsync(url, multipartFormContent);
                }
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

            return new ServiceResult(false, "An error occurred while updating the consent. Please try later.");
        }

        public async Task<ServiceResult> EnableConsentAsync(int consentId, string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?id={consentId}&methodname=updateConsentActive");
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

            return new ServiceResult(false, "An error occurred while enabling the consent. Please try later.");
        }

        public async Task<ServiceResult> DisableConsentAsync(int consentId, string token)
        {
            try
            {
                if (_client.DefaultRequestHeaders.Contains(_accessTokenHeaderName))
                {
                    _client.DefaultRequestHeaders.Remove(_accessTokenHeaderName);
                }

                _client.DefaultRequestHeaders.Add(_accessTokenHeaderName, $"Bearer {token}");

                HttpResponseMessage response = await _client.GetAsync($"get/onboarding/dataframe-by-id?id={consentId}&methodname=updateConsentInActive");
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

            return new ServiceResult(false, "An error occurred while disabling the consent. Please try later.");
        }
    }
}
