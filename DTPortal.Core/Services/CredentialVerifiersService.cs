using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Google.Apis.Logging;
using iTextSharp.text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Targets.Wrappers;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace DTPortal.Core.Services
{
    public class CredentialVerifiersService : ICredentialVerifiersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CredentialVerifiersService> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly ICredentialService _credentialService;
        private readonly ICacheClient _cacheClient;
        private readonly IHelper _helper;
        public CredentialVerifiersService(IUnitOfWork unitOfWork,
            ILogger<CredentialVerifiersService> logger,
            IOrganizationService organizationService,
            ICredentialService credentialService,
            ICacheClient cacheClient,
            IHelper helper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _organizationService = organizationService;
            _credentialService = credentialService;
            _cacheClient = cacheClient;
            _helper = helper;
        }
        public async Task<ServiceResult> GetCredentialVerifierDTOsListAsync()
        {
            try
            {
                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetAllAsync();
                if (credentialVerifiersListInDb == null)
                {
                    _logger.LogError("No Credential Verifiers Data found");
                    return new ServiceResult(false, "No Credential Verifiers Data found");
                }

                var credential = await _credentialService.GetCredentialNameIdListAsync();
                if (credential == null || !credential.Success)
                {
                    _logger.LogError("Creddential Data found");
                    return new ServiceResult(false, "Credential Data found");
                }
                var credentialDict = (Dictionary<string, string>)credential.Resource;


                var organizationDict = await GetOrganizationsDictionary();
                if (organizationDict == null)
                {
                    _logger.LogError("GetOrganizationsDictionary failed");
                    return new ServiceResult(false, "Get Organizations Data found");
                }

                var list = new List<CredentialVerifierDTO>();
                foreach (var item in credentialVerifiersListInDb)
                {
                    var credentialVerifierDTO = new CredentialVerifierDTO()
                    {
                        id = item.Id,
                        credentialId = item.CredentialId,
                        credentialName = credentialDict[item.CredentialId],
                        organizationId = item.OrganizationId,
                        organizationName = organizationDict[item.OrganizationId],
                        configuration = JsonConvert.DeserializeObject<List<CredentialConfig>>(item.Configuration),
                        attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(item.Attributes),
                        emails = JsonConvert.DeserializeObject<List<string>>(item.Emails),
                        status = item.Status,
                        createdDate = item.CreatedDate,
                        updatedDate = item.UpdatedDate,
                    };
                    list.Add(credentialVerifierDTO);
                }
                return new ServiceResult(true, "Successfully recieved Credential verifiers list", list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> CreateCredentialVerifierAsync(CredentialVerifierDTO credentialVerifierDTO)
        {
            try
            {
                var credentialVerifierinDb = await _unitOfWork.CredentialVerifiers.IsCredentialAlreadyExists(credentialVerifierDTO);
                if (credentialVerifierinDb)
                {
                    _logger.LogError("Credential Verifier Already Exists");
                    return new ServiceResult(false, "Credential Verifier Already Exists");
                }

                var data = new CredentialVerifier()
                {
                    Id = credentialVerifierDTO.id,
                    CredentialId = credentialVerifierDTO.credentialId,
                    OrganizationId = credentialVerifierDTO.organizationId,
                    Configuration = JsonConvert.SerializeObject(credentialVerifierDTO.configuration),
                    Attributes = JsonConvert.SerializeObject(credentialVerifierDTO.attributes),
                    Emails = JsonConvert.SerializeObject(credentialVerifierDTO.emails),
                    Validity=credentialVerifierDTO.validity,
                    CreatedDate = DateTime.Now,
                    Status = "APPROVAL REQUIRED"
                };
                if (credentialVerifierDTO.domainConfig != null)
                {
                    data.Domains = JsonConvert.SerializeObject(credentialVerifierDTO.domainConfig);
                }
                await _unitOfWork.CredentialVerifiers.AddAsync(data);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Your Request Has Sent For Approval");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create credential verifier : " + ex.ToString());
                return new ServiceResult(false, "Failed to create credential verifier : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> UpdateCredentialVerifierAsync(CredentialVerifierDTO credentialVerifierDTO)
        {
            try
            {
                var credentialVerifierinDb = await _unitOfWork.CredentialVerifiers.GetByIdAsync(credentialVerifierDTO.id);
                if (credentialVerifierinDb == null)
                {
                    _logger.LogError("Failed to get Credential Verifer Data");
                    return new ServiceResult(false, "Failed to get Credential Verifer Data");
                }

                //var isCredentialExists = await _unitOfWork.CredentialVerifiers.IsCredentialAlreadyExists(credentialVerifierDTO);
                //if (isCredentialExists)
                //{
                //    _logger.LogError("Credential Verifier Already Exists");
                //    return new ServiceResult(false, "Credential Verifier Already Exists");
                //}

                credentialVerifierinDb.CredentialId = credentialVerifierDTO.credentialId;
                credentialVerifierinDb.OrganizationId = credentialVerifierDTO.organizationId;
                credentialVerifierinDb.Configuration = JsonConvert.SerializeObject(credentialVerifierDTO.configuration);
                credentialVerifierinDb.Attributes = JsonConvert.SerializeObject(credentialVerifierDTO.attributes);
                credentialVerifierinDb.Emails = JsonConvert.SerializeObject(credentialVerifierDTO.emails);
                credentialVerifierinDb.UpdatedDate = DateTime.Now;
                credentialVerifierinDb.Status = credentialVerifierDTO.status;


                _unitOfWork.CredentialVerifiers.Update(credentialVerifierinDb);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Successfully Updated Credential verifier");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update credential verifier : " + ex.ToString());
                return new ServiceResult(false, "Failed to update credential verifier : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> GetCredentialVerifierByIdAsync(int id)
        {
            try
            {
                var credentialVerifierinDb = await _unitOfWork.CredentialVerifiers.GetByIdAsync(id);
                if (credentialVerifierinDb == null)
                {
                    _logger.LogError("No Credential Verifier Data found");
                    return new ServiceResult(false, "No Credential Verifier Data found");
                }


                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialVerifierinDb.CredentialId);
                if (credential == null)
                {
                    _logger.LogError("Creddential Data found");
                    return new ServiceResult(false, "Credential Data found");
                }
                var CredentialName = credential.DisplayName;


                var organizationDict = await GetOrganizationsDictionary();
                if (organizationDict == null)
                {
                    _logger.LogError("GetOrganizationsDictionary failed");
                    return new ServiceResult(false, "Get Organizations Data found");
                }
                var OrganizationName = organizationDict[credentialVerifierinDb.OrganizationId];

                var credentialVerifierDTO = new CredentialVerifierDTO()
                {
                    id = id,
                    credentialId = credentialVerifierinDb.CredentialId,
                    credentialName = CredentialName,
                    organizationId = credentialVerifierinDb.OrganizationId,
                    organizationName = OrganizationName,
                    configuration = JsonConvert.DeserializeObject<List<CredentialConfig>>(credentialVerifierinDb.Configuration),
                    attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(credentialVerifierinDb.Attributes),
                    emails = JsonConvert.DeserializeObject<List<string>>(credentialVerifierinDb.Emails),
                    status = credentialVerifierinDb.Status,
                    remarks= credentialVerifierinDb.Remarks,
                    createdDate = credentialVerifierinDb.CreatedDate,
                    updatedDate = credentialVerifierinDb.UpdatedDate,
                };
                if (credentialVerifierinDb.Domains != null)
                {
                    credentialVerifierDTO.domainConfig=JsonConvert.DeserializeObject<DomainConfig>(credentialVerifierinDb.Domains);
                }
                if(credentialVerifierinDb.Validity != null)
                {
                    credentialVerifierDTO.validity = (int)credentialVerifierinDb.Validity;
                }
                return new ServiceResult(true, "Successfully got Credential verifier", credentialVerifierDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> GetCredentialVerifiersListByOrganizationIdAsync(string organizationId)
        {
            try
            {
                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetCredentialListDataByOrganizationIdAsync(organizationId);
                if (credentialVerifiersListInDb == null)
                {
                    _logger.LogError("No Credential Verifiers Data found");
                    return new ServiceResult(false, "No Credential Verifiers Data found");
                }

                var credential = await _credentialService.GetCredentialNameIdListAsync();
                if (credential == null || !credential.Success)
                {
                    _logger.LogError("Creddential Data found");
                    return new ServiceResult(false, "Credential Data found");
                }
                var credentialDict = (Dictionary<string, string>)credential.Resource;


                var organizationDict = await GetOrganizationsDictionary();
                if (organizationDict == null)
                {
                    _logger.LogError("GetOrganizationsDictionary failed");
                    return new ServiceResult(false, "Get Organizations Data found");
                }

                var list = new List<CredentialVerifierDTO>();
                foreach (var item in credentialVerifiersListInDb)
                {
                    var credentialVerifierDTO = new CredentialVerifierDTO()
                    {
                        id = item.Id,
                        credentialId = item.CredentialId,
                        credentialName = credentialDict[item.CredentialId],
                        organizationId = item.OrganizationId,
                        organizationName = organizationDict[item.OrganizationId],
                        configuration = JsonConvert.DeserializeObject<List<CredentialConfig>>(item.Configuration),
                        attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(item.Attributes),
                        emails = JsonConvert.DeserializeObject<List<string>>(item.Emails),
                        status = item.Status,
                        createdDate = item.CreatedDate,
                        updatedDate = item.UpdatedDate,
                    };
                    list.Add(credentialVerifierDTO);
                }
                return new ServiceResult(true, "Successfully recieved Credential verifiers list", list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> GetActiveCredentialVerifiersListAsync(string token)
        {
            try
            {
                Accesstoken accessToken = null;
                try
                {
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        token);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        return new ServiceResult(false, "UnAuthorized");
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = "Internal Error";
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Internal Error" + error.error_description);
                }

                var client = await _unitOfWork.Client.GetClientByClientIdAsync(accessToken.ClientId);

                if (null == client)
                {
                    _logger.LogError("Failed to Client Details :" + accessToken.ClientId);
                    return new ServiceResult(false, "Invalid Scopes");
                }

                string organizationId = client.OrganizationUid;

                if (string.IsNullOrEmpty(organizationId))
                {
                    _logger.LogError("Failed to Client Details :" + accessToken.ClientId);
                    return new ServiceResult(false, "Failed to get Organization Details");
                }

                var subscriber = await _unitOfWork.Subscriber.GetSubscriberInfoBySUID(accessToken.UserId);

                if (null == subscriber)
                {
                    _logger.LogError("Failed to Subscriber Details :" + accessToken.UserId);
                    return new ServiceResult(false, "Failed to get Subscriber Details");
                }

                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetActiveCredentialVerifierListAsync();
                if (credentialVerifiersListInDb == null)
                {
                    _logger.LogError("No Credential Verifiers Data found");
                    return new ServiceResult(false, "No Credential Verifiers Data found");
                }

                _logger.LogInformation("Credential Verifiers Count: " + credentialVerifiersListInDb.Count());

                _logger.LogInformation(
                    "Fetching Credential Verifiers List for OrganizationId: {OrganizationId}, SubscriberEmail: {SubscriberEmail}",
                    organizationId,
                    subscriber.Email
                );

                var list = new List<CredentialVerifiersListDTO>();

                foreach (var item in credentialVerifiersListInDb)
                {
                    _logger.LogDebug(
                        "Processing CredentialVerifier. CredentialId: {CredentialId}, OrganizationId: {OrganizationId}",
                        item.CredentialId,
                        item.OrganizationId
                    );

                    var mails = new List<string>();

                    if (!string.IsNullOrEmpty(item.Emails))
                    {
                        mails = JsonConvert.DeserializeObject<List<string>>(item.Emails);

                        _logger.LogDebug(
                            "Deserialized Emails for CredentialId {CredentialId}: {Emails}",
                            item.CredentialId,
                            JsonConvert.SerializeObject(mails)
                        );
                    }

                    bool isEmailMatched = mails.Any(mail =>
                        mail.Equals(subscriber.Email, StringComparison.OrdinalIgnoreCase));

                    if (item.OrganizationId == organizationId && isEmailMatched)
                    {
                        _logger.LogInformation(
                            "Email matched for CredentialId {CredentialId}. Adding to response list.",
                            item.CredentialId
                        );

                        var credentialVerifierDTO = new CredentialVerifiersListDTO
                        {
                            credentialName = item.Credential.CredentialName,
                            displayName = item.Credential.DisplayName,
                            credentialId = item.CredentialId,
                            organizationId = item.OrganizationId,
                            logo = item.Credential.Logo,
                            attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(item.Attributes)
                        };

                        list.Add(credentialVerifierDTO);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Skipped CredentialId {CredentialId}. OrgMatch: {OrgMatch}, EmailMatch: {EmailMatch}",
                            item.CredentialId,
                            item.OrganizationId == organizationId,
                            isEmailMatched
                        );
                    }
                }

                _logger.LogInformation(
                    "Total Credential Verifiers found: {Count}",
                    list.Count
                );

                // Print full list (use Debug to avoid noisy production logs)
                _logger.LogDebug(
                    "Credential Verifiers List Response: {CredentialVerifiersList}",
                    JsonConvert.SerializeObject(list)
                );

                return new ServiceResult(true, "Successfully received Credential verifiers list", list);

            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> GetActiveCredentialVerifiersListByOrganizationIdAsync(string orgId, string token)
        {
            try
            {
                Accesstoken accessToken = null;
                try
                {
                    accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                        token);
                    if (null == accessToken)
                    {
                        _logger.LogError("Access token not recieved from cache." +
                            "Expired or Invalid access token");
                        return new ServiceResult(false, "UnAuthorized");
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Access Token Record");
                    ErrorResponseDTO error = new ErrorResponseDTO();
                    error.error = "Internal Error";
                    error.error_description = _helper.GetRedisErrorMsg(
                        ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                    return new ServiceResult(false, "Internal Error" + error.error_description);
                }
                var scopesList = accessToken.Scopes.Split(" ");

                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetActiveCredentialListByOrganizationIdAsync(orgId);
                if (credentialVerifiersListInDb == null)
                {
                    _logger.LogError("No Credential Verifiers Data found");
                    return new ServiceResult(false, "No Credential Verifiers Data found");
                }

                var credential = await _credentialService.GetCredentialNameIdListAsync();
                if (credential == null || !credential.Success)
                {
                    _logger.LogError("Creddential Data found");
                    return new ServiceResult(false, "Credential Data found");
                }
                var credentialDict = (Dictionary<string, string>)credential.Resource;


                var organizationDict = await GetOrganizationsDictionary();
                if (organizationDict == null)
                {
                    _logger.LogError("GetOrganizationsDictionary failed");
                    return new ServiceResult(false, "Get Organizations Data found");
                }

                var list = new List<CredentialVerifierDTO>();
                foreach (var item in credentialVerifiersListInDb)
                {
                    var credentialVerifierDTO = new CredentialVerifierDTO()
                    {
                        id = item.Id,
                        credentialId = item.CredentialId,
                        credentialName = credentialDict[item.CredentialId],
                        organizationId = item.OrganizationId,
                        organizationName = organizationDict[item.OrganizationId],
                        configuration = JsonConvert.DeserializeObject<List<CredentialConfig>>(item.Configuration),
                        attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(item.Attributes),
                        emails = JsonConvert.DeserializeObject<List<string>>(item.Emails),
                        status = item.Status,
                        createdDate = item.CreatedDate,
                        updatedDate = item.UpdatedDate,
                    };
                    list.Add(credentialVerifierDTO);
                }
                return new ServiceResult(true, "Successfully recieved Credential verifiers list", list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> GetCredentialsListByOrganizationId(string organizationId)
        {
            try
            {
                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetCredentialsListByOrganizationIdAsync(organizationId);
                return new ServiceResult(true, "Successfully recieved Credential verifiers list", credentialVerifiersListInDb);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> ActivateCredentialById(int id)
        {
            try
            {
                var credentialVerifierinDb = await _unitOfWork.CredentialVerifiers.GetByIdAsync(id);
                if (credentialVerifierinDb == null)
                {
                    _logger.LogError("Failed to get Credential Verifer Data");
                    return new ServiceResult(false, "Failed to get Credential Verifer Data");
                }
                credentialVerifierinDb.UpdatedDate = DateTime.Now;
                credentialVerifierinDb.Status = "SUBSCRIBED";


                _unitOfWork.CredentialVerifiers.Update(credentialVerifierinDb);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Successfully Activated Credential verifier");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update credential verifier : " + ex.ToString());
                return new ServiceResult(false, "Failed to update credential verifier : " + ex.ToString());
            }
        }

        public async Task<ServiceResult> RejectCredentialById(int id, string remarks)
        {
            try
            {
                var credentialVerifierinDb = await _unitOfWork.CredentialVerifiers.GetByIdAsync(id);
                if (credentialVerifierinDb == null)
                {
                    _logger.LogError("Failed to get Credential Verifer Data");
                    return new ServiceResult(false, "Failed to get Credential Verifer Data");
                }
                credentialVerifierinDb.UpdatedDate = DateTime.Now;
                credentialVerifierinDb.Status = "REJECTED";
                credentialVerifierinDb.Remarks = remarks;


                _unitOfWork.CredentialVerifiers.Update(credentialVerifierinDb);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Successfully Updated Credential verifier");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update credential verifier : " + ex.ToString());
                return new ServiceResult(false, "Failed to update credential verifier : " + ex.ToString());
            }
        }

        public async Task<Dictionary<string, string>> GetOrganizationsDictionary()
        {
            try
            {
                var dict = new Dictionary<string, string>();

                var organizationsList = await _organizationService.GetOrganizationNamesAndIdAysnc();

                foreach (var organization in organizationsList)
                {
                    var data = organization.Split(',');
                    dict[data[1]] = data[0];
                }
                return dict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult> GetCredentialVerifierListByIssuerId(string orgId)
        {
            try
            {
                var credentialVerifiersListInDb = await _unitOfWork.CredentialVerifiers.GetCredentialVerifierListByIssuerIdAsync(orgId);
                if (credentialVerifiersListInDb == null)
                {
                    _logger.LogError("No Credential Verifiers Data found");
                    return new ServiceResult(false, "No Credential Verifiers Data found");
                }
                var organizationDict = await GetOrganizationsDictionary();
                if (organizationDict == null)
                {
                    _logger.LogError("GetOrganizationsDictionary failed");
                    return new ServiceResult(false, "Get Organizations Data found");
                }

                var list = new List<CredentialVerifierDTO>();
                foreach (var item in credentialVerifiersListInDb)
                {
                    var credentialVerifierDTO = new CredentialVerifierDTO()
                    {
                        id = item.Id,
                        credentialId = item.CredentialId,
                        credentialName = item.Credential.CredentialName,
                        organizationId = item.OrganizationId,
                        organizationName = organizationDict[item.Credential.OrganizationId],
                        configuration = JsonConvert.DeserializeObject<List<CredentialConfig>>(item.Configuration),
                        attributes = JsonConvert.DeserializeObject<List<DataAttributes>>(item.Attributes),
                        emails = JsonConvert.DeserializeObject<List<string>>(item.Emails),
                        status = item.Status,
                        createdDate = item.CreatedDate,
                        updatedDate = item.UpdatedDate,
                    };
                    list.Add(credentialVerifierDTO);
                }
                return new ServiceResult(true, "Successfully recieved Credential verifiers list", list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get credential verifier list : " + ex.ToString());
                return new ServiceResult(false, "Failed to get credential verifier list : " + ex.ToString());
            }
        }
    }
}
