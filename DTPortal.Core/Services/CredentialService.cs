using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Enums;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Google.Apis.Auth.OAuth2;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static iTextSharp.text.pdf.AcroFields;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Attribute = DTPortal.Core.DTOs.Attribute;

namespace DTPortal.Core.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly ILogger<CredentialService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOrganizationService _organizationService;
        private readonly IEmailSender _emailSender;
        private readonly IWalletConfigurationService _walletConfigurationService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserDataService _userDataService;
        private readonly ISelfServiceConfigurationService _selfServiceConfigurationService;
        private readonly ICacheClient _cacheClient;
        private readonly Helper _helper;
        public CredentialService(ILogger<CredentialService> logger,
            IUnitOfWork unitOfWork,
            HttpClient httpClient,
            IConfiguration configuration,
            IOrganizationService organizationService,
            IHttpClientFactory httpClientFactory,
            IEmailSender emailSender,
            ICategoryService categoryService,
            IWalletConfigurationService walletConfigurationService,
            ISelfServiceConfigurationService selfServiceConfigurationService,
            IWebHostEnvironment environment,
            IUserDataService userDataService,
            ICacheClient cacheClient,
            Helper helper)
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
            _userDataService = userDataService;
            _selfServiceConfigurationService = selfServiceConfigurationService;
            _cacheClient = cacheClient;
            _helper = helper;
        }

        public async Task<ServiceResult> GenerateCredentialOffer
            (Dictionary<string, IssuerId> credentialOffer)
        {
            try
            {
                string json = JsonConvert.SerializeObject(credentialOffer);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/credentialOffer", content);
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
                var credentialList = await _unitOfWork.Credential.GetCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var credentialdtoList = new List<CredentialDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialdto = new CredentialDTO()
                    {
                        Id = credential.Id,

                        credentialUId = credential.CredentialUid,

                        displayName = credential.DisplayName,

                        authenticationScheme = credential.AuthenticationScheme,

                        dataAttributes = JsonConvert.DeserializeObject<List<DataAttributesDTO>>(credential.DataAttributes),

                        categoryId = credential.CategoryId,

                        credentialName = credential.CredentialName,

                        organizationId = credential.OrganizationId,

                        credentialOffer = credential.CredentialOffer,

                        verificationDocType = credential.VerificationDocType,

                        createdDate = (DateTime)credential.CreatedDate,

                        logo = credential.Logo,

                        status = credential.Status
                    };
                    credentialdtoList.Add(credentialdto);
                }
                return new ServiceResult(true, "Credential List Successfully", credentialdtoList);
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
                var credentialList = await _unitOfWork.Credential.GetCredentialListByOrgIdAsync(orgId);

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var credentialdtoList = new List<CredentialDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialdto = new CredentialDTO()
                    {
                        Id = credential.Id,

                        credentialUId = credential.CredentialUid,

                        displayName = credential.DisplayName,

                        authenticationScheme = credential.AuthenticationScheme,

                        dataAttributes = JsonConvert.DeserializeObject<List<DataAttributesDTO>>(credential.DataAttributes),

                        categoryId = credential.CategoryId,

                        credentialName = credential.CredentialName,

                        organizationId = credential.OrganizationId,

                        credentialOffer = credential.CredentialOffer,

                        verificationDocType = credential.VerificationDocType,

                        createdDate = (DateTime)credential.CreatedDate,

                        logo = credential.Logo,

                        status = credential.Status
                    };
                    credentialdtoList.Add(credentialdto);
                }
                return new ServiceResult(true, "Credential List Successfully", credentialdtoList);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetActiveCredentialList(string token)
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
                var credentialList = await _unitOfWork.Credential.GetActiveCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var Categories = await _categoryService.GetCategoryNameAndIdPairAsync();

                if (Categories == null)
                {
                    return new ServiceResult(false, "Failed to get Categories List");
                }

                var credentialDTOList = new List<CredentialListDTO>();

                foreach (var credential in credentialList)
                {
                    var credentialListDTO = new CredentialListDTO()
                    {
                        authenticationScheme = credential.AuthenticationScheme,

                        displayName = credential.DisplayName,

                        categoryId = credential.CategoryId,

                        credentialName = credential.CredentialName,

                        organizationId = credential.OrganizationId,

                        credentialId = credential.CredentialUid,

                        status = credential.Status,

                        logo = credential.Logo,

                        documentName = credential.VerificationDocType,

                        shareAuthenticationScheme = credential.SharingAuthenticationScheme,

                        viewAuthenticationScheme= credential.ViewingAuthenticationScheme
                    };
                    if (Categories.ContainsKey(credential.CategoryId))
                    {
                        credentialListDTO.categoryName = Categories[credential.CategoryId];
                    }
                    credentialDTOList.Add(credentialListDTO);
                }
                return new ServiceResult(true, "Credential List Successfully", credentialDTOList);
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
                var credential = await _unitOfWork.Credential.GetCredentialByIdAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                var credentialDto = new CredentialDTO()
                {
                    Id = credential.Id,
                    credentialUId = credential.CredentialUid,
                    displayName = credential.DisplayName,
                    authenticationScheme = credential.AuthenticationScheme,
                    dataAttributes = JsonConvert.DeserializeObject<List<DataAttributesDTO>>(credential.DataAttributes),
                    categoryId = credential.CategoryId,
                    verificationDocType = credential.VerificationDocType,
                    credentialName = credential.CredentialName,
                    organizationId = credential.OrganizationId,
                    credentialOffer = credential.CredentialOffer,
                    status = credential.Status,
                    remarks = credential.Remarks,
                    logo = credential.Logo,
                    trustUrl=credential.TrustUrl,
                    signedDocument = credential.SignedDocument,
                    createdDate = (DateTime)credential.CreatedDate
                };
                if (credential.Validity!=null){
                    credentialDto.validity = (int)credential.Validity;
                }
                if (credential.Categories != null)
                {
                    credentialDto.categories = JsonConvert.DeserializeObject<List<int>>(credential.Categories);
                }
                return new ServiceResult(true, "Get Credential Success", credentialDto);
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
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                var credentialDto = new CredentialDTO()
                {
                    Id = credential.Id,
                    authenticationScheme = credential.AuthenticationScheme,
                    credentialUId = credential.CredentialUid,
                    verificationDocType = credential.VerificationDocType,
                    displayName = credential.DisplayName,
                    dataAttributes = JsonConvert.DeserializeObject<List<DataAttributesDTO>>(credential.DataAttributes),
                    categoryId = credential.CategoryId,
                    credentialName = credential.CredentialName,
                    organizationId = credential.OrganizationId,
                    credentialOffer = credential.CredentialOffer,
                    status = credential.Status,
                    logo = credential.Logo,
                    remarks = credential.Remarks,
                    trustUrl=credential.TrustUrl,
                    createdDate = (DateTime)credential.CreatedDate
                };
                if (credential.Validity != null)
                {
                    credentialDto.validity = (int)credential.Validity;
                }
                if (credential.Categories != null)
                {
                    credentialDto.categories = JsonConvert.DeserializeObject<List<int>>(credential.Categories);
                }
                return new ServiceResult(true, "Get Credential Success", credentialDto);
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
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(Id);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                JObject json = JObject.Parse(credential.CredentialOffer);

                var obj1 = json[credential.OrganizationId]["supportedCredentials"][0]["credentialType"];

                var jsonObject = obj1.ToString();

                var attributeslist = json[credential.OrganizationId]["supportedCredentials"][0][jsonObject];

                var attributes = JsonConvert.DeserializeObject<List<Attribute>>(attributeslist.ToString());

                var supportedCredentials = new Dictionary<string, object>();

                supportedCredentials["credentialId"] = json[credential.OrganizationId]["supportedCredentials"][0]["credentialId"].ToString();

                supportedCredentials["credentialType"] = json[credential.OrganizationId]["supportedCredentials"][0]["credentialType"].ToString();

                supportedCredentials["trustUrl"] = json[credential.OrganizationId]["supportedCredentials"][0]["trustUrl"].ToString();

                supportedCredentials["isoNamespace"] = json[credential.OrganizationId]["supportedCredentials"][0]["isoNamespace"].ToString();

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

                return new ServiceResult(true, "Get Credential Success", CredentialDetails);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential By Id::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }

        public async Task<ServiceResult> CreateCredentialAsync(CredentialDTO credentialDto)
        {
            try
            {
                var guid = Guid.NewGuid().ToString();

                var isCredentialExist = await _unitOfWork.Credential.IsCredentialExistsAsync(credentialDto.credentialName);

                if (isCredentialExist)
                {
                    return new ServiceResult(false, "Credential Name Already Exist");
                }

                var isCredentialDisplayNameExist = await _unitOfWork.Credential.IsCredentialDisplayExistAsync(credentialDto.displayName);

                if (isCredentialDisplayNameExist)
                {
                    return new ServiceResult(false, "Credential Display Name Already Exist");
                }
                var credential = new Credential()
                {
                    CredentialUid = guid,
                    AuthenticationScheme = credentialDto.authenticationScheme,
                    CredentialName = credentialDto.credentialName,
                    DisplayName = credentialDto.displayName,
                    DataAttributes = JsonConvert.SerializeObject(credentialDto.dataAttributes),
                    VerificationDocType = credentialDto.verificationDocType,
                    OrganizationId = credentialDto.organizationId,
                    CategoryId = credentialDto.categoryId,
                    Logo = credentialDto.logo,
                    Status = "PENDING",
                    TrustUrl = credentialDto.trustUrl,
                    CreatedDate = DateTime.Now,
                    Validity=credentialDto.validity
                };
                if (credentialDto.categories != null)
                {
                    credential.Categories = JsonConvert.SerializeObject(credentialDto.categories);
                }
                List<AttributeData> Data = new List<AttributeData>();

                var format = await _unitOfWork.WalletConfiguration.GetCredentialFormats();

                var formatList = new List<string>();

                List<CredentialFormats> credentialFormats = JsonConvert.DeserializeObject<List<CredentialFormats>>(format);

                foreach (var credentialFormat in credentialFormats)
                {
                    if (credentialFormat.isSelected)
                    {
                        formatList.Add(credentialFormat.Name);
                    }
                }

                foreach (var item in credentialDto.dataAttributes)
                {

                    AttributeData attributeData = new AttributeData()
                    {
                        AttributeName = item.attribute,
                        DataType = item.dataType,
                        DisplayName = item.displayName,
                    };
                    Data.Add(attributeData);
                }

                var supportedCredential = new SupportedCredential()
                {
                    CredentialName = credentialDto.credentialName,
                    CredentialType = credentialDto.credentialName,
                    format = formatList,
                    TrustUrl=credentialDto.trustUrl,
                    proofType = "DataIntegrityProof",
                    revocation = "RevocationList2020Status",
                    data = Data
                };

                var supportedCredentials = new Dictionary<string, SupportedCredential>();

                supportedCredentials[guid] = supportedCredential;

                var issuerId = new IssuerId()
                {
                    IssuerName = credentialDto.organizationId,

                    SupportedCredentials = supportedCredentials
                };

                var credentialOffer = new Dictionary<string, IssuerId>();

                credentialOffer[credentialDto.organizationId] = issuerId;

                var response = await GenerateCredentialOffer(credentialOffer);

                if (response == null || !response.Success)
                {
                    return new ServiceResult(false, response.Message);
                }

                credential.CredentialOffer = JsonConvert.SerializeObject(response.Resource);

                await _unitOfWork.Credential.AddAsync(credential);

                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Credential Created Successfully. Test your New Credential to send for Activation", guid);
            }
            catch (Exception error)
            {
                _logger.LogError("Create Credential::Database exception: {0}", error);
                return new ServiceResult(false, "Failed to Create Credential");
            }
        }

        public async Task<ServiceResult> UpdateCredential(CredentialDTO credentialDto)
        {
            try
            {
                var credentialInDb = await _unitOfWork.Credential.GetCredentialByIdAsync(credentialDto.Id);

                var isCredentialExist = await _unitOfWork.Credential.IsCredentialExistsAsync(credentialDto.credentialName, credentialInDb.CredentialUid);

                if (isCredentialExist)
                {
                    return new ServiceResult(false, "Credential Name Already Exist");
                }

                var isCredentialDisplayNameExist = await _unitOfWork.Credential.IsCredentialDisplayExistAsync(credentialDto.displayName, credentialInDb.CredentialUid);

                if (isCredentialDisplayNameExist)
                {
                    return new ServiceResult(false, "Credential Display Name Already Exist");
                }

                List<AttributeData> Data = new List<AttributeData>();

                var format = await _unitOfWork.WalletConfiguration.GetCredentialFormats();

                var formatList = new List<string>();

                List<CredentialFormats> credentialFormats = JsonConvert.DeserializeObject<List<CredentialFormats>>(format);

                foreach (var credentialFormat in credentialFormats)
                {
                    if (credentialFormat.isSelected)
                    {
                        formatList.Add(credentialFormat.Name);
                    }
                }

                foreach (var item in credentialDto.dataAttributes)
                {

                    AttributeData attributeData = new AttributeData()
                    {
                        AttributeName = item.attribute,
                        DataType = item.dataType,
                        DisplayName = item.displayName,
                    };
                    Data.Add(attributeData);
                }

                var supportedCredential = new SupportedCredential()
                {
                    CredentialName = credentialDto.credentialName,
                    CredentialType = credentialDto.credentialName,
                    format = formatList,
                    proofType = "DataIntegrityProof",
                    revocation = "RevocationList2020Status",
                    data = Data
                };

                var supportedCredentials = new Dictionary<string, SupportedCredential>();

                supportedCredentials[credentialInDb.CredentialUid] = supportedCredential;

                var issuerId = new IssuerId()
                {
                    IssuerName = credentialDto.organizationId,

                    SupportedCredentials = supportedCredentials
                };

                var credentialOffer = new Dictionary<string, IssuerId>();

                credentialOffer[credentialDto.organizationId] = issuerId;

                var response = await GenerateCredentialOffer(credentialOffer);

                if (response == null || !response.Success)
                {
                    return new ServiceResult(false, response.Message);
                }

                credentialInDb.CredentialOffer = JsonConvert.SerializeObject(response.Resource);
                credentialInDb.AuthenticationScheme = credentialDto.authenticationScheme;
                credentialInDb.CredentialName = credentialDto.credentialName;
                credentialInDb.DataAttributes = JsonConvert.SerializeObject(credentialDto.dataAttributes);
                credentialInDb.OrganizationId = credentialDto.organizationId;
                credentialInDb.VerificationDocType = credentialDto.verificationDocType;
                credentialInDb.TrustUrl = credentialDto.trustUrl;
                credentialInDb.Validity=credentialInDb.Validity;
                if (credentialInDb.Categories != null)
                {
                    credentialInDb.Categories = JsonConvert.SerializeObject(credentialDto.categories);
                }
                await _unitOfWork.Credential.UpdateCredential(credentialInDb);

                return new ServiceResult(true, "Credential Updated Successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Create Credential::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to update Credential");
            }
        }

        public async Task<ServiceResult> TestCredential(string userId, string credentialId)
        {
            var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

            if (credential == null)
            {
                return new ServiceResult(false, "Failed to get Credential Details");
            }
            ServiceResult userDetails = null;

            //if (credential.CredentialName == "SocialBenefitCard")
            //{
            //    userDetails = await _userDataService.GetSocialBenefitCardDetails(userId);
            //}
            //else if (credential.CredentialName == "mDL")
            //{
            //    userDetails = await _userDataService.GetMdlProfile(userId);
            //}
            //else
            //{
            //    userDetails = await _userDataService.GetPidProfile(userId);
            //}
            var url = credential.TrustUrl + "/" + userId;

            userDetails = await _userDataService.GetProfile(url);
            if (userDetails == null || !userDetails.Success)
            {
                return new ServiceResult(false, "Failed to get User Details");
            }

            var Data = new Dictionary<string, object>();

            var jsonObject = JObject.Parse(JsonConvert.SerializeObject(userDetails.Resource));

            foreach (var property in jsonObject.Properties())
            {
                Data[property.Name] = property.Value.Type == JTokenType.Object
                    ? property.Value.ToString()
                    : property.Value;
            }

            var testCredentialDTO = new TestCredentialDTO()
            {
                issuerID = credential.OrganizationId,
                suid = userId,
                credentialId = credential.CredentialUid,
                credentialType = credential.CredentialName,
                Data = Data
            };

            var testCredentialResponse = await TestCredentialData(testCredentialDTO);
            _logger.LogInformation("Test Credential Response: {response}", testCredentialResponse);

            if (!testCredentialResponse.Success)
            {
                _logger.LogError(testCredentialResponse.Message);

                return new ServiceResult(false, testCredentialResponse.Message);
            }

            var response = await UpdateVcTestData(credentialId, testCredentialResponse.Resource.ToString());
            _logger.LogInformation("Update VC Test Data Response: {response}", response);

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

        public async Task<ServiceResult> GetUserProfile(string userId, string credentialId)
        {
            var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);
            if (credential == null)
            {
                return new ServiceResult(false, "Failed to get Credential Details");
            }

            ServiceResult userDetails = null;

            if(credential.CredentialName== "SocialBenefitCard")
            {
                userDetails = await _userDataService.GetSocialBenefitCardDetails(userId);
            }
            else if (credential.CredentialName == "mDL")
            {
                userDetails = await _userDataService.GetMdlProfile(userId);
            }
            else
            {
                userDetails = await _userDataService.GetPidProfile(userId);
            }
            if (userDetails == null || !userDetails.Success)
            {
                return new ServiceResult(false, userDetails.Message);
            }
            var Data = new Dictionary<string, object>();

            var jsonObject = JObject.Parse(JsonConvert.SerializeObject(userDetails.Resource));

            foreach (var property in jsonObject.Properties())
            {
                Data[property.Name] = property.Value.Type switch
                {
                    JTokenType.Object => property.Value.ToObject<Dictionary<string, object>>(),
                    JTokenType.Array => property.Value.ToObject<List<object>>(),
                    _ => property.Value.ToObject<object>()
                };
            }

            return new ServiceResult(true, "Successfully Get User Profile", Data);
        }

        public async Task<ServiceResult> TestCredentialData(TestCredentialDTO testCredentialDTO)
        {
            try
            {
                _logger.LogInformation("TestCredentialData started.");

                // Serialize DTO
                _logger.LogInformation("Serializing TestCredentialDTO...");
                string json = JsonConvert.SerializeObject(testCredentialDTO);
                _logger.LogInformation("Serialized JSON: {json}", json);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending POST request to: MDOCProvisioning/testCredential");

                HttpResponseMessage response = await _client.PostAsync("MDOCProvisioning/testCredential", content);

                // Log the basic response info
                _logger.LogInformation("Received HTTP response. StatusCode: {statusCode}", response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Response Body received: {responseBody}", responseBody);

                    _logger.LogInformation("Deserializing APIResponse...");

                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseBody);

                    if (apiResponse.Success)
                    {
                        _logger.LogInformation("API Response indicates Success. Message: {msg}", apiResponse.Message);
                        return new ServiceResult(true, apiResponse.Message, apiResponse.Result);
                    }
                    else
                    {
                        _logger.LogError("API returned failure: {msg}", apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError("Request to {uri} failed with status code {statusCode}",
                        response.RequestMessage.RequestUri, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in TestCredentialData: {msg}", ex.Message);
            }

            _logger.LogInformation("TestCredentialData ended with null result.");
            return null;
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
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

                return new ServiceResult(true, "Credential Updated Successfully");
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
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                credential.TestVcData = vcData;

                credential.Status = "APPROVAL_REQUIRED";

                await _unitOfWork.Credential.UpdateCredential(credential);

                return new ServiceResult(true, "Credential Updated Successfully");
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
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

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

                await _unitOfWork.Credential.UpdateCredential(credential);

                return new ServiceResult(true, "Credential Activated Successfully");
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
                var credential = await _unitOfWork.Credential.GetCredentialByUidAsync(credentialId);

                if (credential == null)
                {
                    return new ServiceResult(false, "Credential Data Not Found");
                }

                credential.Status = "REJECTED";
                credential.Remarks = remarks;
                await _unitOfWork.Credential.UpdateCredential(credential);

                return new ServiceResult(true, "Credential Rejected Successfully");
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
                    return new ServiceResult(false, "Credential Data Not Found");
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

                return new ServiceResult(true, "Get Credential Details Success", walletConfigurationDetailsDTO);
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
                var credentialVerifierList = await _unitOfWork.CredentialVerifiers.GetCredentialsListByOrganizationIdAsync(orgId);
                if (credentialVerifierList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }

                var credentialList = await _unitOfWork.Credential.GetVerifiableCredentialList(credentialVerifierList);

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }
                var organizationCategoryResult = await _selfServiceConfigurationService.GetCategoryByOrganizationId(orgId);

                if(organizationCategoryResult == null || !organizationCategoryResult.Success)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }
                OrganizationCategoryDTO organizationCategoryDTO =(OrganizationCategoryDTO)organizationCategoryResult.Resource;

                List<string> credentialNameIdList = new List<string>();

                foreach (var credential in credentialList)
                {
                    if (credential.Categories != null)
                    {
                        var orgCategoriesList=JsonConvert.DeserializeObject<List<int>>(credential.Categories);

                        if (orgCategoriesList.Contains(organizationCategoryDTO.CategoryId))
                        {
                            credentialNameIdList.Add(credential.DisplayName + "," + credential.CredentialUid);
                        }
                    }
                }

                return new ServiceResult(true, "Credential List Successfully", credentialNameIdList);
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
                var credentialList = await _unitOfWork.Credential.GetCredentialListAsync();

                if (credentialList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var credential in credentialList)
                {
                    dict[credential.CredentialUid] = credential.DisplayName;
                }
                return new ServiceResult(true, "Credential List Successfully", dict);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> GetAuthSchemesList()
        {
            try
            {
                var authSchemeList = await _unitOfWork.AuthScheme.ListAuthSchemesAsync();
                if (authSchemeList == null)
                {
                    return new ServiceResult(false, "Failed to get Credential List");
                }
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var authScheme in authSchemeList)
                {
                    if (authScheme.SupportsProvisioning == 1)
                    {
                        dict[authScheme.Name] = authScheme.DisplayName;
                    }
                }
                return new ServiceResult(true, "Credential List Successfully", dict);
            }
            catch (Exception error)
            {
                _logger.LogError("Get Credential List::Database exception: {0}", error);

                return new ServiceResult(false, "Failed to get Credential List");
            }
        }

        public async Task<ServiceResult> SendToApproval
            (string credentialId, string signedDocument)
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

                return new ServiceResult(true, "Credential Sent For Approval");
            }
            catch (Exception error)
            {
                _logger.LogError("Update Credential ::Database exception: {0}", error);

                return new ServiceResult(false, error.Message);
            }
        }
    }
}