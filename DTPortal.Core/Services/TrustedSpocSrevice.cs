using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Google.Api.Gax.ResourceNames;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class TrustedSpocSrevice : ITrustedSpocService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IConfiguration _configuration;

        public TrustedSpocSrevice(HttpClient httpclient
            , IConfiguration configuration,
            ILogger<ConfigurationService> logger)
        {
            _client = httpclient;
            _logger = logger;
            _configuration = configuration;
            _client.BaseAddress = new Uri(configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
            //_client.BaseAddress = new Uri(configuration["APIServiceLocations:TrustedSpocBaseAddress"]);
        }
        public async Task<IEnumerable<TrustedSpocListUpdated>> GetTrustedSpocList()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
                HttpResponseMessage response = await client.GetAsync($"spoc/get/all");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<TrustedSpocListUpdated>>(apiResponse.Result.ToString());
                        //return JsonConvert.DeserializeObject<IEnumerable<TrustedSpocListDTO>>(apiResponse.Result.ToString());
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
        public async Task<IEnumerable<TrustedSpocListNewDTO>> GetTrustedSpocList1()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:TrustedSpocBaseAddress"]);
                HttpResponseMessage response = await client.GetAsync($"get-all/trusted-spocs/");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<TrustedSpocListNewDTO>>(apiResponse.Result.ToString());
                        //return JsonConvert.DeserializeObject<IEnumerable<TrustedSpocListDTO>>(apiResponse.Result.ToString());
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
        public async Task<APIResponse> AddTrustedSpocAsync(TrustedUserRequestDTO request)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);

            var httpResponse = await client.PostAsJsonAsync(
                "spoc/save",
                request
            );

            if (!httpResponse.IsSuccessStatusCode)
            {
                return new APIResponse("Unable to process trusted user request");
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<APIResponse>();

            return response ?? new APIResponse("Invalid response from Trusted User API");
        }
        public async Task<ServiceResult> AddTrustedSpocAsync1(TrustedSpocAddDTO trustedSpocAddDTO)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:TrustedSpocBaseAddress"]);

                // Verify Organization TIN
                //var veirfyOrgTinResult = await VerifyOrganizationTin(trustedSpocAddDTO.OrganizationTin);
                //if( veirfyOrgTinResult == null || !veirfyOrgTinResult.Success)
                //{
                //    var message = (veirfyOrgTinResult.Message == null) ? "Organization Tin Verification Failed" : ("Organization Tin Verification Failed : " + veirfyOrgTinResult.Message);
                //    _logger.LogError(message);
                //    return new ServiceResult(false, message);
                //}

                //// verify CEO TIN
                //var veirfyCeoTinResult = await VerifyCeoTin(trustedSpocAddDTO.CeoTin);
                //if (veirfyCeoTinResult == null || !veirfyCeoTinResult.Success)
                //{
                //    var message = (veirfyCeoTinResult.Message == null) ? "CEO Tin Verification Failed" : ("CEO Tin Verification Failed : " + veirfyCeoTinResult.Message);
                //    _logger.LogError(message);
                //    return new ServiceResult(false, message);
                //}


                //var wrapper = JsonConvert.DeserializeObject<TinVerificationDTO>(veirfyOrgTinResult.Resource.ToString());
                //var registrationResult = wrapper.GetClientRegistrationResponse.GetClientRegistrationResult;
                //var organizationName = registrationResult.TaxPayerName;

                //var wrapper1 = JsonConvert.DeserializeObject<TinVerificationDTO>(veirfyCeoTinResult.Resource.ToString());
                //var registrationResult1 = wrapper1.GetClientRegistrationResponse.GetClientRegistrationResult;
                //var ceoName = registrationResult1.TaxPayerName;

                //string ceouraresponse = JsonConvert.SerializeObject(veirfyCeoTinResult.Resource, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                //string orguraresponse = JsonConvert.SerializeObject(veirfyOrgTinResult.Resource, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                //TrustedSpocAddNewDTO trustedSpocAddNewDTO = new TrustedSpocAddNewDTO()
                //{
                //    OrgName = organizationName,
                //    OrgTin = trustedSpocAddDTO.OrganizationTin,
                //    CeoName = ceoName,
                //    CeoTin = trustedSpocAddDTO.CeoTin,
                //    SpocUgpassEmail = trustedSpocAddDTO.SpocEmail,
                //    OrgUraResponse = orguraresponse,
                //    CeoUraResponse = ceouraresponse
                //};
                TrustedSpocAddNewDTO trustedSpocAddNewDTO = new TrustedSpocAddNewDTO()
                {
                    SpocName = trustedSpocAddDTO.SpocName,
                    SpocEmail = trustedSpocAddDTO.SpocEmail,
                    MobileNo = trustedSpocAddDTO.MobileNo,
                    IdDocumentNo = trustedSpocAddDTO.IdDocumentNo,
                };
                string json = JsonConvert.SerializeObject(trustedSpocAddNewDTO, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("post/save-trusted-spoc", content);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public async Task<ServiceResult> VerifyOrganizationTin(string organizationTin)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:TrustedSpocBaseAddress"]);
                string json = JsonConvert.SerializeObject(
                                new
                                {
                                    tin = organizationTin,
                                    typeOfUser = "ORG"
                                },
                                new JsonSerializerSettings
                                {
                                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                                });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("post/verify-tin", content);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public async Task<ServiceResult> VerifyCeoTin(string ceoTin)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_configuration["APIServiceLocations:TrustedSpocBaseAddress"]);
                string json = JsonConvert.SerializeObject(
                                new
                                {
                                    tin = ceoTin,
                                    typeOfUser = "INDIVIDUAL"
                                },
                                new JsonSerializerSettings
                                {
                                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                                });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("post/verify-tin", content);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public async Task<TrustedSpocListDTO> GetSpocDetailsByIDasync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/trusted-spocs/by-id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var trustedSpoc = JsonConvert.DeserializeObject<TrustedSpocListDTO>(apiResponse.Result.ToString());
                        return trustedSpoc;

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

        public async Task<ServiceResult> SuspendSpoc(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/deactivate-trusted-spoc/{id}");
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

        public async Task<ServiceResult> RemoveSuspensionSpoc(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"trusted-spocs/remove/suspension/{id}");
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

        public async Task<ServiceResult> ReInviteSpoc(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"re-invite/trusted-spocs/{id}");
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
