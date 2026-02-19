using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using System.IO;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using iTextSharp.text.pdf.qrcode;
using iTextSharp.text;
using System.Globalization;
using static System.Net.WebRequestMethods;
using static DTPortal.Common.CommonResponse;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
//using static System.Formats.Asn1.AsnWriter;

namespace DTPortal.Core.Services
{
    public class UserProfileService : IUserProfileService
    {
        // Initialize logger
        private readonly ILogger<UserInfoService> _logger;
        // Initialize Db
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;
        //private readonly ITokenManager _tokenManager;
        private readonly idp_configuration idpConfiguration;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly OpenIdConnect openidconnect;
        private readonly OIDCConstants OIDCConstants;
        private readonly IConfiguration configuration;
        private readonly IClientService _clientService;
        private readonly IPushNotificationClient _pushNotificationClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClientsPurposeService _clientsPurposeService;
        private readonly IPurposeService _purposeService;
        //private readonly ITokenManagerService _tokenManagerService;
        private readonly ITransactionProfileRequestService _transactionProfileRequestService;
        private readonly ITransactionProfileConsentService _transactionProfileConsentService;
        private readonly ITransactionProfileStatusService _transactionProfileStatusService;
        private readonly IAttributeServiceTransactionsService _attributeServiceTransactionsService;
        private readonly IDataPivotService _dataPivotService;
        private readonly IScopeService _scopeService;
        private readonly IHelper _helper;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserClaimService _userClaimService;
        public UserProfileService(ILogger<UserInfoService> logger,
            IUnitOfWork unitofWork, ICacheClient cacheClient,
            //ITokenManager tokenManager,
            IGlobalConfiguration globalConfiguration, IConfiguration Configuration,
            IClientService clientService,
            IPushNotificationClient pushNotificationClient,
            IHttpClientFactory httpClientFactory,
            IClientsPurposeService clientsPurposeService,
            IPurposeService purposeService,
            //ITokenManagerService tokenManagerService,
            ITransactionProfileRequestService transactionProfileRequestService,
            ITransactionProfileConsentService transactionProfileConsentService,
            ITransactionProfileStatusService transactionProfileStatusService,
            IAttributeServiceTransactionsService attributeServiceTransactionsService,
            IDataPivotService dataPivotService,
            IScopeService scopeService,
            IHelper helper,
            IWebHostEnvironment environment,
            IEConsentService econsentService,
            IUserClaimService userClaimService
            )
        {
            _logger = logger;
            _unitOfWork = unitofWork;
            _cacheClient = cacheClient;
            //_tokenManager = tokenManager;
            _globalConfiguration = globalConfiguration;
            configuration = Configuration;
            _clientService = clientService;
            _pushNotificationClient = pushNotificationClient;
            _httpClientFactory = httpClientFactory;
            _clientsPurposeService = clientsPurposeService;
            _purposeService = purposeService;
            //_tokenManagerService = tokenManagerService;
            _transactionProfileConsentService = transactionProfileConsentService;
            _transactionProfileRequestService = transactionProfileRequestService;
            _transactionProfileStatusService = transactionProfileStatusService;
            _attributeServiceTransactionsService = attributeServiceTransactionsService;
            _dataPivotService = dataPivotService;
            _scopeService = scopeService;
            _helper = helper;
            _environment = environment;
            _userClaimService = userClaimService;

            idpConfiguration = _globalConfiguration.GetIDPConfiguration();
            if (null == idpConfiguration)
            {
                _logger.LogError("Get IDP Configuration failed");
                throw new NullReferenceException();
            }

            var errorConfiguration = _globalConfiguration.GetErrorConfiguration();
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            OIDCConstants = errorConfiguration.OIDCConstants;
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            openidconnect = JsonConvert.DeserializeObject<OpenIdConnect>
                (idpConfiguration.openidconnect.ToString());
            //_econsentService = econsentService;
        }
        public async Task<TransactionProfileRequestResponse> AddTransactionProfileRequest(string transactionId, int clientId, string Suid, string requestDetails)
        {
            try
            {
                TransactionProfileRequest transactionProfileRequest = new TransactionProfileRequest()
                {
                    TransactionId = transactionId,
                    ClientId = clientId,
                    Suid = Suid,
                    RequestDetails = requestDetails,
                    CreatedDate = DateTime.Now
                };
                var response = await _transactionProfileRequestService.AddProfileRequest(transactionProfileRequest);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddTransactionProfileRequest Failed   " + ex.Message);
                return null;
            }
        }
        public async Task<TransactionProfileConsentResponse> AddTransactionProfileConsent(int transactionId, string ConsentStatus, string ConsentDataSignature, string RequestedProfileAttributes, string approvedAttributes)
        {
            try
            {
                TransactionProfileConsent transactionProfileConsent = new TransactionProfileConsent()
                {
                    TransactionId = transactionId,
                    ConsentStatus = ConsentStatus,
                    ConsentDataSignature = ConsentDataSignature,
                    RequestedProfileAttributes = RequestedProfileAttributes,
                    CreatedDate = DateTime.Now,
                    ApprovedProfileAttributes = approvedAttributes
                };
                var response = await _transactionProfileConsentService.AddProfileConsent(transactionProfileConsent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Add transactionProfileConsent Failed   " + ex.Message);
                return null;
            }
        }
        public async Task<TransactionProfileStatusResponse> AddTransactionProfileStatus(int transactionId, string TransactionStatus, string FailedReason, string PivotSignedConsent, int DatapivotId)
        {
            try
            {
                TransactionProfileStatus transactionProfileStatus = new TransactionProfileStatus()
                {
                    TransactionId = transactionId,
                    TransactionStatus = TransactionStatus,
                    FailedReason = FailedReason,
                    PivotSignedConsent = PivotSignedConsent,
                    CreatedDate = DateTime.Now,
                    DatapivotId = DatapivotId
                };
                var response = await _transactionProfileStatusService.AddProfileStatus(transactionProfileStatus);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Add TransactionProfileStatus Failed   " + ex.Message);
                return null;
            }
        }
        public async Task<TransactionProfileConsentResponse> UpdateTransactionProfileConsent(int transactionId, string approvedAttributes, string ConsentStatus)
        {
            try
            {
                var transactionprofileConsent = new TransactionProfileConsent()
                {
                    TransactionId = transactionId,
                    UpdatedDate = DateTime.Now,
                    ConsentStatus = ConsentStatus,
                    ApprovedProfileAttributes = approvedAttributes
                };
                var response = await _transactionProfileConsentService.UpdateTransactionProfileConsent(transactionprofileConsent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Update TransactionProfileConsent Failed   " + ex.Message);
                return null;
            }
        }
        [NonAction]
        public bool ValidateScopes(string requestScopes, string clientScopes)
        {
            var Scopes = requestScopes.Split(new char[] { ' ', '\t' });
            var clientcopes = clientScopes.Split(new char[] { ' ', '\t' });
            var count = 0;
            foreach (var item in Scopes)
            {
                if (clientcopes.Contains(item))
                {
                    count++;
                }
            }
            if (count != Scopes.Length)
            {

                return false;
            }

            return true;
        }

        [NonAction]
        public bool ValidateProfiles(string requestScopes, string clientScopes)
        {
            var Scopes = requestScopes.Split(new char[] { ',', '\t' });
            var clientcopes = clientScopes.Split(new char[] { ',', '\t' });
            var count = 0;
            foreach (var item in Scopes)
            {
                if (clientcopes.Contains(item))
                {
                    count++;
                }
            }
            if (count != Scopes.Length)
            {

                return false;
            }

            return true;
        }

        public async Task<ServiceResult> GetNiraToken()
        {
            var client = new HttpClient();

            var url = "https://api.ugpass.go.ug/nira-api/login";

            client.DefaultRequestHeaders.Add("daes_authorization", "VUpneWQ3OEp9eVMvKV1WOkxKTEtoakBxZjllSlFrSA==");

            client.DefaultRequestHeaders.Add("identifier", "abcd");

            var response = await client.GetAsync(url);

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
        public async Task<ServiceResult> GetUserDetailsNira(string userId)
        {
            var tokenResponse = await GetNiraToken();
            if (!tokenResponse.Success)
            {
                return tokenResponse;
            }
            var tokenResult = JsonConvert.DeserializeObject<TokenResponseDTO>(tokenResponse.Resource.ToString());

            var client = new HttpClient();

            var url = $"https://api.ugpass.go.ug/nira-api/profile/{userId}";

            client.DefaultRequestHeaders.Add("daes_authorization", "VUpneWQ3OEp9eVMvKV1WOkxKTEtoakBxZjllSlFrSA==");

            client.DefaultRequestHeaders.Add("identifier", "abcd");

            client.DefaultRequestHeaders.Add("access_token", tokenResult.access_token);

            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                if (apiResponse.Success)
                {
                    var niraResponse = JsonConvert.DeserializeObject<NiraResponseDTO>(apiResponse.Result.ToString());
                    return new ServiceResult(true, apiResponse.Message, niraResponse);
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
        public bool CheckPurposes(string requestPurposes, IEnumerable<Purpose> PurposeListinDb)
        {
            var requestPurposeList = requestPurposes.Split(new char[] { ',' });
            foreach (var purpose in requestPurposeList)
            {
                foreach (var purposes in PurposeListinDb)
                {
                    var purposeId = purposes.Id.ToString();
                    if (purposeId.Equals(purpose))
                    {
                        if (purposes.UserConsentRequired)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool ValidatePurposes(string requestPurposes, IEnumerable<string> clientPurposesinDb)
        {
            var purposes = requestPurposes.Split(new char[] { ',' });

            var count = 0;

            foreach (var purpose in purposes)
            {
                if (clientPurposesinDb.Contains(purpose))
                {
                    count++;
                }
            }
            if (purposes.Length == count)
            {
                return true;
            }
            return false;
        }

        public List<string> GetClaimsList(string[] scopesList, IEnumerable<Scope> scopesinDb)
        {
            List<string> claimsList = new List<string>();
            string[] claims;
            foreach (var scope in scopesList)
            {
                foreach (var scopes in scopesinDb)
                {
                    if (scopes.Name.Equals(scope))
                    {
                        if (scopes.IsClaimsPresent)
                        {
                            claims = scopes.ClaimsList.Split(new char[] { ' ', '\t' });
                            claimsList.AddRange(claims);
                        }
                    }
                }
            }
            return claimsList;
        }
        public async Task<string> GetSubscriberPhoto(string url)
        {
            _logger.LogDebug("-->GetSubscriberPhoto");
            string response = null;
            var errorMessage = string.Empty;

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Invalid Input Parameter");
                return response;
            }

            _logger.LogDebug("Photo Url: {0}", url);
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();

                // Call the webservice with Get method
                var result = await client.GetAsync(url);

                // Check the status code
                if (result.IsSuccessStatusCode)
                {
                    // Read the response
                    byte[] content = await result.Content.ReadAsByteArrayAsync();
                    response = Convert.ToBase64String(content);
                    return response;
                }
                else
                {
                    _logger.LogError("GetSubscriberPhoto failed returned" +
                        ",Status code : {0}", result.StatusCode);
                    return null;
                }
            }
            catch (TimeoutException error)
            {
                _logger.LogError("GetSubscriberPhoto failed due to timeout exception: {0}",
                    error.Message);
                return null;
            }
            catch (Exception error)
            {
                _logger.LogError("GetSubscriberPhoto failed: {0}", error.Message);
                return null;
            }
        }
        public async Task<UserData> GetUserBasicDataAsync(
            string UserId, string UserIdType)
        {
            var raSubscriber = new SubscriberView();

            switch (UserIdType)
            {
                case "3":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByEmail(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "4":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByPhone(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "2":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfobyDocType(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "1":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfobyDocType(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "5":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoBySUID(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                default:
                    {
                        _logger.LogError("Incorrect input received");
                        return null;
                    };
            }

            DateTime? dob = null;
            if (DateTime.TryParse(raSubscriber.DateOfBirth, out DateTime parsed))
            {
                dob = parsed;
            }

            var userBasicProfile = new UserData()
            {
                CertificateStatus = raSubscriber.CertificateStatus,
                Country = raSubscriber.Country,
                DateOfBirth = dob,
                DisplayName = raSubscriber.DisplayName,
                Email = raSubscriber.Email,
                FcmToken = raSubscriber.FcmToken,
                Gender = raSubscriber.Gender,
                IdDocNumber = raSubscriber.IdDocNumber,
                Loa = raSubscriber.Loa,
                MobileNumber = raSubscriber.MobileNumber,
                SubscriberStatus = raSubscriber.SubscriberStatus,
                SubscriberUid = raSubscriber.SubscriberUid
            };

            if (raSubscriber.IdDocType == "1")
            {
                userBasicProfile.IdDocType = "National ID";
            }
            if (raSubscriber.IdDocType == "3")
            {
                userBasicProfile.IdDocType = "Passport";
            }
            try
            {
                var response = await GetSubscriberPhoto(raSubscriber.SelfieUri);
                if (null == response)
                {
                    _logger.LogError("Failed to get Subscriber Photo");
                    //return null;
                }
                userBasicProfile.Photo = response;
            }
            catch (Exception)
            {

            }

            return userBasicProfile;
        }
        public async Task<UserBasicProfile> GetUserBasicProfileAsync(
            string UserId, string UserIdType, ProfileConfig config)
        {
            var raSubscriber = new SubscriberView();

            switch (UserIdType)
            {
                case "EMAIL":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByEmail(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "PHONE_NUMBER":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByPhone(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "PASSPORT":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfobyDocType(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "CARD_NUMBER":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfobyDocType(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case "SUID":
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoBySUID(UserId);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                default:
                    {
                        _logger.LogError("Incorrect input received");
                        return null;
                    };
            }

            DateTime? dob = null;

            if (DateTime.TryParse(raSubscriber.DateOfBirth, out DateTime parsedDob))
            {
                dob = parsedDob;
            }

            var userBasicProfile = new UserBasicProfile()
            {
                CertificateStatus = raSubscriber.CertificateStatus,
                Country = raSubscriber.Country,
                DateOfBirth = dob, // ✅ safely assigned after parsing
                DisplayName = raSubscriber.DisplayName,
                Email = raSubscriber.Email,
                FcmToken = raSubscriber.FcmToken,
                Gender = raSubscriber.Gender,
                IdDocNumber = raSubscriber.IdDocNumber,
                //IdDocType = raSubscriber.IdDocType,
                Loa = raSubscriber.Loa,
                MobileNumber = raSubscriber.MobileNumber,
                SubscriberStatus = raSubscriber.SubscriberStatus,
                SubscriberUid = raSubscriber.SubscriberUid
            };

            if (raSubscriber.IdDocType == "1")
            {
                userBasicProfile.IdDocType = "National ID";
            }
            if (raSubscriber.IdDocType == "3")
            {
                userBasicProfile.IdDocType = "Passport";
            }
            try
            {
                var response = await GetSubscriberPhoto(raSubscriber.SelfieUri);
                //if (null == response)
                //{
                //    _logger.LogError("Failed to get Subscriber Photo");
                //    return null;
                //}
                userBasicProfile.Photo = response;
            }
            catch (Exception)
            {

            }

            var subscriberCard = await _unitOfWork.SubscriberCardDetail.GetSubscriberCard(raSubscriber.SubscriberUid);

            if (subscriberCard != null)
            {
                userBasicProfile.SubscriberCard = subscriberCard.CardDocumnet;
            }
            else
            {
                var _client = new HttpClient();

                var url = configuration.GetValue<string>("APIServiceLocations:Simulation") + $"api/update/Card/{userBasicProfile.SubscriberUid}";

                var content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(url,content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        userBasicProfile.SubscriberCard = apiResponse.Result.ToString();
                    }
                    else
                    {
                        userBasicProfile.SubscriberCard = null;
                    }
                }
                else
                {
                    userBasicProfile.SubscriberCard = null;
                }                
            }
            return userBasicProfile;
        }
        public async Task<GetUserProfileResponse> GetUserProfileDataAsync(
            GetUserProfileRequest request)
        {
            if (null == request || string.IsNullOrEmpty(request.UserId) ||
                string.IsNullOrEmpty(request.UserIdType))
            {
                _logger.LogError("Invalid Parameters Recieved");
                return new GetUserProfileResponse("Invalid Parameters");
            }
            HttpClient client = _httpClientFactory.CreateClient();
            ProfileConfig config = new ProfileConfig();
            UserBasicProfile userBasicProfile = new UserBasicProfile();
            UserMDLProfile userMDLProfile = new UserMDLProfile();
            UserHealthProfile userHealthProfile = new UserHealthProfile();
            UserSocialProfile userSocialProfile = new UserSocialProfile();

            TransactionProfileRequest transactionProfileRequest = new TransactionProfileRequest();
            TransactionProfileConsent transactionProfileConsent = new TransactionProfileConsent();
            TransactionProfileStatus transactionProfileStatus = new TransactionProfileStatus();

            TransactionProfileRequestResponse transactionProfileRequestResponse;
            TransactionProfileConsentResponse transactionProfileConsentResponse;
            TransactionProfileStatusResponse transactionProfileStatusResponse;

            int transactionid;

            var transactionId = Guid.NewGuid().ToString();

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize(request);


            transactionProfileRequest.TransactionId = transactionId;

            transactionProfileRequest.RequestDetails = jsonRequest;

            transactionProfileRequest.CreatedDate = DateTime.Now;

            transactionProfileStatus.CreatedDate = DateTime.Now;

            var clientinDb = await _clientService.GetClientByClientIdAsync(request.ClientId);
            if (null == clientinDb)
            {
                transactionProfileRequestResponse = await _transactionProfileRequestService.AddProfileRequest(transactionProfileRequest);

                transactionid = await _transactionProfileRequestService.GetIdByTransactionId(transactionId);
                if (transactionid == -1)
                {
                    return new GetUserProfileResponse("Failed to get transaction details");
                }
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Client not found", null, -1);
                return new GetUserProfileResponse("User not found");
            }

            transactionProfileRequest.ClientId = clientinDb.Id;

            userBasicProfile = await GetUserBasicProfileAsync(request.UserId,
                    request.UserIdType, config);

            if (null == userBasicProfile)
            {
                transactionProfileRequestResponse = await _transactionProfileRequestService.AddProfileRequest(transactionProfileRequest);
                transactionid = await _transactionProfileRequestService.GetIdByTransactionId(transactionId);
                if (transactionid == -1)
                {
                    return new GetUserProfileResponse("Failed to get transaction details");
                }
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "user not found", null, -1);
                _logger.LogError("GetUserBasicProfileAsync Failed");
                return new GetUserProfileResponse("User not found");
            }
            transactionProfileRequest.Suid = userBasicProfile.SubscriberUid;

            var response = await _transactionProfileRequestService.AddProfileRequest(transactionProfileRequest);

            if (response == null || !response.Success)
            {
                return new GetUserProfileResponse(response.Message);
            }

            transactionid = await _transactionProfileRequestService.GetIdByTransactionId(transactionId);
            if (transactionid == -1)
            {
                return new GetUserProfileResponse("Failed to get transaction details");
            }

            transactionProfileStatus.TransactionId = transactionid;
            transactionProfileStatus.CreatedDate = DateTime.Now;
            if (request.ProfileType == null ||
                request.ProfileType == "BASIC" ||
                request.ProfileType == "HEALTH" ||
                request.ProfileType == "MDL" ||
                request.ProfileType == "CREDITSCORE" ||
                request.ProfileType == "SOCIALPROFILE")
            {
                config.UserStatusUrl = configuration["Profile:Basic:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Basic:ProfileServiceUrl"];

                //var daesClaims1 = new GetUserBasicProfileResult();
                //daesClaims1.suid = "Test";
                //    daesClaims1.name = null;
                //return new GetUserProfileResponse(daesClaims1);

                userBasicProfile = await GetUserBasicProfileAsync(request.UserId,
                    request.UserIdType, config);
                if (null == userBasicProfile)
                {

                    transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "GetUserBasicProfileAsync Failed", null, -1);
                    _logger.LogError("GetUserBasicProfileAsync Failed");
                    return new GetUserProfileResponse("User not found");
                }
            }
            else
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Invalid Profile Type", null, -1);
                _logger.LogError("Invalid Profile Type");
                return new GetUserProfileResponse("Invalid Profile Type");
            }
            if (request.ProfileType == "BASIC")
            {
                transactionProfileStatus.DatapivotId = 5;
            }

            if (request.ProfileType == "HEALTH")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];

                userHealthProfile.blood_group = "A+";
                userHealthProfile.height = 182;
                userHealthProfile.weight = 80;
                userHealthProfile.bmi = 24.2;

            }

            if (userBasicProfile.SubscriberStatus != StatusConstants.ACTIVE)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "User Account is not Active", null, -1);
                _logger.LogInformation("User account is not Active");
                return new GetUserProfileResponse("User Account is not Active");
            }

            var Purpose = request.Purpose;

            var PurposeConsentRequired = true;

            var PurposeinDb = await _clientsPurposeService.GetPurposeByClientId(request.ClientId);

            var purposesListinDb = await _purposeService.GetPurposeListAsync();

            if (null == purposesListinDb)
            {
                transactionProfileStatus.UpdatedDate = DateTime.Now;
                transactionProfileStatus.TransactionStatus = "FAILED";
                transactionProfileStatus.FailedReason = "Failed to get purpose list";
                transactionProfileStatus.DatapivotId = -1;
                transactionProfileStatusResponse = await _transactionProfileStatusService.AddProfileStatus(transactionProfileStatus);
                if (transactionProfileStatusResponse == null || !transactionProfileStatusResponse.Success)
                {
                    _logger.LogError("Adding transaction profile response failed");
                }
                return new GetUserProfileResponse("Failed to get the purposes list");
            }
            if (PurposeConsentRequired)
            {
                if (!string.IsNullOrEmpty(Purpose))
                {
                    if (!ValidatePurposes(Purpose, PurposeinDb))
                    {
                        transactionProfileStatus.UpdatedDate = DateTime.Now;
                        transactionProfileStatus.TransactionStatus = "FAILED";
                        transactionProfileStatus.FailedReason = "request purposes not matched with client purposes";
                        transactionProfileStatus.DatapivotId = -1;
                        transactionProfileStatusResponse = await _transactionProfileStatusService.AddProfileStatus(transactionProfileStatus);
                        if (transactionProfileStatusResponse == null || !transactionProfileStatusResponse.Success)
                        {
                            _logger.LogError("Adding transaction profile response failed");
                        }
                        return new GetUserProfileResponse("Invalid Request");
                    }
                    PurposeConsentRequired = CheckPurposes(request.Purpose, purposesListinDb);
                }
            }

            var tempAuthNSessId = string.Empty;

            try
            {
                // Generate sessionid
                tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            }
            catch (Exception error)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Get Unique Key failed", null, -1);
                _logger.LogError("GetUniqueKey failed: {0}", error.Message);
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            var clientInDb = await _clientService.GetClientByClientIdAsync(
                request.ClientId);
            if (null == clientInDb)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Failed to get client Details", null, -1);
                _logger.LogError(OIDCConstants.ClientNotFound);
                return new GetUserProfileResponse(OIDCConstants.ClientNotFound);
            }

            // Validate client scopes
            var isTrue = ValidateScopes(request.Scopes, clientInDb.Scopes);
            if (false == isTrue)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Scopes not matched", null, -1);
                _logger.LogError(OIDCConstants.ClientScopesNotMatched);
                return new GetUserProfileResponse(OIDCConstants.ClientScopesNotMatched);
            }

            // Requested Scopes
            var Scopes = request.Scopes.Split(new char[] { ' ', '\t' });
            if (Scopes.Contains("kycprofile"))
            {
                var kycurl = configuration["KycUrl"];
                kycurl += userBasicProfile.IdDocNumber;
                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(kycurl);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {kycurl} failed with status code {result.StatusCode}");
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Failed to connect Datapivot", "", -1);
                        return new GetUserProfileResponse("Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Failed to connect to datapivot", "", -1);
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<KycResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.success)
                {
                    return new GetUserProfileResponse(apiResponse.message);
                }
                userBasicProfile.KycDocument = apiResponse.result;
            }
            if (Scopes.Contains("ekycprofile"))
            {
                var kycurl = configuration["EKycUrl"];

                kycurl += userBasicProfile.IdDocNumber;

                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(kycurl);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {kycurl} failed with status code {result.StatusCode}");
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Failed to connect Datapivot", "", -1);
                        return new GetUserProfileResponse("Internal error");
                    }
                }

                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Failed to connect to datapivot", "", -1);
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<KycResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.success)
                {
                    return new GetUserProfileResponse(apiResponse.message);
                }
                userBasicProfile.EKycDocument = apiResponse.result;
            }
            // Get all scopes from Db
            var scopesinDB = await _unitOfWork.Scopes.ListAllScopeAsync();
            if (null == scopesinDB)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Failed to get Scopes from DB", null, -1);
                _logger.LogError("ListAllScopeAsync failed");
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            // Get all Claims from Database
            var claimsinDb = await _unitOfWork.UserClaims.
                ListAllUserClaimAsync();
            if (null == claimsinDb)
            {
                transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Failed to get Claims from DB", null, -1);
                _logger.LogError("ListAllUserClaimAsync failed");
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            //var deselectScopesAndClaims = (bool)clientInDb.DeselectScopesClaims;
            var deselectScopesAndClaims = true;
            var consentScopes = new List<ScopeInfo>() { };

            var daesClaim = new GetUserBasicProfileResult();

            var userApprovedClaims = new List<string>() { };

            var scopeConsentRequired = false;

            HashSet<string> requestedProfileAttributes = new HashSet<string>();

            if (!PurposeConsentRequired)
            {
                userApprovedClaims = GetClaimsList(Scopes, scopesinDB);
            }
            else
            {
                // Get all requested scopes details
                foreach (var reqScope in Scopes)
                {
                    foreach (var scopeinDb in scopesinDB)
                    {
                        if ((reqScope == scopeinDb.Name) &&
                            (scopeinDb.UserConsent == true))
                        {
                            scopeConsentRequired = true;
                            ScopeInfo scopeInfo = new ScopeInfo();
                            scopeInfo.Name = scopeinDb.Name;
                            scopeInfo.DisplayName = scopeinDb.DisplayName;
                            scopeInfo.Description = scopeinDb.Description;

                            if (deselectScopesAndClaims)
                                scopeInfo.Mandatory = scopeinDb.DefaultScope;
                            else
                                scopeInfo.Mandatory = true;

                            if (scopeinDb.IsClaimsPresent)
                            {
                                scopeInfo.ClaimsInfo = new List<ClaimInfo>() { };
                                // Parse space seperated claims into list
                                var claims = scopeinDb.ClaimsList.Split(
                                    new char[] { ' ', '\t' });

                                foreach (var claim in claims)
                                {
                                    foreach (var claiminDb in claimsinDb)
                                    {
                                        if ((claim == claiminDb.Name) &&
                                            (claiminDb.UserConsent == true))
                                        {
                                            scopeConsentRequired = true;
                                            ClaimInfo claimInfo = new ClaimInfo();
                                            claimInfo.Name = claiminDb.Name;
                                            claimInfo.DisplayName = claiminDb.DisplayName;
                                            claimInfo.Description = claiminDb.Description;

                                            if (deselectScopesAndClaims)
                                                claimInfo.Mandatory = claiminDb.DefaultClaim;
                                            else
                                                claimInfo.Mandatory = true;

                                            scopeInfo.ClaimsInfo.Add(claimInfo);
                                            requestedProfileAttributes.Add(claiminDb.DisplayName);
                                        }
                                    }
                                }
                            }
                            scopeInfo.ClaimsPresent = scopeinDb.IsClaimsPresent;
                            consentScopes.Add(scopeInfo);
                        }
                    }
                }
                if (scopeConsentRequired)
                {
                    Clientdetails clientdetails = new Clientdetails();
                    clientdetails.AppName = clientInDb.ApplicationName;
                    clientdetails.ClientId = clientInDb.ClientId;

                    var authScheme = configuration["Consent_Authentication"];

                    // Prepare temporary session object
                    TemporarySession temporarySession = new TemporarySession
                    {
                        TemporarySessionId = tempAuthNSessId,
                        UserId = userBasicProfile.SubscriberUid,
                        DisplayName = userBasicProfile.DisplayName,
                        PrimaryAuthNSchemeList = new List<string> { authScheme },
                        AuthNSuccessList = new List<string>(),
                        Clientdetails = clientdetails,
                        IpAddress = string.Empty,
                        UserAgentDetails = string.Empty,
                        TypeOfDevice = string.Empty,
                        MacAddress = DTInternalConstants.NOT_AVAILABLE,
                        withPkce = false,
                        AdditionalValue = DTInternalConstants.pending,
                        AllAuthNDone = false,
                        //TransactionId=transactionId,
                        AuthNStartTime = DateTime.Now.ToString("s"),
                        CoRelationId = Guid.NewGuid().ToString()
                    };

                    // Create temporary session
                    var task = await _cacheClient.Add(CacheNames.TemporarySession,
                        tempAuthNSessId, temporarySession);
                    if (DTInternalConstants.S_OK != task.retValue)
                    {
                        _logger.LogError("_cacheClient.Add failed");
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Failed to Add Record in DB", null, -1);
                        return new GetUserProfileResponse(OIDCConstants.InternalError);
                    }

                    transactionProfileConsent.ConsentStatus = "PENDING";

                    transactionProfileConsent.CreatedDate = DateTime.Now;

                    transactionProfileConsent.RequestedProfileAttributes = string.Join(",", requestedProfileAttributes);

                    transactionProfileConsent.TransactionId = transactionid;

                    var Consentresult = await _transactionProfileConsentService.AddProfileConsent(transactionProfileConsent);

                    if (null == Consentresult || !Consentresult.Success)
                    {
                        return new GetUserProfileResponse("Failed to add Consent Status");
                    }
                    // Send notification to mobile
                    var eConsentNotification = new EConsentNotification()
                    {
                        AuthnScheme = authScheme,
                        AuthnToken = tempAuthNSessId,
                        RegistrationToken = userBasicProfile.FcmToken,
                        ApplicationName = clientInDb.ApplicationName,
                        ConsentScopes = consentScopes,
                        DeselectScopesAndClaims = deselectScopesAndClaims
                    };

                    try
                    {
                        var result = await _pushNotificationClient.SendEConsentNotification(
                            eConsentNotification);
                        if (null == result)
                        {
                            transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Failed to send Notification", null, -1);
                            _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                                " failed");
                            return new GetUserProfileResponse("Failed to send Notification");
                        }
                    }
                    catch (Exception error)
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Error sending Notification", null, -1);
                        _logger.LogError("_pushNotificationClient." +
                            "SendAuthnNotification " +
                            "failed : {0}", error.Message);
                        return new GetUserProfileResponse("Failed to send Notification");
                    }

                    DateTime date1 = DateTime.Now;
                    DateTime date2 = DateTime.Now.AddMinutes(1);
                    bool isError = false;
                    string message = string.Empty;
                    TemporarySession tempSession = null;

                    while (0 > DateTime.Compare(date1, date2))
                    {
                        // Check whether the temporary session exists
                        var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                            tempAuthNSessId);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("Temporary Session Expired/Not Found");
                            message = OIDCConstants.InternalError;
                            isError = true;
                            break;
                        }

                        // Get the temporary session object
                        tempSession = await _cacheClient.Get<TemporarySession>
                            (CacheNames.TemporarySession,
                            tempAuthNSessId);
                        if (null == tempSession)
                        {
                            _logger.LogError("Get Temporary Session Failed,Expired/Not Found");
                            message = OIDCConstants.InternalError;
                            isError = true;
                            break;
                        }

                        if (!tempSession.AdditionalValue.Equals(DTInternalConstants.pending))
                            break;

                        // Wait 500 Milli Seconds(Half Second) without blocking
                        await Task.Delay(500);
                        date1 = DateTime.Now;
                    }

                    if (isError)
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", message, null, -1);
                        return new GetUserProfileResponse(message);
                    }
                    if (tempSession.AdditionalValue.Equals(DTInternalConstants.pending))
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Request TimeOut", null, -1);
                        //transactionProfileConsentResponse = await UpdateTransactionProfileConsent(transactionid, "", "PENDING");
                        return new GetUserProfileResponse("Request TimeOut");
                    }
                    if (tempSession.AdditionalValue.Equals(DTInternalConstants.FailedStatus))
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "NA", null, -1);
                        //transactionProfileConsentResponse = await UpdateTransactionProfileConsent(transactionid, "", "Request Iimeout");

