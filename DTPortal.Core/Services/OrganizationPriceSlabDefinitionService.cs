using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using DTPortal.Core.DTOs;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class OrganizationPriceSlabDefinitionService : IOrganizationPriceSlabDefinitionService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationPriceSlabDefinitionService> _logger;

        public OrganizationPriceSlabDefinitionService(IMCValidationService mcValidationService,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OrganizationPriceSlabDefinitionService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:PriceModelServiceBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _mcValidationService = mcValidationService;
            _client = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<OrganizationPriceSlabDefinitionDTO>> GetAllPriceSlabDefinitionsAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-all-org-price-slabs");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<OrganizationPriceSlabDefinitionDTO>>(apiResponse.Result.ToString());
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

        public async Task<IList<OrganizationPriceSlabDefinitionDTO>> GetPriceSlabDefinitionAsync(int serviceId, string organizationUid)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get-org-priceslab?orgId={organizationUid}&serviceId={serviceId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<IList<OrganizationPriceSlabDefinitionDTO>>(apiResponse.Result.ToString());
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

        public async Task<bool> IsOrganizationPriceSlabExists(int serviceId, string organizationUid)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/org-price-slab/check-combination?orgId={organizationUid}&serviceId={serviceId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    return Convert.ToBoolean(apiResponse.Result);
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

        public async Task<ServiceResult> AddPriceSlabDefinitionAsync(IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false)
        {
            try
            {
                var isExists = await IsOrganizationPriceSlabExists(priceSlabDefinitions[0].ServiceDefinitions.Id, priceSlabDefinitions[0].OrganizationUid);
                if (isExists == true)
                {
                    _logger.LogError($"Organization specific price slab with combination Service Name ={priceSlabDefinitions[0].ServiceDefinitions.ServiceDisplayName} and for organization = {priceSlabDefinitions[0].OrganizationUid} already exists");
                    return new ServiceResult(false, "Price Slab already exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OrganizationPriceSlabActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OrganizationPriceSlabActivityId, OperationTypeConstants.Create, priceSlabDefinitions[0].CreatedBy,
                        JsonConvert.SerializeObject(priceSlabDefinitions));
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

                string json = JsonConvert.SerializeObject(priceSlabDefinitions,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/add-org-price-slab", content);
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

            return new ServiceResult(false, "An error occurred while creating the organization price slab. Please try later.");
        }

        public async Task<ServiceResult> UpdatePriceSlabDefinitionAsync(IList<OrganizationPriceSlabDefinitionDTO> priceSlabDefinitions, bool makerCheckerFlag = false)
        {
            try
            {
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OrganizationPriceSlabActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.OrganizationPriceSlabActivityId, OperationTypeConstants.Update, priceSlabDefinitions[0].UpdatedBy,
                        JsonConvert.SerializeObject(priceSlabDefinitions));
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

                string json = JsonConvert.SerializeObject(priceSlabDefinitions,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/update-org-price-slab", content);
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

            return new ServiceResult(false, "An error occurred while updating the organization price slab. Please try later.");
        }
    }
}
