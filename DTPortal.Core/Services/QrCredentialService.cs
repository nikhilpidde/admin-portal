using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Attribute = DTPortal.Core.DTOs.Attribute;

namespace DTPortal.Core.Services
{
    public class QrCredentialService : IQrCredentialService
    {
        private readonly ILogger<QrCredentialService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOrganizationService _organizationService;
        private readonly IEmailSender _emailSender;
        private readonly IWalletConfigurationService _walletConfigurationService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;
        private readonly ICacheClient _cacheClient;
        private readonly Helper _helper;
        public QrCredentialService(ILogger<QrCredentialService> logger,
            IUnitOfWork unitOfWork,
            HttpClient httpClient,
            IConfiguration configuration,
            IOrganizationService organizationService,
            IHttpClientFactory httpClientFactory,
            IEmailSender emailSender,
            ICategoryService categoryService,
            IWalletConfigurationService walletConfigurationService,
            IWebHostEnvironment environment,
            ICacheClient cacheClient,
            Helper helper
            )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:GenerateCredentialOfferBaseAddress"]);
            _client = httpClient;
            _organizationService = organizationService;
            _httpClientFactory = httpClientFactory;
            _emailSender = emailSender;
            _walletConfigurationService = walletConfigurationService;
            _categoryService = categoryService;
            _environment = environment;
            _cacheClient = cacheClient;
            _helper = helper;
        }
        public static object ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
                JsonValueKind.Object => element.EnumerateObject()
                                              .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
                _ => null
            };
        }

        public async Task<ServiceResult> GenerateCredentialOffer
            (Dictionary<string, QrIssuerId> credentialOffer)
        {
            try
            {
                string json = JsonConvert.SerializeObject(credentialOffer);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/QRCredentialOffer", content);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetCredentialList()
        {
            try
            {
                var credentialList = await _unitOfWork.QrCredential.GetCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var credentialdtoList = new List<QrCredentialDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialdto = new QrCredentialDTO()
                    {
                        Id = credential.Id,

                        credentialUId = credential.CredentialUid,

                        dataAttributes = JsonConvert.DeserializeObject<QrAttributesDTO>(credential.DataAttributes),

                        credentialName = credential.CredentialName,

                        displayName = credential.DisplayName,

                        organizationId = credential.OrganizationId,

                        credentialOffer = credential.CredentialOffer,

                        createdDate = (DateTime)credential.CreatedDate,

                        status = credential.Status,

                        portraitVerificationRequired=(bool)credential.PortraitVerificationRequired
                    };
                    credentialdtoList.Add(credentialdto);
                }
                return new ServiceResult(true, "Get QrCredential List Successfully", credentialdtoList);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetCredentialListByOrgId(string orgId)
        {
            try
            {
                var credentialList = await _unitOfWork.QrCredential.GetCredentialListByOrgIdAsync(orgId);

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var credentialdtoList = new List<QrCredentialDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialdto = new QrCredentialDTO()
                    {
                        Id = credential.Id,

                        credentialUId = credential.CredentialUid,

                        dataAttributes = JsonConvert.DeserializeObject<QrAttributesDTO>(credential.DataAttributes),

                        credentialName = credential.CredentialName,

                        displayName = credential.DisplayName,

                        organizationId = credential.OrganizationId,

                        credentialOffer = credential.CredentialOffer,

                        createdDate = (DateTime)credential.CreatedDate,

                        status = credential.Status
                    };
                    credentialdtoList.Add(credentialdto);
                }
                return new ServiceResult(true, "Get QrCredential List Successfully", credentialdtoList);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetActiveCredentialList()
        {
            try
            {
                var credentialList = await _unitOfWork.QrCredential.GetActiveCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var Categories = await _categoryService.GetCategoryNameAndIdPairAsync();

                if (Categories == null)
                {
                    return new ServiceResult(false, "Failed to get Categories List");
                }

                var credentialDTOList = new List<QrCredentialListDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialListDTO = new QrCredentialListDTO()
                    {
                        credentialName = credential.CredentialName,

                        displayName=credential.DisplayName,

                        organizationId = credential.OrganizationId,

                        credentialId = credential.CredentialUid,

                        status = credential.Status,

                        portraitVerificationRequired=(bool)credential.PortraitVerificationRequired
                    };

                    credentialDTOList.Add(credentialListDTO);
                }
                return new ServiceResult(true, "Get Active QrCredential List Successfully", credentialDTOList);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetCredentialById(int Id)
        {
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByIdAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                var credentialDto = new QrCredentialDTO()
                {
                    Id = credential.Id,
                    credentialUId = credential.CredentialUid,
                    dataAttributes = JsonConvert.DeserializeObject<QrAttributesDTO>(credential.DataAttributes),
                    credentialName = credential.CredentialName,
                    displayName = credential.DisplayName,
                    organizationId = credential.OrganizationId,
                    credentialOffer = credential.CredentialOffer,
                    status = credential.Status,
                    remarks = credential.Remarks,
                    createdDate = (DateTime)credential.CreatedDate
                };

                return new ServiceResult(true, "Get QrCredential Success", credentialDto);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential By Id::Database exception: {0}", error);
                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> GetCredentialByUid(string Id)
        {
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "QrCredential Data Not Found");
                }

                var credentialDto = new QrCredentialDTO()
                {
                    Id = credential.Id,
                    credentialUId = credential.CredentialUid,
                    dataAttributes = JsonConvert.DeserializeObject<QrAttributesDTO>(credential.DataAttributes),
                    credentialName = credential.CredentialName,
                    displayName=credential.DisplayName,
                    organizationId = credential.OrganizationId,
                    credentialOffer = credential.CredentialOffer,
                    status = credential.Status,
                    remarks = credential.Remarks,
                    createdDate = (DateTime)credential.CreatedDate,
                    portraitVerificationRequired = (bool)credential.PortraitVerificationRequired
                };

                return new ServiceResult(true, "Get QrCredential Success", credentialDto);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential By Id::Database exception: {0}", error);
                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> GetCredentialOfferByUid(string Id, string token)
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
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                JObject json = JObject.Parse(credential.CredentialOffer);

                var obj1 = json[credential.OrganizationId]["supportedCredentials"][0]["credentialType"];

                var jsonObject = obj1.ToString();

                var attributeslist = json[credential.OrganizationId]["supportedCredentials"][0][jsonObject];

                var attributes = JsonConvert.DeserializeObject<QrAttributes>(attributeslist.ToString());

                var supportedCredentials = new Dictionary<string, object>();

                supportedCredentials["credentialId"] = json[credential.OrganizationId]["supportedCredentials"][0]["credentialId"].ToString();

                supportedCredentials["credentialType"] = json[credential.OrganizationId]["supportedCredentials"][0]["credentialType"].ToString();

                supportedCredentials["isoNamespace"] = json[credential.OrganizationId]["supportedCredentials"][0]["isoNamespace"].ToString();

                supportedCredentials["faceValidation"]= (bool)json[credential.OrganizationId]["supportedCredentials"][0]["faceValidation"];

                var typeToken = json[credential.OrganizationId]["supportedCredentials"][0]["type"];

                var typeList = typeToken.ToObject<List<string>>();

                supportedCredentials["type"] = typeList;

                supportedCredentials["schema"] = json[credential.OrganizationId]["supportedCredentials"][0]["schema"].ToString();

                var typeToken1 = json[credential.OrganizationId]["supportedCredentials"][0]["format"];

                var typeList1 = typeToken1.ToObject<List<string>>();

                supportedCredentials["format"] = typeList1;

                supportedCredentials["proofType"] = json[credential.OrganizationId]["supportedCredentials"][0]["proofType"].ToString();

                var revocation = new Revocation()
                {
                    Type = json[credential.OrganizationId]["supportedCredentials"][0]["revocation"]["type"].ToString(),

                    RevocationListURL = json[credential.OrganizationId]["supportedCredentials"][0]["revocation"]["revocationListURL"].ToString()
                };

                supportedCredentials["revocation"] = revocation;

                supportedCredentials[jsonObject] = attributes;

                Dictionary<string, object> CredentialDetails = new Dictionary<string, object>();

                CredentialDetails["id"] = json[credential.OrganizationId]["id"].ToString();

                CredentialDetails["issuerName"] = json[credential.OrganizationId]["IssuerName"].ToString();

                CredentialDetails["issuerKey"] = json[credential.OrganizationId]["issuerKey"].ToString();

                CredentialDetails["issuerCertificateChain"] = json[credential.OrganizationId]["issuerCertificateChain"].ToString();

                CredentialDetails["supportedCredentials"] = supportedCredentials;

                Dictionary<string,object> issuerOffer= new Dictionary<string, object>();

                issuerOffer[credential.OrganizationId] = CredentialDetails;

                return new ServiceResult(true, "Get QrCredential Offer Success", issuerOffer);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential By Id::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> CreateCredentialAsync(QrCredentialDTO credentialDto)
        {
            try
            {
                var guid = Guid.NewGuid().ToString();

                var isCredentialExist = await _unitOfWork.
                    QrCredential.IsCredentialExistsAsync(credentialDto.credentialName);

                if (isCredentialExist)
                {
                    return new ServiceResult(false, "Credential Name Already Exist");
                }

                var credential = new QrCredential()
                {
                    CredentialUid = guid,
                    CredentialName = credentialDto.credentialName,
                    DisplayName=credentialDto.displayName,
                    DataAttributes = JsonConvert.SerializeObject(credentialDto.dataAttributes),
                    OrganizationId = credentialDto.organizationId,
                    PortraitVerificationRequired=credentialDto.portraitVerificationRequired,
                    Status = "PENDING",
                    CreatedDate = DateTime.Now
                };

                List<AttributeData> publicData = new List<AttributeData>();

                List<AttributeData> privateData = new List<AttributeData>();

                var format = await _unitOfWork.WalletConfiguration.GetCredentialFormats();

                var formatList = new List<string>();

                List<CredentialFormats> credentialFormats = 
                    JsonConvert.DeserializeObject<List<CredentialFormats>>(format);

                foreach (var credentialFormat in credentialFormats)
                {
                    if (credentialFormat.isSelected)
                    {
                        formatList.Add(credentialFormat.Name);
                    }
                }

                foreach (var item in credentialDto.dataAttributes.publicAttributes)
                {

                    AttributeData attributeData = new AttributeData()
                    {
                        AttributeName = item.attribute,
                        DataType = item.dataType,
                        DisplayName = item.displayName,
                    };
                    publicData.Add(attributeData);
                }

                foreach (var item in credentialDto.dataAttributes.privateAttributes)
                {

                    AttributeData attributeData = new AttributeData()
                    {
                        AttributeName = item.attribute,
                        DataType = item.dataType,
                        DisplayName = item.displayName,
                    };
                    privateData.Add(attributeData);
                }

                QrAttributes qrAttributes = new QrAttributes()
                {
                    privateData = privateData,
                    publicData = publicData,
                };

                var supportedCredential = new QrSupportedCredential()
                {
                    CredentialName = credentialDto.credentialName,
                    CredentialType = credentialDto.credentialName,
                    format = formatList,
                    faceValidation = credentialDto.portraitVerificationRequired,
                    proofType = "DataIntegrityProof",
                    revocation = "RevocationList2020Status",
                    data = qrAttributes
                };

                var supportedCredentials = new Dictionary<string, QrSupportedCredential>();

                supportedCredentials[guid] = supportedCredential;

                var issuerId = new QrIssuerId()
                {
                    IssuerName = credentialDto.organizationId,

                    SupportedCredentials = supportedCredentials
                };

                var credentialOffer = new Dictionary<string, QrIssuerId>();

                credentialOffer[credentialDto.organizationId] = issuerId;

                var response = await GenerateCredentialOffer(credentialOffer);

                if (response == null || !response.Success)
                {
                    return new ServiceResult(false, response.Message);
                }

                credential.CredentialOffer = JsonConvert.SerializeObject(response.Resource);

                await _unitOfWork.QrCredential.AddAsync(credential);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Credential Created Successfully" +
                    ". Test your QrCredential to send for Activation", guid);
            }
            catch (Exception error)
            {
                _logger.LogError("Create Credential::Database exception: {0}", error);
                return new ServiceResult(false, "Failed to Create Credential");
            }
        }

        public async Task<ServiceResult> UpdateCredential(QrCredentialDTO credentialDto)
        {
            try
            {
                var credentialInDb = await _unitOfWork.Credential.GetCredentialByIdAsync(credentialDto.Id);

                var isCredentialExist = await _unitOfWork.Credential.IsCredentialExistsAsync(credentialDto.credentialName, credentialInDb.CredentialUid);

                if (isCredentialExist)
                {
                    return new ServiceResult(false, "Credential Name Already Exist");
                }
                credentialInDb.DisplayName = credentialDto.displayName;
                credentialInDb.CredentialName = credentialDto.credentialName;
                credentialInDb.DataAttributes = JsonConvert.SerializeObject(credentialDto.dataAttributes);
                credentialInDb.OrganizationId = credentialDto.organizationId;

                await _unitOfWork.Credential.UpdateCredential(credentialInDb);

                return new ServiceResult(true, "QrCredential Updated Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Create Credential::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to update Credential");
            }
        }

        public async Task<ServiceResult> TestCredential(QrTestCredentialRequest request)
        {
            var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(request.CredentialId);

            if (credential == null)
            {
                return new ServiceResult(false, "Failed to get Credential Details");
            }
            Dictionary<string, object> Data = new Dictionary<string, object>();

            foreach (var kvp in request.Data)
            {
                JsonElement[] jsonElements = System.Text.Json.JsonSerializer.Deserialize<JsonElement[]>(kvp.Value.GetRawText());
                List<object> attributeData = new List<object>();
                foreach (JsonElement item in jsonElements)
                {
                    object item1 = ConvertJsonElement(item);
                    attributeData.Add(item1);
                }
                Data[kvp.Key]= attributeData;
            }
            var testCredentialDTO = new TestCredentialDTO()
            {
                issuerID = credential.OrganizationId,
                suid = Guid.NewGuid().ToString(),
                credentialId = credential.CredentialUid,
                credentialType = credential.CredentialName,
                Data = Data,
                qr = true
            };
            var testCredentialResponse = await TestCredentialData(testCredentialDTO);

            if (!testCredentialResponse.Success)
            {
                _logger.LogError(testCredentialResponse.Message);

                return new ServiceResult(false, testCredentialResponse.Message);
            }

            var response = await UpdateVcTestData(request.CredentialId, testCredentialResponse.Resource.ToString());

            if (response.Success)
            {
                return new ServiceResult(true, "Test Credential Successful. Sent for Admin Approval");
            }
            else
            {
                _logger.LogError(response.Message);
                return new ServiceResult(false, response.Message);

            }
        }


        public async Task<ServiceResult> TestCredentialData(TestCredentialDTO testCredentialDTO)
        {
            try
            {
                string json = JsonConvert.SerializeObject(testCredentialDTO);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/testCredential", content);
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
                    return new ServiceResult(false, response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetVcStatus(string credentialData)
        {
            try
            {
                Dictionary<string, string> vcObject = new Dictionary<string, string>();

                vcObject["VC"] = credentialData;

                string json = JsonConvert.SerializeObject(vcObject);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/getVCStatus", content);
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
                    return new ServiceResult(false, response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> UpdateCredential(string credentialId, string status)
        {
            try
            {
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                credential.Status = status;

                await _unitOfWork.Credential.UpdateCredential(credential);

                return new ServiceResult(true, "QrCredential Updated Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Update Credential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> UpdateVcTestData(string credentialId, string vcData)
        {
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                credential.TestVcData = vcData;

                credential.Status = "APPROVAL_REQUIRED";

                await _unitOfWork.QrCredential.UpdateCredential(credential);

                return new ServiceResult(true, "QrCredential Updated Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Update Credential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> ActivateCredential(string credentialId)
        {
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                var getVcStatusResponse = await GetVcStatus(credential.TestVcData);

                if (!getVcStatusResponse.Success)
                {
                    _logger.LogError(getVcStatusResponse.Message);

                    return new ServiceResult(false, getVcStatusResponse.Message);
                }

                var organizationDetailsResponse = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.OrganizationId);

                if (organizationDetailsResponse == null || !organizationDetailsResponse.Success)
                {
                    return new ServiceResult(false, organizationDetailsResponse.Message);
                }

                var organizationDetails = (OrganizationDTO)organizationDetailsResponse.Resource;
                credential.Status = "ACTIVE";

                await _unitOfWork.QrCredential.UpdateCredential(credential);

                return new ServiceResult(true, "QrCredential Activated Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Update Credential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> RejectCredential(string credentialId, string remarks)
        {
            try
            {
                var credential = await _unitOfWork.QrCredential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "QrCredential Data Not Found");
                }

                credential.Status = "REJECTED";
                credential.Remarks = remarks;
                await _unitOfWork.QrCredential.UpdateCredential(credential);

                return new ServiceResult(true, "QrCredential Rejected Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Update Credential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> GetCredentialDetails(string credentialUid)
        {
            try
            {
                var walletConfigurationResponse = await _walletConfigurationService.GetConfiguration();

                var walletConfiguration = (WalletConfigurationResponse)walletConfigurationResponse.Resource;

                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialUid);

                if (credential == null)
                {
                    return new ServiceResult(false, "QrCredential Data Not Found");
                }

                JObject json = JObject.Parse(credential.CredentialOffer);

                var formatTypes = json[credential.OrganizationId]["supportedCredentials"][0]["format"];

                var formatList = formatTypes.ToObject<List<string>>();

                var issuerKey = json[credential.OrganizationId]["issuerKey"].ToString();

                string[] issuerKeyArray = issuerKey.Split(':');

                var formatDisplayNamesList = new List<string>();

                WalletConfigurationDTO walletConfigurationDTO = new WalletConfigurationDTO();

                List<WalletConfigurationDetailsDTO> walletConfigurationDetailsDTO = new List<WalletConfigurationDetailsDTO>();

                foreach (var item in walletConfiguration.CredentialFormats)
                {
                    foreach (var format in formatList)
                    {
                        if (format == item.Name)
                        {
                            if (item.Name == "vc+json-Id")
                            {
                                walletConfigurationDetailsDTO.Add(new WalletConfigurationDetailsDTO()
                                {
                                    format = item.DisplayName,
                                    bindingMethod = "Decentralized Identifier(DID)",
                                    supportedMethod = "Key"
                                });
                            }
                            if (item.Name == "mso_mdoc")
                            {
                                walletConfigurationDetailsDTO.Add(new WalletConfigurationDetailsDTO()
                                {
                                    format = item.DisplayName,
                                    bindingMethod = "CBOR Signing and Encryption(ISO-18013-5 MDL)",
                                    supportedMethod = ""
                                });
                            }
                        }
                    }
                }

                return new ServiceResult(true, "Get QrCredential Details Success", walletConfigurationDetailsDTO);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential By Id::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> GetCredentialNameIdListAsync(string organizationId)
        {
            var credentialList = await _unitOfWork.Credential.GetCredentialNameIdListAsync(organizationId);

            if (credentialList == null)
            {
                return new ServiceResult(false, "Failed to get Credential List");
            }
            return new ServiceResult(true, "Get Credential List Success", credentialList);
        }

        public async Task<ServiceResult> GetVerifiableCredentialList(string orgId)
        {
            try
            {
                var credentialVerifierList = await _unitOfWork.QrCredentialVerifiers.GetCredentialsListByOrganizationIdAsync(orgId);
                if (credentialVerifierList == null)
                {
                    return new ServiceResult(false, "Failed to get QrCredential List");
                }

                var credentialList = await _unitOfWork.QrCredential.GetVerifiableCredentialList(credentialVerifierList);

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get QrCredential List");
                }
                List<string> credentialNameIdList = new List<string>();

                foreach (var credential in credentialList)
                {
                    credentialNameIdList.Add(credential.CredentialName + "," + credential.CredentialUid);
                }

                return new ServiceResult(true, "Get QrCredential List Successfully", credentialNameIdList);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetCredentialNameIdListAsync()
        {
            try
            {
                var credentialList = await _unitOfWork.QrCredential.GetCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var credential in credentialList)
                {
                    dict[credential.CredentialUid] = credential.CredentialName;
                }
                return new ServiceResult(true, "Get QrCredential List Successfully", dict);
            }
            catch (Exception error)
            {
                _logger.LogError("Get QrCredential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get QrCredential List");
            }
        }

        public async Task<ServiceResult> SendToApproval(string credentialId, string signedDocument)
        {
            try
            {
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                credential.SignedDocument = signedDocument;

                credential.Status = "APPROVAL_REQUIRED";

                await _unitOfWork.Credential.UpdateCredential(credential);

                return new ServiceResult(true, "QrCredential Sent For Approval");
            }
            catch (Exception error)
            {
                _logger.LogError("Update QrCredential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }
    }
}