                        return new GetUserProfileResponse("Request TimeOut");
                    }
                    if (tempSession.AdditionalValue.Equals(DTInternalConstants.deny))
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Consent Denied", null, -1);
                        //transactionProfileConsentResponse = await UpdateTransactionProfileConsent(transactionid, "", "Consent Denied");
                        return new GetUserProfileResponse("Access Denied");
                    }
                    if (tempSession.AdditionalValue.Equals("Wrong Pin"))
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Wrong Pin", null, -1);
                        //transactionProfileConsentResponse = await UpdateTransactionProfileConsent(transactionid, "", "Wrong pin");
                        return new GetUserProfileResponse("Wrong pin");
                    }
                    if (tempSession.AdditionalValue.Equals("Subscriber Denied Authentication"))
                    {
                        transactionProfileStatusResponse = await AddTransactionProfileStatus(transactionid, "FAILED", "Subscriber Denied Authentication", null, -1);

                        //transactionProfileConsentResponse = await UpdateTransactionProfileConsent(transactionid, "", "Subscriber Denied Authentication");
                        return new GetUserProfileResponse("Access Denied");
                    }
                    // Get all requested scopes details
                    foreach (var reqScope in Scopes)
                    {
                        foreach (var approvedScope in tempSession.allowedScopesAndClaims)
                        {
                            if ((reqScope == approvedScope.name) &&
                                approvedScope.claimsPresent)
                            {
                                userApprovedClaims.AddRange(approvedScope.claims);
                            }
                        }
                    }
                    HashSet<string> ApprovedProfileSet = new HashSet<string>(userApprovedClaims);
                    HashSet<string> ApprovedClaims = new HashSet<string>();
                    foreach (var item in consentScopes)
                    {
                        foreach (var claims in item.ClaimsInfo)
                        {
                            if (userApprovedClaims.Contains(claims.Name))
                            {
                                ApprovedClaims.Add(claims.DisplayName);
                            }
                        }
                    }
                    transactionProfileConsent.ApprovedProfileAttributes = string.Join(",", ApprovedClaims);
                    transactionProfileConsent.UpdatedDate = DateTime.Now;
                    transactionProfileConsent.ConsentStatus = "APPROVED";
                    transactionProfileConsentResponse = await _transactionProfileConsentService.UpdateTransactionProfileConsent(transactionProfileConsent);
                }
            }

            var daesClaims = new GetUserBasicProfileResult();
            if (userApprovedClaims.Contains(Claims.SUID))
                daesClaims.suid = userBasicProfile.SubscriberUid;
            if (userApprovedClaims.Contains(Claims.Name))
                daesClaims.name = userBasicProfile.DisplayName;
            if (userApprovedClaims.Contains(Claims.BirthDate))
                daesClaims.birthdate = userBasicProfile.DateOfBirth.ToString();
            if (userApprovedClaims.Contains(Claims.Gender))
                daesClaims.gender = userBasicProfile.Gender;

            if (userApprovedClaims.Contains(Claims.Email))
                daesClaims.email = userBasicProfile.Email;
            if (userApprovedClaims.Contains(Claims.PhoneNumber))
                daesClaims.phone_number = userBasicProfile.MobileNumber;
            if (userApprovedClaims.Contains(Claims.LOA))
                daesClaims.loa = userBasicProfile.Loa;
            if (userApprovedClaims.Contains(Claims.IdDocumentNumber))
                daesClaims.id_document_number = userBasicProfile.IdDocNumber;
            if (userApprovedClaims.Contains(Claims.IdDocumentType))
                daesClaims.id_document_type = userBasicProfile.IdDocType;
            if (userApprovedClaims.Contains(Claims.Country))
                daesClaims.country = userBasicProfile.Country;
            if (userApprovedClaims.Contains(Claims.Email_Verified))
                daesClaims.email_verified = true;
            if (userApprovedClaims.Contains(Claims.PhoneNumber_Verified))
                daesClaims.phone_number_verified = true;
            if (userApprovedClaims.Contains("photo"))
                daesClaims.photo = userBasicProfile.Photo;
            if (userApprovedClaims.Contains("ekycdocument"))
                daesClaims.Ekyc_document = userBasicProfile.EKycDocument;
            if (userApprovedClaims.Contains("kycdocument"))
                daesClaims.kyc_document = userBasicProfile.KycDocument;

            if (userApprovedClaims.Contains("issue_date"))
                daesClaims.issue_date = userMDLProfile.issue_date;
            if (userApprovedClaims.Contains("expiry_date"))
                daesClaims.expiry_date = userMDLProfile.expiry_date;
            if (userApprovedClaims.Contains("issuing_country"))
                daesClaims.issuing_country = userMDLProfile.issuing_country;
            if (userApprovedClaims.Contains("issuing_authority"))
                daesClaims.issuing_authority = userMDLProfile.issuing_authority;
            if (userApprovedClaims.Contains("document_number"))
                daesClaims.document_number = userMDLProfile.document_number;

            if (userApprovedClaims.Contains("blood_group"))
                daesClaims.blood_group = userHealthProfile.blood_group;
            if (userApprovedClaims.Contains("height"))
                daesClaims.height = userHealthProfile.height;
            if (userApprovedClaims.Contains("weight"))
                daesClaims.weight = userHealthProfile.weight;
            if (userApprovedClaims.Contains("bmi"))
                daesClaims.bmi = userHealthProfile.bmi;
            if (request.ProfileType == "SOCIALPROFILE")
            {
                if (userApprovedClaims.Contains("address"))
                    daesClaims.address = userSocialProfile.address;
                if (userApprovedClaims.Contains("age"))
                    daesClaims.age = userSocialProfile.age;
                if (userApprovedClaims.Contains("birthdate"))
                    daesClaims.birthdate = userSocialProfile.birthdate;
                if (userApprovedClaims.Contains("email"))
                    daesClaims.email = userSocialProfile.email;
                if (userApprovedClaims.Contains("income"))
                    daesClaims.income = userSocialProfile.income;
                if (userApprovedClaims.Contains("x_cst_indv_Doc_type"))
                    daesClaims.x_cst_indv_Doc_type = userSocialProfile.x_cst_indv_Doc_type;
                if (userApprovedClaims.Contains("x_cst_indv_Doc_value"))
                    daesClaims.x_cst_indv_Doc_value = userSocialProfile.x_cst_indv_Doc_value;
                if (userApprovedClaims.Contains("x_cst_indv_children"))
                    daesClaims.x_cst_indv_children = userSocialProfile.x_cst_indv_children;
                if (userApprovedClaims.Contains("x_cst_indv_suid"))
                    daesClaims.x_cst_indv_suid = userSocialProfile.x_cst_indv_suid;
                if (userApprovedClaims.Contains("name"))
                    daesClaims.name = userSocialProfile.name;
                if (userApprovedClaims.Contains("phone_sanitized"))
                    daesClaims.phone_number = userSocialProfile.phone_sanitized;
                if (userApprovedClaims.Contains("occupation"))
                    daesClaims.occupation = userSocialProfile.occupation;
                if (userApprovedClaims.Contains("programs"))
                    daesClaims.programs = userSocialProfile.Programs;
                if (userApprovedClaims.Contains(Claims.SUID))
                {
                    daesClaims.suid = userSocialProfile.x_cst_indv_suid;
                }
            }

            /*if (userApprovedClaims.Contains("Creditscore"))
                daesClaims.Creditscore = creditScoreDetail.Creditscore;
            if (userApprovedClaims.Contains("JobTitle"))
                daesClaims.JobTitle = creditScoreDetail.JobTitle;
            if (userApprovedClaims.Contains("IncomeLevel"))
                daesClaims.IncomeLevel = creditScoreDetail.IncomeLevel;
            if (userApprovedClaims.Contains(Claims.Email))
                daesClaims.email = creditScoreDetail.Email;
            if (userApprovedClaims.Contains(Claims.PhoneNumber))
                daesClaims.phone_number = creditScoreDetail.PhoneNo;
            if (userApprovedClaims.Contains(Claims.Name))
                daesClaims.name = creditScoreDetail.Name;*/

            transactionProfileStatus.UpdatedDate = DateTime.Now;
            transactionProfileStatus.TransactionStatus = "SUCCESS";
            var transactionProfileResponse = await _transactionProfileStatusService.AddProfileStatus(transactionProfileStatus);
            if (transactionProfileResponse == null || !transactionProfileResponse.Success)
            {
                _logger.LogError("Adding transaction profile response failed");
            }
            _logger.LogDebug("<--GetUserProfileDataAsync");
            return new GetUserProfileResponse(daesClaims);
        }

        public async Task<GetUserProfileResponse> GetUserProfileDataAsync1(
           GetUserProfileRequest request)
        {
            if (null == request || string.IsNullOrEmpty(request.UserId) ||
                string.IsNullOrEmpty(request.UserIdType))
            {
                _logger.LogError("Invalid Parameters Recieved");
                return new GetUserProfileResponse("Invalid Parameters");
            }

            ProfileConfig config = new ProfileConfig();
            UserBasicProfile userBasicProfile = new UserBasicProfile();
            UserMDLProfile userMDLProfile = new UserMDLProfile();
            UserHealthProfile userHealthProfile = new UserHealthProfile();

            if (request.ProfileType == null ||
                request.ProfileType == "BASIC" ||
                request.ProfileType == "HEALTH" ||
                request.ProfileType == "MDL")
            {
                config.UserStatusUrl = configuration["Profile:Basic:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Basic:ProfileServiceUrl"];

                //var daesClaims1 = new GetUserBasicProfileResult();
                //daesClaims1.suid = "Test";
                //    daesClaims1.name = null;
                //return new GetUserProfileResponse(daesClaims1);

                userBasicProfile = await GetUserBasicProfileAsync(request.UserId,
                    request.UserIdType, config);
                if (null == userBasicProfile)
                {
                    _logger.LogError("GetUserBasicProfileAsync Failed");
                    return new GetUserProfileResponse("User not found");
                }
            }
            else
            {
                _logger.LogError("Invalid Profile Type");
                return new GetUserProfileResponse("Invalid Profile Type");
            }

            if (request.ProfileType == "MDL")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];

                userMDLProfile.document_number = "KL0520140009308";
                userMDLProfile.issue_date = "2021-04-19T22:00:00Z";
                userMDLProfile.issuing_country = "IN";
                userMDLProfile.issuing_authority = "IN-TG";
                userMDLProfile.expiry_date = "2024-04-19T22:00:00Z";
            }
            if (request.ProfileType == "HEALTH")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];

                userHealthProfile.blood_group = "A+";
                userHealthProfile.height = 182;
                userHealthProfile.weight = 80;
                userHealthProfile.bmi = 24.2;

            }

            if (userBasicProfile.SubscriberStatus != StatusConstants.ACTIVE)
            {
                _logger.LogInformation("User account is not Active");
                return new GetUserProfileResponse("User Account is not Active");
            }


            var clientInDb = await _clientService.GetClientByClientIdAsync(
                request.ClientId);
            if (null == clientInDb)
            {
                _logger.LogError(OIDCConstants.ClientNotFound);
                return new GetUserProfileResponse(OIDCConstants.ClientNotFound);
            }

            // Validate client scopes
            var isTrue = ValidateScopes(request.Scopes, clientInDb.Scopes);
            if (false == isTrue)
            {
                _logger.LogError(OIDCConstants.ClientScopesNotMatched);
                return new GetUserProfileResponse(OIDCConstants.ClientScopesNotMatched);
            }


            var daesClaims = new GetUserBasicProfileResult();

            daesClaims.suid = userBasicProfile.SubscriberUid;
            daesClaims.name = userBasicProfile.DisplayName;
            daesClaims.birthdate = userBasicProfile.DateOfBirth.ToString();

            daesClaims.gender = userBasicProfile.Gender;

            daesClaims.email = userBasicProfile.Email;
            daesClaims.phone_number = userBasicProfile.MobileNumber;
            daesClaims.loa = userBasicProfile.Loa;
            daesClaims.id_document_number = userBasicProfile.IdDocNumber;
            daesClaims.id_document_type = userBasicProfile.IdDocType;
            daesClaims.country = userBasicProfile.Country;
            daesClaims.email_verified = true;
            daesClaims.phone_number_verified = true;
            daesClaims.photo = userBasicProfile.Photo;

            if (!string.IsNullOrEmpty(userMDLProfile.issue_date))
                daesClaims.issue_date = userMDLProfile.issue_date;

            if (!string.IsNullOrEmpty(userMDLProfile.expiry_date))
                daesClaims.expiry_date = userMDLProfile.expiry_date;
            if (!string.IsNullOrEmpty(userMDLProfile.issuing_country))
                daesClaims.issuing_country = userMDLProfile.issuing_country;
            if (!string.IsNullOrEmpty(userMDLProfile.issuing_authority))
                daesClaims.issuing_authority = userMDLProfile.issuing_authority;
            if (!string.IsNullOrEmpty(userMDLProfile.document_number))
                daesClaims.document_number = userMDLProfile.document_number;

            if (!string.IsNullOrEmpty(userHealthProfile.blood_group))
                daesClaims.blood_group = userHealthProfile.blood_group;
            if (userHealthProfile.height != 0)
                daesClaims.height = userHealthProfile.height;
            if (userHealthProfile.weight != 0)
                daesClaims.weight = userHealthProfile.weight;
            if (userHealthProfile.bmi != 0.0)
                daesClaims.bmi = userHealthProfile.bmi;

            _logger.LogDebug("<--GetUserProfileDataAsync");
            return new GetUserProfileResponse(daesClaims);
        }

        public async Task<GetWalletProfileResponse> GetWalletSubscriberProfile(GetWalletSubscriberProfileRequest request)
        {
            UserBasicProfile userBasicProfile = new UserBasicProfile();
            ProfileConfig config = new ProfileConfig();
            var transactionUid = Guid.NewGuid().ToString();

            userBasicProfile = await GetUserBasicProfileAsync(request.Suid, "SUID", config);
            if (userBasicProfile == null)
            {
                return new GetWalletProfileResponse("User not found");
            }
            if (userBasicProfile != null)
            {
                request.UserId = userBasicProfile.Email;
            }

            Accesstoken accessToken = null;
            try
            {
                accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                    request.AccessToken);
                if (null == accessToken)
                {
                    _logger.LogError("Access token not recieved from cache." +
                        "Expired or Invalid access token");
                    return new GetWalletProfileResponse("Internal Error");
                }
            }
            catch (CacheException)
            {
                return null;
            }
            var clientinDb = await _clientService.GetClientByClientIdAsync(accessToken.ClientId);

            if (null == clientinDb)
            {
                return new GetWalletProfileResponse("Client not found");
            }

            var dataPivot = await _dataPivotService.GetPivotByUidAsync(request.DataPivotUid);

            if (null == dataPivot)
            {
                return new GetWalletProfileResponse("DataPivot not found");
            }

            var scope = await _scopeService.GetScopeAsync(dataPivot.ScopeId);
            if (null == scope)
            {
                return new GetWalletProfileResponse("Scope not found");
            }

            request.ProfileType = scope.DisplayName;

            var transactionid = await _transactionProfileRequestService.GetIdByTransactionId(transactionUid);
            //var Scopes = accessToken.Scopes.Split(new char[] { ' ', '\t' });
            var Scopes = scope.Name.Split(new char[] { ' ', '\t' });
            // Get all scopes from Db
            var scopesinDB = await _unitOfWork.Scopes.ListAllScopeAsync();

            var claimsinDb = await _unitOfWork.UserClaims.
               ListAllUserClaimAsync();
            string tempAuthNSessId;

            try
            {
                // Generate sessionid
                tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            }
            catch (Exception)
            {
                return new GetWalletProfileResponse(OIDCConstants.InternalError);
            }

            var deselectScopesAndClaims = true;
            var consentScopes = new List<ScopeInfo>() { };


            var userApprovedClaims = new List<string>() { };

            HashSet<string> requestedProfileAttributes = new HashSet<string>();

            foreach (var reqScope in Scopes)
            {
                foreach (var scopeinDb in scopesinDB)
                {
                    if ((reqScope == scopeinDb.Name) &&
                        (scopeinDb.UserConsent == true))
                    {
                        //scopeConsentRequired = true;
                        ScopeInfo scopeInfo = new ScopeInfo();
                        scopeInfo.Name = scopeinDb.Name;
                        scopeInfo.DisplayName = scopeinDb.DisplayName;
                        scopeInfo.Description = scopeinDb.Description;

                        if (deselectScopesAndClaims)
                            scopeInfo.Mandatory = scopeinDb.DefaultScope;
                        else
                            scopeInfo.Mandatory = true;

                        if (scopeinDb.IsClaimsPresent)
                        {
                            scopeInfo.ClaimsInfo = new List<ClaimInfo>() { };
                            // Parse space seperated claims into list
                            var claims = scopeinDb.ClaimsList.Split(
                                new char[] { ' ', '\t' });

                            foreach (var claim in claims)
                            {
                                foreach (var claiminDb in claimsinDb)
                                {
                                    if ((claim == claiminDb.Name) &&
                                        (claiminDb.UserConsent == true))
                                    {
                                        //scopeConsentRequired = true;
                                        ClaimInfo claimInfo = new ClaimInfo();
                                        claimInfo.Name = claiminDb.Name;
                                        claimInfo.DisplayName = claiminDb.DisplayName;
                                        claimInfo.Description = claiminDb.Description;
                                        claimInfo.Mandatory = true;

                                        scopeInfo.ClaimsInfo.Add(claimInfo);
                                        requestedProfileAttributes.Add(claiminDb.DisplayName);
                                    }
                                }
                            }
                        }
                        scopeInfo.ClaimsPresent = scopeinDb.IsClaimsPresent;
                        consentScopes.Add(scopeInfo);
                    }
                }
            }

            var transactionProfileConsent = new TransactionProfileConsent();
            transactionProfileConsent.ConsentStatus = "PENDING";

            transactionProfileConsent.CreatedDate = DateTime.Now;

            transactionProfileConsent.RequestedProfileAttributes = string.Join(",", requestedProfileAttributes);

            transactionProfileConsent.TransactionId = transactionid;

            var result1 = new GetWalletProfileResult();

            result1.ApplicationName = clientinDb.ApplicationName;
            result1.AuthnScheme = dataPivot.AuthScheme;
            result1.DeselectScopesAndClaims = false;
            result1.ConsentScopes = consentScopes;
            result1.TransactionId = transactionUid;
            return new GetWalletProfileResponse(result1);
        }

        public async Task<GetUserProfileResponse> GetUserProfileDataAsync2(
            GetUserDataRequest request)
        {
            var transactionUid = Guid.NewGuid().ToString();

            if (null == request || string.IsNullOrEmpty(request.Token) ||
                string.IsNullOrEmpty(request.Suid))
            {
                _logger.LogError("Invalid Parameters Recieved");
                return new GetUserProfileResponse("Invalid Parameters");
            }

            Accesstoken accessToken = null;

            try
            {
                accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                    request.Token);
                if (null == accessToken)
                {
                    _logger.LogError("Access token not recieved from cache." +
                        "Expired or Invalid access token");
                    return new GetUserProfileResponse("UnAuthorized");
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Access Token Record");
                ErrorResponseDTO error = new ErrorResponseDTO();
                error.error = OIDCConstants.InternalError;
                error.error_description = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                return new GetUserProfileResponse("Internal Error" + error.error_description);
            }

            HttpClient client = _httpClientFactory.CreateClient();

            List<string> Scopes = new List<string>();

            ProfileConfig config = new ProfileConfig();

            UserBasicProfile userBasicProfile = new UserBasicProfile();

            UserMDLProfile userMDLProfile = new UserMDLProfile();

            UserHealthProfile userHealthProfile = new UserHealthProfile();

            UserSocialProfile userSocialProfile = new UserSocialProfile();

            VisaProfile visaProfile = new VisaProfile();

            TradeLicenceProfile tradeLicence = new TradeLicenceProfile();

            BusinessEstablishmentProfile businessProfile = new BusinessEstablishmentProfile();

            var clientinDb = await _clientService.GetClientByClientIdAsync(accessToken.ClientId);

            if (null == clientinDb)
            {
                return new GetUserProfileResponse("Client not found");
            }

            userBasicProfile = await GetUserBasicProfileAsync(request.Suid,
                   "SUID", config);

            if (null == userBasicProfile)
            {
                return new GetUserProfileResponse("User not found");
            }

            request.UserId = userBasicProfile.Email;

            var profile = request.ProfileType;

            if (request.ProfileType == "pid")
            {
                request.ProfileType = "Basic profile information";
            }

            if (request.ProfileType == "mDL")
            {
                request.ProfileType = "MDL Profile";
            }

            if (request.ProfileType == "SocialBenefitCard")
            {
                request.ProfileType = "Social Profile";
            }

            if (request.ProfileType == "ResidentProfile")
            {
                if (userBasicProfile.cardStatus == null)
                {
                    return new GetUserProfileResponse("Card Not Approved");
                }
                if (userBasicProfile.cardStatus == null || userBasicProfile.cardStatus != "APPROVED")
                {
                    return new GetUserProfileResponse("Card Not Approved");
                }
            }
            //var transactionid = await _transactionProfileRequestService.GetIdByTransactionId(transactionUid);

            request.ProfileType = profile;
            if (request.ProfileType == null ||
                request.ProfileType == "pid" ||
                request.ProfileType == "HEALTH" ||
                request.ProfileType == "mDL" ||
                request.ProfileType == "CREDITSCORE" ||
                request.ProfileType == "SocialBenefitCard" ||
                request.ProfileType == "Visa" ||
                request.ProfileType == "NonResidentProfile" ||
                request.ProfileType == "BusinessEstablishment" ||
                request.ProfileType == "TradeLicense" ||
                request.ProfileType == "ResidentProfile" ||
                request.ProfileType == "AuthenticationProfile" ||
                request.ProfileType == "VisitorProfile" ||
                request.ProfileType == "HealthInsuranceProfile")
            {

            }
            else
            {
                _logger.LogError("Invalid Profile Type");
                return new GetUserProfileResponse("Invalid Profile Type");
            }

            /*if (request.ProfileType == "mDL")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];
                var randomNumber = GenerateRandomMDLID();
                userMDLProfile.document_number = randomNumber;
                userMDLProfile.issue_date = "19/04/2021";
                userMDLProfile.issuing_country = userBasicProfile.Country;
                userMDLProfile.issuing_authority = userBasicProfile.Country + "-TG";
                userMDLProfile.expiry_date = "19/04/2031";
                userMDLProfile.international_permit = "Issued";

                string displayName = userBasicProfile.DisplayName;

                string surname = displayName.Split(' ')[0];

                string otherName = "";
                try
                {
                    otherName = displayName.Substring(displayName.IndexOf(' ') + 1);
                }
                catch (Exception e)
                {

                }
                var result = await GetMdlDocument(surname, otherName, userBasicProfile.IdDocNumber, userBasicProfile.Gender, "B+", userBasicProfile.Country, userBasicProfile.Photo, userBasicProfile.DateOfBirth, randomNumber, userMDLProfile.expiry_date, userMDLProfile.issue_date);

                if (result != null)
                {
                    string doc = Convert.ToBase64String(result);
                    userMDLProfile.mdl_document = doc;
                }
                datapivotId = 3;
            }*/

            if (request.ProfileType == "mDL")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];
                var document = configuration["Siva_Document"];

                var randomNumber = GenerateRandomMDLID();
                userMDLProfile.document_number = randomNumber;
                userMDLProfile.issue_date = "19/04/2021";
                userMDLProfile.issuing_country = "IN";
                userMDLProfile.issuing_authority = userBasicProfile.Country + "-TG";
                userMDLProfile.expiry_date = "19/04/2031";
                userMDLProfile.international_permit = "Issued";

                var result = await GetMdlDocument(userBasicProfile.DisplayName, userBasicProfile.Photo, userBasicProfile.DateOfBirth, randomNumber, userMDLProfile.expiry_date, userMDLProfile.issue_date);
                if (result != null)
                {
                    string doc = Convert.ToBase64String(result);
                    userMDLProfile.mdl_document = doc;
                }
                //datapivotId = 3;
            }

            if (request.ProfileType == "HEALTH")
            {
                config.UserStatusUrl = configuration["Profile:Health:UserStatusUrl"];
                config.ProfileUrl = configuration["Profile:Health:ProfileServiceUrl"];

                userHealthProfile.blood_group = "A+";
                userHealthProfile.height = 182;
                userHealthProfile.weight = 80;
                userHealthProfile.bmi = 24.2;
            }

            if (request.ProfileType == "pid")
            {
                //datapivotId = 5;
            }

            if ((request.ProfileType == "NonResidentProfile"))
            {
                //datapivotId = 9;
            }

            if ((request.ProfileType == "ResidentProfile"))
            {
                //datapivotId = 9;
            }
            if (request.ProfileType == "SocialBenefitCard")
            {
                var url = configuration["BeneficiaryCardUrl"];
                var profileObject = new SocialProfileDTO();

                profileObject.x_cst_indv_suid = userBasicProfile.SubscriberUid;

                StringContent content = new StringContent(JsonConvert.SerializeObject(profileObject), Encoding.UTF8, "application/json");

                HttpResponseMessage result;

                try
                {
                    result = await client.PostAsync(url, content);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {url} failed with status code {result.StatusCode}");
                        return new GetUserProfileResponse("Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.Success)
                {
                    return new GetUserProfileResponse(apiResponse.Message);
                }
                userSocialProfile = JsonConvert.DeserializeObject<UserSocialProfile>(apiResponse.Result.ToString());
                //datapivotId = 4;
            }
            if (request.ProfileType == "Visa")
            {
                var url = configuration["VisaUrl"];
                url = url + userBasicProfile.IdDocNumber;
                HttpResponseMessage result;

                try
                {
                    result = await client.GetAsync(url);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {url} failed with status code {result.StatusCode}");
                        return new GetUserProfileResponse("Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<VisaDetailsResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.Success)
                {
                    return new GetUserProfileResponse(apiResponse.Message);
                }
                visaProfile = apiResponse.Result[0];
                //datapivotId = 4;
            }

            if (request.ProfileType == "TradeLicense")
            {
                var url = configuration["TradeLicenceUrl"];
                url = url + userBasicProfile.IdDocNumber;
                HttpResponseMessage result;

                try
                {
                    result = await client.GetAsync(url);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {url} failed with status code {result.StatusCode}");
                        return new GetUserProfileResponse("Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.Success)
                {
                    return new GetUserProfileResponse(apiResponse.Message);
                }
                tradeLicence = JsonConvert.DeserializeObject<TradeLicenceProfile>(apiResponse.Result.ToString());
                //Scopes.Add("TradeLicence Profile");
                //datapivotId = 10;
            }

            if (request.ProfileType == "BusinessEstablishment")
            {
                var url = configuration["BusinessEstablishmentUrl"];
                url = url + userBasicProfile.IdDocNumber;
                HttpResponseMessage result;

                try
                {
                    result = await client.GetAsync(url);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {url} failed with status code {result.StatusCode}");
                        return new GetUserProfileResponse("Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new GetUserProfileResponse("Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseString);
                if (apiResponse == null)
                {
                    return new GetUserProfileResponse("Internal error");
                }
                if (!apiResponse.Success)
                {
                    return new GetUserProfileResponse(apiResponse.Message);
                }
                businessProfile = JsonConvert.DeserializeObject<BusinessEstablishmentProfile>(apiResponse.Result.ToString());
                //Scopes.Add("Business Establishment Profile");
                //datapivotId = 11;
            }

            if (request.ProfileType == "AuthenticationProfile")
            {
                //Scopes.Add("AuthenticationProfile");
                //datapivotId = 11;
            }
            if (request.ProfileType == "VisitorProfile")
            {
                //Scopes.Add("VisitorProfile");
                //datapivotId = 12;
            }
            //if (userBasicProfile.SubscriberStatus != StatusConstants.ACTIVE)
            //{ 
            //    _logger.LogInformation("User account is not Active");
            //    statusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "User not Active", "", -1);
            //    return new GetUserProfileResponse("User Account is not Active");
            //}
            Scopes.Add(request.ProfileType);
            var tempAuthNSessId = string.Empty;

            try
            {
                // Generate sessionid
                tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            }
            catch (Exception error)
            {

                _logger.LogError("GetUniqueKey failed: {0}", error.Message);
                //statusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Failed to get Unique Key", "", -1);
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            var clientInDb = await _clientService.GetClientByClientIdAsync(
                accessToken.ClientId);
            if (null == clientInDb)
            {
                _logger.LogError(OIDCConstants.ClientNotFound);
                //statusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Client Not Found", "", -1);
                return new GetUserProfileResponse(OIDCConstants.ClientNotFound);
            }

            // Validate client scopes
            //var isTrue = ValidateScopes(Scopes[0], clientInDb.Scopes);
            //if (false == isTrue)
            //{
            //    _logger.LogError(OIDCConstants.ClientScopesNotMatched);
            //    statusResponse = await AddTransactionProfileStatus(transactionid, "Failed", "Scopes Not Matched", "", -1);
            //    return new GetUserProfileResponse(OIDCConstants.ClientScopesNotMatched);
            //}

            // Requested Scopes
            //var Scopes = request.Scopes.Split(new char[] { ' ', '\t' });

            // Get all scopes from Db
            var scopesinDB = await _unitOfWork.Scopes.ListAllScopeAsync();
            if (null == scopesinDB)
            {
                _logger.LogError("ListAllScopeAsync failed");
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            // Get all Claims from Database
            var claimsinDb = await _unitOfWork.UserClaims.
                ListAllUserClaimAsync();

            if (null == claimsinDb)
            {
                _logger.LogError("ListAllUserClaimAsync failed");
                return new GetUserProfileResponse(OIDCConstants.InternalError);
            }

            //var deselectScopesAndClaims = (bool)clientInDb.DeselectScopesClaims;
            var deselectScopesAndClaims = true;
            var consentScopes = new List<ScopeInfo>() { };

            var daesClaim = new GetUserBasicProfileResult();

            var userApprovedClaims = new List<string>() { };


            HashSet<string> requestedProfileAttributes = new HashSet<string>();

            foreach (var reqScope in Scopes)
            {
                foreach (var scopeinDb in scopesinDB)
                {
                    if ((reqScope == scopeinDb.Name) &&
                        (scopeinDb.UserConsent == true))
                    {
                        ScopeInfo scopeInfo = new ScopeInfo();
                        scopeInfo.Name = scopeinDb.Name;
                        scopeInfo.DisplayName = scopeinDb.DisplayName;
                        scopeInfo.Description = scopeinDb.Description;

                        if (deselectScopesAndClaims)
                            scopeInfo.Mandatory = scopeinDb.DefaultScope;
                        else
                            scopeInfo.Mandatory = true;

                        if (scopeinDb.IsClaimsPresent)
                        {
                            scopeInfo.ClaimsInfo = new List<ClaimInfo>() { };
                            var claims = scopeinDb.ClaimsList.Split(
                                new char[] { ' ', '\t' });

                            foreach (var claim in claims)
                            {
                                foreach (var claiminDb in claimsinDb)
                                {
                                    if ((claim == claiminDb.Name))
                                    {
                                        ClaimInfo claimInfo = new ClaimInfo();
                                        claimInfo.Name = claiminDb.Name;
                                        claimInfo.DisplayName = claiminDb.DisplayName;
                                        claimInfo.Description = claiminDb.Description;

                                        if (deselectScopesAndClaims)
                                            claimInfo.Mandatory = claiminDb.DefaultClaim;
                                        else
                                            claimInfo.Mandatory = true;

                                        scopeInfo.ClaimsInfo.Add(claimInfo);
                                        requestedProfileAttributes.Add(claiminDb.DisplayName);
                                        userApprovedClaims.Add(claiminDb.Name);
                                    }
                                }
                            }
                        }
                        scopeInfo.ClaimsPresent = scopeinDb.IsClaimsPresent;
                        consentScopes.Add(scopeInfo);
                    }
                }
            }

            var approvedAttributes = string.Join(",", userApprovedClaims);
            var daesClaims = new GetUserBasicProfileResult();
            if (userApprovedClaims.Contains(Claims.SUID))
                daesClaims.suid = userBasicProfile.SubscriberUid;
            if (userApprovedClaims.Contains(Claims.Name))
                daesClaims.name = userBasicProfile.DisplayName;
            if (userApprovedClaims.Contains(Claims.BirthDate))
                daesClaims.birthdate = userBasicProfile.DateOfBirth.ToString();


            if (userApprovedClaims.Contains(Claims.BirthDate) && request.ProfileType == "mDL")
            {
                daesClaims.birthdate = userBasicProfile.DateOfBirth?.ToString("dd/MM/yyyy");
            }

            if (userApprovedClaims.Contains(Claims.BirthDate) && request.ProfileType == "HealthInsuranceProfile")
            {
                daesClaims.birthdate = userBasicProfile.DateOfBirth?.ToString("dd/MM/yyyy");
            }

            if (userApprovedClaims.Contains(Claims.BirthDate) && request.ProfileType == "pid")
            {
                daesClaims.birthdate = userBasicProfile.DateOfBirth?.ToString("dd/MM/yyyy");
            }


            if (userApprovedClaims.Contains(Claims.Gender))
                daesClaims.gender = userBasicProfile.Gender;

            if (userApprovedClaims.Contains(Claims.Email))
                daesClaims.email = userBasicProfile.Email;
            if (userApprovedClaims.Contains(Claims.PhoneNumber))
                daesClaims.phone_number = userBasicProfile.MobileNumber;
            if (userApprovedClaims.Contains(Claims.LOA))
                daesClaims.loa = userBasicProfile.Loa;
            if (userApprovedClaims.Contains(Claims.IdDocumentNumber))
                daesClaims.id_document_number = userBasicProfile.IdDocNumber;
            if (userApprovedClaims.Contains(Claims.IdDocumentType))
                daesClaims.id_document_type = userBasicProfile.IdDocType;
            if (userApprovedClaims.Contains(Claims.Country))
                daesClaims.country = userBasicProfile.Country;
            if (userApprovedClaims.Contains(Claims.Email_Verified))
                daesClaims.email_verified = true;
            if (userApprovedClaims.Contains(Claims.PhoneNumber_Verified))
                daesClaims.phone_number_verified = true;
            if (userApprovedClaims.Contains("photo"))
                daesClaims.photo = userBasicProfile.Photo;
            if (userApprovedClaims.Contains(Claims.Pid_Document))
                daesClaims.pid_document = userBasicProfile.pid_document;
            if (userApprovedClaims.Contains("cardStatus"))
                daesClaims.cardStatus = userBasicProfile.cardStatus;
            if (userApprovedClaims.Contains("issuedOn"))
                daesClaims.issuedOn = userBasicProfile.issuedOn;
            if (userApprovedClaims.Contains("idDocument"))
                daesClaims.idDocument = userBasicProfile.idDocument;
            if (userApprovedClaims.Contains("idNumber"))
                daesClaims.idNumber = userBasicProfile.idNumber;
            if (userApprovedClaims.Contains("familyBook"))
                daesClaims.family_book = userBasicProfile.FamilyBook;
            if (userApprovedClaims.Contains("SubscriberCard"))
                daesClaims.subscriber_card = userBasicProfile.SubscriberCard;

            if (userApprovedClaims.Contains("address"))
                daesClaims.address = userBasicProfile.Address;
            if (userApprovedClaims.Contains("occupation"))
                daesClaims.occupation = userBasicProfile.Occupation;

            if (userApprovedClaims.Contains("VisaStatus"))
                daesClaims.VisaStatus = userBasicProfile.VisaStatus;
            if (userApprovedClaims.Contains("Nationality"))
                daesClaims.Nationality = userBasicProfile.Nationality;
            if (userApprovedClaims.Contains("VisaNumber"))
                daesClaims.VisaNumber = userBasicProfile.VisaNumber;
            if (userApprovedClaims.Contains("SignedVisaDocument"))
                daesClaims.SignedVisaDocument = userBasicProfile.SignedVisaDocument;
            if (userApprovedClaims.Contains("IssueDate"))
                daesClaims.IssueDate = userBasicProfile.IssueDate;
            if (userApprovedClaims.Contains("ExpiryDate"))
                daesClaims.ExpiryDate = userBasicProfile.ExpiryDate;
            if (userApprovedClaims.Contains("NonResidentCardNumber"))
                daesClaims.NonResidentCardNumber = userBasicProfile.NonResidentCardNumber;
            if (userApprovedClaims.Contains("FatherName"))
                daesClaims.FatherName = userBasicProfile.FatherName;
            if (userApprovedClaims.Contains("MotherName"))
                daesClaims.MotherName = userBasicProfile.MotherName;
            if (userApprovedClaims.Contains("PlaceOfBirth"))
                daesClaims.PlaceOfBirth = userBasicProfile.PlaceOfBirth;
            if (userApprovedClaims.Contains("BloodGroup"))
                daesClaims.BloodGroup = userBasicProfile.BloodGroup;
            if (userApprovedClaims.Contains("VisitorCardPdf"))
                daesClaims.VisitorCardPdf = userBasicProfile.VisitorCardPdf;
            if (userApprovedClaims.Contains("VisitorCardNumber"))
                daesClaims.VisitorCardNumber = userBasicProfile.VisitorCardNumber;
            if (userApprovedClaims.Contains("VisitorStatus"))
                daesClaims.VisitorStatus = userBasicProfile.VisitorStatus;
            if (request.ProfileType == "VisitorProfile")
            {
                daesClaims.pid_document = userBasicProfile.PassPortImage;
            }


            if (userApprovedClaims.Contains("Resident_Card_Status"))
                daesClaims.cardStatus = userBasicProfile.cardStatus;
            if (userApprovedClaims.Contains("Resident_Card_IssuedOn"))
                daesClaims.issuedOn = userBasicProfile.issuedOn;
            if (userApprovedClaims.Contains("Resident_Id_Document"))
                daesClaims.idDocument = userBasicProfile.idDocument;
            if (userApprovedClaims.Contains("Resident_Id_Number"))
                daesClaims.idNumber = userBasicProfile.idNumber;

            if (userApprovedClaims.Contains("issue_date"))
                daesClaims.issue_date = userMDLProfile.issue_date;
            if (userApprovedClaims.Contains("expiry_date"))
                daesClaims.expiry_date = userMDLProfile.expiry_date;
            if (userApprovedClaims.Contains("issuing_country"))
                daesClaims.issuing_country = userMDLProfile.issuing_country;
            if (userApprovedClaims.Contains("issuing_authority"))
                daesClaims.issuing_authority = userMDLProfile.issuing_authority;
            if (userApprovedClaims.Contains("document_number"))
                daesClaims.document_number = userMDLProfile.document_number;
            if (userApprovedClaims.Contains("mdl_document"))
                daesClaims.mdl_document = userMDLProfile.mdl_document;
            if (userApprovedClaims.Contains("international_permit"))
                daesClaims.international_permit = userMDLProfile.international_permit;
            if (userApprovedClaims.Contains("blood_group"))
                daesClaims.blood_group = userHealthProfile.blood_group;
            if (userApprovedClaims.Contains("height"))
                daesClaims.height = userHealthProfile.height;
            if (userApprovedClaims.Contains("weight"))
                daesClaims.weight = userHealthProfile.weight;
            if (userApprovedClaims.Contains("bmi"))
                daesClaims.bmi = userHealthProfile.bmi;

            if (userApprovedClaims.Contains("Insured_Member"))
                daesClaims.InsuredMember = userBasicProfile.DisplayName;
            if (userApprovedClaims.Contains("Policy_Number"))
                daesClaims.PolicyNumber = GeneratePolicyNumber();
            if (userApprovedClaims.Contains("Policy_Name"))
                daesClaims.PolicyName = "Star Health Young Silver Plan";
            if (userApprovedClaims.Contains("Policy_Start_Date"))
            {
                string dateString = "01/08/2024";
                DateTime policyStartDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                daesClaims.PolicyStartDate = dateString;
            }
            //daesClaims.PolicyStartDate = new DateTime(2024, 8, 1);
            if (userApprovedClaims.Contains("Policy_End_Date"))
            {
                string dateString = "01/08/2025";
                DateTime policyStartDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                daesClaims.PolicyEndDate = dateString;
            }
            //daesClaims.PolicyEndDate = new DateTime(2025, 8, 1);
            if (userApprovedClaims.Contains("Policy_Status"))
                daesClaims.PolicyStatus = "Active";
            if (userApprovedClaims.Contains("HealthInsurance_Document"))
            {
                string startDateString = "01/08/2024";
                DateTime policyStartDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                string endDateString = "01/08/2025";
                DateTime policyEndDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var result = await GetHealthInsuranceCard(
                    userBasicProfile.DisplayName, userBasicProfile.DateOfBirth, daesClaims.PolicyNumber, policyStartDate,
                    policyEndDate, daesClaims.PolicyStatus, userBasicProfile.Photo, userBasicProfile.MobileNumber, userBasicProfile.Gender);

                if (result != null)
                {
                    string doc = Convert.ToBase64String(result);
                    daesClaims.HealthInsuranceDocument = doc;
                }
            }



            if (request.ProfileType == "SocialBenefitCard")
            {
                if (userApprovedClaims.Contains("address"))
                    daesClaims.address = userSocialProfile.address;
                if (userApprovedClaims.Contains("age"))
                    daesClaims.age = userSocialProfile.age;
                if (userApprovedClaims.Contains("birthdate"))
                    daesClaims.birthdate = userSocialProfile.birthdate;
                if (userApprovedClaims.Contains("email"))
                    daesClaims.email = userSocialProfile.email;
                if (userApprovedClaims.Contains("income"))
                    daesClaims.income = userSocialProfile.income;
                if (userApprovedClaims.Contains("x_cst_indv_Doc_type"))
                    daesClaims.x_cst_indv_Doc_type = userSocialProfile.x_cst_indv_Doc_type;
                if (userApprovedClaims.Contains("x_cst_indv_Doc_value"))
                    daesClaims.x_cst_indv_Doc_value = userSocialProfile.x_cst_indv_Doc_value;
                if (userApprovedClaims.Contains("x_cst_indv_children"))
                    daesClaims.x_cst_indv_children = userSocialProfile.x_cst_indv_children;
                if (userApprovedClaims.Contains("x_cst_indv_suid"))
                    daesClaims.x_cst_indv_suid = userSocialProfile.x_cst_indv_suid;
                if (userApprovedClaims.Contains("name"))
                    daesClaims.name = userSocialProfile.name;
                if (userApprovedClaims.Contains("phone_sanitized"))
                    daesClaims.phone_sanitized = userSocialProfile.phone_sanitized;
                if (userApprovedClaims.Contains("occupation"))
                    daesClaims.occupation = userSocialProfile.occupation;
                if (userApprovedClaims.Contains("programs"))
                {
                    daesClaims.programs = userSocialProfile.Programs;
                }
            }

            if (request.ProfileType == "Visa")
            {
                if (userApprovedClaims.Contains("name"))
                    daesClaims.name = visaProfile.FullName;
                if (userApprovedClaims.Contains("birthdate"))
                    daesClaims.birthdate = visaProfile.DateOfBirth;
                if (userApprovedClaims.Contains("gender"))
                    daesClaims.gender = visaProfile.Gender;
                if (userApprovedClaims.Contains("email"))
                    daesClaims.email = visaProfile.Email;
                if (userApprovedClaims.Contains("phone_number"))
                    daesClaims.phone_number = visaProfile.MobileNumber;
                if (userApprovedClaims.Contains("Document"))
                    daesClaims.Document = visaProfile.PassportDocumentImage;
                if (userApprovedClaims.Contains("country"))
                    daesClaims.country = visaProfile.CountryCode;
                if (userApprovedClaims.Contains("VisaType"))
                    daesClaims.VisaType = visaProfile.VisaType;
                if (userApprovedClaims.Contains("PassportNumber"))
                    daesClaims.PassportNumber = visaProfile.PassportNumber;
                if (userApprovedClaims.Contains("PassportExpiryDate"))
                    daesClaims.PassportExpiryDate = visaProfile.PassportExpiryDate;
                if (userApprovedClaims.Contains("VisaIssuedOn"))
                    daesClaims.VisaIssuedOn = visaProfile.VisaIssuedOn;
                if (userApprovedClaims.Contains("NumberOfEntries"))
                    daesClaims.NumberOfEntries = visaProfile.NumberOfEntries;
                if (userApprovedClaims.Contains("VisaStatus"))
                    daesClaims.VisaStatus = visaProfile.VisaStatus;
                if (userApprovedClaims.Contains("visaIssuedAt"))
                    daesClaims.VisaIssuedOn = visaProfile.VisaIssuedAt;
                if (userApprovedClaims.Contains("Nationality"))
                    daesClaims.Nationality = visaProfile.Nationality;
                if (userApprovedClaims.Contains("VisaValidUpto"))
                    daesClaims.VisaValidUpto = visaProfile.VisaValidUntil;
                if (userApprovedClaims.Contains("PassportIssueDate"))
                    daesClaims.PassportIssueDate = visaProfile.PassportIssueDate;
                if (userApprovedClaims.Contains("VisaNumber"))
                    daesClaims.VisaNumber = visaProfile.Uid;
                if (userApprovedClaims.Contains("SignedVisaDocument"))
                    daesClaims.SignedVisaDocument = visaProfile.SignedVisaDocument;
            }

            if (request.ProfileType == "BusinessEstablishment")
            {
                if (userApprovedClaims.Contains("name"))
                    daesClaims.name = businessProfile.ApplicantName;
                if (userApprovedClaims.Contains("photo"))
                    daesClaims.photo = businessProfile.ApplicantPhoto;
                if (userApprovedClaims.Contains("pid_document"))
                    daesClaims.pid_document = businessProfile.ApplicantPassport;
                if (userApprovedClaims.Contains("VisaDocument"))
                    daesClaims.VisaDocument = businessProfile.ApplicantVisa;
                if (userApprovedClaims.Contains("TradeLicenseNumber"))
                    daesClaims.TradeLicenseNumber = businessProfile.TradelicenseNumber;
                if (userApprovedClaims.Contains("TradeLicenseDocument"))
                    daesClaims.TradeLicenseDocument = businessProfile.TradelicenseDocument;
                if (userApprovedClaims.Contains("CompanyName"))
                    daesClaims.CompanyName = businessProfile.CompanyName;
                if (userApprovedClaims.Contains("PassportNumber"))
                    daesClaims.PassportNumber = businessProfile.PassportNumber;
                if (userApprovedClaims.Contains("EstablishmentCard"))
                    daesClaims.EstablishmentCard = businessProfile.EstablishmentCard;
                if (userApprovedClaims.Contains("EstablishmentCardNumber"))
                    daesClaims.EstablishmentCardNumber = businessProfile.EstablishmentCardNumber;
                if (userApprovedClaims.Contains("EstablishmentCardStatus"))
                    daesClaims.cardStatus = businessProfile.ApplicationStatus;
                if (userApprovedClaims.Contains("IssueDate"))
                    daesClaims.IssueDate = businessProfile.IssueDate;
                if (userApprovedClaims.Contains("PaymentReferenceNumber"))
                    daesClaims.PaymentReferenceNumber = businessProfile.PaymentReferenceNumber;
                if (userApprovedClaims.Contains("VisaNumber"))
                    daesClaims.VisaNumber = businessProfile.VisaNumber;
            }

            if (request.ProfileType == "TradeLicense")
            {
                if (userApprovedClaims.Contains("name"))
                    daesClaims.name = tradeLicence.ApplicantName;
                if (userApprovedClaims.Contains("photo"))
                    daesClaims.photo = tradeLicence.ApplicantPhoto;
                if (userApprovedClaims.Contains("pid_document"))
                    daesClaims.pid_document = tradeLicence.ApplicantPassport;
                if (userApprovedClaims.Contains("VisaDocument"))
                    daesClaims.VisaDocument = tradeLicence.ApplicantVisa;
                if (userApprovedClaims.Contains("TradeLicenseNumber"))
                    daesClaims.TradeLicenseNumber = tradeLicence.TradelicenseNumber;
                if (userApprovedClaims.Contains("TradeLicenseDocument"))
                    daesClaims.TradeLicenseDocument = tradeLicence.TradelicenseDocument;
                if (userApprovedClaims.Contains("CompanyName"))
                    daesClaims.CompanyName = tradeLicence.CompanyName;
                if (userApprovedClaims.Contains("PassportNumber"))
                    daesClaims.PassportNumber = tradeLicence.PassportNumber;
                if (userApprovedClaims.Contains("EstablishmentCardStatus"))
                    daesClaims.cardStatus = tradeLicence.ApplicationStatus;
                if (userApprovedClaims.Contains("VisaNumber"))
                    daesClaims.VisaNumber = tradeLicence.VisaNumber;
                if (userApprovedClaims.Contains("IssueDate"))
                    daesClaims.IssueDate = tradeLicence.IssueDate;
                if (userApprovedClaims.Contains("ExpiryDate"))
                    daesClaims.ExpiryDate = tradeLicence.ExpiryDate;
                if (userApprovedClaims.Contains("PaymentReferenceNumber"))
                    daesClaims.PaymentReferenceNumber = tradeLicence.PaymentReferenceNumber;
            }



            _logger.LogDebug("<--GetUserProfileDataAsync");
            return new GetUserProfileResponse(daesClaims);
        }
        public async Task<byte[]> GetMdlDocument(string displayname, string photo, DateTime? dob, string drivinglicensenumber, string expiryDate, string issuedDate)
        {
            try
            {
                var fileName = "MDL_DOCUMENT.pdf";
                var filePath = Path.Combine(_environment.WebRootPath, "assets", fileName);
                var licenseNumber = drivinglicensenumber;
                var referenceNumber = GenerateRandomMDLReferenceID();
                var fields = new FieldCoordinatesDTO.MdlFields
                {
                    LicenseNumber = licenseNumber,
                    Name = displayname,
                    DOB = dob,
                    Image = photo,
                    ExpiryDate = expiryDate,
                    IssueDate = issuedDate,
                    PdfString = filePath,
                    Locations = new List<FieldCoordinatesDTO.Location>
            {
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "licensenumber",
                    CoordinateX = 61.6f,
                    CoordinateY = 142f,
                    Height = 18f,
                    Width = 261.6f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "name",
                    CoordinateX = 61.6f,
                    CoordinateY = 217.44f,
                    Height = 18f,
                    Width = 336.8f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "expirydate",
                    CoordinateX = 250.4f,
                    CoordinateY = 475.5f,
                    Height = 17f,
                    Width = 88f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "dob",
                    CoordinateX = 250.4f,
                    CoordinateY = 505.5f,
                    Height = 17f,
                    Width = 88f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "image",
                    CoordinateX = 427.2f,
                    CoordinateY = 110.24f,
                    Height = 117.6f,
                    Width = 104f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "issuedate",
                    CoordinateX = 149.6f,
                    CoordinateY = 322f,
                    Height = 18f,
                    Width = 135.2f
                }
            }
                };

                byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(fields.PdfString);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (PdfReader reader = new PdfReader(pdfBytes))
                    {
                        using (PdfStamper stamper = new PdfStamper(reader, ms))
                        {
                            PdfContentByte cb = stamper.GetOverContent(1); // Get content to write on first page

                            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                            // Loop through locations and insert them into PDF
                            foreach (var location in fields.Locations)
                            {
                                cb.SetFontAndSize(bf, location.Height);

                                float x = location.CoordinateX;
                                float y = reader.GetPageSize(1).Height - location.CoordinateY - location.Height; // Invert y-coordinate

                                cb.BeginText();

                                if (location.FieldTitle.Equals("NAME", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] nameLines = SplitTextIntoLines(fields.Name, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.SetTextMatrix(x, y);
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("issuedate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.IssueDate;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("licensenumber", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    //cb.SetColorFill(BaseColor.RED); // Set the color to red
                                    string[] nameLines = SplitTextIntoLines(fields.LicenseNumber, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                    //cb.SetColorFill(BaseColor.BLACK); // Reset the color to black
                                }
                                else if (location.FieldTitle.Equals("expirydate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string expirydate = fields.ExpiryDate;
                                    cb.ShowText(expirydate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("dob", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    DateTime dateOfBirth = (DateTime)fields.DOB;
                                    string formattedDate = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    cb.ShowText(formattedDate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                cb.EndText();

                                if (location.FieldTitle.Equals("image", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    var s = fields.Image;
                                    byte[] imageBytes = Convert.FromBase64String(fields.Image);
                                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

                                    // Resize the image proportionally
                                    float maxWidth = 100; // Set the maximum width you desire
                                    float maxHeight = 100; // Set the maximum height you desire
                                    image.ScaleAbsolute(maxWidth, maxHeight);
                                    // Add the image to the bottom layer
                                    image.SetAbsolutePosition(x, y); // Adjust for top-left origin
                                    stamper.GetOverContent(1).AddImage(image);
                                }
                            }
                        }
                    }

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed while generating MDL Card : " + ex);
                Monitor.SendException(ex);
                return null;
            }
        }

        /*public async Task<byte[]> GetMdlDocument(string surname, string othername, string NationalIDNumber, string Gender, string BloodGroup, string Country, string photo, DateTime? dob, string drivinglicensenumber, string expiryDate, string issuedDate)
        {
            try
            {
                var fileName = "MDL_DOCUMENT.pdf";

                var filePath = Path.Combine(_environment.WebRootPath, "assets", fileName);

                var licenseNumber = drivinglicensenumber;

                var fields = new FieldCoordinatesDTO.MdlFields
                {
                    LicenseNumber = licenseNumber,
                    Surname = surname,
                    DOB = dob,
                    Image = photo,
                    ExpiryDate = expiryDate,
                    IssueDate = issuedDate,
                    PdfString = filePath,
                    OtherName = othername,
                    NationalIDNumber = NationalIDNumber,
                    Gender = Gender,
                    BloodGroup = BloodGroup,
                    Country = Country,
                    LicenseExpiryDate = expiryDate,
                    LicenseIssueDate = issuedDate,
                    Locations = new List<FieldCoordinatesDTO.Location>
            {
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "Surname",
                    CoordinateX = 234f,
                    CoordinateY = 125f,
                    Height = 14f,
                    Width = 259.2f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "OtherName",
                    CoordinateX = 234f,
                    CoordinateY = 150f,
                    Height = 14f,
                    Width = 275.2f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "NationalIDNumber",
                    CoordinateX = 234f,
                    CoordinateY = 179f,
                    Height = 14f,
                    Width = 79.99f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "LicenseNumber",
                    CoordinateX = 323.4f,
                    CoordinateY = 179f,
                    Height = 14f,
                    Width = 110.39f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "DOB",
                    CoordinateX = 234f,
                    CoordinateY = 205f,
                    Height = 14f,
                    Width = 156.00f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "Gender",
                    CoordinateX = 234f,
                    CoordinateY = 234f,
                    Height = 14f,
                    Width = 80f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "BloodGroup",
                    CoordinateX = 234f,
                    CoordinateY = 262f,
                    Height = 14f,
                    Width = 59.19f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "IssueDate",
                    CoordinateX = 234f,
                    CoordinateY = 293f,
                    Height = 14f,
                    Width = 63f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "ExpiryDate",
                    CoordinateX = 308f,
                    CoordinateY = 293f,
                    Height = 14f,
                    Width = 63f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "Country",
                    CoordinateX = 383.2f,
                    CoordinateY = 293f,
                    Height = 14f,
                    Width = 108.8f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "Image",
                    CoordinateX = 88f,
                    CoordinateY = 146.4f,
                    Height = 171.2f,
                    Width = 127.19f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "LicenseIssueDate",
                    CoordinateX = 268.8f,
                    CoordinateY = 468f,
                    Height = 14f,
                    Width = 68f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "LicenseExpiryDate",
                    CoordinateX = 348.8f,
                    CoordinateY = 468,
                    Height = 14f,
                    Width = 68f
                },


            }
                };

                byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(fields.PdfString);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (PdfReader reader = new PdfReader(pdfBytes))
                    {
                        using (PdfStamper stamper = new PdfStamper(reader, ms))
                        {
                            PdfContentByte cb = stamper.GetOverContent(1); // Get content to write on first page

                            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                            // Loop through locations and insert them into PDF
                            foreach (var location in fields.Locations)
                            {
                                cb.SetFontAndSize(bf, location.Height);

                                float x = location.CoordinateX;
                                float y = reader.GetPageSize(1).Height - location.CoordinateY - location.Height; // Invert y-coordinate

                                cb.BeginText();

                                if (location.FieldTitle.Equals("LicenseNumber", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.LicenseNumber;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("Surname", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.Surname;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("ExpiryDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string expirydate = fields.ExpiryDate;
                                    cb.ShowText(expirydate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("DOB", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    DateTime dateOfBirth = (DateTime)fields.DOB;
                                    string formattedDate = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    cb.ShowText(formattedDate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("IssueDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.IssueDate;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("OtherName", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.OtherName;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("NationalIDNumber", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.NationalIDNumber;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("Gender", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.Gender;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("BloodGroup", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.BloodGroup;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("Country", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.Country;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("LicenseExpiryDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.LicenseExpiryDate;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                else if (location.FieldTitle.Equals("LicenseIssueDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    cb.SetFontAndSize(bf, location.Height);
                                    cb.SetTextMatrix(x, y);
                                    string issuedate = fields.LicenseIssueDate;
                                    cb.ShowText(issuedate);
                                    bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                }
                                cb.EndText();

                                if (location.FieldTitle.Equals("Image", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    var s = fields.Image;
                                    byte[] imageBytes = Convert.FromBase64String(fields.Image);
                                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

                                    // Resize the image proportionally
                                    float maxWidth = 127; // Set the maximum width you desire
                                    float maxHeight = 171; // Set the maximum height you desire
                                    image.ScaleAbsolute(maxWidth, maxHeight);
                                    // Add the image to the bottom layer
                                    image.SetAbsolutePosition(x, y); // Adjust for top-left origin
                                    stamper.GetOverContent(1).AddImage(image);
                                }
                            }
                        }
                    }

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }*/
        public async Task<byte[]> GetHealthInsuranceCard(string insuredmember, DateTime? dob, string policyNumber, DateTime policyStartDate, DateTime policyEndDate, string policyStatus, string photo, string phoneNumber, string gender)
        {
            try
            {
                var fileName = "HealthInsurance_DOCUMENT.pdf";
                var filePath = Path.Combine(_environment.WebRootPath, "assets", fileName);
                var fields = new FieldCoordinatesDTO.HealthCardFields
                {
                    InsuredMember = insuredmember,
                    DOB = (DateTime)dob,
                    PolicyNumber = policyNumber,
                    PolicyName = "Star Health Plan",
                    PolicyStartDate = policyStartDate,
                    PolicyEndDate = policyEndDate,
                    PolicyStatus = policyStatus,
                    Gender = gender,
                    Photo = photo,
                    PhoneNumber = phoneNumber,
                    PdfString = filePath,
                    Locations = new List<FieldCoordinatesDTO.Location>
            {
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "InsuredMember",
                    CoordinateX = 224f,
                    CoordinateY = 312.04f,
                    Height = 13f,
                    Width = 262.4f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "dob",
                    CoordinateX = 224f,
                    CoordinateY = 331f,
                    Height = 13f,
                    Width = 128f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "policyNumber",
                    CoordinateX = 224f,
                    CoordinateY = 367f,
                    Height = 13f,
                    Width = 158f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "policyName",
                    CoordinateX = 224f,
                    CoordinateY = 347.64f,
                    Height = 13f,
                    Width = 158f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "policyStartDate",
                    CoordinateX = 224f,
                    CoordinateY = 387f,
                    Height = 13f,
                    Width = 128f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "policyEndDate",
                    CoordinateX = 224f,
                    CoordinateY = 407.29f,
                    Height = 13f,
                    Width = 128f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "policyStatus",
                    CoordinateX = 224f,
                    CoordinateY = 427.08f,
                    Height = 13f,
                    Width = 128f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "gender",
                    CoordinateX = 443.2f,
                    CoordinateY = 452f,
                    Height = 12f,
                    Width = 61f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "phoneNumber",
                    CoordinateX = 224f,
                    CoordinateY = 447.61f,
                    Height = 13f,
                    Width = 128f
                },
                new FieldCoordinatesDTO.Location
                {
                    FieldTitle = "photo",
                    CoordinateX = 393.2f,
                    CoordinateY = 338.24f,
                    Height = 105.6f,
                    Width = 105.6f
                }
            }
                };

                // Read the PDF file into a byte array
                byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(fields.PdfString);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (PdfReader reader = new PdfReader(pdfBytes))
                    {
                        using (PdfStamper stamper = new PdfStamper(reader, ms))
                        {
                            PdfContentByte cb = stamper.GetOverContent(1);

                            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                            // Loop through locations and insert them into PDF
                            foreach (var location in fields.Locations)
                            {
                                cb.SetFontAndSize(bf, location.Height);

                                float x = location.CoordinateX;
                                float y = reader.GetPageSize(1).Height - location.CoordinateY - location.Height; // Invert y-coordinate

                                cb.BeginText();


                                if (location.FieldTitle.Equals("insuredmember", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] nameLines = SplitTextIntoLines(fields.InsuredMember, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    var multipleLines = false;
                                    if (nameLines.Length > 1)
                                    {
                                        multipleLines = true;
                                    }
                                    var first = true;
                                    foreach (string line in nameLines)
                                    {
                                        if (multipleLines == true && first == true)
                                        {
                                            cb.SetFontAndSize(bf, location.Height - 2);
                                            y += (location.Height * 0.85f);
                                            cb.SetTextMatrix(x, y);
                                            cb.ShowText(line);
                                            y -= location.Height + lineSpacing;
                                            cb.SetTextMatrix(x, y);
                                            first = false;
                                        }
                                        else
                                        {
                                            if (multipleLines == false)
                                                cb.SetFontAndSize(bf, location.Height);
                                            else
                                                cb.SetFontAndSize(bf, location.Height - 2);
                                            cb.SetTextMatrix(x, y);
                                            cb.ShowText(line);
                                            y -= location.Height + lineSpacing;
                                            cb.SetTextMatrix(x, y);
                                        }
                                    }
                                }
                                else if (location.FieldTitle.Equals("dob", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    DateTime dateOfBirth = (DateTime)fields.DOB;
                                    string formattedDate = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    //string formattedDate = fields.DOB.ToString("dd/MM/yyyy");
                                    string[] dobLines = SplitTextIntoLines(formattedDate, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in dobLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("policyNumber", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    string[] nameLines = SplitTextIntoLines(fields.PolicyNumber, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("policyName", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    string[] nameLines = SplitTextIntoLines(fields.PolicyName, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("policyStartDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    DateTime dateOfBirth = (DateTime)fields.PolicyStartDate;
                                    string formattedDate = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    //string formattedDate = fields.DOB.ToString("dd/MM/yyyy");
                                    string[] dobLines = SplitTextIntoLines(formattedDate, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in dobLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("policyEndDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    DateTime dateOfBirth = (DateTime)fields.PolicyEndDate;
                                    string formattedDate = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    //string formattedDate = fields.DOB.ToString("dd/MM/yyyy");
                                    string[] dobLines = SplitTextIntoLines(formattedDate, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in dobLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("policyStatus", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    string[] nameLines = SplitTextIntoLines(fields.PolicyStatus, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("gender", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    string[] nameLines = SplitTextIntoLines(fields.Gender, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                else if (location.FieldTitle.Equals("phoneNumber", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    string[] nameLines = SplitTextIntoLines(fields.PhoneNumber, location.Width, bf, location.Height);
                                    float lineSpacing = 2f;
                                    foreach (string line in nameLines)
                                    {
                                        cb.ShowText(line);
                                        y -= location.Height + lineSpacing;
                                        cb.SetTextMatrix(x, y);
                                    }
                                }
                                cb.EndText();

                                if (location.FieldTitle.Equals("photo", StringComparison.OrdinalIgnoreCase))
                                {
                                    cb.SetTextMatrix(x, y);
                                    var s = fields.Photo;
                                    byte[] imageBytes = Convert.FromBase64String(fields.Photo);
                                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

                                    // Resize the image proportionally
                                    float maxWidth = 100; // Set the maximum width you desire
                                    float maxHeight = 100; // Set the maximum height you desire
                                    image.ScaleAbsolute(maxWidth, maxHeight);
                                    // Add the image to the bottom layer
                                    image.SetAbsolutePosition(x, y); // Adjust for top-left origin
                                    stamper.GetOverContent(1).AddImage(image);
                                }
                            }
                        }
                    }

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed while generating Health Insurance Card : " + ex);
                Monitor.SendException(ex);
                return null;
            }
        }
        private string[] SplitTextIntoLines(string text, float maxWidth, BaseFont font, float fontSize)
        {
            List<string> lines = new List<string>();
            StringBuilder currentLine = new StringBuilder();
            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                string candidate = currentLine + " " + word;
                float width = font.GetWidthPoint(candidate, fontSize);
                if (width > maxWidth)
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
                currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return lines.ToArray();
        }
        public string GenerateRandomMDLID()
        {
            string monthYear = DateTime.Now.ToString("MMyyyy");

            Random random = new Random();
            string randomDigits = random.Next(1000000, 10000000).ToString();

            return $"TS{monthYear}{randomDigits}";
        }

        public string GenerateRandomMDLReferenceID()
        {
            Random random = new Random();

            string randomDigits = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                randomDigits += random.Next(0, 10).ToString();
            }

            return $"DLCTS{randomDigits}";

        }

        public string GeneratePolicyNumber()
        {
            Random random = new Random();

            string sixDigitNumber = random.Next(100000, 999999).ToString();

            int twoDigitNumber = DateTime.Now.Month;
            string formattedTwoDigitSequence = twoDigitNumber.ToString("D2");

            int currentYear = DateTime.Now.Year;

            int sixDigitSerial = random.Next(100000, 999999);

            string randomCode = $"P/{sixDigitNumber}/{formattedTwoDigitSequence}/{currentYear}/{sixDigitSerial}";

            return randomCode;

        }

        public async Task<APIResponse> GetUserProfileDataNewAsync(
            GetUserProfileRequest request)
        {
            if (null == request || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserIdType))
            {
                _logger.LogError("Invalid Parameters Received");

                return new APIResponse("Invalid Parameters");
            }

            ProfileConfig config = new ProfileConfig();

            UserData userBasicProfile = new UserData();

            Accesstoken accessToken = null;

            var reqScopes = request.ProfileType.Split(',');

            try
            {
                accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                    request.Token);
                if (null == accessToken)
                {
                    _logger.LogError("Access token not recieved from cache." +
                        "Expired or Invalid access token");
                    return new APIResponse("UnAuthorized");
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Access Token Record");
                ErrorResponseDTO error = new ErrorResponseDTO();
                error.error = OIDCConstants.InternalError;
                error.error_description = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                return new APIResponse("Internal Error" + error.error_description);
            }

            userBasicProfile = await GetUserBasicDataAsync(request.UserId,
                        request.UserIdType);

            if (null == userBasicProfile)
            {
                _logger.LogError("GetUserBasicProfileAsync Failed");

                return new APIResponse("User not found");
            }

            var clientInDb = await _clientService.GetClientProfilesAndPurposesAsync(accessToken.ClientId);

            if (null == clientInDb)
            {
                _logger.LogError("Client not found");

                return new APIResponse("Client not found");
            }
            var eConsentClient = clientInDb.EConsentClients.FirstOrDefault(s => s.Status == "ACTIVE");

            if (null == eConsentClient)
            {
                _logger.LogError("EConsent Client not found");

                return new APIResponse("EConsent Client not found");
            }
            var transactionUid = Guid.NewGuid().ToString();

            var suid = userBasicProfile.SubscriberUid;

            List<string> scopesList = new List<string>();

            HashSet<string> requestedAttributes = new HashSet<string>();

            string[] Scopes = new string[1];

            if (!string.IsNullOrEmpty(request.ProfileType))
            {
                Scopes = request.ProfileType.Split(new char[] { ',', '\t' });

                foreach (var scope in Scopes)
                {
                    var profileId = await _scopeService.GetScopeIdByNameAsync(scope);

                    if (profileId == -1)
                    {
                        _logger.LogError("Profile not found");

                        return new APIResponse("Profile not found");
                    }
                    scopesList.Add(profileId.ToString());
                }

                request.ProfileType = string.Join(',', scopesList);
            }
            else
            {
                _logger.LogError("Empty Profiles");

                return new APIResponse("Invalid Profiles");
            }
            Scopes=scopesList.ToArray();
            if (!string.IsNullOrEmpty(request.Purpose))
            {
                var purposeId = await _purposeService.GetPurposeIdByNameAsync(request.Purpose);
                if (purposeId == -1)
                {
                    _logger.LogError("Purpose not found");

                    return new APIResponse("Purpose not found");
                }
                request.Purpose = purposeId.ToString();
            }

            var transactionResponse = await AddTransactionProfileRequest(transactionUid, clientInDb.Id, suid, JsonConvert.SerializeObject(request));

            if (!transactionResponse.Success)
            {
                _logger.LogError("Failed to Add Transactions");

                return new APIResponse("Internal Error");
            }

            var transactionId = transactionResponse.Result.Id;

            var profilesList = await _userClaimService.GetAttributes();

            foreach (var scope in scopesList)
            {
                var scopeinDb = await _unitOfWork.Scopes.GetByIdAsync(int.Parse(scope));

                if (scopeinDb != null)
                {
                    if (!scopeinDb.IsClaimsPresent)
                    {
                        _logger.LogError("Profile has no Attributes");

                        return new APIResponse("Profile Has No Attributes");
                    }
                    var claims = scopeinDb.ClaimsList.Split(new char[] { ' ', '\t' });
                    foreach (var claim in claims)
                    {
                        requestedAttributes.Add(claim);
                    }
                }
            }

            await AddTransactionProfileConsent(transactionId, "NA", "", string.Join(",", requestedAttributes), "");

            if (userBasicProfile.SubscriberStatus != StatusConstants.ACTIVE)
            {
                _logger.LogError("User account is not Active");

                await AddTransactionProfileStatus(transactionId, "FAILED", "User Account Not Active", "", -1);

                return new APIResponse("User Account is not Active");
            }

            var Purpose = request.Purpose;

            var PurposeConsentRequired = true;

            List<string> clientPurposes = new List<string>();

            if (eConsentClient.Purposes != null)
            {
                clientPurposes = eConsentClient.Purposes.Split(',').ToList();
            }

            var PurposesInDb = await _purposeService.GetPurposeListAsync();

            if (null == PurposesInDb)
            {
                _logger.LogError("Failed to get the purposes list");
                return new APIResponse("Failed to get the purposes list");
            }

            if (!string.IsNullOrEmpty(Purpose))
            {
                if (!ValidatePurposes(Purpose, clientPurposes))
                {
                    await AddTransactionProfileStatus(transactionId, "FAILED", "Invalid Purposes", "", -1);

                    return new APIResponse("Invalid Purpose");
                }
                PurposeConsentRequired = CheckPurposes(request.Purpose, PurposesInDb);
            }

            var tempAuthNSessId = string.Empty;

            try
            {
                tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            }
            catch (Exception error)
            {
                _logger.LogError("GetUniqueKey failed: {0}", error.Message);
                return new APIResponse(OIDCConstants.InternalError);
            }

            // Validate client scopes

            var isTrue = ValidateProfiles(request.ProfileType, eConsentClient.Scopes);
            if (false == isTrue)
            {
                _logger.LogError(OIDCConstants.ClientScopesNotMatched);

                await AddTransactionProfileStatus(transactionId, "FAILED", OIDCConstants.ClientScopesNotMatched, "", -1);

                return new APIResponse(OIDCConstants.ClientScopesNotMatched);
            }

            // Requested Scopes


            // Get all scopes from Db
            var scopesinDB = await _unitOfWork.Scopes.ListAllScopeAsync();
            if (null == scopesinDB)
            {
                _logger.LogError("ListAllScopeAsync failed");

                await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);

                return new APIResponse(OIDCConstants.InternalError);
            }
            var userProfileResponse = await GetProfile(reqScopes, userBasicProfile.IdDocNumber);

            if (!userProfileResponse.Success)
            {
                await AddTransactionProfileStatus(transactionId, "FAILED", userProfileResponse.Message, "", -1);
                return new APIResponse(userProfileResponse.Message);
            }

            var userProfile = (Dictionary<string, object>)userProfileResponse.Resource;
            // Get all Claims from Database
            var claimsinDb = await _unitOfWork.UserClaims.
                ListAllUserClaimAsync();
            if (null == claimsinDb)
            {
                _logger.LogError("ListAllUserClaimAsync failed");
                await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);
                return new APIResponse(OIDCConstants.InternalError);
            }

            var deselectScopesAndClaims = true;

            var consentScopes = new List<ScopeInfo>() { };

            var daesClaim = new GetUserBasicProfileResult();

            var userApprovedClaims = new List<string>() { };

            HashSet<string> ApprovedClaims = new HashSet<string>();

            HashSet<string> requestedProfileAttributes = new HashSet<string>();

            var ProfilesSet = new Dictionary<string, UserProfilesConsent>();

            if (!PurposeConsentRequired)
            {
                userApprovedClaims = GetClaimsList(reqScopes, scopesinDB);
                foreach (var item in userApprovedClaims)
                {
                    ApprovedClaims.Add(item);
                }
            }
            else
            {
                foreach (var reqScope in Scopes)
                {
                    foreach (var scopeinDb in scopesinDB)
                    {
                        if (reqScope == scopeinDb.Id.ToString())
                        {
                            var userProfileConsent = await _unitOfWork.UserProfilesConsent.
                                GetUserProfilesConsentByProfileAsync(userBasicProfile.SubscriberUid, clientInDb.ClientId, scopeinDb.Id.ToString());

                            if (userProfileConsent != null)
                            {
                                ProfilesSet[scopeinDb.Name] = userProfileConsent;
                            }

                            ScopeInfo scopeInfo = new ScopeInfo();
                            scopeInfo.Name = scopeinDb.Name;
                            scopeInfo.DisplayName = scopeinDb.DisplayName;
                            scopeInfo.Description = scopeinDb.Description;
                            scopeInfo.Mandatory = scopeinDb.DefaultScope;

                            if (scopeinDb.IsClaimsPresent)
                            {
                                scopeInfo.ClaimsInfo = new List<ClaimInfo>() { };

                                var claims = scopeinDb.ClaimsList.Split(
                                    new char[] { ' ', '\t' });

                                List<Attributes> attributesList = new List<Attributes>();

                                if (userProfileConsent != null)
                                {
                                    attributesList = JsonConvert.DeserializeObject<List<Attributes>>(userProfileConsent.Attributes);
                                }
                                foreach (var claim in claims)
                                {
                                    foreach (var claiminDb in claimsinDb)
                                    {
                                        if (claim == claiminDb.Name)
                                        {
                                            Attributes AttributeinDb = null;
                                            foreach (var attribute in attributesList)
                                            {
                                                if (attribute.name == claim)
                                                {
                                                    AttributeinDb = attribute;
                                                }
                                            }
                                            if (AttributeinDb == null || !scopeinDb.SaveConsent)
                                            {
                                                ClaimInfo claimInfo = new ClaimInfo();
                                                claimInfo.Name = claiminDb.Name;
                                                claimInfo.DisplayName = claiminDb.DisplayName;
                                                claimInfo.Description = claiminDb.Description;
                                                claimInfo.Mandatory = claiminDb.DefaultClaim;
                                                scopeInfo.ClaimsInfo.Add(claimInfo);
                                                requestedProfileAttributes.Add(claiminDb.Id.ToString());
                                            }
                                            else
                                            {
                                                userApprovedClaims.Add(claim);
                                            }
                                        }
                                    }
                                }
                            }
                            if (scopeInfo.ClaimsInfo.Any())
                            {
                                scopeInfo.ClaimsPresent = scopeinDb.IsClaimsPresent;
                                consentScopes.Add(scopeInfo);
                            }
                        }
                    }
                }

                if (consentScopes.Count > 0)
                {
                    Clientdetails clientdetails = new Clientdetails();
                    clientdetails.AppName = clientInDb.ApplicationName;
                    clientdetails.ClientId = clientInDb.ClientId;

                    //var authScheme = configuration["Consent_Authentication"];
                    var authScheme = "USER_AUTH_SELECTION";

                    // Prepare temporary session object
                    TemporarySession temporarySession = new TemporarySession
                    {
                        TemporarySessionId = tempAuthNSessId,
                        UserId = userBasicProfile.SubscriberUid,
                        DisplayName = userBasicProfile.DisplayName,
                        PrimaryAuthNSchemeList = new List<string> { authScheme },
                        AuthNSuccessList = new List<string>(),
                        Clientdetails = clientdetails,
                        IpAddress = string.Empty,
                        UserAgentDetails = string.Empty,
                        TypeOfDevice = string.Empty,
                        MacAddress = DTInternalConstants.NOT_AVAILABLE,
                        withPkce = false,
                        AdditionalValue = DTInternalConstants.pending,
                        AllAuthNDone = false,
                        AuthNStartTime = DateTime.Now.ToString("s"),
                        CoRelationId = Guid.NewGuid().ToString(),
                        TransactionId = transactionId
                    };

                    // Create temporary session
                    var task = await _cacheClient.Add(CacheNames.TemporarySession,
                        tempAuthNSessId, temporarySession);
                    if (DTInternalConstants.S_OK != task.retValue)
                    {
                        _logger.LogError("_cacheClient.Add failed");
                        await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);
                        return new APIResponse(OIDCConstants.InternalError);
                    }

                    // Send notification to mobile
                    var eConsentNotification = new EConsentNotification()
                    {
                        AuthnScheme = authScheme,
                        AuthnToken = tempAuthNSessId,
                        RegistrationToken = userBasicProfile.FcmToken,
                        ApplicationName = clientInDb.ApplicationName,
                        ConsentScopes = consentScopes,
                        DeselectScopesAndClaims = deselectScopesAndClaims
                    };
                    try
                    {
                        var result = await _pushNotificationClient.SendEConsentNotification(
                            eConsentNotification);
                        if (null == result)
                        {
                            _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                                " failed");
                            await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);
                            return new APIResponse("Failed to send Notification");
                        }
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("_pushNotificationClient." +
                            "SendAuthnNotification " +
                            "failed : {0}", error.Message);
                        await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);
                        return new APIResponse("Failed to send Notification");
                    }

                    await UpdateTransactionProfileConsent(transactionId, string.Join(", ", requestedAttributes), "PENDING");

                    DateTime date1 = DateTime.Now;
                    DateTime date2 = DateTime.Now.AddMinutes(1);
                    bool isError = false;
                    string message = string.Empty;
                    TemporarySession tempSession = null;

                    while (0 > DateTime.Compare(date1, date2))
                    {
                        // Check whether the temporary session exists
                        var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                            tempAuthNSessId);

                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("Temporary Session Expired/Not Found");
                            message = OIDCConstants.InternalError;
                            isError = true;
                            break;
                        }

                        // Get the temporary session object
                        tempSession = await _cacheClient.Get<TemporarySession>
                            (CacheNames.TemporarySession,
                            tempAuthNSessId);
                        if (null == tempSession)
                        {
                            _logger.LogError("Get Temporary Session Failed,Expired/Not Found");
                            await AddTransactionProfileStatus(transactionId, "FAILED", "Internal Error", "", -1);
                            message = OIDCConstants.InternalError;
                            isError = true;
                            break;
                        }

                        if (!tempSession.AdditionalValue.Equals(DTInternalConstants.pending))
                            break;

                        // Wait 500 Milli Seconds(Half Second) without blocking
                        await Task.Delay(500);
                        date1 = DateTime.Now;
                    }

                    if (isError)
                    {
                        await AddTransactionProfileStatus(transactionId, "FAILED", message, "", -1);
                        return new APIResponse(message);
                    }
                    if (tempSession.AdditionalValue == DTInternalConstants.DeniedConsent)
                    {
                        await AddTransactionProfileStatus(transactionId, "SUCCESS", tempSession.AdditionalValue, "", -1);
                        return new APIResponse(tempSession.AdditionalValue);
                    }
                    if (tempSession.AdditionalValue == DTInternalConstants.pending)
                    {
                        await AddTransactionProfileStatus(transactionId, "FAILED", "Request Timed Out", "", -1);
                        return new APIResponse("Request Timed Out");
                    }
                    if (tempSession.AdditionalValue != "true")
                    {
                        await AddTransactionProfileStatus(transactionId, "FAILED", tempSession.AdditionalValue, "", -1);
                        //await UpdateTransactionProfileConsent(transactionId, "NA", "FAILED");
                        return new APIResponse(tempSession.AdditionalValue);

                    }
                    foreach (var reqScope in Scopes)
                    {
                        foreach (var approvedScope in tempSession.allowedScopesAndClaims)
                        {
                            List<Attributes> attributes = new List<Attributes>();
                            var scope = await _scopeService.GetScopeIdByNameAsync(approvedScope.name);
                            var id = scope.ToString();
                            if ((reqScope == id) &&
                                approvedScope.claimsPresent)
                            {
                                foreach (var claim in approvedScope.claims)
                                {
                                    userApprovedClaims.Add(claim);
                                }
                            }
                        }
                    }
                }
            }
            ApprovedClaims = new HashSet<string>(userApprovedClaims);

            await UpdateTransactionProfileConsent(transactionId, string.Join(",", ApprovedClaims), "");

            Dictionary<string, object> response = new Dictionary<string, object>();

            foreach (var item in ApprovedClaims)
            {
                if (userProfile.ContainsKey(item))
                {
                    response[item] = userProfile[item];
                }
            }
            APIResponse aPIResponse = new APIResponse()
            {
                Success = true,
                Result = response,
                Message = "Get User Profile Success"
            };

            await AddTransactionProfileStatus(transactionId, "SUCCESS", "", "", -1);
            _logger.LogDebug("<--GetUserProfileDataAsync");
            return aPIResponse;
        }

        public async Task<ServiceResult> GetProfile(string[] scopes, string documentNumber)
        {
            HttpClient client = _httpClientFactory.CreateClient();

            var raSubscriber = await GetUserBasicProfileAsync(documentNumber,
                    "PASSPORT", null);

            if (null == raSubscriber)
            {
                _logger.LogError("Subscriber details not found");
                return new ServiceResult(false, "User not Found");
            }

            var userDetails = new Dictionary<string, object>();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None
            };

            JObject keyValuePairs = JObject.Parse(JsonConvert.SerializeObject(raSubscriber, settings));

            foreach (var keyValue in keyValuePairs.Properties())
            {
                userDetails[keyValue.Name] = keyValue.Value.ToString();
            }

            if (scopes.Contains("kycprofile"))
            {
                var kycurl = configuration["KycUrl"];
                kycurl += documentNumber;
                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(kycurl);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {kycurl} failed with status code {result.StatusCode}");

                        return new ServiceResult(false, "Internal error");
                    }
                }
                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new ServiceResult(false, "Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<KycResponse>(responseString);
                if (apiResponse == null)
                {
                    return new ServiceResult(false, "Internal error");
                }
                if (!apiResponse.success)
                {
                    return new ServiceResult(false, apiResponse.message);
                }
                userDetails["kyc_document"] = apiResponse.result;
            }
            if (scopes.Contains("ekycprofile"))
            {
                var kycurl = configuration["EKycUrl"];

                kycurl += documentNumber;

                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(kycurl);
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Request to {kycurl} failed with status code {result.StatusCode}");
                        return new ServiceResult(false, "Internal error");
                    }
                }

                catch (Exception)
                {

                    _logger.LogError("Failed to connect Datapivot");
                    return new ServiceResult(false, "Internal error");
                }

                var responseString = await result.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<KycResponse>(responseString);
                if (apiResponse == null)
                {
                    return new ServiceResult(false, "Internal error");
                }
                if (!apiResponse.success)
                {
                    return new ServiceResult(false, apiResponse.message);
                }
                userDetails["Ekyc_document"] = apiResponse.result;
            }
            return new ServiceResult(true, "Success", userDetails);
        }

        public async Task<ServiceResult> GetAgentDetails(GetAgentDetailsDTO request)
        {
            _logger.LogInformation("Get Agent Details Started");

            var raSubscriber = await _unitOfWork.Subscriber.GetSubscriberInfobyDocType(request.Agent);
            if (raSubscriber == null)
            {
                return new ServiceResult(false, "Agent Not Found");
            }

            var tempAuthNSessId = string.Empty;
            try
            {
                tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            }
            catch (Exception error)
            {
                _logger.LogError(error.Message, error);
                return new ServiceResult(false, OIDCConstants.InternalError);
            }
            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = raSubscriber.SubscriberUid,
                DisplayName = raSubscriber.DisplayName,
                PrimaryAuthNSchemeList = new List<string> { "DeviceAuthentication" },
                AuthNSuccessList = new List<string>(),
                Clientdetails = null,
                IpAddress = string.Empty,
                UserAgentDetails = string.Empty,
                TypeOfDevice = string.Empty,
                MacAddress = DTInternalConstants.NOT_AVAILABLE,
                withPkce = false,
                AdditionalValue = DTInternalConstants.pending,
                AllAuthNDone = false,
                AuthNStartTime = DateTime.Now.ToString("s"),
                CoRelationId = Guid.NewGuid().ToString()
            };

            var task = await _cacheClient.Add(CacheNames.TemporarySession,
                tempAuthNSessId, temporarySession);
            if (DTInternalConstants.S_OK != task.retValue)
            {
                return new ServiceResult(false, OIDCConstants.InternalError);
            }
            var walletDelegationNotification = new WalletDelegationNotification
            {
                AuthnScheme = "DeviceAuthentication",
                AuthnToken = tempAuthNSessId,
                RegistrationToken = raSubscriber.FcmToken,
                Principal = request.Principal,
                DelegationPurpose = request.DelegationPurpose,
                NotaryInformation = request.NotaryInformation,
                ValidityPeriod = request.ValidityPeriod,
                Context = "POADelegation"
            };
            try
            {
                var result = await _pushNotificationClient.SendWalletDelegationNotification(
                    walletDelegationNotification);
                if (null == result)
                {
                    return new ServiceResult(false, "Failed to send Notification");
                }
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Failed to send Notification");
            }
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now.AddMinutes(2);
            bool isError = false;
            string message = string.Empty;
            TemporarySession tempSession = null;

            while (0 > DateTime.Compare(date1, date2))
            {
                // Check whether the temporary session exists
                var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                    tempAuthNSessId);
                if (CacheCodes.KeyExist != isExists.retValue)
                {
                    _logger.LogError("Temporary Session Expired/Not Found");
                    message = OIDCConstants.InternalError;
                    isError = true;
                    break;
                }

                tempSession = await _cacheClient.Get<TemporarySession>
                    (CacheNames.TemporarySession,
                    tempAuthNSessId);
                if (null == tempSession)
                {
                    _logger.LogError("Get Temporary Session Failed,Expired/Not Found");
                    message = OIDCConstants.InternalError;
                    isError = true;
                    break;
                }

                if (!tempSession.AdditionalValue.Equals(DTInternalConstants.pending))
                {
                    _logger.LogInformation("Consent Status :" + temporarySession.AdditionalValue);
                    break;
                }


                await Task.Delay(500);
                date1 = DateTime.Now;
            }
            if (isError)
            {
                return new ServiceResult(false, "Internal Error");
            }
            if (tempSession.AdditionalValue == DTInternalConstants.pending)
            {
                return new ServiceResult(false, "Request Timed Out");
            }
            if (tempSession.AdditionalValue != DTInternalConstants.S_True)
            {
                return new ServiceResult(false, tempSession.AdditionalValue);
            }
            Dictionary<string, string> agentDetails = new Dictionary<string, string>();

            agentDetails["agentName"] = raSubscriber.DisplayName;

            agentDetails["agentEmail"] = raSubscriber.Email;

            agentDetails["agentSuid"] = raSubscriber.SubscriberUid;

            _logger.LogInformation("Get Agent Details Ended");

            return new ServiceResult(true, "Get Agent Details Success", agentDetails);
        }

    }
}