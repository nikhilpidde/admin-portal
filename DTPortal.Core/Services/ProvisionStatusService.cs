using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DTPortal.Core.Services
{
    public class ProvisionStatusService : IProvisionStatusService
    {
        private readonly ILogger<ProvisionStatusService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        public ProvisionStatusService(ILogger<ProvisionStatusService> logger, 
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:GenerateCredentialOfferBaseAddress"]);
            _client=httpClient;
        }
        public async Task<ServiceResult> GetProvisionStatus(string Suid,string credentialId)
        {

            var provisionStatus=await _unitOfWork.ProvisionStatus.GetProvisionStatus(Suid, credentialId);

            if(provisionStatus == null)
            {
                return new ServiceResult(false, "Provision Status Not Found");
            }
            else
            {
                return new ServiceResult(true, "Get Provision Status Success",provisionStatus);
            }
        }

        public async Task<ServiceResult> AddProvisionStatus
            (string Suid, string credentialId,string status,string documentId)
        {
            try
            {
                var provisionStatus = new ProvisionStatus()
                {
                    Suid = Suid,
                    CredentialId = credentialId,
                    CreatedDate = DateTime.Now,
                    DocumentId = documentId,
                    Status = "PROVISIONED"
                };

                await _unitOfWork.ProvisionStatus.AddAsync(provisionStatus);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Successfully Added Provision Status");
            }
            catch(Exception ex)
            {

                _logger.LogError("AddProvisionStatusFailed {0}", ex);

                return new ServiceResult(false, "Failed to Add Provision Status");
            }
        }

        public async Task<ServiceResult> RevokeProvision(string credentialId,string documentId)
        {
            try
            {
                var provisionStatus = await _unitOfWork.ProvisionStatus.GetProvisionStatusByDocumentId(credentialId, documentId);

                if (provisionStatus == null)
                {
                    return new ServiceResult(false, "Provision Details Not Found");
                }

                var revokeCredential=await RevokeCredential(credentialId, provisionStatus.Suid);

                if(revokeCredential == null || !revokeCredential.Success)
                {
                    return new ServiceResult(false, revokeCredential.Message);
                }
                var provisionStatus1 = new ProvisionStatus()
                {
                    Suid = provisionStatus.Suid,
                    CredentialId = credentialId,
                    CreatedDate = DateTime.Now,
                    DocumentId = documentId,
                    Status = "REVOKED"
                };

                await _unitOfWork.ProvisionStatus.AddAsync(provisionStatus1);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Provision Revoked Successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to revoke Provision {0}", ex);

                return new ServiceResult(false, "Failed to Revoke Provision");
            }
        }

        public async Task<ServiceResult> DeleteProvision(string credentialId, string Suid)
        {
            try
            {
                var provisionStatus = await _unitOfWork.ProvisionStatus.GetProvisionStatus(Suid, credentialId);

                if (provisionStatus == null)
                {
                    return new ServiceResult(false, "Provision Details Not Found");
                }
                var provisionStatus1 = new ProvisionStatus()
                {
                    Suid = provisionStatus.Suid,
                    CredentialId = credentialId,
                    CreatedDate = DateTime.Now,
                    DocumentId = provisionStatus.DocumentId,
                    Status = "DELETED"
                };

                await _unitOfWork.ProvisionStatus.AddAsync(provisionStatus1);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Provision Deleted Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Delete Provision {0}", ex);

                return new ServiceResult(false, "Failed to Delete Provision");
            }
        }

        public async Task<ServiceResult> RevokeCredential(string credentialId,string Suid)
        {
            var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

            if(credential == null)
            {
                return new ServiceResult(false, "Credential Not Found");
            }

            RevokeCredentialDTO revokeCredentialDTO = new RevokeCredentialDTO()
            {
                issuerID=credential.OrganizationId,
                suid=Suid,
                credentialType=credential.CredentialName
            };

            string json = JsonConvert.SerializeObject(revokeCredentialDTO);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/revokeCredential", content);
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
            else
            {
                _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                   $"with status code={response.StatusCode}");
                return new ServiceResult(false, "Internal Error");
            }
        }

        public async Task<ServiceResult> DeleteCredential(string credentialId, string Suid)
        {
            var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

            if (credential == null)
            {
                return new ServiceResult(false, "Credential Not Found");
            }

            Dictionary<string, string> deleteCredential = new Dictionary<string, string>();

            deleteCredential["suid"] = Suid;

            deleteCredential["profileType"] = credential.CredentialName;

            string json = JsonConvert.SerializeObject(deleteCredential);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/deleteVC", content);

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
            else
            {
                _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                   $"with status code={response.StatusCode}");
                return new ServiceResult(false, "Internal Error");
            }
        }
    }
}