using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Enums;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using FirebaseAdmin.Auth;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ocsp;

//using Org.BouncyCastle.Asn1.Cms;
//using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;
using static DTPortal.Common.EncryptionLibrary;

namespace DTPortal.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;
        private readonly IPushNotificationClient _pushNotificationClient;
        private readonly ITokenManager _tokenManager;
        private readonly ILogClient _LogClient;
        private readonly IPKIServiceClient _pkiServiceClient;
        private readonly IRAServiceClient _raServiceClient;
        private readonly SSOConfig ssoConfig;
        private readonly MessageConstants Constants;
        private readonly OIDCConstants OIDCConstants;
        private readonly IFido2 _fido2;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IAssertionValidationClient _assertionValidationClient;
        private readonly IIpRestriction _iPRestriction;
        private readonly IConfiguration configuration;
        private readonly IHelper _helper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ThresholdConfiguration thresholdConfiguration;
        private readonly ITransactionProfileRequestService _transactionProfileRequestService;
        private readonly ITransactionProfileConsentService _transactionProfileConsentService;
        private readonly ITransactionProfileStatusService _transactionProfileStatusService;
        private readonly IEConsentService _econsentService;
        private readonly IScopeService _scopeService;
        private readonly IAuthSchemeSevice _authSchemeService;
        private readonly IUserInfoService _userInfoService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            IUnitOfWork unitOfWork,
            ICacheClient cacheClient,
            IPushNotificationClient pushNotificationClient,
            ITokenManager tokenManager,
            ILogClient LogClient,
            IFido2 fido2,
            IPKIServiceClient pkiServiceClient,
            IRAServiceClient raServiceClient,
            IGlobalConfiguration globalConfiguration,
            IAssertionValidationClient assertionValidationClient,
            IIpRestriction iPRestriction,
            IConfiguration Configuration,
            IHttpClientFactory httpClientFactory,
            IHelper helper,
            ITransactionProfileStatusService transactionProfileStatusService,
            ITransactionProfileRequestService transactionProfileRequestService,
            ITransactionProfileConsentService transactionProfileConsentService,
            IEConsentService eConsentService,
            IScopeService scopeService,
            IAuthSchemeSevice authSchemeService,
            IHttpContextAccessor httpContextAccessor,
            IUserInfoService userInfoService
            )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cacheClient = cacheClient;
            _pushNotificationClient = pushNotificationClient;
            _tokenManager = tokenManager;
            _LogClient = LogClient;
            _pkiServiceClient = pkiServiceClient;
            _raServiceClient = raServiceClient;
            _iPRestriction = iPRestriction;
            configuration = Configuration;
            _helper = helper;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            _fido2 = fido2;
            _globalConfiguration = globalConfiguration;
            _assertionValidationClient = assertionValidationClient;
            _transactionProfileConsentService = transactionProfileConsentService;
            _transactionProfileRequestService = transactionProfileRequestService;
            _transactionProfileStatusService = transactionProfileStatusService;
            _econsentService = eConsentService;
            _scopeService = scopeService;
            _authSchemeService = authSchemeService;
            _userInfoService = userInfoService;
            // Get SSO Configuration
            ssoConfig = _globalConfiguration.GetSSOConfiguration();
            if (null == ssoConfig)
            {
                _logger.LogError("Get SSO Configuration failed");
                throw new NullReferenceException();
            }

            var errorConfiguration = _globalConfiguration.
                GetErrorConfiguration();
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            Constants = errorConfiguration.Constants;
            if (null == Constants)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }
            OIDCConstants = errorConfiguration.OIDCConstants;
            if (null == OIDCConstants)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }
            thresholdConfiguration = _globalConfiguration.GetThresholdConfiguration();
            if (null == thresholdConfiguration)
            {
                _logger.LogError("Get Threshold Configuration failed");
                throw new NullReferenceException();
            }
        }

        private string StringToBase64(string input)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
        }


        public string GetOSFromUserAgent(string userAgent)

        {

            if (string.IsNullOrEmpty(userAgent))

                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("windows nt"))

                return "Windows";

            if (userAgent.Contains("mac os x") && !userAgent.Contains("iphone") && !userAgent.Contains("ipad"))

                return "MacOS";

            if (userAgent.Contains("android"))

                return "Android";

            if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))

                return "iOS";

            if (userAgent.Contains("linux"))

                return "Linux";

            return "Unknown";

        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [NonAction]
        async Task<int> SendAdminLog(string moduleName,
            string serviceName,
            string activityName,
            string logMessageType,
            string logMessage,
            string FullName,
            string dataTransformation = null)
        {
            AdminLogMessage adminLogMessage
                = new AdminLogMessage(moduleName,
                serviceName,
                activityName,
                logMessage,
                logMessageType,
                FullName,
                dataTransformation);

            string logWithChecksum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(adminLogMessage,
               new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

            return await _LogClient.SendAdminLogMessage(logWithChecksum);
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static string Base64Encode(string plainText)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch
            {
                return null;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static string Base64Decode(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String
                    (base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch
            {
                return null;
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Generate base 64 encoded string of random byte array
        public async Task<string> GetFaceById(string Suid)
        {
            var user = await _unitOfWork.Subscriber.GetSubscriberInfoBySUID(Suid);
            _logger.LogDebug("-->GetSubscriberPhoto");

            // Local Variable Declaration
            string response = null;
            var errorMessage = string.Empty;

            if (user == null || string.IsNullOrEmpty(user.SelfieUri))
            {
                _logger.LogError("Get face : Invalid Input Parameter");
                return response;
            }

            //_logger.LogDebug("Photo Url: {0}", url);
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();

                // Call the webservice with Get method
                using var result = await client.GetAsync(user.SelfieUri);

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
        public async Task<string> GetSubscriberPhoto(string url)
        {
            _logger.LogDebug("-->GetSubscriberPhoto");

            // Local Variable Declaration
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
                using var result = await client.GetAsync(url);

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
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<TransactionProfileConsentResponse> UpdateTransactionProfileConsent
            (int transactionId, string approvedAttributes, string ConsentStatus)
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
                var response = await _transactionProfileConsentService.UpdateTransactionConsent(transactionprofileConsent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Update TransactionProfileConsent Failed   " + ex.Message);
                return null;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<string> GetFaceByUrl(string SelfieUri)
        {
            _logger.LogInformation("-->GetSubscriberPhoto");

            // Local Variable Declaration
            string response = null;
            var errorMessage = string.Empty;

            if (string.IsNullOrEmpty(SelfieUri))
            {
                _logger.LogError("Get face : Invalid Input Parameter");
                return response;
            }

            //_logger.LogDebug("Photo Url: {0}", url);
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();

                var result = await client.GetAsync(SelfieUri);

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
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> GetVerifierUrl()
        {
            var client = _httpClientFactory.CreateClient();

            var url = configuration["VerifierUrl"];
            var SelectedClaims = new SelectedClaims()
            {
                Document = new List<string> { "idDocNumber", "email" }
            };
            var verifierRequestDTO = new VerifierRequestDTO()
            {
                Type = "UAEIDCredential",
                SelectedClaims = SelectedClaims,
                scope = "UAEIDCredential",
                clientID = ""
            };

            var content = new StringContent(JsonConvert.SerializeObject(verifierRequestDTO), Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(url, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                return new Response()
                {
                    Success = apiResponse.Success,
                    Message = apiResponse.Message,
                    Result = apiResponse.Result != null ? apiResponse.Result.ToString() : null
                };
            }
            else
            {
                _logger.LogError($"Request to {url} failed with status code {response.StatusCode}");
                return new Response()
                {
                    Success = false,
                    Message = "Internal Error",
                    Result = null
                };
            }
        }
        private async Task<Response> VerifyFace(string authData, string id)
        {
            Response response1 = new Response();
            try
            {
                var raUser = await _unitOfWork.Subscriber.GetSubscriberInfoBySUID(id);
                if (raUser == null)
                {
                    response1.Success = false;
                    response1.Message = "Subscriber not found";
                    return response1;
                }

                var SubcriberFace = raUser.Selfie;

                if (string.IsNullOrEmpty(SubcriberFace))
                {
                    response1.Success = false;
                    response1.Message = "Subscriber Face Not Found";
                    return response1;
                }

                //HttpClient client = new HttpClient();

                var faceDTO = new FaceAuthenticationDTO()
                {
                    image1 = authData,
                    image2 = SubcriberFace
                };

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", "YWRtaW46SzlyemdyNDd3eg==");

                //client.DefaultRequestHeaders.Add("Authorization", "Basic YWRtaW46SzlyemdyNDd3eg==");
                var url = configuration["FaceVerifyUrl"];
                string json = JsonConvert.SerializeObject(faceDTO);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await client.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        response1.Success = true;
                        response1.Message = apiResponse.Message;
                    }
                    else
                    {
                        response1.Success = false;
                        response1.Message = Constants.FaceVerifyFailed;
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    response1.Success = false;
                    response1.Message = "Internal Error";
                    _logger.LogError(response.StatusCode.ToString());
                }
                return response1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                response1.Success = false;
                response1.Message = "Internal Error";
                return response1;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private string GenerateRandomNumber(int bytes)
        {
            _logger.LogDebug("-->GenerateRandomNumber");
            var randomNumber = string.Empty;

            // Instantiate random number generator 
            Random rand = new Random();

            // Validate input
            if (0 == bytes)
            {
                _logger.LogError("GenerateRandomNumber failed: Input parameter not" +
                    " received");
                return null;
            }

            // Instantiate an array of byte 
            Byte[] b = new Byte[bytes];

            try
            {
                rand.NextBytes(b);

                randomNumber = Convert.ToBase64String(b);
            }
            catch (Exception error)
            {
                _logger.LogError("GenerateRandomNumber failed: {0}", error.Message);
                return null;
            }

            _logger.LogInformation("GenerateRandomNumber Response:{0}",
                randomNumber);
            _logger.LogDebug("<--GenerateRandomNumber");
            return randomNumber;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [NonAction]
        public double CalculateTime(string startTime)
        {
            try
            {
                // Convert string to DateTime Object
                CultureInfo provider = CultureInfo.InvariantCulture;

                // Parse Start Date Time
                DateTime startDateTime = DateTime.ParseExact(startTime,
                new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss" }, provider,
                DateTimeStyles.None);

                // Current Date Time
                DateTime endDateTime = DateTime.Now;

                // Calculate Total Time
                TimeSpan totalTime = endDateTime - startDateTime;

                _logger.LogInformation("Start Date Time:{0}", startDateTime);
                _logger.LogInformation("End Date Time:{0}", endDateTime);
                _logger.LogInformation("Total Time in Seconds:{0}",
                    totalTime.TotalSeconds);

                return Math.Round(totalTime.TotalSeconds, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Calculate Time Failed:{0}", ex.Message);
                return -1;
            }
        }
        public async Task<int> AuthenticateClient(string credentials)
        {
            _logger.LogDebug("-->AuthenticateClient");

            int result = -1;

            // Validate input
            if (null == credentials)
            {
                _logger.LogError("encoding.GetString failed : Input parameter not" +
                    " received");
                return result;
            }

            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String
                    (credentials));
            }
            catch (Exception error)
            {
                _logger.LogError("encoding.GetString failed : {0}", error.Message);
                return result;
            }

            int separator = credentials.IndexOf(':');
            if (-1 == separator)
            {
                _logger.LogError("credentials not received");
                return result;
            }

            string clientId = credentials.Substring(0, separator);
            string clientSecret = credentials.Substring(separator + 1);

            var client = await _unitOfWork.Client.GetClientByClientIdAsync
                (clientId);
            if (null == client)
            {
                _logger.LogError("GetClientByClientIdAsync failed : not found");
                return result;
            }
            else
            {
                if (client.ClientSecret != clientSecret)
                {
                    _logger.LogError("Client secret not matched");
                    return result;
                }
            }
            _logger.LogDebug("<--AuthenticateClient");
            return 0;
        }
        public async Task<APIResponse> VerifyQr(string code)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var url = configuration["VerificationUrl"];

                url += code;

                using var response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());

                    return apiResponse;
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                               $"with status code={response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Validate the authorization code and other parameters 
        [NonAction]
        private async Task<GetAccessTokenErrResponse> ValidateAuthZCodeDetails(
            GetAccessTokenRequest getAccessTokenReq
            )
        {
            _logger.LogDebug("-->ValidateAuthZCodeDetails");

            GetAccessTokenErrResponse response = new GetAccessTokenErrResponse();

            // Validate input
            if (null == getAccessTokenReq)
            {
                _logger.LogError("encoding.GetString failed : Input parameter not" +
                        " received");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            Authorizationcode authzCode = null;

            try
            {
                // Get the authorization code record
                authzCode = await _cacheClient.Get<Authorizationcode>
                    ("AuthorizationCode", getAccessTokenReq.code);
                if (null == authzCode)
                {
                    response.error = "invalid_grant,";
                    response.error_description = "codeNotFound/expiredCode";
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Authorization Code Record");
                response.error = OIDCConstants.InternalError;
                response.error_description = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_AUTHZ_CODE_GET_FAILED);
                return response;
            }

            // Validate the redirect_uri
            if (authzCode.RedirectUrl != authzCode.RedirectUrl)
            {
                response.error = "invalid_grant,";
                response.error_description = "redirectUriMismatch";
                return response;
            }

            _logger.LogInformation("ValidateAuthZCodeDetails Response:{0}",
                response);
            _logger.LogDebug("<--ValidateAuthZCodeDetails");
            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [NonAction]
        private async Task<AccessTokenResponse> GenerateAccessToken(
            string AuthorizationCode)
        {
            _logger.LogDebug("-->GenerateAccessToken");
            AccessTokenResponse response = new AccessTokenResponse();
            var credentials = string.Empty;
            var accessToken = string.Empty;

            // Validate input
            if (null == AuthorizationCode)
            {
                _logger.LogError("encoding.GetString failed : Input parameter not" +
                        " received");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            // Encode authorization code
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String
                    (AuthorizationCode));
            }
            catch (Exception error)
            {
                _logger.LogError("encoding.GetString failed : {0}", error.Message);
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            int separator = credentials.IndexOf(':');
            if (-1 == separator)
            {
                _logger.LogError("credentials not received");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            string clientId = credentials.Substring(0, separator);
            string clientSecret = credentials.Substring(separator + 1);

            ClientKey client = new ClientKey
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                CreatedOn = DateTime.Now
            };

            // Get encryption keys
            var encKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            if (null == encKey)
            {
                _logger.LogError("EncDecKeys.GetByIdAsync failed, not found");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            // Generate access token
            try
            {
                accessToken = EncryptionLibrary.GenerateToken(client, DateTime.Now,
                    encKey.Key1.ToString(), "appshield");
            }
            catch (Exception error)
            {
                _logger.LogError("GenerateToken failed: {0}", error.Message);
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            response.accessToken = accessToken;
            response.error = "";

            _logger.LogInformation("GenerateAccessToken Response:{0}", response);
            _logger.LogDebug("<--GenerateAccessToken");
            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [NonAction]
        private async Task<AccessTokenResponse> GeneratePrivateKeyJwtAccessToken
            (string AuthorizationCode)
        {
            _logger.LogDebug("-->GeneratePrivateKeyJwtAccessToken");

            AccessTokenResponse response = new AccessTokenResponse();
            var credentials = string.Empty;
            var accessToken = string.Empty;

            // Validate input
            if (null == AuthorizationCode)
            {
                _logger.LogError("encoding.GetString failed : Input parameter not" +
                        " received");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            // Encode authorization code
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String
                    (AuthorizationCode));
            }
            catch (Exception error)
            {
                _logger.LogError("encoding.GetString failed : {0}", error.Message);
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            ClientKey client = new ClientKey
            {
                ClientId = AuthorizationCode
            };

            var encKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            if (null == encKey)
            {
                _logger.LogError("EncDecKeys.GetByIdAsync failed, not found");
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            // Generate access token
            try
            {
                accessToken = EncryptionLibrary.GenerateToken(client, DateTime.Now,
                    encKey.Key1.ToString(), "appshield");
            }
            catch (Exception error)
            {
                _logger.LogError("GenerateToken failed: {0}", error.Message);
                response.error = "internal error";
                response.error_description = "internal error";
                return response;
            }

            response.accessToken = accessToken;
            response.error = string.Empty;

            _logger.LogInformation("GeneratePrivateKeyJwtAccessToken Response:{0}",
                response);
            _logger.LogDebug("<--GeneratePrivateKeyJwtAccessToken");
            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static string ComputeCodeChallenge(string code_verifier)
        {
            var code_challenge = "";

            // Validate input
            if (null == code_verifier)
            {
                return null;
            }

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var challengeBytes = sha256.ComputeHash(Encoding.UTF8.
                        GetBytes(code_verifier));
                    code_challenge = Convert.ToBase64String(challengeBytes)
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
                }
            }
            catch (Exception)
            {
                return null;
            }

            return code_challenge;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private string GenerateRandomOTP(int iOTPLength)
        {
            _logger.LogDebug("-->GenerateRandomOTP");

            // Validate input
            if (0 == iOTPLength)
            {
                _logger.LogError("Invalid Input Parameter");
                return null;
            }

            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6",
                    "7", "8", "9", "0" };

            string sOTP = String.Empty;

            string sTempChars = String.Empty;

            Random rand = new Random();

            try
            {
                for (int i = 0; i < iOTPLength; i++)
                {

                    int p = rand.Next(0, saAllowedCharacters.Length);

                    sTempChars = saAllowedCharacters[rand.Next(0,
                        saAllowedCharacters.Length)];

                    sOTP += sTempChars;

                }
            }
            catch (Exception error)
            {
                _logger.LogError("GenerateRandomOTP failed: {0}", error.Message);
                return null;
            }

            _logger.LogDebug("<--GenerateRandomOTP");
            return sOTP;

        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private IList<string> GetRandomNosList(int size, int otpLength)
        {
            var res = new List<string>(size);

            for (int i = 0; i < size; i++)
            {
                var temp = GenerateRandomOTP(otpLength);
                if (null == temp)
                {
                    _logger.LogError("GenerateRandomOTP failed");
                    return null;
                }

                if (res.Distinct().Count() != res.Count())
                {
                    size++;
                }
                else
                {
                    res.Add(temp);
                }
            }

            return res;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private int GetRandomIndex(int maxNumber)
        {
            // shuffle random codes
            var r = new Random();
            return r.Next(maxNumber);
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            _logger.LogDebug("-->TimeBetween");

            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;

            // see if start comes before end
            if (start < end)
            {
                return start <= now && now <= end;
            }

            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private async Task<bool> CheckTimeRestrictionforUser(int userId)
        {
            _logger.LogDebug("-->CheckTimeRestrictionforUser");

            // Validate input
            if (0 == userId)
            {
                _logger.LogError("Invalid Input Parameter");
                return false;
            }

            var timeBasedAccessList = await _unitOfWork.TimeBasedAccess.
                EnumActiveTimeBasedAccess();
            if (null == timeBasedAccessList)
            {
                _logger.LogError("EnumActiveTimeBasedAccess failed, not found");
                return false;
            }

            var userInDb = await _unitOfWork.Users.GetUserByIdWithRoleAsync(userId);
            if (null == userInDb)
            {
                _logger.LogError("GetUserByIdWithRoleAsync failed, not found");
                return false;
            }

            foreach (var item in timeBasedAccessList)
            {
                string[] roles = item.ApplicableRoles.Split(',');

                if (roles.Contains(userInDb.Role.Name))
                {
                    _logger.LogInformation("START TIME: {0}", item.StartTime.ToString());
                    _logger.LogInformation("END TIME: {0}", item.EndTime.ToString());
                    _logger.LogInformation("NOW TIME: {0}", DateTime.Now.ToString());

                    _logger.LogInformation("START DATE: {0}", item.StartDate.ToString());
                    _logger.LogInformation("END DATE: {0}", item.EndDate.ToString());
                    _logger.LogInformation("TIME OF DAY: {0}", DateTime.Now.TimeOfDay.ToString());

                    // convert everything to TimeSpan
                    TimeSpan start = item.StartTime.Value.ToTimeSpan();
                    TimeSpan end = item.EndTime.Value.ToTimeSpan();
                    TimeSpan now = DateTime.Now.TimeOfDay;

                    if (!item.EndDate.HasValue)
                    {
                        item.EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                        _logger.LogInformation("END DATE: {0}", item.EndDate.ToString());
                    }

                    DateOnly StartDate = (DateOnly)item.StartDate;
                    DateOnly EndDate = (DateOnly)item.EndDate;

                    DateTime sDate = StartDate.ToDateTime(TimeOnly.Parse("12:00 AM"));
                    DateTime eDate = EndDate.ToDateTime(TimeOnly.Parse("12:00 AM"));

                    if (DateTime.Now > Convert.ToDateTime(sDate) &&
                        DateTime.Now < Convert.ToDateTime(eDate))
                    {
                        var isTrue = TimeBetween(DateTime.Now, start, end);
                        if (true == isTrue)
                        {
                            return true;
                        }
                    }
                }
            }

            _logger.LogInformation("CheckTimeRestrictionforUser Response:{0}",
                false);
            _logger.LogDebug("<--CheckTimeRestrictionforUser");
            return false;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public Response ValidateClient(ValidateClientRequest requestObj)
        {
            _logger.LogDebug("--->ValidateClient");

            // Variable Declaration
            Response response = new Response();
            var ConfigObject = new JObject();
            var count = 0;

            // Validate input
            if (null == requestObj)
            {
                _logger.LogError("Invalid Input Parameter");
                response.Success = false;
                response.Message = "Invalid Input";
                return response;
            }

            if (StatusConstants.ACTIVE != requestObj.clientDetailsInDb.Status)
            {
                _logger.LogError(OIDCConstants.ClientNotActive);
                response.Success = false;
                response.Message = OIDCConstants.ClientNotActive;
                return response;
            }

            // Validate client scopes
            var Scopes = requestObj.clientDetails.scopes.Split(new char[]
            { ' ', '\t' });
            if (0 == Scopes.Length)
            {
                _logger.LogError("scopes not received");
                response.Success = false;
                response.Message = "client scopes not found";
                return response;
            }

            var clientScopes = requestObj.clientDetailsInDb.Scopes.Split(
                new char[] { ' ', '\t' });
            if (0 == clientScopes.Length)
            {
                _logger.LogError("scopes not received from client");
                response.Success = false;
                response.Message = "client scopes not found";
                return response;
            }

            foreach (var item in Scopes)
            {
                if (clientScopes.Contains(item))
                {
                    count++;
                }
            }
            if (count != Scopes.Length)
            {
                _logger.LogError("client scopes not matched");
                response.Success = false;
                response.Message = "Client scopes not matched";
                return response;
            }

            // Validate response types
            var Responsetypes = requestObj.clientDetails.response_type.Split
                (new char[] { ' ', '\t' });
            var clientResponseTypes = requestObj.clientDetailsInDb.ResponseTypes.Split
                (new char[] { ' ', '\t' });
            count = 0;
            foreach (var item in Responsetypes)
            {
                if (clientResponseTypes.Contains(item))
                {
                    count++;
                }
            }
            if (count != Responsetypes.Length)
            {
                _logger.LogError("Client response types not matched");
                response.Success = false;
                response.Message = "Client response types not matched";
                return response;
            }

            // Validate redirecturi
            if (requestObj.clientDetailsInDb.RedirectUri !=
                requestObj.clientDetails.redirect_uri)
            {
                _logger.LogError("Client RedirectUri not matched");
                response.Success = false;
                response.Message = "Client RedirectUri not matched";
                return response;
            }

            response.Success = true;
            response.Message = "Client details are matched";
            response.Result = requestObj.clientDetailsInDb.ApplicationName;
            _logger.LogDebug("<--ValidateClient");
            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private async Task<Response> VerifyPassword(string authData, string id)
        {
            Response response = new Response();

            // Get UserAuthData
            var userAuthData =
                await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                (id, AuthNSchemeConstants.PASSWORD);
            if (null == userAuthData)
            {
                _logger.LogError("GetUserAuthDataAsync failed, not found");
                response.Success = false;
                response.Message = "User authentication data not found";
                return response;
            }

            string encryptionPassword = string.Empty;
            var DecryptedPasswd = string.Empty;

            // Get Encryption Key
            var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(
                DTInternalConstants.ENCDEC_KEY_ID);
            if (null == EncKey)
            {
                _logger.LogError("EncDecKeys.GetByIdAsync failed, not found");
                response.Success = false;
                response.Message = Constants.InternalError;

                return response;
            }

            try
            {
                encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
            }
            catch (Exception error)
            {
                _logger.LogError("Encoding.UTF8.GetString:{0}",
                    error.Message);
                response.Success = false;
                response.Message = Constants.InternalError;
            }

            try
            {
                // Decrypt Password
                DecryptedPasswd = EncryptionLibrary.DecryptText(
                    userAuthData.AuthData,
                    encryptionPassword,
                    DTInternalConstants.PasswordSalt);
            }
            catch (Exception error)
            {
                _logger.LogError("DecryptText failed, found exception: {0}",
                    error.Message);
                response.Success = false;
                response.Message = Constants.InternalError;
            }

            // Compare password
            if (authData != DecryptedPasswd)
            {
                _logger.LogError("PASSWORD: Wrong credentials");
                response.Success = false;
                response.Message = Constants.WrongCredentials;

                return response;
            }
            else
            {
                _logger.LogInformation("PASSWORD: Credentials matched");
                response.Success = true;

                return response;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private async Task<Response> VerifyFido2(string authData, string id)
        {
            Response response = new Response();

            // Get UserAuthData
            var userAuthData =
                await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                (id, AuthNSchemeConstants.FIDO2);
            if (null == userAuthData)
            {
                _logger.LogError("GetUserAuthDataAsync failed, not found");
                response.Success = false;
                response.Message = "User authentication data not found";
                return response;
            }

            var userTempAuthData = await _unitOfWork.UserAuthData.
                GetUserTempAuthDataAsync(id,
                AuthNSchemeConstants.FIDO2);
            if (null != userTempAuthData)
            {
                if (userTempAuthData.Status == StatusConstants.HOLD)
                {
                    TimeSpan timediff = (TimeSpan)(DateTime.Now -
                        userTempAuthData.Expiry);
                    if (timediff.Hours > 24)
                    {
                        userTempAuthData.Status = StatusConstants.EXPIRED;

                        try
                        {
                            _unitOfWork.UserAuthData.Update(userTempAuthData);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserTempAuthDataAsync failed" +
                                ",{0}", error.Message);
                            response.Success = false;
                            response.Message = Constants.InternalError;
                        }
                    }
                    else
                    {
                        userAuthData = userTempAuthData;
                    }
                }
            }

            string encryptionPassword = string.Empty;
            string DecryptedPasswd = string.Empty;

            // Get Encryption Key
            var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(
                DTInternalConstants.ENCDEC_KEY_ID);
            if (null == EncKey)
            {
                _logger.LogError("EncDecKeys.GetByIdAsync failed, not found");
                response.Success = false;
                response.Message = Constants.InternalError;

                return response;
            }

            try
            {
                encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
            }
            catch (Exception error)
            {
                _logger.LogError("Encoding.UTF8.GetString:{0}",
                    error.Message);
                response.Success = false;
                response.Message = Constants.InternalError;
            }

            try
            {
                // Decrypt Password
                DecryptedPasswd = EncryptionLibrary.DecryptText(
                    userAuthData.AuthData,
                    encryptionPassword,
                    DTInternalConstants.PasswordSalt);
            }
            catch (Exception error)
            {
                _logger.LogError("DecryptText failed, found exception: {0}",
                    error.Message);
                response.Success = false;
                response.Message = Constants.InternalError;
            }

            var fido2 = authData.Split(new char[] { '#' });
            if (fido2.Count() < 2)
            {
                _logger.LogError("fido2 options not found");
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            var options = AssertionOptions.FromJson(fido2[1]);
            if (options == null)
            {
                _logger.LogError("fido2 options not found");
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            var credential = JsonConvert.DeserializeObject<StoredCredential>
                (DecryptedPasswd);
            if (credential == null)
            {
                _logger.LogError("fido2 options not found");
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            var assertionData = JsonConvert.DeserializeObject
                <AuthenticatorAssertionRawResponse>(fido2[0]);
            if (null == assertionData)
            {
                _logger.LogError("assertionData not found");
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            try
            {
                var result = await _fido2.MakeAssertionAsync(assertionData,
                    options,
                    credential.PublicKey,
                    credential.SignatureCounter,
                    args => Task.FromResult(credential.UserHandle.SequenceEqual
                    (args.UserHandle)));
                if (result.Status.Equals("ok"))
                {

                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = result.Status;
                }
            }
            catch (Exception error)
            {
                _logger.LogError("MakeAssertionAsync failed: {0}",
                    error.Message);

                response.Success = false;
            }

            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private async Task<VerifyPinResponse> VerifyPushNotification(
            TemporarySession tempSession,
            VerifyPinRequest request)
        {
            VerifyPinRequest verifyPinRequest = new VerifyPinRequest();
            verifyPinRequest.subscriberDigitalID = request.subscriberDigitalID;
            verifyPinRequest.authenticationPin = request.authenticationPin;
            verifyPinRequest.correlationId = tempSession.CoRelationId;

            var verifyPinResponse = await _pkiServiceClient.
                VerifyPin(verifyPinRequest);
            if (null == verifyPinResponse)
            {
                _logger.LogError("PKI service error");
                verifyPinResponse = new VerifyPinResponse();
                verifyPinResponse.success = false;
                verifyPinResponse.message = "Pin verification failed at server";
                return verifyPinResponse;
            }

            if ((verifyPinResponse.success == true) &&
                    (DTInternalConstants.SuccessMsg == verifyPinResponse.result))
            {
                _logger.LogDebug("PKI service response: {0}",
                    verifyPinResponse.message);
                return verifyPinResponse;
            }

            else if ((verifyPinResponse.success == false) &&
                   (Constants.PinVerifyFailed == verifyPinResponse.message))
            {
                _logger.LogDebug("PKI service response: {0}",
                     verifyPinResponse.message);
                return verifyPinResponse;
            }

            return verifyPinResponse;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private VerifyPinResponse VerifyFace(string authData)
        {
            _logger.LogInformation("---- Face Verification Started -----");
            var verifyPinResponse = new VerifyPinResponse();

            try
            {
                var decryptData = PKIMethods.Instance.PKIDecryptSecureWireData(authData);
                if (decryptData == null)
                {
                    _logger.LogError("Face Match Request : Failed to Decrypt Data");

                    verifyPinResponse.success = false;
                    verifyPinResponse.message = "Internal Error";
                    return verifyPinResponse;
                }

                var verifyFaceDto = JsonConvert.DeserializeObject<FaceVerifyRequest>(decryptData);

                if (null == verifyFaceDto)
                {
                    _logger.LogError("Face Match Request : Failed to Deserialize Object");

                    verifyPinResponse.success = false;
                    verifyPinResponse.message = "Internal Error";
                    return verifyPinResponse;
                }
                var faceMatchScore = (verifyFaceDto.faceMatchScore) * 100;

                if (verifyFaceDto.OS == "ANDROID")
                {
                    _logger.LogInformation("Threshold {0} , Face Match Score {1}", thresholdConfiguration.Android_Threshold, faceMatchScore);

                    if (faceMatchScore < thresholdConfiguration.Android_Threshold)
                    {
                        verifyPinResponse.success = true;
                        verifyPinResponse.message = "Verify Face Success";
                        return verifyPinResponse;
                    }
                    else
                    {
                        verifyPinResponse.success = false;
                        verifyPinResponse.message = Constants.FaceVerifyFailed;
                        return verifyPinResponse;
                    }
                }
                else if (verifyFaceDto.OS == "IOS")
                {
                    _logger.LogInformation("Threshold {0} , Face Match Score {1}", thresholdConfiguration.Ios_Threshold, faceMatchScore);

                    if (faceMatchScore < thresholdConfiguration.Ios_Threshold)
                    {
                        verifyPinResponse.success = true;
                        verifyPinResponse.message = "Verify Face Success";
                        return verifyPinResponse;
                    }
                    else
                    {
                        verifyPinResponse.success = false;
                        verifyPinResponse.message = Constants.FaceVerifyFailed;
                        return verifyPinResponse;
                    }
                }
                else
                {
                    verifyPinResponse.success = false;
                    verifyPinResponse.message = "Invalid OS Type";
                    return verifyPinResponse;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Verify Face Failed: {0}", ex.Message);
                verifyPinResponse.success = false;
                verifyPinResponse.message = "Internal Error";
            }

            return verifyPinResponse;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<UserTable> GetAdminUserByType(int requestType,
            string input)
        {
            var user = new UserTable();

            switch (requestType)
            {
                case (int)InputType.emailId:
                    {
                        user = await _unitOfWork.Users.GetUserbyEmailAsync
                            (input);
                        if (null == user)
                        {
                            _logger.LogError("Subscriber details not found");
                            return null;
                        }
                        break;
                    }
                case (int)InputType.phoneno:
                    {
                        user = await _unitOfWork.Users.GetUserbyPhonenoAsync
                            (input);
                        if (null == user)
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
                    }
                    ;
            }

            return user;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<SubscriberView> GetSubscriberbyType(int requestType,
            string input)
        {
            var raSubscriber = new SubscriberView();

            switch (requestType)
            {
                case (int)InputType.emailId:
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByEmail(input);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found : " + input);
                            return null;
                        }
                        break;
                    }
                case (int)InputType.phoneno:
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberInfoByPhone(input);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found : " + input);
                            return null;
                        }
                        break;
                    }
                case (int)InputType.emiratesId:
                    {
                        raSubscriber = await _unitOfWork.Subscriber.
                            GetSubscriberDetailsByEmiratesId(input);
                        if (null == raSubscriber)
                        {
                            _logger.LogError("Subscriber details not found : " + input);
                            return null;
                        }
                        break;
                    }
                default:
                    {
                        _logger.LogError("Incorrect input received");
                        return null;
                    }
                    ;
            }
            return raSubscriber;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<SubscriberRaDatum> GetSubscriberRaData(string suid)
        {
            var raSubscriber = new SubscriberRaDatum();

            raSubscriber = await _unitOfWork.Subscriber.GetSubscriberRaDatumBySuid(suid);

            if (raSubscriber == null)
            {
                return new SubscriberRaDatum();
            }

            return raSubscriber;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<SubscriberView> GetSubscriberbyOrgnizationEmail(string input)
        {

            var raSubscriber = await _unitOfWork.Subscriber.
                 GetSubscriberInfoByOrgnizationEmail(input);
            if (raSubscriber.Count == 0 || raSubscriber.Count > 1)
            {
                _logger.LogError("Subscriber details not found : " + input);
                return null;
            }

            return raSubscriber.First();
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<List<LoginProfile>> GetSubscriberOrgnizationByEmail(string email)
        {
            var list = new List<LoginProfile>();
            var orgEmaillist = await _unitOfWork.OrgnizationEmail.
                 GetSubscriberOrgnizationByEmailAsync(email);
            if (orgEmaillist.Count == 0)
            {
                _logger.LogError("Subscriber details not found");
                return list;
            }

            foreach (var orgEmail in orgEmaillist)
            {
                list.Add(new LoginProfile()
                {
                    Email = email,
                    OrgnizationId = orgEmail.OrganizationUid
                });
            }

            return list;
        }

        public async Task<List<LoginProfile>> GetSubscriberOrgnizationBySuid(string suid)
        {
            var list = new List<LoginProfile>();
            var orgEmaillist = await _unitOfWork.OrgnizationEmail.
                 GetSubscriberOrgnizationBySuidAsync(suid);
            if (orgEmaillist.Count == 0)
            {
                _logger.LogError("Subscriber details not found");
                return list;
            }

            foreach (var orgEmail in orgEmaillist)
            {
                list.Add(new LoginProfile()
                {
                    Email = orgEmail.EmployeeEmail,
                    OrgnizationId = orgEmail.OrganizationUid
                });
            }

            return list;
        }

        public async Task<List<LoginProfile>> GetSubscriberOrgnizationByPhoneNumber(string phoneno)
        {
            var list = new List<LoginProfile>();
            var orgEmaillist = await _unitOfWork.OrgnizationEmail.GetSubscriberOrgnizationByPhoneNumberAsync(phoneno);
            if (orgEmaillist.Count == 0)
            {
                _logger.LogError("Subscriber details not found");
                return list;
            }

            foreach (var orgEmail in orgEmaillist)
            {
                list.Add(new LoginProfile()
                {
                    Email = orgEmail.EmployeeEmail,
                    OrgnizationId = orgEmail.OrganizationUid
                });
            }

            return list;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~   
        public async Task<Response> ValidateSession(string globalSessionId)
        {
            _logger.LogDebug("--->ValidateSession");

            // Variable declaration
            Response response = new Response();

            // Validate input
            if (null == globalSessionId)
            {
                _logger.LogError("Invalid input parameter");
                response.Success = false;
                response.Message = "Invalid input parameter";

                return response;
            }
            /*
                        var isExists = await _cacheClient.Exists("GlobalSession",
                            globalSessionId);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                                "not found");
                            response.Success = false;
                            response.Message = "GlobalSession expired/not exists";

                            return response;
                        }
            */
            GlobalSession sessInCache = null;
            try
            {
                sessInCache = await _cacheClient.Get<GlobalSession>("GlobalSession",
                    globalSessionId);
                if (null == sessInCache)
                {
                    _logger.LogError("_cacheClient.Get failed, GlobalSession " +
                        "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                response.Success = false;
                response.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                    ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                return response;
            }

            long loginTicks = long.Parse(sessInCache.LoggedInTime);
            // Compare session timeout
            TimeSpan loggedInTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - loginTicks);

            //var loggedInTime = DateTime.Now - Convert.ToDateTime(globalSession.LoggedInTime);

            long lastAccessTicks = long.Parse(sessInCache.LastAccessTime);
            TimeSpan sessionTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - lastAccessTicks);


            // Compare session timeout
            //var sessionTime = DateTime.Now - Convert.ToDateTime(sessInCache.LastAccessTime);
            //var loggedInTime = DateTime.Now - Convert.ToDateTime(sessInCache.LoggedInTime);
            if (sessionTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout ||
                loggedInTime.TotalMinutes >= ssoConfig.sso_config.session_timeout)
            {
                // Remove global session
                var cacheres = await _cacheClient.Remove("GlobalSession",
                    globalSessionId);
                if (0 != cacheres.retValue)
                {
                    _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                            "remove failed");

                    response.Success = false;
                    response.Message = "Internal server error";

                    return response;
                }

                // globalsession 
                var isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                    sessInCache.UserId);
                if (CacheCodes.KeyExist == isExists.retValue)
                {
                    IList<string> userSessions = null;

                    try
                    {
                        // Get usersessions
                        userSessions = await _cacheClient.Get<IList<string>>(
                            CacheNames.UserSessions,
                            sessInCache.UserId);
                        if (null == userSessions)
                        {
                            _logger.LogError("Failed to get user sessions");
                            response.Success = false;
                            response.Message = Constants.UserSessionNotFound;
                            return response;
                        }
                    }
                    catch (CacheException ex)
                    {
                        _logger.LogError("Failed to get user sessions");
                        response.Success = false;
                        response.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                            ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                        return response;
                    }

                    if (userSessions.Count > 0)
                    {
                        //var res = await _cacheClient.Remove(
                        //    CacheNames.GlobalSession,
                        //    userSessions.First());
                        //if (0 != res.retValue)
                        //{
                        //    _logger.LogError("GlobalSession Remove failed");
                        //    response.Success = false;
                        //    response.Message = Constants.InternalError;
                        //    return response;
                        //}

                        var res = await _cacheClient.Remove(CacheNames.UserSessions,
                            sessInCache.UserId);
                        if (0 != res.retValue)
                        {
                            _logger.LogError("UserSessions Remove failed");
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                }

                _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                response.Success = false;
                response.Message = "GlobalSession expired/not exists";
                return response;
            }

            // Compare ideal timeout
            //var idealTime = DateTime.Now.Ticks.ToString() - Convert.ToDateTime(sessInCache.LastAccessTime);

            TimeSpan idealTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - lastAccessTicks);

            if (idealTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout)
            {

                // Remove global session
                var cacheres = await _cacheClient.Remove("GlobalSession",
                    globalSessionId);
                if (0 != cacheres.retValue)
                {
                    _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                            "remove failed");

                    response.Success = false;
                    response.Message = "Internal server error";

                    return response;
                }


                // globalsession 
                var isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                    sessInCache.UserId);
                if (CacheCodes.KeyExist == isExists.retValue)
                {
                    IList<string> userSessions = null;

                    try
                    {
                        // Get usersessions
                        userSessions = await _cacheClient.Get<IList<string>>(
                            CacheNames.UserSessions,
                            sessInCache.UserId);
                        if (null == userSessions)
                        {
                            _logger.LogError("Failed to get user sessions");
                            response.Success = false;
                            response.Message = Constants.UserSessionNotFound;
                            return response;
                        }
                    }
                    catch (CacheException ex)
                    {
                        _logger.LogError("Failed to get user sessions");
                        response.Success = false;
                        response.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                            ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                        return response;
                    }

                    if (userSessions.Count > 0)
                    {
                        //var res = await _cacheClient.Remove(
                        //    CacheNames.GlobalSession,
                        //    userSessions.First());
                        //if (0 != res.retValue)
                        //{
                        //    _logger.LogError("GlobalSession Remove failed");
                        //    response.Success = false;
                        //    response.Message = Constants.InternalError;
                        //    return response;
                        //}

                        var res = await _cacheClient.Remove(CacheNames.UserSessions,
                            sessInCache.UserId);
                        if (0 != res.retValue)
                        {
                            _logger.LogError("UserSessions Remove failed");
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                }

                _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                response.Success = false;
                response.Message = "GlobalSession expired/not exists";

                return response;
            }

            sessInCache.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

            var cacheRes = await _cacheClient.Add("GlobalSession", globalSessionId,
                sessInCache);
            if (0 != cacheRes.retValue)
            {
                _logger.LogError("_cacheClient.Add failed, GlobalSession");
                response.Success = false;
                response.Message = "Internal server error";

                return response;
            }

            response.Success = true;
            response.Message = "";

            _logger.LogDebug("<--ValidateSession");
            return response;
        }
        public async Task<Response> CustomValidateSession(string globalSessionId)
        {
            _logger.LogDebug("--->ValidateSession");

            // Variable declaration
            Response response = new Response();

            // Validate input
            if (null == globalSessionId)
            {
                _logger.LogError("Invalid input parameter");
                response.Success = false;
                response.Message = "Invalid input parameter";
                return response;
            }
            /*
                        var isExists = _cacheClient.KeyExists("GlobalSession",
                            globalSessionId);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                                "not found");
                            response.Success = false;
                            response.Message = "GlobalSession expired/not exists";

                            return response;
                        }
            */
            GlobalSession globalSession = null;
            try
            {
                // Get GlobalSession
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession,
                    globalSessionId);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    response.Success = false;
                    response.Message = Constants.SessionMismatch;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                response.Success = false;
                response.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                    ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                return response;
            }

            // Compare session timeout
            //var sessionTime = DateTime.Now - Convert.ToDateTime(globalSession.LastAccessTime);

            long loginTicks = long.Parse(globalSession.LoggedInTime);
            // Compare session timeout
            TimeSpan loggedInTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - loginTicks);

            //var loggedInTime = DateTime.Now - Convert.ToDateTime(globalSession.LoggedInTime);

            long lastAccessTicks = long.Parse(globalSession.LastAccessTime);
            TimeSpan sessionTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - lastAccessTicks);

            if (sessionTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout ||
                loggedInTime.TotalMinutes >= ssoConfig.sso_config.session_timeout)
            {
                // Remove global session
                var CacheRes = await _cacheClient.Remove("GlobalSession",
                     globalSession.GlobalSessionId);
                if (0 != CacheRes.retValue)
                {
                    _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                            "remove failed");

                    response.Success = false;
                    response.Message = "Internal server error";

                    return response;
                }

                var res = await _cacheClient.Remove(CacheNames.UserSessions,
                           globalSession.UserId);
                if (0 != res.retValue)
                {
                    _logger.LogError("UserSessions Remove failed");
                    //response.Success = false;
                    //response.Message = Constants.InternalError;
                    //return response;
                }

                _logger.LogError("GlobalSession expired/not exists ");
                response.Success = false;
                response.Message = "GlobalSession expired/not exists";
                return response;
            }

            globalSession.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

            var cacheRes = await _cacheClient.Add("GlobalSession", globalSessionId,
                globalSession);
            if (0 != cacheRes.retValue)
            {
                _logger.LogError("_cacheClient.Add failed, GlobalSession");
                response.Success = false;
                response.Message = "Internal server error";

                return response;
            }

            response.Success = true;
            response.Message = "";

            _logger.LogDebug("ValidateSession Response:{0}", response);
            _logger.LogDebug("<--ValidateSession");
            return response;
        }

        public async Task<(JourneyApiResponse Success, IcpErrorResponse Error)>
            CreateJourney_FromConfig(UserLookupItem userInfo)
        {
            var journey = new JourneyRequest();

            if (userInfo.DocumentType == "1")
            {
                journey.journeyType = configuration["ICP:Journey:journeyType"];
                journey.emiratesIdNumber = userInfo.DocumentId;
            }

            if (userInfo.DocumentType == "2" || userInfo.DocumentType == "3")
            {
                journey.journeyType = configuration["ICP:Journey:journeyType"];
                journey.passportDetails = new PassportDetails
                {
                    passportNumber = userInfo.DocumentId,
                    passportType = configuration["ICP:Journey:passportDetails:passportType"],
                    nationality = userInfo.Nationality
                };
            }

            if (userInfo.DocumentType == "5")
            {
                journey.journeyType = configuration["ICP:Journey:journeyType"];
                journey.uaeKycId = userInfo.DocumentId;
            }

            if (string.IsNullOrEmpty(configuration["ICP:Journey:consent"]))
            {
                journey.consent = configuration["ICP:Journey:consent"];
            }
            else
            {
                journey.consent = "false";
            }

            _logger.LogInformation("Journey - "+ JsonConvert.SerializeObject(journey));

            return await SendJourneyAsync(journey);
        }


        private async Task<(JourneyApiResponse Success, IcpErrorResponse Error)>
            SendJourneyAsync(JourneyRequest journey)
        {
            _logger.LogInformation("Sending journey request to ICP OTK service");
            _logger.LogInformation("Journey Request: "+ JsonConvert.SerializeObject(journey));
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{configuration["ICP:BaseUrl"]}/otk-service/create-journey-url"
            );

            request.Headers.Add(
                "x-transaction-key",
                configuration["ICP:TransactionKey"]
            );

            //request.Content = new StringContent(
            //    JsonConvert.SerializeObject(journey),
            //    Encoding.UTF8,
            //    "application/json"
            //);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(journey, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }),
                Encoding.UTF8,
                "application/json"
            );

            var data = JsonConvert.SerializeObject(journey);
            _logger.LogInformation("Info - "+ data);
            using var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var success = JsonConvert.DeserializeObject<JourneyApiResponse>(json);
                return (success, null);
            }
            else
            {
                var error = JsonConvert.DeserializeObject<IcpErrorResponse>(json);
                return (null, error);
            }
        }


        public async Task<VerifyUserResponse> ICPVerifyUser(VerifyUserRequest request)
        {
            _logger.LogDebug("--->VerifyUser");

            // Input validation
            if (null == request || string.IsNullOrEmpty(request.clientId) ||
                string.IsNullOrEmpty(request.userInput))
            {
                _logger.LogError(Constants.InvalidArguments);
                return new VerifyUserResponse(Constants.InvalidArguments);
            }

            _logger.LogDebug("client id is {0}", request.clientId);
            var errorMessage = string.Empty;
            var verifierUrl = string.Empty;
            var clientInDb = new Client();
            bool isMobileUser = false;
            try
            {
                // Get Client details
                clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(
                    request.clientId);
                if (null == clientInDb)
                {
                    _logger.LogError(OIDCConstants.ClientNotFound);
                    return new VerifyUserResponse(OIDCConstants.ClientNotFound);
                }
            }
            catch (Exception )
            {
                errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                Monitor.SendMessage(errorMessage);
                return new VerifyUserResponse(errorMessage);
            }

            // Check client status
            if (clientInDb.Status != StatusConstants.ACTIVE)
            {
                _logger.LogError("{0}: {1}",
                    OIDCConstants.ClientNotActive, clientInDb.Status);
                return new VerifyUserResponse(OIDCConstants.ClientNotActive);
            }

            UserLookupItem userInfo = new UserLookupItem();
            Domain.Models.UserTable user = new Domain.Models.UserTable();
            var authScheme = new List<string>() { };
            string journeyToken = null;

            // Client id validation[Check for DT ADMIN PORTAL]
            if (request.clientId.Equals(DTInternalConstants.DTPortalClientId))
            {
                _logger.LogInformation("client id is {0} : DT ADMIN PORTAL",
                    request.clientId);

                // Get User information
                user = await GetAdminUserByType(request.type, request.userInput);
                if (null == user)
                {
                    _logger.LogError(Constants.SubscriberNotFound);
                    return new VerifyUserResponse(
                        Constants.SubscriberNotFound);
                }

                userInfo.Suid = user.Uuid;
                userInfo.Id = user.Id;
                userInfo.DisplayName = user.FullName;
                userInfo.Status = user.Status;
                userInfo.DeviceToken = string.Empty;
                userInfo.MobileNumber = user.MobileNo;
                userInfo.EmailId = user.MailId;

                userInfo.DocumentId = null;

                if (user.AuthScheme.Equals("DEFAULT"))
                {
                    authScheme = ssoConfig.authentication_schemes.
                        dtportal_auth_scheme;
                }
                else
                {
                    authScheme = new List<string> { user.AuthScheme };
                }
                if (user.Status.Equals(StatusConstants.NEW))
                {
                    authScheme = new List<string> { "PASSWORD" };
                }

                if (user.Status.Equals("SET_FIDO2") ||
                    user.Status.Equals("CHANGE_PASSWORD"))
                {
                    authScheme = new List<string> { "PASSWORD" };
                }

                // Check Time Restriction
                var istrue = await CheckTimeRestrictionforUser(userInfo.Id);
                if (istrue == true)
                {
                    _logger.LogError(Constants.TimeRestrictionApplied);
                    return new VerifyUserResponse(
                        Constants.TimeRestrictionApplied);
                }

                var isIP = configuration.GetValue<bool>("IPRestriction");
                if (true == isIP)
                {
                    // Check IP Restriction
                    istrue = await _iPRestriction.CheckIPRestriction(request.ip);
                    if (istrue == false)
                    {
                        _logger.LogError("IP RESTRICTION");
                        return new VerifyUserResponse(
                            "User cannot login from this IP");
                    }
                }

                var userRole = await _unitOfWork.Roles.GetByIdAsync((int)user.RoleId);
                if (null == userRole)
                {
                    _logger.LogError(Constants.InternalError);
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ROLE_GET_FAILED);
                    return new VerifyUserResponse(errorMessage);
                }

                if (StatusConstants.ACTIVE != userRole.Status)
                {
                    _logger.LogError("User role is not Active");
                    return new VerifyUserResponse(
                        "User role is not Active");
                }
            }
            else
            {
                _logger.LogInformation("client id:{0} And Application Name:{1}",
                    request.clientId, clientInDb.ApplicationName);

                SubscriberView raSubscriber = new SubscriberView();

                // Get Subscriber information from Registration Authority
                try
                {
                    raSubscriber = await GetSubscriberbyType(request.type,
                        request.userInput);
                    if (null == raSubscriber)
                    {
                        if (request.type != (int)InputType.emailId)
                        {
                            _logger.LogError(Constants.SubscriberNotFound);
                            return new VerifyUserResponse(
                                Constants.SubscriberNotFound);
                        }

                        raSubscriber = await GetSubscriberbyOrgnizationEmail(request.userInput);
                        if (null == raSubscriber)
                        {
                            _logger.LogError(Constants.SubscriberNotFound);
                            return new VerifyUserResponse(
                                Constants.SubscriberNotFound);

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                    Monitor.SendMessage(errorMessage);
                    return new VerifyUserResponse(errorMessage);
                }
                _logger.LogInformation("IsMobileUser" + raSubscriber.IsSmartphoneUser.ToString());
                if (raSubscriber.IsSmartphoneUser == 0)
                {
                    _logger.LogInformation("Mobile User");
                    isMobileUser = true;
                }
                _logger.LogInformation("IsMobileUser" + isMobileUser.ToString());
                userInfo.Suid = raSubscriber.SubscriberUid;
                userInfo.DisplayName = raSubscriber.DisplayName;
                userInfo.Status = raSubscriber.SubscriberStatus;
                userInfo.DeviceToken = raSubscriber.FcmToken;
                userInfo.MobileNumber = raSubscriber.MobileNumber;
                userInfo.EmailId = raSubscriber.Email;

                userInfo.DocumentId = raSubscriber.IdDocNumber;
                userInfo.DocumentType = raSubscriber.IdDocType;
                userInfo.NationalId = string.IsNullOrEmpty(raSubscriber.NationalId) ? "" : raSubscriber.NationalId;

                //authScheme = ssoConfig.authentication_schemes.
                //rasub_auth_scheme;

                if (clientInDb.AuthScheme == 0)
                {
                    authScheme = await _authSchemeService.GetDefaultAuthScheme();
                }
                else
                {
                    authScheme = await _authSchemeService.GetAuthSchemesListById((int)clientInDb.AuthScheme);
                }
                if (authScheme == null || authScheme.Count == 0)
                {
                    return new VerifyUserResponse(
                                "Failed to get AuthScheme Details");
                }

                if (request.type == (int)InputType.emailId)
                {
                    var orgnizationlist = await GetSubscriberOrgnizationByEmail(request.userInput);
                    if (orgnizationlist.Count != 0)
                    {
                        userInfo.LoginProfile = orgnizationlist;
                    }
                    else
                    {
                        userInfo.LoginProfile = null;
                    }
                }
                else if (request.type == (int)InputType.phoneno)
                {
                    var orgnizationlist = await GetSubscriberOrgnizationBySuid(raSubscriber.SubscriberUid);
                    if (orgnizationlist.Count != 0)
                    {
                        userInfo.LoginProfile = orgnizationlist;
                    }
                    else
                    {
                        userInfo.LoginProfile = null;
                    }
                }
                else
                {
                    userInfo.LoginProfile = null;
                }

                if (configuration["UseICP"] == "true")
                {
                    authScheme = new List<string> { "ICP" };

                    var radata = await GetSubscriberRaData(raSubscriber.SubscriberUid);
                    if (radata != null)
                    {
                        userInfo.Nationality = radata.CountryName;
                    }

                    var icpResponse = await CreateJourney_FromConfig(userInfo);

                    if (icpResponse.Error != null)
                    {
                        _logger.LogError(
                            "ICP Error | Code:{0} | Message:{1}",
                            icpResponse.Error.code,
                            icpResponse.Error.message);

                        return new VerifyUserResponse(icpResponse.Error.message);
                    }

                    journeyToken = icpResponse.Success.result.journeyToken;
                }
            }

            // Check Admin/Subscriber status
            if ((StatusConstants.ACTIVE != userInfo.Status) &&
                (StatusConstants.NEW != userInfo.Status) &&
                ("CHANGE_PASSWORD" != userInfo.Status) &&
                ("SET_FIDO2" != userInfo.Status))
            {
                if (userInfo.Status == StatusConstants.SUSPENDED)
                {
                    var id = string.Empty;
                    if (request.clientId.Equals(DTInternalConstants.DTPortalClientId))
                    {
                        id = userInfo.Id.ToString();
                    }
                    else
                    {
                        id = userInfo.Suid;
                    }

                    UserLoginDetail userLoginDetails = null;
                    try
                    {
                        // Get userlogin details
                        userLoginDetails = await _unitOfWork.UserLoginDetail.
                        GetUserLoginDetailAsync(id);
                    }
                    catch (Exception)
                    {
                        errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                        Monitor.SendMessage(errorMessage);
                        return new VerifyUserResponse(errorMessage);
                    }

                    if (null != userLoginDetails)
                    {
                        // Compare time difference
                        TimeSpan ts = (TimeSpan)(DateTime.Now - userLoginDetails.
                            BadLoginTime);
                        if (ts.TotalMinutes < (ssoConfig.sso_config.
                            account_lock_time * 60))
                        {
                            _logger.LogError(Constants.SubAccountSuspended);
                            return new VerifyUserResponse(
                                Constants.SubAccountSuspended);
                        }

                        // UPDATE STATUS TO ACTIVE
                        if (request.clientId ==
                            DTInternalConstants.DTPortalClientId)
                        {
                            if (!string.IsNullOrEmpty(user.OldStatus))
                            {
                                user.Status = user.OldStatus;
                            }
                            else
                            {
                                user.Status = StatusConstants.ACTIVE;
                            }

                            try
                            {
                                _unitOfWork.Users.Update(user);
                                await _unitOfWork.SaveAsync();
                            }
                            catch
                            {
                                _logger.LogError("Subscriber status update failed");
                                errorMessage = _helper.GetErrorMsg(
                                    ErrorCodes.DB_USER_UPDATE_STATUS_FAILED);
                                Monitor.SendMessage(errorMessage);
                                return new VerifyUserResponse(errorMessage);
                            }
                        }
                        else
                        {
                            var statusUpdateRequest = new SubscriberStatusUpdateRequest();
                            statusUpdateRequest.description =
                                LogClientServices.SubscriberStatusUpdate;
                            statusUpdateRequest.subscriberStatus =
                                StatusConstants.ACTIVE;
                            statusUpdateRequest.subscriberUniqueId = userInfo.Suid;
                            var statusResponse = await _raServiceClient.
                                SubscriberStatusUpdate(statusUpdateRequest);
                            if (null == statusResponse)
                            {
                                _logger.LogError("SubscriberStatusUpdate failed");
                                errorMessage = _helper.GetErrorMsg(
                                    ErrorCodes.RA_USER_STATUS_UPDATE_FAILED);
                                return new VerifyUserResponse(errorMessage);
                            }
                            if (false == statusResponse.success)
                            {
                                _logger.LogError("SubscriberStatusUpdate failed, " +
                                    "{0}", statusResponse.message);
                                return new VerifyUserResponse(statusResponse.message);
                            }

                            var tempSession = new TemporarySession();
                            var client = new Clientdetails();
                            client.ClientId = clientInDb.ClientId;
                            client.AppName = client.AppName;

                            tempSession.CoRelationId = Guid.NewGuid().ToString();
                            tempSession.AuthNStartTime = DateTime.Now.ToString();
                            tempSession.Clientdetails = client;

                            var resp = await _LogClient.SendAuthenticationLogMessage(
                                tempSession,
                                userInfo.Suid,
                                LogClientServices.SubscriberStatusUpdate,
                                "Subscriber status changed to ACTIVE",
                                LogClientServices.Success,
                                LogClientServices.Business,
                                false
                                );
                            if (false == resp.Success)
                            {
                                _logger.LogError("SendAuthenticationLogMessage failed: " +
                                    "{0}", resp.Message);
                                // return new VerifyUserResponse(
                                //    Constants.InternalError);
                            }
                        }
                        userLoginDetails.DeniedCount = 0;
                        userLoginDetails.WrongCodeCount = 0;
                        userLoginDetails.WrongPinCount = 0;

                        try
                        {
                            _unitOfWork.UserLoginDetail.Update(userLoginDetails);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserPasswordDetailAsync update" +
                                "failed : {0}", error.Message);
                            errorMessage = _helper.GetErrorMsg(
                                ErrorCodes.DB_USER_UPDATE_LOGIN_DETAILS_FAILED);
                            Monitor.SendMessage(errorMessage);
                            return new VerifyUserResponse(errorMessage);
                        }
                    }
                    else
                    {
                        _logger.LogError("UserLoginDetail failed: not found");
                        errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_LOGIN_DETAILS_NOT_FOUND);
                        return new VerifyUserResponse(errorMessage);
                    }
                }
                else
                {
                    _logger.LogError("_unitOfWork.UserTable.GetUserStatus: {0}",
                        userInfo.Status);
                    return new VerifyUserResponse(String.Format(
                        Constants.SubNotActive));
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
                _logger.LogError("GetUniqueKey failed: {0}", error.Message);
                errorMessage = _helper.GetErrorMsg(
                     ErrorCodes.GENERATE_UNIQUE_KEY_FAILED);
                Monitor.SendMessage(errorMessage);
                return new VerifyUserResponse(errorMessage);
            }

            // Prepare clientdetails object
            Clientdetails clientdetails = new Clientdetails
            {
                ClientId = request.clientId,
                Scopes = string.Empty,
                RedirectUrl = string.Empty,
                ResponseType = string.Empty,
                AppName = clientInDb.ApplicationName
            };

            // Prepare temporary session object
            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = userInfo.Suid,
                DisplayName = userInfo.DisplayName,
                PrimaryAuthNSchemeList = authScheme,
                AuthNSuccessList = new List<string>(),
                Clientdetails = clientdetails,
                IpAddress = request.ip,
                UserAgentDetails = request.userAgent,
                TypeOfDevice = request.typeOfDevice,
                MacAddress = DTInternalConstants.NOT_AVAILABLE,
                withPkce = false,
                AdditionalValue = DTInternalConstants.pending,
                AuthNStartTime = DateTime.Now.ToString("s"),
                CoRelationId = Guid.NewGuid().ToString(),
                LoginType = request.type.ToString(),
                DocumentId = userInfo.DocumentId,
                DeviceToken = userInfo.DeviceToken,
                NotificationAuthNDone = false,
                NotificationAdditionalValue = DTInternalConstants.pending,
                JourneyToken = journeyToken
            };

            if (userInfo.LoginProfile != null)
            {
                temporarySession.LoginProfile = userInfo.LoginProfile;
            }

            var randomcodes = string.Empty;
            if (authScheme[0] == AuthNSchemeConstants.PUSH_NOTIFICATION && isMobileUser == false)
            {
                RandomGenerator randomGenerator = new RandomGenerator();
                try
                {
                    // Generate three random numbers
                    var randomNumbersList = randomGenerator.GenerateRandomNumbers(3);

                    // Get random index out of three
                    var randomIndex = randomGenerator.GetRandomIndex(3);

                    // Convert list to string
                    randomcodes = string.Join(",", randomNumbersList);

                    // Store one out of three numbers
                    temporarySession.RandomCode = randomNumbersList.
                        ElementAt(randomIndex).ToString();
                }
                catch (Exception error)
                {
                    _logger.LogError("GenerateRandomNumbers failed: {0}", error.Message);
                    errorMessage = _helper.GetErrorMsg(
                         ErrorCodes.GENERATE_RANDOM_CODES_FAILED);
                    Monitor.SendMessage(errorMessage);
                    return new VerifyUserResponse(errorMessage);
                }
            }

            if (authScheme.Contains(AuthNSchemeConstants.WALLET))
            {
                var verifierRequest = await GetVerifierUrl();
                if (verifierRequest == null || !verifierRequest.Success)
                {
                    return new VerifyUserResponse(verifierRequest.Message);
                }
                verifierUrl = verifierRequest.Result.ToString();
            }

            var options = new AssertionOptions();

            // If authentication scheme is FIDO2, Generation options and send
            if (authScheme.Contains(AuthNSchemeConstants.FIDO2))
            {
                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.
                    GetUserAuthDataAsync(userInfo.Suid,
                    AuthNSchemeConstants.FIDO2);
                if (null == userAuthData)
                {
                    _logger.LogError("GetUserAuthDataAsync failed, not found");
                    return new VerifyUserResponse(string.Format(
                        Constants.SubNotProvisioned,
                        AuthNSchemeConstants.FIDO2));
                }

                if (userAuthData.Expiry.HasValue)
                {
                    // Get UserAuthData
                    var userInactiveAuthData = await _unitOfWork.UserAuthData.
                        GetUserInactiveAuthDataAsync(userInfo.Suid,
                        AuthNSchemeConstants.FIDO2);
                    if (null == userAuthData)
                    {
                        _logger.LogError("GetUserAuthDataAsync failed, not found");
                        return new VerifyUserResponse(string.Format(
                            Constants.SubNotProvisioned,
                            AuthNSchemeConstants.FIDO2));
                    }


                    TimeSpan timedif = (TimeSpan)(DateTime.Now -
                        userAuthData.Expiry);
                    if (null == userAuthData.Expiry || (timedif.TotalMinutes > 0))
                    {
                        userInactiveAuthData.Status = StatusConstants.ACTIVE;

                        _logger.LogInformation("User Authentication status" +
                            " is updated to EXPIRED");

                        try
                        {
                            _unitOfWork.UserAuthData.Update(userInactiveAuthData);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserTempAuthDataAsync " +
                                "failed,{0}", error.Message);
                            Monitor.SendMessage(errorMessage);
                            return new VerifyUserResponse(Constants.InternalError);
                        }

                        userAuthData.Status = StatusConstants.EXPIRED;

                        _logger.LogInformation("User Authentication status" +
                            " is updated to EXPIRED");

                        try
                        {
                            _unitOfWork.UserAuthData.Update(
                                userAuthData);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserTempAuthDataAsync " +
                                "failed,{0}", error.Message);
                            Monitor.SendMessage("GetUserTempAuthDataAsync " +
                                "failed, " + error.Message);
                            return new VerifyUserResponse(
                                Constants.InternalError);
                        }


                        userAuthData = userInactiveAuthData;
                    }
                }

                // Get Encryption Key
                var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(
                    DTInternalConstants.ENCDEC_KEY_ID);
                if (null == EncKey)
                {
                    _logger.LogError("EncDecKeys.GetByIdAsync failed," +
                        " not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }

                var userTempAuthData = await _unitOfWork.UserAuthData.
                    GetUserTempAuthDataAsync(userInfo.Suid,
                    AuthNSchemeConstants.FIDO2);
                if (null != userTempAuthData)
                {
                    if (userTempAuthData.Status == StatusConstants.HOLD)
                    {
                        _logger.LogInformation("User Authentication status" +
                            " is on HOLD");

                        if (null == userTempAuthData.Expiry)
                        {
                            userTempAuthData.Expiry = DateTime.Now;
                        }

                        TimeSpan timediff = (TimeSpan)(DateTime.Now -
                            userTempAuthData.Expiry);
                        if (null == userTempAuthData.Expiry || (timediff.Hours > 24))
                        {
                            userTempAuthData.Status = StatusConstants.EXPIRED;
                            _logger.LogInformation("User Authentication status" +
                                " is updated to EXPIRED");

                            try
                            {
                                _unitOfWork.UserAuthData.Update(
                                    userTempAuthData);
                                await _unitOfWork.SaveAsync();
                            }
                            catch (Exception error)
                            {
                                _logger.LogError("GetUserTempAuthDataAsync " +
                                    "failed,{0}", error.Message);
                                Monitor.SendMessage("GetUserTempAuthDataAsync " +
                                "failed, " + error.Message);
                                return new VerifyUserResponse(
                                    Constants.InternalError);
                            }
                        }
                        else
                        {
                            userAuthData = userTempAuthData;
                        }
                    }
                }

                string encryptionPassword = string.Empty;
                var DecryptedPasswd = string.Empty;

                try
                {
                    encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
                }
                catch (Exception error)
                {
                    _logger.LogError("Encoding.UTF8.GetString:{0}",
                        error.Message);
                    Monitor.SendMessage($"Encoding.UTF8.GetString: {error.Message}");
                    return new VerifyUserResponse(Constants.InternalError);
                }

                try
                {
                    // Decrypt Password
                    DecryptedPasswd = EncryptionLibrary.DecryptText(
                        userAuthData.AuthData,
                        encryptionPassword,
                        DTInternalConstants.PasswordSalt);
                }
                catch (Exception error)
                {
                    _logger.LogError("DecryptText failed, found exception: {0}",
                        error.Message);
                    Monitor.SendMessage($"DecryptText failed, found exception: {error.Message}");
                    return new VerifyUserResponse(Constants.InternalError);
                }

                var credential = JsonConvert.DeserializeObject<StoredCredential>
                    (DecryptedPasswd);
                if (null == credential)
                {
                    _logger.LogError("DeserializeObject failed, " +
                        "StoredCredential not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }

                var userVerification = "preferred";
                var extensions = new AuthenticationExtensionsClientInputs()
                {
                    SimpleTransactionAuthorization = AuthNSchemeConstants.FIDO2,
                    GenericTransactionAuthorization = new TxAuthGenericArg
                    {
                        ContentType = "text/plain",
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                    },
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true
                };

                // Create options
                var userVerificationOptions = userVerification.
                    ToEnum<UserVerificationRequirement>();

                try
                {
                    options = _fido2.GetAssertionOptions(new List
                        <PublicKeyCredentialDescriptor>
                        { credential.Descriptor }, userVerificationOptions, extensions);
                }
                catch
                {
                    _logger.LogError("_fido2 GetAssertionOptions failed, " +
                        "not found");
                    Monitor.SendMessage("_fido2 GetAssertionOptions failed, " +
                        "not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }
            }

            if (!configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                var logResponse =await _LogClient.SendAuthenticationLogMessage(
                temporarySession,
                temporarySession.UserId,
                LogClientServices.AuthenticationInitiated,
                LogClientServices.AuthenticationInitiated,
                LogClientServices.Success,
                LogClientServices.Business,
                false
                );
                if (false == logResponse.Success)
                {
                    _logger.LogError("Failed to send log message to central " +
                        "log server");
                    // return new VerifyUserResponse(Constants.InternalError);
                }
            }

            // Create temporary session
            var task = await _cacheClient.Add(CacheNames.TemporarySession,
                tempAuthNSessId, temporarySession);
            if (DTInternalConstants.S_OK != task.retValue)
            {
                _logger.LogError("_cacheClient.Add failed");
                errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                     ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                return new VerifyUserResponse(errorMessage);
            }

            if (authScheme[0] == AuthNSchemeConstants.PUSH_NOTIFICATION && isMobileUser == false)
            {
                // Send notification to mobile
                var authnNotification = new AuthnNotification()
                {
                    AuthnScheme = AuthNSchemeConstants.PUSH_NOTIFICATION,
                    RandomCodes = randomcodes,
                    AuthnToken = tempAuthNSessId,
                    RegistrationToken = userInfo.DeviceToken,
                    ApplicationName = clientInDb.ApplicationName
                };
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Response.Cookies.Append("TempSession", StringToBase64(tempAuthNSessId));
                    context.Response.Cookies.Append("UserName", StringToBase64(userInfo.DisplayName));
                }
                _logger.LogDebug("NOTIFICATION RANDOM CODES:'{0}' ",
                    authnNotification.RandomCodes);

                try
                {
                    var result = await _pushNotificationClient.SendAuthnNotification(
                        authnNotification);
                    if (null == result)
                    {
                        _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                            " failed");
                        return new VerifyUserResponse(
                            Constants.NotificationSendFailed);
                    }
                    _logger.LogInformation("Push notification send successfully to user : " + temporarySession.UserId + " (" + temporarySession.DisplayName + ")");
                }
                catch (Exception error)
                {
                    _logger.LogError("_pushNotificationClient." +
                        "SendAuthnNotification failed : {0}", error.Message);
                    Monitor.SendMessage($"_pushNotificationClient SendAuthnNotification failed : {error.Message}");
                    return new VerifyUserResponse(
                        Constants.NotificationSendFailed);
                }
            }

            // return success object to browser
            verifyUserResult response = new verifyUserResult
            {
                AuthnToken = tempAuthNSessId,
                AuthenticationSchemes = authScheme,
                userName = userInfo.DisplayName
            };
            if (authScheme.Contains(AuthNSchemeConstants.PUSH_NOTIFICATION) && isMobileUser == false)
            {
                response.RandomCode = temporarySession.RandomCode;
            }
            if (authScheme.Contains(AuthNSchemeConstants.FIDO2))
            {
                response.Fido2Options = options.ToJson().ToString();
            }
            if (authScheme.Contains(AuthNSchemeConstants.WALLET))
            {
                response.VerifierUrl = verifierUrl;
                response.VerifierCode = verifierUrl.Substring(verifierUrl.LastIndexOf('/') + 1);
            }
            //if (authScheme.Contains(AuthNSchemeConstants.ICP))
            //{
            //    response.JourneyToken = journeyToken;
            //}
            response.MobileUser = isMobileUser;
            _logger.LogInformation("VerifyUser response: {0}",
                JsonConvert.SerializeObject(response));
            _logger.LogDebug("<--VerifyUser");
            return new VerifyUserResponse(response);
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<VerifyUserResponse> VerifyUser(VerifyUserRequest request)
        {
            _logger.LogDebug("--->VerifyUser");

            // Input validation
            if (null == request || string.IsNullOrEmpty(request.clientId) ||
                string.IsNullOrEmpty(request.userInput))
            {
                _logger.LogError(Constants.InvalidArguments);
                return new VerifyUserResponse(Constants.InvalidArguments);
            }

            _logger.LogDebug("client id is {0}", request.clientId);
            var errorMessage = string.Empty;
            var verifierUrl = string.Empty;
            var clientInDb = new Client();
            bool isMobileUser = false;
            try
            {
                // Get Client details
                clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(
                    request.clientId);
                if (null == clientInDb)
                {
                    _logger.LogError(OIDCConstants.ClientNotFound);
                    return new VerifyUserResponse(OIDCConstants.ClientNotFound);
                }
            }
            catch (Exception)
            {
                errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                Monitor.SendMessage(errorMessage);
                return new VerifyUserResponse(errorMessage);
            }

            // Check client status
            if (clientInDb.Status != StatusConstants.ACTIVE)
            {
                _logger.LogError("{0}: {1}",
                    OIDCConstants.ClientNotActive, clientInDb.Status);
                return new VerifyUserResponse(OIDCConstants.ClientNotActive);
            }

            UserLookupItem userInfo = new UserLookupItem();
            Domain.Models.UserTable user = new Domain.Models.UserTable();
            var authScheme = new List<string>() { };
            string journeyToken = null;

            // Client id validation[Check for DT ADMIN PORTAL]
            if (request.clientId.Equals(DTInternalConstants.DTPortalClientId))
            {
                _logger.LogInformation("client id is {0} : DT ADMIN PORTAL",
                    request.clientId);

                // Get User information
                user = await GetAdminUserByType(request.type, request.userInput);
                if (null == user)
                {
                    _logger.LogError(Constants.SubscriberNotFound);
                    return new VerifyUserResponse(
                        Constants.SubscriberNotFound);
                }

                userInfo.Suid = user.Uuid;
                userInfo.Id = user.Id;
                userInfo.DisplayName = user.FullName;
                userInfo.Status = user.Status;
                userInfo.DeviceToken = string.Empty;
                userInfo.MobileNumber = user.MobileNo;
                userInfo.EmailId = user.MailId;

                userInfo.DocumentId = null;

                if (user.AuthScheme.Equals("DEFAULT"))
                {
                    authScheme = ssoConfig.authentication_schemes.
                        dtportal_auth_scheme;
                }
                else
                {
                    authScheme = new List<string> { user.AuthScheme };
                }
                if (user.Status.Equals(StatusConstants.NEW))
                {
                    authScheme = new List<string> { "PASSWORD" };
                }

                if (user.Status.Equals("SET_FIDO2") ||
                    user.Status.Equals("CHANGE_PASSWORD"))
                {
                    authScheme = new List<string> { "PASSWORD" };
                }

                // Check Time Restriction
                var istrue = await CheckTimeRestrictionforUser(userInfo.Id);
                if (istrue == true)
                {
                    _logger.LogError(Constants.TimeRestrictionApplied);
                    return new VerifyUserResponse(
                        Constants.TimeRestrictionApplied);
                }

                var isIP = configuration.GetValue<bool>("IPRestriction");
                if (true == isIP)
                {
                    // Check IP Restriction
                    istrue = await _iPRestriction.CheckIPRestriction(request.ip);
                    if (istrue == false)
                    {
                        _logger.LogError("IP RESTRICTION");
                        return new VerifyUserResponse(
                            "User cannot login from this IP");
                    }
                }

                var userRole = await _unitOfWork.Roles.GetByIdAsync((int)user.RoleId);
                if (null == userRole)
                {
                    _logger.LogError(Constants.InternalError);
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ROLE_GET_FAILED);
                    return new VerifyUserResponse(errorMessage);
                }

                if (StatusConstants.ACTIVE != userRole.Status)
                {
                    _logger.LogError("User role is not Active");
                    return new VerifyUserResponse(
                        "User role is not Active");
                }
            }
            else
            {
                _logger.LogInformation("client id:{0} And Application Name:{1}",
                    request.clientId, clientInDb.ApplicationName);

                SubscriberView raSubscriber = new SubscriberView();

                // Get Subscriber information from Registration Authority
                try
                {
                    raSubscriber = await GetSubscriberbyType(request.type,
                        request.userInput);
                    if (null == raSubscriber)
                    {
                        if (request.type != (int)InputType.emailId)
                        {
                            _logger.LogError(Constants.SubscriberNotFound);
                            return new VerifyUserResponse(
                                Constants.SubscriberNotFound);
                        }

                        raSubscriber = await GetSubscriberbyOrgnizationEmail(request.userInput);
                        if (null == raSubscriber)
                        {
                            _logger.LogError(Constants.SubscriberNotFound);
                            return new VerifyUserResponse(
                                Constants.SubscriberNotFound);

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                    Monitor.SendMessage(errorMessage);
                    return new VerifyUserResponse(errorMessage);
                }
                _logger.LogInformation("IsMobileUser" + raSubscriber.IsSmartphoneUser.ToString());
                if (raSubscriber.IsSmartphoneUser == 0)
                {
                    _logger.LogInformation("Mobile User");
                    isMobileUser = true;
                }
                _logger.LogInformation("IsMobileUser" + isMobileUser.ToString());
                userInfo.Suid = raSubscriber.SubscriberUid;
                userInfo.DisplayName = raSubscriber.DisplayName;
                userInfo.Status = raSubscriber.SubscriberStatus;
                userInfo.DeviceToken = raSubscriber.FcmToken;
                userInfo.MobileNumber = raSubscriber.MobileNumber;
                userInfo.EmailId = raSubscriber.Email;

                userInfo.DocumentId = raSubscriber.IdDocNumber;
                userInfo.DocumentType = raSubscriber.IdDocType;
                userInfo.NationalId = string.IsNullOrEmpty(raSubscriber.NationalId) ? "" : raSubscriber.NationalId;

                //authScheme = ssoConfig.authentication_schemes.
                //rasub_auth_scheme;

                if (clientInDb.AuthScheme == 0)
                {
                    authScheme = await _authSchemeService.GetDefaultAuthScheme();
                }
                else
                {
                    authScheme = await _authSchemeService.GetAuthSchemesListById((int)clientInDb.AuthScheme);
                }
                if (authScheme == null || authScheme.Count == 0)
                {
                    return new VerifyUserResponse(
                                "Failed to get AuthScheme Details");
                }

                if (request.type == (int)InputType.emailId)
                {
                    var orgnizationlist = await GetSubscriberOrgnizationByEmail(request.userInput);
                    if (orgnizationlist.Count != 0)
                    {
                        userInfo.LoginProfile = orgnizationlist;
                    }
                    else
                    {
                        userInfo.LoginProfile = null;
                    }
                }
                else if (request.type == (int)InputType.phoneno)
                {
                    var orgnizationlist = await GetSubscriberOrgnizationBySuid(raSubscriber.SubscriberUid);
                    if (orgnizationlist.Count != 0)
                    {
                        userInfo.LoginProfile = orgnizationlist;
                    }
                    else
                    {
                        userInfo.LoginProfile = null;
                    }
                }
                else
                {
                    userInfo.LoginProfile = null;
                }

                //if (configuration["KYCFACE"] == "true")
                //{

                //    var radata = await GetSubscriberRaData(raSubscriber.SubscriberUid);
                //    if (radata != null)
                //    {
                //        userInfo.Nationality = radata.CountryName;
                //    }

                //    var icpResponse = await CreateJourney_FromConfig(userInfo);

                //    if (icpResponse.Error != null)
                //    {
                //        _logger.LogError(
                //            "ICP Error | Code:{0} | Message:{1}",
                //            icpResponse.Error.code,
                //            icpResponse.Error.message);

                //        return new VerifyUserResponse(icpResponse.Error.message);
                //    }

                //    journeyToken = icpResponse.Success.result.journeyToken;
                //}
            }

            // Check Admin/Subscriber status
            if ((StatusConstants.ACTIVE != userInfo.Status) &&
                (StatusConstants.NEW != userInfo.Status) &&
                ("CHANGE_PASSWORD" != userInfo.Status) &&
                ("SET_FIDO2" != userInfo.Status))
            {
                if (userInfo.Status == StatusConstants.SUSPENDED)
                {
                    var id = string.Empty;
                    if (request.clientId.Equals(DTInternalConstants.DTPortalClientId))
                    {
                        id = userInfo.Id.ToString();
                    }
                    else
                    {
                        id = userInfo.Suid;
                    }

                    UserLoginDetail userLoginDetails = null;
                    try
                    {
                        // Get userlogin details
                        userLoginDetails = await _unitOfWork.UserLoginDetail.
                        GetUserLoginDetailAsync(id);
                    }
                    catch (Exception)
                    {
                        errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                        Monitor.SendMessage(errorMessage);
                        return new VerifyUserResponse(errorMessage);
                    }

                    if (null != userLoginDetails)
                    {
                        // Compare time difference
                        TimeSpan ts = (TimeSpan)(DateTime.Now - userLoginDetails.
                            BadLoginTime);
                        if (ts.TotalMinutes < (ssoConfig.sso_config.
                            account_lock_time * 60))
                        {
                            _logger.LogError(Constants.SubAccountSuspended);
                            return new VerifyUserResponse(
                                Constants.SubAccountSuspended);
                        }

                        // UPDATE STATUS TO ACTIVE
                        if (request.clientId ==
                            DTInternalConstants.DTPortalClientId)
                        {
                            if (!string.IsNullOrEmpty(user.OldStatus))
                            {
                                user.Status = user.OldStatus;
                            }
                            else
                            {
                                user.Status = StatusConstants.ACTIVE;
                            }

                            try
                            {
                                _unitOfWork.Users.Update(user);
                                await _unitOfWork.SaveAsync();
                            }
                            catch
                            {
                                _logger.LogError("Subscriber status update failed");
                                errorMessage = _helper.GetErrorMsg(
                                    ErrorCodes.DB_USER_UPDATE_STATUS_FAILED);
                                Monitor.SendMessage(errorMessage);
                                return new VerifyUserResponse(errorMessage);
                            }
                        }
                        else
                        {
                            var statusUpdateRequest = new SubscriberStatusUpdateRequest();
                            statusUpdateRequest.description =
                                LogClientServices.SubscriberStatusUpdate;
                            statusUpdateRequest.subscriberStatus =
                                StatusConstants.ACTIVE;
                            statusUpdateRequest.subscriberUniqueId = userInfo.Suid;
                            var statusResponse = await _raServiceClient.
                                SubscriberStatusUpdate(statusUpdateRequest);
                            if (null == statusResponse)
                            {
                                _logger.LogError("SubscriberStatusUpdate failed");
                                errorMessage = _helper.GetErrorMsg(
                                    ErrorCodes.RA_USER_STATUS_UPDATE_FAILED);
                                return new VerifyUserResponse(errorMessage);
                            }
                            if (false == statusResponse.success)
                            {
                                _logger.LogError("SubscriberStatusUpdate failed, " +
                                    "{0}", statusResponse.message);
                                return new VerifyUserResponse(statusResponse.message);
                            }

                            var tempSession = new TemporarySession();
                            var client = new Clientdetails();
                            client.ClientId = clientInDb.ClientId;
                            client.AppName = client.AppName;

                            tempSession.CoRelationId = Guid.NewGuid().ToString();
                            tempSession.AuthNStartTime = DateTime.Now.ToString();
                            tempSession.Clientdetails = client;

                            var resp =await _LogClient.SendAuthenticationLogMessage(
                                tempSession,
                                userInfo.Suid,
                                LogClientServices.SubscriberStatusUpdate,
                                "Subscriber status changed to ACTIVE",
                                LogClientServices.Success,
                                LogClientServices.Business,
                                false
                                );
                            if (false == resp.Success)
                            {
                                _logger.LogError("SendAuthenticationLogMessage failed: " +
                                    "{0}", resp.Message);
                                // return new VerifyUserResponse(
                                //    Constants.InternalError);
                            }
                        }
                        userLoginDetails.DeniedCount = 0;
                        userLoginDetails.WrongCodeCount = 0;
                        userLoginDetails.WrongPinCount = 0;

                        try
                        {
                            _unitOfWork.UserLoginDetail.Update(userLoginDetails);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserPasswordDetailAsync update" +
                                "failed : {0}", error.Message);
                            errorMessage = _helper.GetErrorMsg(
                                ErrorCodes.DB_USER_UPDATE_LOGIN_DETAILS_FAILED);
                            Monitor.SendMessage(errorMessage);
                            return new VerifyUserResponse(errorMessage);
                        }
                    }
                    else
                    {
                        _logger.LogError("UserLoginDetail failed: not found");
                        errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_LOGIN_DETAILS_NOT_FOUND);
                        return new VerifyUserResponse(errorMessage);
                    }
                }
                else
                {
                    _logger.LogError("_unitOfWork.UserTable.GetUserStatus: {0}",
                        userInfo.Status);
                    return new VerifyUserResponse(String.Format(
                        Constants.SubNotActive));
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
                _logger.LogError("GetUniqueKey failed: {0}", error.Message);
                errorMessage = _helper.GetErrorMsg(
                     ErrorCodes.GENERATE_UNIQUE_KEY_FAILED);
                Monitor.SendMessage(errorMessage);
                return new VerifyUserResponse(errorMessage);
            }

            // Prepare clientdetails object
            Clientdetails clientdetails = new Clientdetails
            {
                ClientId = request.clientId,
                Scopes = string.Empty,
                RedirectUrl = string.Empty,
                ResponseType = string.Empty,
                AppName = clientInDb.ApplicationName
            };

            // Prepare temporary session object
            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = userInfo.Suid,
                DisplayName = userInfo.DisplayName,
                PrimaryAuthNSchemeList = authScheme,
                AuthNSuccessList = new List<string>(),
                Clientdetails = clientdetails,
                IpAddress = request.ip,
                UserAgentDetails = request.userAgent,
                TypeOfDevice = request.typeOfDevice,
                MacAddress = DTInternalConstants.NOT_AVAILABLE,
                withPkce = false,
                AdditionalValue = DTInternalConstants.pending,
                AuthNStartTime = DateTime.Now.ToString("s"),
                CoRelationId = Guid.NewGuid().ToString(),
                LoginType = request.type.ToString(),
                DocumentId = userInfo.DocumentId,
                DeviceToken = userInfo.DeviceToken,
                NotificationAuthNDone = false,
                NotificationAdditionalValue = DTInternalConstants.pending,
                JourneyToken = journeyToken
            };

            if (userInfo.LoginProfile != null)
            {
                temporarySession.LoginProfile = userInfo.LoginProfile;
            }

            var isICPFace = configuration["UseICPForAuth"];
            var randomcodes = string.Empty;
            if ((authScheme[0] == AuthNSchemeConstants.PUSH_NOTIFICATION)
                || (authScheme[0] == AuthNSchemeConstants.UAEKYCFACE))
            {
                RandomGenerator randomGenerator = new RandomGenerator();
                try
                {
                    // Generate three random numbers
                    var randomNumbersList = randomGenerator.GenerateRandomNumbers(3);

                    // Get random index out of three
                    var randomIndex = randomGenerator.GetRandomIndex(3);

                    // Convert list to string
                    randomcodes = string.Join(",", randomNumbersList);

                    // Store one out of three numbers
                    temporarySession.RandomCode = randomNumbersList.
                        ElementAt(randomIndex).ToString();
                }
                catch (Exception error)
                {
                    _logger.LogError("GenerateRandomNumbers failed: {0}", error.Message);
                    errorMessage = _helper.GetErrorMsg(
                         ErrorCodes.GENERATE_RANDOM_CODES_FAILED);
                    Monitor.SendMessage(errorMessage);
                    return new VerifyUserResponse(errorMessage);
                }
            }

            if (authScheme.Contains(AuthNSchemeConstants.WALLET))
            {
                var verifierRequest = await GetVerifierUrl();
                if (verifierRequest == null || !verifierRequest.Success)
                {
                    return new VerifyUserResponse(verifierRequest.Message);
                }
                verifierUrl = verifierRequest.Result.ToString();
            }

            var options = new AssertionOptions();

            // If authentication scheme is FIDO2, Generation options and send
            if (authScheme.Contains(AuthNSchemeConstants.FIDO2))
            {
                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.
                    GetUserAuthDataAsync(userInfo.Suid,
                    AuthNSchemeConstants.FIDO2);
                if (null == userAuthData)
                {
                    _logger.LogError("GetUserAuthDataAsync failed, not found");
                    return new VerifyUserResponse(string.Format(
                        Constants.SubNotProvisioned,
                        AuthNSchemeConstants.FIDO2));
                }

                if (userAuthData.Expiry.HasValue)
                {
                    // Get UserAuthData
                    var userInactiveAuthData = await _unitOfWork.UserAuthData.
                        GetUserInactiveAuthDataAsync(userInfo.Suid,
                        AuthNSchemeConstants.FIDO2);
                    if (null == userAuthData)
                    {
                        _logger.LogError("GetUserAuthDataAsync failed, not found");
                        return new VerifyUserResponse(string.Format(
                            Constants.SubNotProvisioned,
                            AuthNSchemeConstants.FIDO2));
                    }


                    TimeSpan timedif = (TimeSpan)(DateTime.Now -
                        userAuthData.Expiry);
                    if (null == userAuthData.Expiry || (timedif.TotalMinutes > 0))
                    {
                        userInactiveAuthData.Status = StatusConstants.ACTIVE;

                        _logger.LogInformation("User Authentication status" +
                            " is updated to EXPIRED");

                        try
                        {
                            _unitOfWork.UserAuthData.Update(userInactiveAuthData);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserTempAuthDataAsync " +
                                "failed,{0}", error.Message);
                            Monitor.SendMessage(errorMessage);
                            return new VerifyUserResponse(Constants.InternalError);
                        }

                        userAuthData.Status = StatusConstants.EXPIRED;

                        _logger.LogInformation("User Authentication status" +
                            " is updated to EXPIRED");

                        try
                        {
                            _unitOfWork.UserAuthData.Update(
                                userAuthData);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("GetUserTempAuthDataAsync " +
                                "failed,{0}", error.Message);
                            Monitor.SendMessage("GetUserTempAuthDataAsync " +
                                "failed, " + error.Message);
                            return new VerifyUserResponse(
                                Constants.InternalError);
                        }


                        userAuthData = userInactiveAuthData;
                    }
                }

                // Get Encryption Key
                var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(
                    DTInternalConstants.ENCDEC_KEY_ID);
                if (null == EncKey)
                {
                    _logger.LogError("EncDecKeys.GetByIdAsync failed," +
                        " not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }

                var userTempAuthData = await _unitOfWork.UserAuthData.
                    GetUserTempAuthDataAsync(userInfo.Suid,
                    AuthNSchemeConstants.FIDO2);
                if (null != userTempAuthData)
                {
                    if (userTempAuthData.Status == StatusConstants.HOLD)
                    {
                        _logger.LogInformation("User Authentication status" +
                            " is on HOLD");

                        if (null == userTempAuthData.Expiry)
                        {
                            userTempAuthData.Expiry = DateTime.Now;
                        }

                        TimeSpan timediff = (TimeSpan)(DateTime.Now -
                            userTempAuthData.Expiry);
                        if (null == userTempAuthData.Expiry || (timediff.Hours > 24))
                        {
                            userTempAuthData.Status = StatusConstants.EXPIRED;
                            _logger.LogInformation("User Authentication status" +
                                " is updated to EXPIRED");

                            try
                            {
                                _unitOfWork.UserAuthData.Update(
                                    userTempAuthData);
                                await _unitOfWork.SaveAsync();
                            }
                            catch (Exception error)
                            {
                                _logger.LogError("GetUserTempAuthDataAsync " +
                                    "failed,{0}", error.Message);
                                Monitor.SendMessage("GetUserTempAuthDataAsync " +
                                "failed, " + error.Message);
                                return new VerifyUserResponse(
                                    Constants.InternalError);
                            }
                        }
                        else
                        {
                            userAuthData = userTempAuthData;
                        }
                    }
                }

                string encryptionPassword = string.Empty;
                var DecryptedPasswd = string.Empty;

                try
                {
                    encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
                }
                catch (Exception error)
                {
                    _logger.LogError("Encoding.UTF8.GetString:{0}",
                        error.Message);
                    Monitor.SendMessage($"Encoding.UTF8.GetString: {error.Message}");
                    return new VerifyUserResponse(Constants.InternalError);
                }

                try
                {
                    // Decrypt Password
                    DecryptedPasswd = EncryptionLibrary.DecryptText(
                        userAuthData.AuthData,
                        encryptionPassword,
                        DTInternalConstants.PasswordSalt);
                }
                catch (Exception error)
                {
                    _logger.LogError("DecryptText failed, found exception: {0}",
                        error.Message);
                    Monitor.SendMessage($"DecryptText failed, found exception: {error.Message}");
                    return new VerifyUserResponse(Constants.InternalError);
                }

                var credential = JsonConvert.DeserializeObject<StoredCredential>
                    (DecryptedPasswd);
                if (null == credential)
                {
                    _logger.LogError("DeserializeObject failed, " +
                        "StoredCredential not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }

                var userVerification = "preferred";
                var extensions = new AuthenticationExtensionsClientInputs()
                {
                    SimpleTransactionAuthorization = AuthNSchemeConstants.FIDO2,
                    GenericTransactionAuthorization = new TxAuthGenericArg
                    {
                        ContentType = "text/plain",
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                    },
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true
                };

                // Create options
                var userVerificationOptions = userVerification.
                    ToEnum<UserVerificationRequirement>();

                try
                {
                    options = _fido2.GetAssertionOptions(new List
                        <PublicKeyCredentialDescriptor>
                        { credential.Descriptor }, userVerificationOptions, extensions);
                }
                catch
                {
                    _logger.LogError("_fido2 GetAssertionOptions failed, " +
                        "not found");
                    Monitor.SendMessage("_fido2 GetAssertionOptions failed, " +
                        "not found");
                    return new VerifyUserResponse(
                        Constants.InternalError);
                }
            }

            if (!configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                var logResponse =await _LogClient.SendAuthenticationLogMessage(
                temporarySession,
                temporarySession.UserId,
                LogClientServices.AuthenticationInitiated,
                LogClientServices.AuthenticationInitiated,
                LogClientServices.Success,
                LogClientServices.Business,
                false
                );
                if (false == logResponse.Success)
                {
                    _logger.LogError("Failed to send log message to central " +
                        "log server");
                    // return new VerifyUserResponse(Constants.InternalError);
                }
            }

            // Create temporary session
            var task = await _cacheClient.Add(CacheNames.TemporarySession,
                tempAuthNSessId, temporarySession);
            if (DTInternalConstants.S_OK != task.retValue)
            {
                _logger.LogError("_cacheClient.Add failed");
                errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                     ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                return new VerifyUserResponse(errorMessage);
            }

            if (authScheme[0] == AuthNSchemeConstants.PUSH_NOTIFICATION || authScheme[0] == AuthNSchemeConstants.UAEKYCFACE)
            {
                // Send notification to mobile
                var authnNotification = new AuthnNotification()
                {
                    AuthnScheme = authScheme[0],
                    RandomCodes = randomcodes,
                    AuthnToken = tempAuthNSessId,
                    RegistrationToken = userInfo.DeviceToken,
                    ApplicationName = clientInDb.ApplicationName,
                    DeviceName = GetOSFromUserAgent(temporarySession.UserAgentDetails),
                    Timestamp = DateTime.UtcNow.ToString("s")
                };
                authnNotification.AuthnScheme = authScheme[0];
                _logger.LogInformation("AuthSchema - {AuthSchema}", authnNotification.AuthnScheme.ToString());

                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Response.Cookies.Append("TempSession", StringToBase64(tempAuthNSessId));
                    context.Response.Cookies.Append("UserName", StringToBase64(userInfo.DisplayName));
                }
                _logger.LogDebug("NOTIFICATION RANDOM CODES:'{0}' ",
                    authnNotification.RandomCodes);

                try
                {
                    var result = await _pushNotificationClient.SendAuthnNotification(
                        authnNotification);
                    if (null == result)
                    {
                        _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                            " failed");
                        return new VerifyUserResponse(
                            Constants.NotificationSendFailed);
                    }
                    _logger.LogInformation("Push notification send successfully to user : " + temporarySession.UserId + " (" + temporarySession.DisplayName + ")");
                }
                catch (Exception error)
                {
                    _logger.LogError("_pushNotificationClient." +
                        "SendAuthnNotification failed : {0}", error.Message);
                    Monitor.SendMessage($"_pushNotificationClient SendAuthnNotification failed : {error.Message}");
                    return new VerifyUserResponse(
                        Constants.NotificationSendFailed);
                }
            }

            // return success object to browser
            verifyUserResult response = new verifyUserResult
            {
                AuthnToken = tempAuthNSessId,
                AuthenticationSchemes = authScheme,
                userName = userInfo.DisplayName,
                userMail = userInfo.EmailId,
            };

            if (authScheme.Contains(AuthNSchemeConstants.PUSH_NOTIFICATION)
                || authScheme.Contains(AuthNSchemeConstants.UAEKYCFACE))
            {
                response.RandomCode = temporarySession.RandomCode;
            }
            if (authScheme.Contains(AuthNSchemeConstants.FIDO2))
            {
                response.Fido2Options = options.ToJson().ToString();
            }
            if (authScheme.Contains(AuthNSchemeConstants.WALLET))
            {
                response.VerifierUrl = verifierUrl;
                response.VerifierCode = verifierUrl.Substring(verifierUrl.LastIndexOf('/') + 1);
            }
            //if (authScheme.Contains(AuthNSchemeConstants.UAEKYCFACE))
            //{
            //    response.JourneyToken = journeyToken;
            //}
            response.MobileUser = isMobileUser;
            _logger.LogInformation("VerifyUser response: {0}",
                JsonConvert.SerializeObject(response));
            _logger.LogDebug("<--VerifyUser");
            return new VerifyUserResponse(response);
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<VerifyUserAuthDataResponse> VerifyUserAuthData(
            VerifyUserAuthDataRequest request)
        {
            _logger.LogDebug("-->VerifyUserAuthData");
            _logger.LogInformation("Request - {0}",
                JsonConvert.SerializeObject(request));
            _logger.LogDebug("AuthNToken: {0}, AuthNScheme: {1}",
                request.authnToken, request.authenticationScheme);
            // Validate input
            if (
                (null == request) ||
                (string.IsNullOrEmpty(request.authnToken)) ||
                (string.IsNullOrEmpty(request.authenticationScheme)) ||
                (string.IsNullOrEmpty(request.authenticationData))
                )
            {
                _logger.LogError(Constants.InvalidArguments);
                VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }

            var errorMessage = string.Empty;
            var randomcodes = string.Empty;
            var randomcode = string.Empty;
            var verifierUrl = string.Empty;
            /*
                        // Check whether the temporary session exists
                        var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                            request.authnToken);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError(Constants.TempSessionExpired);
                            Response response = new Response();
                            response.Success = false;
                            response.Message = Constants.TempSessionExpired;
                            return response;
                        }
            */
            TemporarySession tempSession = null;

            try
            {
                // Get the temporary session object
                tempSession = await _cacheClient.Get<TemporarySession>
                    (CacheNames.TemporarySession,
                    request.authnToken);
                if (null == tempSession)
                {
                    _logger.LogError(Constants.TempSessionExpired);
                    VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    response.Message = Constants.TempSessionExpired;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Temporary Session Record");
                VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_TEMP_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            var isAuthSchm = false;
            isAuthSchm = false;

            // If already, there any authentication success count
            bool check = !tempSession.AuthNSuccessList.Any();
            if (check)
            {
                if (tempSession.AuthNSuccessList.Contains(
                    request.authenticationScheme))
                {
                    isAuthSchm = true;
                }
            }
            if (true == isAuthSchm)
            {
                _logger.LogError(Constants.AuthSchemeAlreadyAuthenticated);
                VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                response.Success = false;
                response.Message = Constants.AuthSchemeAlreadyAuthenticated;
                return response;
            }

            _logger.LogDebug("TempSession ClientID is {0}",
                tempSession.Clientdetails.ClientId);

            var userId = new UserTable();
            UserLookupItem userInfo = new UserLookupItem();

            // Check client is Internal/External
            if (tempSession.Clientdetails.ClientId ==
                DTInternalConstants.DTPortalClientId)
            {
                _logger.LogDebug("Client is DT ADMIN PORTAL");

                // Get UserId
                userId = await _unitOfWork.Users.GetUserbyUuidAsync
                    (tempSession.UserId);
                if (null == userId)
                {
                    _logger.LogError("GetUserIdByName failed, not found");
                    VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    errorMessage = _helper.GetErrorMsg(
                        ErrorCodes.DB_USER_GET_DETAILS_FAILED);
                    response.Message = errorMessage;
                    return response;
                }

                userInfo.Id = userId.Id;
                userInfo.Suid = userId.Uuid;
                userInfo.DisplayName = userId.FullName;
                userInfo.MobileNumber = userId.MobileNo;
                userInfo.EmailId = userId.MailId;
                userInfo.Status = userId.Status;
                if (userInfo.Status.Equals(StatusConstants.SUSPENDED))
                {
                    _logger.LogError("User account is SUSPENDED");
                    VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    response.Message = Constants.SubAccountSuspended;
                    return response;
                }
            }
            else
            {
                _logger.LogDebug("Client is EXTERNAL CLIENT");
                userInfo.DisplayName = tempSession.DisplayName;
                userInfo.Suid = tempSession.UserId;
                userInfo.DocumentId = tempSession.DocumentId;
                userInfo.DeviceToken = tempSession.DeviceToken;
            }

            var wrongPin = false;
            var wrongCode = false;
            var denyCount = false;
            var wrongFace = false;
            var isAuthNPassed = false;
            double pinVerifyTime = default;
            var isICPFace = configuration["UseICPForAuth"];

            // Password authnscheme
            if (request.authenticationScheme.Equals(AuthNSchemeConstants.PASSWORD))
            {
                // Verify Password
                var passwordRes = await VerifyPassword(request.authenticationData,
                    userInfo.Suid);
                if (passwordRes.Success == false)
                {
                    isAuthNPassed = false;
                }
                else
                {
                    isAuthNPassed = true;
                }
            }

            // FIDO2 authentication scheme
            else if (request.authenticationScheme == AuthNSchemeConstants.FIDO2)
            {
                // Verify Fido2
                var fido2Res = await VerifyFido2(request.authenticationData, userInfo.Suid);
                if (fido2Res.Success == false)
                {
                    isAuthNPassed = false;
                }
                else
                {
                    isAuthNPassed = true;
                }
            }

            else if (request.authenticationScheme.Equals(AuthNSchemeConstants.WEB_FACE))
            {
                var faceRes = await VerifyFace(request.authenticationData, userInfo.Suid);
                if (faceRes.Success == false)
                {
                    if (faceRes.Message != Constants.FaceVerifyFailed)
                    {
                        var response = new VerifyUserAuthDataResponse();
                        response.Success = false;
                        response.Message = faceRes.Message;
                        return response;
                    }
                    else
                    {
                        isAuthNPassed = false;
                        wrongFace = true;
                    }
                }
                else
                {
                    isAuthNPassed = true;
                }

            }

            else if (request.authenticationScheme == AuthNSchemeConstants.WEB_PIN)
            {
                var secureData = PKIMethods.Instance.
                    PKIGenerateSHA256Hash(request.authenticationData);
                var verifyPinRequest = new VerifyPinRequest();
                verifyPinRequest.subscriberDigitalID = userInfo.Suid;
                verifyPinRequest.authenticationPin = secureData;
                var pinVerificationStartTime = DateTime.Now.ToString("s");

                var verifyPinResponse = await VerifyPushNotification(tempSession,
                    verifyPinRequest);

                pinVerifyTime = CalculateTime(pinVerificationStartTime);

                if (false == verifyPinResponse.success)
                {
                    if (verifyPinResponse.message.Equals
                        (Constants.PinVerifyFailed))
                    {
                        _logger.LogInformation("Wrong Pin");
                        wrongPin = true;
                        isAuthNPassed = false;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyPinResponse.message))
                        {
                            _logger.LogError("PKI service error: {0}",
                                verifyPinResponse.message);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = verifyPinResponse.message;
                            return response;
                        }
                        else
                        {
                            _logger.LogError("PKI service error: {0}",
                                    Constants.InternalError);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            errorMessage = _helper.GetErrorMsg(
                                ErrorCodes.PKI_SERVICE_ERROR);
                            response.Message = errorMessage;
                            return response;
                        }
                    }
                }
                else
                {
                    isAuthNPassed = true;
                    _logger.LogInformation("Pin Verified: {0}",
                        verifyPinResponse.success);
                }
            }

            // Push notification authentication scheme
            else if (request.authenticationScheme == AuthNSchemeConstants.PUSH_NOTIFICATION ||
                request.authenticationScheme == AuthNSchemeConstants.FACE ||
                request.authenticationScheme == AuthNSchemeConstants.PIN ||
                request.authenticationScheme == AuthNSchemeConstants.UAEKYCFACE)
            {
                // Check if subscriber denied authentication
                if (string.Equals(request.approved, DTInternalConstants.E_False))
                {
                    _logger.LogError(Constants.SubscriberNotApproved);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.SubscriberNotApproved;
                    denyCount = true;
                    isAuthNPassed = false;
                    tempSession.NotificationAuthNDone = false;
                    tempSession.NotificationAdditionalValue = Constants.UserDeniedAuthN;
                }

                // Check if random code matched
                if (request.randomCode != tempSession.RandomCode)
                {
                    _logger.LogError(Constants.RandomCodeNotMatched);
                    _logger.LogError("Code in temp session: {0}----Code from request: {1}",
                        tempSession.RandomCode, request.randomCode);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.RandomCodeNotMatched;
                    wrongCode = true;
                    isAuthNPassed = false;
                    tempSession.NotificationAuthNDone = false;
                    tempSession.NotificationAdditionalValue = Constants.WrongCode;
                }
                if (request.authenticationScheme == AuthNSchemeConstants.FACE)
                {
                    var verifyFaceResponse = VerifyFace(request.authenticationData);
                    if (false == verifyFaceResponse.success)
                    {
                        if (verifyFaceResponse.message.Equals
                            (Constants.FaceVerifyFailed))
                        {
                            _logger.LogInformation("Wrong Pin");
                            wrongFace = true;
                            isAuthNPassed = false;
                            tempSession.NotificationAuthNDone = false;
                            tempSession.NotificationAdditionalValue = Constants.WrongFace;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(verifyFaceResponse.message))
                            {
                                _logger.LogError("PKI service error: {0}",
                                    verifyFaceResponse.message);
                                var response = new VerifyUserAuthDataResponse();
                                response.Success = false;
                                response.Message = verifyFaceResponse.message;
                                return response;
                            }
                            else
                            {
                                _logger.LogError("PKI service error: {0}",
                                        Constants.InternalError);
                                var response = new VerifyUserAuthDataResponse();
                                response.Success = false;
                                response.Message = "Internal Error";
                                return response;
                            }
                        }
                    }
                    else
                    {
                        isAuthNPassed = true;
                        tempSession.NotificationAuthNDone = true;
                        tempSession.NotificationAdditionalValue = DTInternalConstants.S_True;
                        _logger.LogInformation("Face Verified: {0}",
                            verifyFaceResponse.success);
                    }
                }
                else if (request.authenticationScheme == AuthNSchemeConstants.UAEKYCFACE)
                {
                    var verifyFaceResponse = await VerifyUaeKycAuthentication(request.authenticationData);
                    if (false == verifyFaceResponse.Success)
                    {
                        if (!string.IsNullOrEmpty(verifyFaceResponse.Message))
                        {
                            _logger.LogError("Something went wrong: {0}",
                                verifyFaceResponse.Message);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = verifyFaceResponse.Message;
                            return response;
                        }
                        else
                        {
                            _logger.LogError("Something went wrong: {0}",
                                    Constants.InternalError);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = "Internal Error";
                            return response;
                        }
                    }
                    else
                    {
                        isAuthNPassed = true;
                        tempSession.NotificationAuthNDone = true;
                        tempSession.NotificationAdditionalValue = DTInternalConstants.S_True;
                        _logger.LogInformation("Face Verified: {0}",
                            verifyFaceResponse.Success);
                    }
                }
                else
                {
                    var verifyPinRequest = new VerifyPinRequest();
                    verifyPinRequest.subscriberDigitalID = tempSession.UserId;
                    verifyPinRequest.authenticationPin = request.authenticationData;
                    var pinVerificationStartTime = DateTime.Now.ToString("s");

                    // VerifyPIN from registration authority
                    var verifyPinResponse = await VerifyPushNotification(tempSession,
                        verifyPinRequest);

                    // Calculate Pin Verification Time
                    pinVerifyTime = CalculateTime(pinVerificationStartTime);

                    if (false == verifyPinResponse.success)
                    {
                        if (verifyPinResponse.message.Equals
                            (Constants.PinVerifyFailed))
                        {
                            _logger.LogInformation("Wrong Pin");
                            wrongPin = true;
                            isAuthNPassed = false;
                            tempSession.NotificationAuthNDone = false;
                            tempSession.NotificationAdditionalValue = Constants.WrongPin;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(verifyPinResponse.message))
                            {
                                _logger.LogError("PKI service error: {0}",
                                    verifyPinResponse.message);
                                var response = new VerifyUserAuthDataResponse();
                                response.Success = false;
                                response.Message = verifyPinResponse.message;
                                return response;
                            }
                            else
                            {
                                _logger.LogError("PKI service error: {0}",
                                        Constants.InternalError);
                                var response = new VerifyUserAuthDataResponse();
                                response.Success = false;
                                errorMessage = _helper.GetErrorMsg(
                                    ErrorCodes.PKI_SERVICE_ERROR);
                                response.Message = errorMessage;
                                return response;
                            }
                        }
                    }
                    else
                    {
                        isAuthNPassed = true;
                        tempSession.NotificationAuthNDone = true;
                        tempSession.NotificationAdditionalValue = DTInternalConstants.S_True;
                        _logger.LogInformation("Pin Verified: {0}",
                            verifyPinResponse.success);
                    }
                }
            }

            // QRCode authentication Schema
            else if (request.authenticationScheme ==
                AuthNSchemeConstants.QRCODE)
            {
                var verifyPinRequest = new VerifyPinRequest();
                verifyPinRequest.subscriberDigitalID = tempSession.UserId;
                verifyPinRequest.authenticationPin = request.authenticationData;
                var pinVerificationStartTime = DateTime.Now.ToString("s");

                // VerifyPIN from registration authority
                //var verifyPinResponse = await VerifyPushNotification(tempSession,
                //    verifyPinRequest);

                // Calculate Pin Verification Time
                pinVerifyTime = CalculateTime(pinVerificationStartTime);

                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                // For Bypassing HSM Verification
                VerifyPinResponse verifyPinResponse =
                    new VerifyPinResponse();
                verifyPinResponse.success = true;
                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                if (false == verifyPinResponse.success)
                {
                    if (verifyPinResponse.message.Equals
                        (Constants.PinVerifyFailed))
                    {
                        _logger.LogInformation("Wrong Pin");
                        wrongPin = true;
                        isAuthNPassed = false;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyPinResponse.message))
                        {
                            _logger.LogError("PKI service error: {0}",
                                verifyPinResponse.message);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = verifyPinResponse.message;
                            return response;
                        }
                        else
                        {
                            _logger.LogError("PKI service error: {0}",
                                    Constants.InternalError);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            errorMessage = _helper.GetErrorMsg(
                                ErrorCodes.PKI_SERVICE_ERROR);
                            response.Message = errorMessage;
                            return response;
                        }
                    }
                }
                else
                {
                    isAuthNPassed = true;
                    _logger.LogInformation("Pin Verified: {0}",
                        verifyPinResponse.success);
                }
            }

            else if (request.authenticationScheme ==
                AuthNSchemeConstants.WALLET)
            {
                if (request.documentNumber == userInfo.DocumentId)
                {
                    isAuthNPassed = true;
                }
                else
                {
                    isAuthNPassed = false;
                }
            }

            else if (request.authenticationScheme == AuthNSchemeConstants.TOTP)
            {
                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                    (userInfo.Suid, AuthNSchemeConstants.MOBILE_TOTP);
                if (null == userAuthData)
                {
                    VerifyUserAuthDataResponse VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                    VerifyUserAuthDataResponse.Success = false;
                    VerifyUserAuthDataResponse.Message = "User authentication data not found";
                    return VerifyUserAuthDataResponse;
                }

                // Verify TOTP
                var isSuccess = TOTPLibrary.VerifyTOTP(userAuthData.AuthData,
                    request.authenticationData);
                if (false == isSuccess)
                {
                    VerifyUserAuthDataResponse VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                    VerifyUserAuthDataResponse.Success = false;
                    VerifyUserAuthDataResponse.Message = "Wrong credentials";
                    isAuthNPassed = false;
                    wrongPin = true;
                }
                else
                {
                    isAuthNPassed = true;
                }
            }

            else
            {
                _logger.LogError(Constants.AuthSchemeMisMatch);
                var response = new VerifyUserAuthDataResponse();
                response.Success = false;
                response.Message = Constants.AuthSchemeMisMatch;
                return response;
            }

            // Log Authentication Status
            if (!configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                string logMessage = string.Empty;
                string logServiceName = LogClientServices.AuthenticationFailed;
                string logServiceStatus = LogClientServices.Failure;
                bool centralLog = true;

                if (true == wrongPin)
                    logMessage = "Wrong Pin Submitted,Verification Time in seconds: "
                        + pinVerifyTime;
                else if (true == wrongCode)
                    logMessage = "Wrong Code Submitted";
                else if (true == denyCount)
                {
                    logMessage = "Subscriber Denied";
                    logServiceStatus = LogClientServices.Declined;
                }
                else
                {
                    logMessage = "Authentication Verified";
                    logServiceName = LogClientServices.PinVerification;
                    logServiceStatus = LogClientServices.Success;
                    centralLog = false;
                }

                var logResponse =await _LogClient.SendAuthenticationLogMessage(
                tempSession,
                userInfo.Suid,
                logServiceName,
                logMessage,
                logServiceStatus,
                LogClientServices.Business,
                centralLog
                );
                if (false == logResponse.Success)
                {
                    _logger.LogError("SendAuthenticationLogMessage failed: " +
                        "{0}", logResponse.Message);
                    //return logResponse;
                }
            }

            // Failed case
            if ((isAuthNPassed == false) || (denyCount == true) ||
                (wrongCode == true) || (wrongPin == true) || (wrongFace == true))
            {
                // Get userpassword details
                if (userInfo.Id != 0)
                {
                    userInfo.Suid = userInfo.Id.ToString();
                }

                UserLoginDetail userLoginDetails = null;
                try
                {
                    userLoginDetails = await _unitOfWork.UserLoginDetail.
                        GetUserLoginDetailAsync(userInfo.Suid.ToString());
                }
                catch (Exception)
                {
                    var response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                    response.Message = errorMessage;
                    return response;
                }

                if (null == userLoginDetails)
                {
                    _logger.LogDebug("GetUserPasswordDetailAsync failed, not found");
                    var response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    response.Message = Constants.InternalError;

                    userLoginDetails = new UserLoginDetail();

                    // Prepare user login details object
                    userLoginDetails.UserId = userInfo.Suid.ToString();
                    userLoginDetails.BadLoginTime = DateTime.Now;
                    userLoginDetails.IsReversibleEncryption = false;
                    userLoginDetails.IsScrambled = false;
                    userLoginDetails.PriAuthSchId = 0;
                    if (wrongCode == true)
                    {
                        userLoginDetails.WrongCodeCount = 1;
                    }
                    if ((wrongPin == true || wrongFace == true) || (request.authenticationScheme == "PASSWORD"))
                    {
                        userLoginDetails.WrongPinCount = 1;
                    }
                    if (denyCount == true)
                    {
                        userLoginDetails.DeniedCount = 1;
                    }

                    try
                    {
                        await _unitOfWork.UserLoginDetail.AddAsync(userLoginDetails);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("UserLoginDetail.AddAsync failed: {0}",
                            error.Message);
                        response.Success = false;
                        errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_ADD_LOGIN_DETAILS_FAILED);
                        response.Message = errorMessage;
                        return response;
                    }
                }
                else
                {
                    // Prepare user password object
                    userLoginDetails.UserId = userInfo.Suid.ToString();

                    userLoginDetails.BadLoginTime = DateTime.Now;
                    if (wrongCode == true)
                    {
                        userLoginDetails.WrongCodeCount = userLoginDetails.
                            WrongCodeCount + 1;
                    }
                    if ((wrongPin == true || wrongFace == true) || (request.authenticationScheme == "PASSWORD"))
                    {
                        userLoginDetails.WrongPinCount = userLoginDetails.
                            WrongPinCount + 1;
                    }
                    if (denyCount == true)
                    {
                        userLoginDetails.DeniedCount = userLoginDetails.
                            DeniedCount + 1;
                    }

                    // Update Login details
                    try
                    {
                        _unitOfWork.UserLoginDetail.Update(userLoginDetails);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("UserLoginDetail Update failed: {0}",
                            error.Message);
                        var response = new VerifyUserAuthDataResponse();
                        response.Success = false;
                        errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_UPDATE_LOGIN_DETAILS_FAILED);
                        response.Message = errorMessage;
                        return response;
                    }
                }

                if ((userLoginDetails.WrongPinCount >=
                    ssoConfig.sso_config.wrong_pin)
                    || (userLoginDetails.WrongCodeCount >=
                    ssoConfig.sso_config.wrong_code)
                    || (userLoginDetails.DeniedCount >=
                    ssoConfig.sso_config.deny_count))
                {
                    // If client is DTADMIN Portal, Change the user status in
                    // DataBase
                    if (tempSession.Clientdetails.ClientId ==
                        DTInternalConstants.DTPortalClientId)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(userInfo.Id);
                        if (null == user)
                        {
                            _logger.LogError("Users GetByIdAsync failed, " +
                                "not found {0}",
                                userInfo.Id);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }

                        user.OldStatus = userInfo.Status;
                        user.Status = StatusConstants.SUSPENDED;
                        _logger.LogInformation("User status is updated to" +
                            " SUSPENDED");

                        try
                        {
                            _unitOfWork.Users.Update(user);
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception error)
                        {
                            _logger.LogError("user status update failed: {0}",
                                error.Message);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                    else
                    {
                        var statusUpdateRequest =
                            new SubscriberStatusUpdateRequest();

                        statusUpdateRequest.description =
                            LogClientServices.SubscriberStatusUpdate;
                        statusUpdateRequest.subscriberStatus =
                            StatusConstants.SUSPENDED;
                        statusUpdateRequest.subscriberUniqueId = userInfo.Suid;

                        var statusResponse = await _raServiceClient.
                            SubscriberStatusUpdate(statusUpdateRequest);
                        if (null == statusResponse)
                        {
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            errorMessage = _helper.GetErrorMsg(
                                ErrorCodes.RA_USER_STATUS_UPDATE_FAILED);
                            response.Message = errorMessage;
                            return response;
                        }
                        if (false == statusResponse.success)
                        {
                            _logger.LogError("SubscriberStatusUpdate failed, " +
                                "{0}", statusResponse.message);
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = statusResponse.message;
                            return response;
                        }
                    }

                    // Send central, service log message
                    var logResponse =await _LogClient.SendAuthenticationLogMessage(
                                tempSession,
                                userInfo.Suid,
                                LogClientServices.SubscriberStatusUpdate,
                                "Subscriber status changed to SUSPENDED",
                                LogClientServices.Success,
                                LogClientServices.Business,
                                false
                                );
                    if (false == logResponse.Success)
                    {
                        _logger.LogError("SendAuthenticationLogMessage failed: " +
                            "{0}", logResponse.Message);
                        //return logResponse;
                    }
                }
            }
            if (false == isAuthNPassed)
            {
                var response = new VerifyUserAuthDataResponse();
                _logger.LogError(Constants.AuthNFailed);
                tempSession.AllAuthNDone = false;
                tempSession.AdditionalValue = DTInternalConstants.E_False;

                if (request.authenticationScheme.Equals(AuthNSchemeConstants.PASSWORD))
                {
                    _logger.LogInformation(Constants.WrongCredentials);
                    response.Message = Constants.WrongCredentials;
                }

                if (request.authenticationScheme.Equals(AuthNSchemeConstants.WALLET))
                {
                    _logger.LogInformation(Constants.WrongCredentials);
                    response.Message = Constants.WrongCredentials;
                }

                if (denyCount == true)
                {
                    _logger.LogInformation(Constants.UserDeniedAuthN);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue =
                        Constants.UserDeniedAuthN;
                    response.Message = Constants.UserDeniedAuthN;
                }
                else if (wrongCode == true)
                {
                    _logger.LogInformation(Constants.WrongCode);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongCode;
                    response.Message = Constants.WrongCode;
                }
                else if (wrongPin == true)
                {
                    _logger.LogInformation(Constants.WrongPin);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongPin;
                    response.Message = Constants.WrongPin;
                }
                else if (wrongFace == true)
                {
                    _logger.LogInformation(Constants.WrongFace);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongFace;
                    response.Message = Constants.WrongFace;
                }

                if (request.authenticationScheme.Equals(AuthNSchemeConstants.TOTP))
                {
                    _logger.LogInformation(Constants.WrongCredentials);
                    response.Message = Constants.WrongCredentials;
                }
                // Return failed response
                response.Success = false;

                // Modify temporary session
                var (retValue1, errorMsg1) = await _cacheClient.Add
                    (CacheNames.TemporarySession, tempSession.TemporarySessionId,
                    tempSession);
                if (0 != retValue1)
                {
                    _logger.LogError("TemporarySession add failed");
                    var cacheResponse = new VerifyUserAuthDataResponse();
                    cacheResponse.Success = false;
                    errorMessage = _helper.GetRedisErrorMsg(retValue1,
                        ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                    cacheResponse.Message = errorMessage;
                    return cacheResponse;
                }
                return response;
            }
            if (true == isAuthNPassed)
            {
                tempSession.AllAuthNDone = true;
                tempSession.AdditionalValue = DTInternalConstants.S_True;

                if (tempSession.Clientdetails.ClientId ==
                        DTInternalConstants.DTPortalClientId)
                {
                    userInfo.Suid = userInfo.Id.ToString();
                }

                UserLoginDetail userLoginDetail = null;
                try
                {
                    userLoginDetail = await _unitOfWork.UserLoginDetail.
                    GetUserLoginDetailAsync(userInfo.Suid.ToString());
                }
                catch (Exception)
                {
                    VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                    response.Message = errorMessage;
                    return response;
                }

                if (null != userLoginDetail)
                {
                    if ((userLoginDetail.WrongCodeCount > 0)
                        || (userLoginDetail.WrongPinCount > 0)
                        || (userLoginDetail.DeniedCount > 0))
                    {
                        userLoginDetail.WrongPinCount = 0;
                        userLoginDetail.WrongCodeCount = 0;
                        userLoginDetail.DeniedCount = 0;

                        try
                        {
                            _unitOfWork.UserLoginDetail.Update(userLoginDetail);
                            await _unitOfWork.SaveAsync();
                        }
                        catch
                        {
                            _logger.LogError("UserLoginDetail update failed");
                            VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_UPDATE_LOGIN_DETAILS_FAILED);
                            response.Message = errorMessage;
                            return response;
                        }
                    }
                }
                if (tempSession.Clientdetails.ClientId ==
                    DTInternalConstants.DTPortalClientId)
                {
                    if (userId.LastLoginTime == null)
                    {
                        userId.LastLoginTime = DateTime.Now;
                    }
                    else
                    {
                        userId.LastLoginTime = userId.CurrentLoginTime;
                    }
                    userId.CurrentLoginTime = DateTime.Now;

                    try
                    {
                        _unitOfWork.Users.Update(userId);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("UserLoginDetail update failed : {0}",
                             error.Message);
                        VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                        response.Success = false;
                        errorMessage = _helper.GetErrorMsg(
                            ErrorCodes.DB_USER_UPDATE_LOGIN_DETAILS_FAILED);
                        response.Message = errorMessage;
                        return response;
                    }
                }
            }
            if (request.authenticationScheme == AuthNSchemeConstants.FACE
                || request.authenticationScheme == AuthNSchemeConstants.PIN)
            {
                tempSession.AuthNSuccessList.Add(AuthNSchemeConstants.PUSH_NOTIFICATION);
            }
            else
            {
                tempSession.AuthNSuccessList.Add(request.authenticationScheme);
            }

            if (tempSession.AuthNSuccessList.Count != tempSession.PrimaryAuthNSchemeList.Count)
            {
                if (true == tempSession.PrimaryAuthNSchemeList[tempSession.AuthNSuccessList.Count].Equals(AuthNSchemeConstants.PUSH_NOTIFICATION)
                    || (true == tempSession.PrimaryAuthNSchemeList[tempSession.AuthNSuccessList.Count].Equals(AuthNSchemeConstants.PUSH_NOTIFICATION) && isICPFace == "true"))
                {
                    var res = GetRandomNosList(3, 3);
                    if (null == res)
                    {
                        _logger.LogError("GetRandomNosList failed");
                        var VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                        VerifyUserAuthDataResponse.Message = "Internal server error";
                        VerifyUserAuthDataResponse.Success = false;
                        return VerifyUserAuthDataResponse;
                    }

                    // shuffle random codes
                    var randomIndex = GetRandomIndex(3);
                    randomcodes = string.Join(", ", res);
                    tempSession.RandomCode = res.ElementAt(randomIndex);
                    randomcode = res.ElementAt(randomIndex);

                    // Send notification to mobile
                    var authSchema = isICPFace == "true" ? AuthNSchemeConstants.UAEKYCFACE : AuthNSchemeConstants.PUSH_NOTIFICATION;
                    var authnNotification = new AuthnNotification()
                    {
                        AuthnScheme = authSchema,
                        RandomCodes = randomcodes,
                        AuthnToken = tempSession.TemporarySessionId,
                        RegistrationToken = userInfo.DeviceToken,
                        ApplicationName = tempSession.Clientdetails.AppName
                    };

                    _logger.LogInformation("NOTIFICATION RANDOM CODES:'{0}' ",
                        authnNotification.RandomCodes);

                    try
                    {
                        var result = await _pushNotificationClient.SendAuthnNotification(
                            authnNotification);
                        if (null == result)
                        {
                            _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                                " failed");
                            var VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                            VerifyUserAuthDataResponse.Message = Constants.NotificationSendFailed;
                            VerifyUserAuthDataResponse.Success = false;
                            return VerifyUserAuthDataResponse;
                        }
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("_pushNotificationClient." +
                            "SendAuthnNotification " +
                            "failed : {0}", error.Message);
                        var VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                        VerifyUserAuthDataResponse.Message = Constants.NotificationSendFailed;
                        VerifyUserAuthDataResponse.Success = false;
                        return VerifyUserAuthDataResponse;
                    }
                }
            }
            // Modify temporary session
            var (retValue, errorMsg) = await _cacheClient.Add(
                CacheNames.TemporarySession,
                tempSession.TemporarySessionId,
                tempSession);
            if (0 != retValue)
            {
                _logger.LogError("TemporarySession add failed");
                var response = new VerifyUserAuthDataResponse();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(retValue,
                    ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                response.Message = errorMessage;
                return response;
            }

            if (tempSession.AuthNSuccessList.Count ==
            tempSession.PrimaryAuthNSchemeList.Count)
            {
                // globalsession 
                var isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                    tempSession.UserId);
                if (CacheCodes.KeyExist == isExists.retValue)
                {
                    IList<string> userSessions = null;
                    try
                    {
                        userSessions = await _cacheClient.Get<IList<string>>
                            (CacheNames.UserSessions, tempSession.UserId);
                        if (userSessions == null)
                        {
                            _logger.LogError("Failed to get User Sessions");
                            var response = new VerifyUserAuthDataResponse();
                            response.Success = false;
                            response.Message = _helper.GetErrorMsg(
                                ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                            return response;
                        }
                    }
                    catch (CacheException ex)
                    {
                        _logger.LogError("Failed to get User Sessions");
                        var response = new VerifyUserAuthDataResponse();
                        response.Success = false;
                        response.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                            ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                        return response;
                    }

                    if (userSessions.Count > 0)
                    {
                        var cacheres = await _cacheClient.Remove(
                            CacheNames.GlobalSession,
                            userSessions.First());
                        if (0 != cacheres.retValue)
                        {
                            _logger.LogError("GlobalSession Remove failed");
                            //var response = new Response();
                            //response.Success = false;
                            //response.Message = Constants.InternalError;
                            //return response;
                        }

                        var res = await _cacheClient.Remove(CacheNames.UserSessions,
                            tempSession.UserId);
                        if (0 != res.retValue)
                        {
                            _logger.LogError("UserSessions Remove failed");
                            //var response = new Response();
                            //response.Success = false;
                            //response.Message = Constants.InternalError;
                            //return response;
                        }
                    }
                }

                // Generate global sessionid
                var GlobalSessionId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

                // Prepare global session object
                GlobalSession globalSession = new GlobalSession
                {
                    GlobalSessionId = GlobalSessionId,
                    UserId = tempSession.UserId,
                    FullName = userInfo.DisplayName,
                    IpAddress = tempSession.IpAddress,
                    MacAddress = tempSession.MacAddress,
                    UserAgentDetails = tempSession.UserAgentDetails,
                    AuthenticationScheme = tempSession.AuthNSuccessList.Last(),
                    //LoggedInTime = DateTime.Now.ToString().Replace("/", "-"),
                    //LastAccessTime = DateTime.Now.ToString().Replace("/", "-"),
                    TypeOfDevice = tempSession.TypeOfDevice,
                    CoRelationId = tempSession.CoRelationId,
                    ClientId = new List<string>() { },
                    OperationsDetails = new List<OperationsDetails>() { }
                };

                globalSession.LoggedInTime = DateTime.UtcNow.Ticks.ToString();
                globalSession.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

                if (tempSession.LoginProfile != null)
                {
                    globalSession.LoginProfile = tempSession.LoginProfile;
                }
                // Add global session in cache
                var cacheAdd = await _cacheClient.Add(CacheNames.GlobalSession,
                    GlobalSessionId, globalSession);
                if (0 != cacheAdd.retValue)
                {
                    _logger.LogError("GlobalSession Add failed");
                    var response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    errorMessage = _helper.GetRedisErrorMsg(cacheAdd.retValue,
                        ErrorCodes.REDIS_GLOBAL_SESS_ADD_FAILED);
                    response.Message = errorMessage;
                    return response;
                }

                var globalSessionList = new List<string>();
                globalSessionList.Add(GlobalSessionId);

                cacheAdd = await _cacheClient.Add(CacheNames.UserSessions,
                    tempSession.UserId,
                    globalSessionList);
                if (0 != cacheAdd.retValue)
                {
                    _logger.LogError("UserSessions Add failed");
                    var response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    errorMessage = _helper.GetRedisErrorMsg(cacheAdd.retValue,
                        ErrorCodes.REDIS_USER_SESS_ADD_FAILED);
                    response.Message = errorMessage;
                    return response;
                }


            }

            // Return success response
            if (tempSession.AdditionalValue == DTInternalConstants.S_True)
            {
                var VerifyUserAuthDataResponse = new VerifyUserAuthDataResponse();
                VerifyUserAuthDataResponse.Success = true;
                VerifyUserAuthDataResponse.Message = DTInternalConstants.AuthNDone;

                if (tempSession.PrimaryAuthNSchemeList.Count() !=
                  tempSession.AuthNSuccessList.Count)
                {
                    if (true == tempSession.PrimaryAuthNSchemeList
                    [tempSession.AuthNSuccessList.Count].Equals("PUSH_NOTIFICATION"))
                    {
                        verifyUserAuthResult fidOptions = new verifyUserAuthResult();
                        fidOptions.RandomCode = randomcode;
                        VerifyUserAuthDataResponse.Result = fidOptions;
                    }
                }
                _logger.LogInformation(DTInternalConstants.AuthNDone);
                _logger.LogDebug("VerifyUserAuthData response: {0}",
                    JsonConvert.SerializeObject(VerifyUserAuthDataResponse));
                _logger.LogDebug("<--VerifyUserAuthData");
                return VerifyUserAuthDataResponse;
            }
            else
            {
                var response = new VerifyUserAuthDataResponse();
                response.Success = false;
                response.Message = Constants.AuthNFailed;
                _logger.LogInformation(Constants.AuthNFailed);
                _logger.LogDebug("VerifyUserAuthData response: {0}",
                    JsonConvert.SerializeObject(response));
                _logger.LogDebug("<--VerifyUserAuthData");
                return response;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<IsUserVerifiedResponse> IsUserVerified(string authNToken)
        {
            _logger.LogDebug("-->IsUserVerified");

            // Validate input
            if (null == authNToken)
            {
                _logger.LogError(Constants.InvalidArguments);
                IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }
            /*
                        // Check whether the temporary session exists
                        var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                            authNToken);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError(Constants.AuthnTokenExpired);
                            IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                            response.Success = false;
                            response.Message = Constants.AuthnTokenExpired;
                            response.Status = DTInternalConstants.FailedMsg;
                            return response;
                        }
            */
            TemporarySession tempSession = null;

            try
            {
                // Get the temporary session object
                tempSession = await _cacheClient.Get<TemporarySession>
                    (CacheNames.TemporarySession, authNToken);
                if (null == tempSession)
                {
                    _logger.LogError(Constants.AuthnTokenExpired);
                    IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                    response.Success = false;
                    response.Message = Constants.AuthnTokenExpired;
                    response.Status = DTInternalConstants.FailedMsg;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Temporary Session Record");
                var response = new IsUserVerifiedResponse();
                response.Success = false;
                var errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_TEMP_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            // If all authn done and status is success
            if ((tempSession.NotificationAuthNDone == true) &&
                (tempSession.NotificationAdditionalValue == DTInternalConstants.S_True))
            {
                IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                response.Success = true;
                response.Status = DTInternalConstants.SuccessMsg;
                return response;
            }

            // If all authn done and status is false
            else if ((tempSession.NotificationAdditionalValue == DTInternalConstants.E_False) &&
                (tempSession.NotificationAuthNDone == true))
            {
                IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                response.Success = false;
                response.Status = DTInternalConstants.FailedStatus;
                response.Message = Constants.AuthNFailed;
                return response;
            }

            // If authn is pending
            else if ((tempSession.NotificationAdditionalValue != DTInternalConstants.pending) &&
                (tempSession.NotificationAuthNDone == false))
            {
                IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                response.Success = false;
                response.Status = DTInternalConstants.FailedStatus;
                response.Message = tempSession.NotificationAdditionalValue;
                return response;
            }
            else
            {
                IsUserVerifiedResponse response = new IsUserVerifiedResponse();
                response.Success = true;
                response.Status = DTInternalConstants.pending;
                return response;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<GetLoginSessionResponse> GetLoginSession(
            string authNToken)
        {
            _logger.LogDebug("--->GetLoginSession");

            // validate input
            if (null == authNToken)
            {
                _logger.LogError(Constants.InvalidArguments);
                GetLoginSessionResponse response = new GetLoginSessionResponse();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }
            TemporarySession tempSession = null;

            try
            {
                // Get the temporary session object
                tempSession = await _cacheClient.Get<TemporarySession>
                    (CacheNames.TemporarySession, authNToken);
                if (null == tempSession)
                {
                    _logger.LogError(Constants.AuthnTokenExpired);
                    GetLoginSessionResponse response = new GetLoginSessionResponse();
                    response.Success = false;
                    response.Message = Constants.AuthnTokenExpired;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Temporary Session Record");
                var response = new GetLoginSessionResponse();
                response.Success = false;
                var errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_TEMP_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            var userGlobalSession = new GlobalSession();

            IList<string> userSessions = null;

            try
            {
                // Get usersessions
                userSessions = await _cacheClient.Get<IList<string>>(
                    CacheNames.UserSessions,
                    tempSession.UserId);
                if (null == userSessions)
                {
                    _logger.LogError("Failed to get user sessions");
                    var cacheResponse = new GetLoginSessionResponse();
                    cacheResponse.Success = false;
                    cacheResponse.Message = Constants.UserSessionNotFound;
                    return cacheResponse;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get user sessions");
                var cacheResponse = new GetLoginSessionResponse();
                cacheResponse.Success = false;
                cacheResponse.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                    ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                return cacheResponse;
            }

            if (userSessions.Count == 0)
            {
                _logger.LogError(Constants.UserSessionNotFound);
                GetLoginSessionResponse response = new GetLoginSessionResponse();
                response.Success = false;
                response.Message = Constants.UserSessionNotFound;

                return response;
            }
            else
            {
                try
                {
                    userGlobalSession = await _cacheClient.Get<GlobalSession>
                        (CacheNames.GlobalSession, userSessions.First());
                    if (null == userGlobalSession)
                    {
                        _logger.LogError("GlobalSession not found");
                        GetLoginSessionResponse cacheResponse =
                            new GetLoginSessionResponse();
                        cacheResponse.Success = false;
                        cacheResponse.Message = Constants.InternalError;
                        return cacheResponse;
                    }
                }
                catch (CacheException ex)
                {
                    _logger.LogError("Failed to get Global Session Record");
                    var cacheResponse = new GetLoginSessionResponse();
                    cacheResponse.Success = false;
                    cacheResponse.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                        ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                    return cacheResponse;
                }
                try
                {
                    userGlobalSession.LoggedClients=new List<string> { tempSession.Clientdetails.ClientId};

                    var (retValue, errorMsg) = await _cacheClient.Add(CacheNames.GlobalSession,
                        userGlobalSession.GlobalSessionId, userGlobalSession);
                    if (0 != retValue)
                    {
                        _logger.LogError("GlobalSession update failed");
                        return new GetLoginSessionResponse()
                        {
                            Success = false,
                            Message = _helper.GetErrorMsg(ErrorCodes.REDIS_GLOBAL_SESS_ADD_FAILED)
                        };
                    }
                }
                catch(CacheException)
                {
                    _logger.LogError("Failed to update Global Session Record");
                    return new GetLoginSessionResponse()
                    {
                        Success = false,
                        Message = _helper.GetErrorMsg(ErrorCodes.REDIS_GLOBAL_SESS_ADD_FAILED)
                    };
                }
                GetLoginSessionResponse response = new GetLoginSessionResponse();
                response.Success = true;
                response.Session = userGlobalSession.GlobalSessionId;
            }
            var scopes = tempSession.Clientdetails.Scopes.Split(new char[] { ' ', '\t' });

            var scopesList = await _scopeService.ListScopeAsync();

            List<ScopeInfoLog> scopeInfoLogs = new List<ScopeInfoLog>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopesList.FirstOrDefault
                    (s => s.Name.Equals(scope, StringComparison.OrdinalIgnoreCase));
                if (scopeDetail != null)
                {
                    ScopeInfoLog scopeInfoLog = new ScopeInfoLog
                    {
                        Name = scopeDetail.Name,
                        DisplayName = scopeDetail.Description,
                        Version = scopeDetail.Version
                    };
                    scopeInfoLogs.Add(scopeInfoLog);
                }
            }
            if (!configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                var authNTime = CalculateTime(tempSession.AuthNStartTime);
                if (-1 == authNTime)
                {
                    GetLoginSessionResponse response = new GetLoginSessionResponse();
                    response.Success = false;
                    response.Message = Constants.InternalError;
                    return response;
                }

                var logMessage = "Total Authentication time taken in seconds: "
                    + authNTime;

                // Send central, service log message
                var logResponse =await _LogClient.SendAuthenticationLogMessage(
                        tempSession,
                        userGlobalSession.UserId,
                        LogClientServices.AuthenticationSuccess,
                        logMessage,
                        LogClientServices.Success,
                        LogClientServices.Business,
                        true,
                        JsonConvert.SerializeObject(scopeInfoLogs)
                        );
                if (false == logResponse.Success)
                {
                    _logger.LogError("SendAuthenticationLogMessage failed: " +
                        "{0}", logResponse.Message);
                }
            }

            // Remove temporary session
            var cacheAdd = await _cacheClient.Remove(CacheNames.TemporarySession,
                authNToken);
            if (0 != cacheAdd.retValue)
            {
                _logger.LogError("TemporarySession Remove Failed");
                GetLoginSessionResponse response = new GetLoginSessionResponse();
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            GetLoginSessionResponse loginResponse = new GetLoginSessionResponse();
            loginResponse.Success = true;
            loginResponse.Session = userGlobalSession.GlobalSessionId;
            loginResponse.Suid = tempSession.UserId;
            _logger.LogDebug("GetLoginSession response: {0}",
                JsonConvert.SerializeObject(loginResponse));
            _logger.LogDebug("<--GetLoginSession");

            return loginResponse;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<GetAuthZCodeResponse> GetAuthorizationCode
            (GetAuthZCodeRequest request)
        {

            _logger.LogDebug("-->GetAuthorizationCode");

            // Validate input
            if (null == request)
            {
                _logger.LogError(Constants.InvalidArguments);
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = Constants.InvalidArguments;

                return response;
            }
            string errorMessage = string.Empty;
            /*
                        // Check globalsession 
                        var RetCode = await _cacheClient.Exists(CacheNames.GlobalSession,
                            request.GlobalSessionId);
                        if (CacheCodes.KeyExist != RetCode.retValue)
                        {
                            _logger.LogError(Constants.SessionMismatch);
                            GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                            response.Success = false;
                            response.Message = Constants.SessionMismatch;
                            return response;
                        }
            */
            Client clientDetails = null;
            try
            {
                // Get client details
                clientDetails = await _unitOfWork.Client.GetClientByClientIdAsync
                (request.ClientDetails.clientId);
                if (null == clientDetails)
                {
                    _logger.LogError("GetIdByClientId failed, ClientId not found");
                    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                    response.Success = false;
                    response.Message = OIDCConstants.ClientNotFound;
                    return response;
                }
            }
            catch (Exception)
            {
                var response = new GetAuthZCodeResponse();
                response.Success = false;
                errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                response.Message = errorMessage;
                return response;
            }

            // Check client state - if not active 
            if (clientDetails.Status != StatusConstants.ACTIVE)
            {
                _logger.LogError(OIDCConstants.ClientNotActive);
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = OIDCConstants.ClientNotActive;
                return response;
            }

            GlobalSession globalSession = null;
            try
            {
                // Get GlobalSession
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession,
                    request.GlobalSessionId);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                    response.Success = false;
                    response.Message = Constants.SessionMismatch;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                var response = new GetAuthZCodeResponse();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            //// Compare session timeout
            //var sessionTime = DateTime.Now - Convert.ToDateTime(
            //    globalSession.LastAccessTime);

            //long loginTicks = long.Parse(globalSession.LoggedInTime);
            //// Compare session timeout
            //TimeSpan loggedInTime =
            //    TimeSpan.FromTicks(DateTime.UtcNow.Ticks - loginTicks);

            // Compare session timeout
            long lastAccessTicks = long.Parse(globalSession.LastAccessTime);
            TimeSpan sessionTime =
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks - lastAccessTicks);

            if (sessionTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout)
            {
                // Remove global session
                var CacheRes = await _cacheClient.Remove("GlobalSession",
                     globalSession.GlobalSessionId);
                if (0 != CacheRes.retValue)
                {
                    _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                            "remove failed");

                    GetAuthZCodeResponse res = new GetAuthZCodeResponse();
                    res.Success = false;
                    res.Message = "Internal server error";
                    return res;
                }

                // globalsession 
                var sesIsExists = await _cacheClient.Exists(CacheNames.UserSessions,
                    globalSession.UserId);
                if (CacheCodes.KeyExist == sesIsExists.retValue)
                {
                    IList<string> userSessions = null;

                    try
                    {
                        // Get usersessions
                        userSessions = await _cacheClient.Get<IList<string>>(
                            CacheNames.UserSessions,
                            globalSession.UserId);
                        if (null == userSessions)
                        {
                            _logger.LogError("Failed to get user sessions");
                            var cacheResponse = new GetAuthZCodeResponse();
                            cacheResponse.Success = false;
                            cacheResponse.Message = Constants.UserSessionNotFound;
                            return cacheResponse;
                        }
                    }
                    catch (CacheException ex)
                    {
                        _logger.LogError("Failed to get user sessions");
                        var cacheResponse = new GetAuthZCodeResponse();
                        cacheResponse.Success = false;
                        cacheResponse.Message = _helper.GetRedisErrorMsg(ex.ErrorCode,
                            ErrorCodes.REDIS_USER_SESS_GET_FAILED);
                        return cacheResponse;
                    }

                    if (userSessions.Count > 0)
                    {
                        var res = await _cacheClient.Remove(CacheNames.UserSessions,
                            globalSession.UserId);
                        if (0 != res.retValue)
                        {
                            _logger.LogError("UserSessions Remove failed");

                            GetAuthZCodeResponse Response = new GetAuthZCodeResponse();
                            Response.Success = false;
                            Response.Message = Constants.InternalError;
                            return Response;
                        }
                    }
                }

                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                _logger.LogError("GlobalSession expired/not exists");
                response.Success = false;
                response.Message = "GlobalSession expired/not exists";

                return response;
            }

            globalSession.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

            // Validate client scopes
            var isTrue = ValidateScopes(request.ClientDetails.scopes,
                clientDetails.Scopes);
            if (false == isTrue)
            {
                _logger.LogError(OIDCConstants.ClientScopesNotMatched);
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = OIDCConstants.ClientScopesNotMatched;
                return response;
            }

            var Scopes = request.ClientDetails.scopes.Split(new char[]
            { ' ', '\t' });

            var userConsent = new UserConsent();

            // Get userconsent
            //try
            //{
            //    userConsent = await _unitOfWork.UserConsent.GetUserConsent(
            //        globalSession.UserId,
            //        request.ClientDetails.clientId);
            //    if (null == userConsent)
            //    {
            //        var scopeList = String.Join(" ", Scopes);
            //        _logger.LogError("User Consent Required for ({0})", scopeList);

            //        GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //        response.Success = false;
            //        response.Message = String.Format("User Consent Required for" +
            //            " ({0})" +
            //            " : [{1}]", scopeList, globalSession.UserId);
            //        return response;
            //    }
            //}
            //catch (Exception error)
            //{
            //    _logger.LogError("GetUserConsent:{0}", error.Message);
            //    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //    return response;
            //}

            //var userScopes = JsonConvert.DeserializeObject<scopes>
            //(userConsent.Scopes);
            //if (null == userScopes)
            //{
            //    _logger.LogError("DeserializeObject failed");
            //    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //    response.Success = false;
            //    response.Message = OIDCConstants.ScopesNotExists;
            //    return response;
            //}

            //var unApprovedScopes = new List<string>();

            //// Check all the client requested scopes has user consent
            //// and user approved
            //var scopesList = await _scopeService.ListScopeAsync();

            //if (scopesList == null)
            //{
            //    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //    response.Success = false;
            //    response.Message = "Failed to get scope";
            //    return response;
            //}

            //foreach (var reqScope in Scopes)
            //{
            //    var scope = scopesList
            //            .FirstOrDefault(s => s.Name.Equals(reqScope, StringComparison.OrdinalIgnoreCase));

            //    if (scope == null)
            //    {
            //        GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //        response.Success = false;
            //        response.Message = "Failed to get scope";
            //        return response;
            //    }

            //    var reqScopeFound = false;
            //    foreach (var consentScope in userScopes.approved_scopes)
            //    {
            //        if (reqScope == consentScope.scope &&
            //           consentScope.permission == true && consentScope.version == scope.Version) ;
            //        {
            //            reqScopeFound = true;
            //        }
            //    }

            //    if (reqScopeFound == false)
            //    {
            //        if (reqScope != OAuth2Constants.openid)
            //            unApprovedScopes.Add(reqScope);
            //    }
            //}

            //if (unApprovedScopes.Any())
            //{
            //    var scopeList = String.Join(" ", unApprovedScopes);
            //    _logger.LogError("User Consent Required for ({0})",
            //        unApprovedScopes.ToArray());
            //    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
            //    response.Success = false;
            //    response.Message = string.Format("User Consent Required for" +
            //        "({0}) : [{1}]",
            //    scopeList, globalSession.UserId);
            //    return response;
            //}

            // Validate response types
            var Responsetypes = request.ClientDetails.response_type.Split(new char[]
            { ' ', '\t' });
            var clientResponseTypes = clientDetails.ResponseTypes.Split(new char[]
            { ' ', '\t' });
            var count = 0;
            foreach (var item in Responsetypes)
            {
                if (clientResponseTypes.Contains(item))
                {
                    count++;
                }
            }
            if (count != Responsetypes.Length)
            {
                _logger.LogError(OIDCConstants.ResponseTypeMisMatch);
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = OIDCConstants.ResponseTypeMisMatch;
            }

            // Validate redirecturi
            if (clientDetails.RedirectUri != request.ClientDetails.redirect_uri)
            {
                _logger.LogError(OIDCConstants.RedirectUriMisMatch);
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = OIDCConstants.RedirectUriMisMatch;
                return response;
            }

            // Check if application name is in globalsession
            if (globalSession.ClientId == null)
            {
                globalSession.ClientId = new List<string>()
                    {
                        clientDetails.ApplicationName.ToString()
                     };
            }
            else
            {
                if (!globalSession.ClientId.Contains(clientDetails.ApplicationName.
                    ToString()))
                {
                    globalSession.ClientId.Add(clientDetails.ApplicationName.
                        ToString());
                }
            }

            // Add globalsession to cache
            var cacheRes = await _cacheClient.Add(CacheNames.GlobalSession,
                request.GlobalSessionId,
                globalSession);
            if (0 != cacheRes.retValue)
            {
                _logger.LogError("GlobalSession add failed");
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            // Generate authorization code
            var AuthZCodeId = EncryptionLibrary.KeyGenerator.GetUniqueKey(92);
            if (null == AuthZCodeId)
            {
                _logger.LogError("GetUniqueKey failed");
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            // Prepare authorization code object
            Authorizationcode AuthZCode = new Authorizationcode
            {
                AuthZCode = AuthZCodeId,
                GlobalSessionId = request.GlobalSessionId,
                ClientId = request.ClientDetails.clientId,
                RedirectUrl = request.ClientDetails.redirect_uri,
                ResponseType = request.ClientDetails.response_type,
                Scopes = request.ClientDetails.scopes
            };

            if (true == request.ClientDetails.withPkce)
            {
                if (request.ClientDetails.grant_type.Equals
                    (OAuth2Constants.authorization_code_with_pkce))
                {
                    if ((null == request.pkcedetails.codeChallenge) ||
                            (null == request.pkcedetails.codeChallengeMethod))
                    {
                        _logger.LogError("No PKCE details found");
                        GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                        response.Success = false;
                        response.Message = OIDCConstants.GrantTypeMismatch;
                        return response;
                    }
                    AuthZCode.withPkce = true;
                    AuthZCode.PkceDetails.codeChallenge =
                        request.pkcedetails.codeChallenge;
                    AuthZCode.PkceDetails.codeChallengeMethod =
                        request.pkcedetails.codeChallengeMethod;
                }
            }

            // If scope has openid, validate nonce, add nonce
            if (AuthZCode.Scopes.Contains(OAuth2Constants.openid))
            {
                if (string.IsNullOrEmpty(request.ClientDetails.nonce))
                {
                    _logger.LogError("Nonce not received");
                    GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                    response.Success = false;
                    response.Message = OIDCConstants.NonceNotReceived;
                    return response;
                }
                AuthZCode.Nonce = request.ClientDetails.nonce;
            }

            // Add authorization code in cache
            var Res = await _cacheClient.Add(CacheNames.AuthorizationCode,
                AuthZCodeId,
                AuthZCode);
            if (0 != Res.retValue)
            {
                _logger.LogError("AuthorizationCode add failed");
                GetAuthZCodeResponse response = new GetAuthZCodeResponse();
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            GetAuthZCodeResponse successResponse = new GetAuthZCodeResponse();
            // return success response
            successResponse.Success = true;
            successResponse.Message = string.Empty;
            successResponse.AuthorizationCode = AuthZCodeId;
            if (request.ClientDetails.scopes.Contains(OAuth2Constants.openid))
            {
                successResponse.State = request.ClientDetails.state;
            }
            else
            {
                successResponse.State = string.Empty;
            }

            _logger.LogDebug("GetAuthorizationCode response: {0}",
                JsonConvert.SerializeObject(successResponse));
            _logger.LogDebug("<-- GetAuthorizationCode");
            return successResponse;
        }

        public async Task<GetAccessTokenResponse> GetAccessToken(
           GetAccessTokenRequest request, string authHeader, string type)
        {
            _logger.LogInformation("--->GetAccessToken");

            _logger.LogInformation(JsonConvert.SerializeObject(request));

            _logger.LogInformation(JsonConvert.SerializeObject(authHeader));

            _logger.LogInformation(JsonConvert.SerializeObject(type));

            // Validate input
            if (null == request || string.IsNullOrEmpty(request.grant_type))
            {
                _logger.LogError("Invalid Arguments");
                GetAccessTokenResponse response = new GetAccessTokenResponse();
                response.error = "Invalid Arguments";
                response.error_description = "Invalid Arguments";
                return response;
            }

            var errorMessage = string.Empty;
            GetAccessTokenResponse successRes = new GetAccessTokenResponse();
            Accesstoken accesstoken = new Accesstoken();
            var clientId = string.Empty;
            Client clientDetails = new Client();

            try
            {
                // Get client details
                clientDetails = await _unitOfWork.Client.
                    GetClientByClientIdAsync(request.client_id);
                if (null == clientDetails)
                {
                    _logger.LogError("GetClientByClientIdAsync failed, ClientId not found");
                    GetAccessTokenResponse response = new GetAccessTokenResponse();
                    response.error = "Invalid client";
                    response.error_description = "Client Not Found";
                    return response;
                }
            }
            catch (Exception)
            {
                var response = new GetAccessTokenResponse();
                response.error = "Internal Error";
                response.error_description = "Internal Error(100108)";
                return response;
            }

            // Check client state - if not active 
            if (clientDetails.Status != StatusConstants.ACTIVE)
            {
                _logger.LogError(OIDCConstants.ClientNotActive);
                GetAccessTokenResponse response = new GetAccessTokenResponse();
                response.error = Constants.InvalidArguments;
                response.error_description = OIDCConstants.ClientNotActive;
                return response;
            }

            var idpConfiguration = _globalConfiguration.GetIDPConfiguration();
            if (null == idpConfiguration)
            {
                _logger.LogError("Get IDP Configuration failed");
                GetAccessTokenResponse response = new GetAccessTokenResponse();
                response.error = Constants.InternalError;
                response.error_description = Constants.InternalError;
                return response;
            }
            var openidconnect = JsonConvert.DeserializeObject<OpenIdConnect>(
                idpConfiguration.openidconnect.ToString());

            if (request.grant_type == OAuth2Constants.ClientCredentials)
            {
                if (type.Contains(OAuth2Constants.PrivateKeyJwt))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(authHeader);

                    bool isTrue = false;
                    string algorithm = jwt.Header.Alg;

                    if (algorithm == "RS256")
                    {
                        isTrue = _tokenManager.VerifyJWTToken
                        (authHeader, request.client_id,
                        openidconnect.token_endpoint, clientDetails.PublicKeyCert);
                    }
                    else if (algorithm == "HS256")
                    {
                        isTrue = _tokenManager.VerifyHSJWTToken
                        (authHeader, request.client_id,
                        openidconnect.token_endpoint, clientDetails.ClientSecret);
                    }
                    else
                    {
                        _logger.LogError("Unsupported JWT signing algorithm");
                        GetAccessTokenResponse response = new GetAccessTokenResponse();
                        response.error = OIDCConstants.InvalidClient;
                        response.error_description = "Unsupported JWT signing algorithm";
                        return response;
                    }
                    if (false == isTrue)
                    {
                        _logger.LogError("VerifyJWTToken: Signature " +
                            "mismatch");
                        GetAccessTokenResponse response =
                            new GetAccessTokenResponse();
                        response.error = OIDCConstants.InvalidClient;
                        response.error_description =
                            OIDCConstants.InvalidClient;
                        return response;
                    }
                }
                else
                {
                    var result = await AuthenticateClient(authHeader);
                    if (0 != result)
                    {
                        _logger.LogError("AuthenticateClient Failed, " +
                            "invalidCredentials");
                        GetAccessTokenResponse response = new GetAccessTokenResponse();
                        response.error = OIDCConstants.InvalidClient;
                        response.error_description = OIDCConstants.InvalidCredentials;
                        return response;
                    }
                }

                var Accesstoken = new AccessTokenResponse();

                Accesstoken.accessToken = EncryptionLibrary.KeyGenerator.GetUniqueKey(144);
                if (null == Accesstoken.accessToken)
                {
                    _logger.LogError("Generate AccessToken Failed");
                    GetAccessTokenResponse response = new GetAccessTokenResponse();
                    response.error = OIDCConstants.InternalError;
                    response.error_description = OIDCConstants.InternalError;
                    return response;
                }

                // Prepare access token object
                accesstoken.AccessToken = Accesstoken.accessToken;
                accesstoken.ClientId = request.client_id;
                accesstoken.GrantType = request.grant_type;
                accesstoken.ExpiresAt = 3600;
                accesstoken.Scopes = clientDetails.Scopes;

                // Add access token record in cache
                var Res = await _cacheClient.Add(CacheNames.AccessToken,
                    Accesstoken.accessToken,
                    accesstoken);
                if (0 != Res.retValue)
                {
                    _logger.LogError("AccessToken Add Failed");
                    GetAccessTokenResponse response = new GetAccessTokenResponse();
                    response.error = OIDCConstants.InternalError;
                    response.error_description = OIDCConstants.InternalError;
                    return response;
                }

                var totalSec = await _cacheClient.TimeToLeave(CacheNames.AccessToken,
                    Accesstoken.accessToken);
                if (CacheCodes.KeyExist == totalSec.retValue)
                {
                    _logger.LogError("TimeToLeave Failed");
                    GetAccessTokenResponse response = new GetAccessTokenResponse();
                    response.error = OIDCConstants.InternalError;
                    response.error_description = OIDCConstants.InternalError;
                    return response;
                }

                successRes.Success = true;
                successRes.access_token = Accesstoken.accessToken;
                successRes.expires_in = (long)totalSec.totalSeconds;
                successRes.token_type = OAuth2Constants.Bearer;
            }

            _logger.LogDebug("<---GetAccessToken");
            return successRes;
        }
        public async Task<Response> LogoutUser(LogoutUserRequest request)
        {

            _logger.LogDebug("-->LogoutUser");

            // Variable declaration
            var errorMessage = string.Empty;
            Response response = new Response();

            // Validate input
            if (string.IsNullOrEmpty(request.GlobalSession))
            {
                _logger.LogError(Constants.InvalidArguments);
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }
            /*
                        var isExists = await _cacheClient.Exists(CacheNames.GlobalSession,
                            request.GlobalSession);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("GlobalSession expired/not exists");
                            response.Success = false;
                            response.Message = "GlobalSession expired/not exists";
                            return response;
                        }
            */
            GlobalSession globalSession = null;
            try
            {
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession, request.GlobalSession);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    response.Success = false;
                    response.Message = Constants.SessionMismatch;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                return response;
            }

            var task = await _cacheClient.Remove(CacheNames.GlobalSession,
                request.GlobalSession);
            if (0 != task.retValue)
            {
                _logger.LogError("GlobalSession Remove failed");
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                    ErrorCodes.REDIS_GLOBAL_SESS_REMOVE_FAILED);
                response.Message = errorMessage;
                return response;
            }

            //var isExists = await _cacheClient.Exists(CacheNames.UserSessions,
            //    globalSession.UserId);
            //if (CacheCodes.KeyExist == isExists.retValue)
            //{
            var result = await _cacheClient.Remove(CacheNames.UserSessions,
                        globalSession.UserId);
            if (0 != result.retValue)
            {
                _logger.LogError("UserSessions Remove failed");
                //response.Success = false;
                //errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                //    ErrorCodes.REDIS_USER_SESS_REMOVE_FAILED);
                //response.Message = errorMessage;
                //return response;
            }
            //}
            //else
            //{
            //    response.Success = false;
            //    response.Message = Constants.SessionsNotFound;
            //    return response;
            //}

            response.Success = true;

            TemporarySession tempSession = new TemporarySession();
            Clientdetails clientdetails = new Clientdetails()
            {
                ClientId = string.Join(" ", globalSession.ClientId),
                AppName = string.Join(" ", globalSession.ClientId)
            };

            tempSession.CoRelationId = globalSession.CoRelationId;
            tempSession.AuthNStartTime = DateTime.Now.ToString("s");
            tempSession.Clientdetails = clientdetails;

            // Send central, service log message
            var logResponse =await _LogClient.SendAuthenticationLogMessage(
                        tempSession,
                        globalSession.UserId,
                        LogClientServices.SubscriberLogOut,
                        LogClientServices.SubscriberLogOut,
                        LogClientServices.Success,
                        LogClientServices.Business,
                        false
                        );
            if (false == logResponse.Success)
            {
                _logger.LogError("SendAuthenticationLogMessage failed: " +
                    "{0}", logResponse.Message);
                //response.Success = false;
                //response.Message = Constants.InternalError;
                //return response;
            }

            _logger.LogDebug("<--LogoutUser failed");
            return response;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<SendMobileNotificationResponse> SendMobileNotification
            (string authnToken)
        {
            _logger.LogDebug("-->SendMobileNotification");

            var authnNotification = new AuthnNotification();

            // Validate input
            if (null == authnToken)
            {
                _logger.LogError(Constants.InvalidArguments);
                SendMobileNotificationResponse sendMobileNotificationRes =
                    new SendMobileNotificationResponse();
                sendMobileNotificationRes.Success = false;
                sendMobileNotificationRes.Message = Constants.
                    InvalidArguments;
                return sendMobileNotificationRes;
            }
            /*
                        // Check whether the temporary session exists
                        var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                            authnToken);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("authnToken expired/does not exists");
                            SendMobileNotificationResponse sendMobileNotificationRes =
                                new SendMobileNotificationResponse();
                            sendMobileNotificationRes.Success = false;
                            sendMobileNotificationRes.Message = Constants.TempSessionExpired;
                            return sendMobileNotificationRes;
                        }
            */
            TemporarySession tempSession = null;

            try
            {
                // Get the temporary session object
                tempSession = await _cacheClient.Get<TemporarySession>
                    (CacheNames.TemporarySession,
                    authnToken);
                if (null == tempSession)
                {
                    _logger.LogError("authnToken expired/does not exists");
                    SendMobileNotificationResponse sendMobileNotificationRes =
                        new SendMobileNotificationResponse();
                    sendMobileNotificationRes.Success = false;
                    sendMobileNotificationRes.Message = Constants.TempSessionExpired;
                    return sendMobileNotificationRes;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Temporary Session Record");
                var response = new SendMobileNotificationResponse();
                response.Success = false;
                response.Message = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_TEMP_SESS_GET_FAILED);
                return response;
            }

            if (tempSession.AllAuthNDone == true)
            {
                _logger.LogError("Subscriber already authenticated");
                SendMobileNotificationResponse sendMobileNotificationRes =
                    new SendMobileNotificationResponse();
                sendMobileNotificationRes.Success = true;
                sendMobileNotificationRes.Message =
                    Constants.SubAlreadyAuthenticated;
                return sendMobileNotificationRes;
            }

            var randomcodes = string.Empty;
            var errorMessage = string.Empty;

            if (tempSession.PrimaryAuthNSchemeList.Contains(
                AuthNSchemeConstants.PUSH_NOTIFICATION))
            {
                RandomGenerator randomGenerator = new RandomGenerator();
                try
                {
                    // Generate three random numbers
                    var randomNumbersList = randomGenerator.GenerateRandomNumbers(3);

                    // Get random index out of three
                    var randomIndex = randomGenerator.GetRandomIndex(3);

                    // Convert list to string
                    randomcodes = string.Join(",", randomNumbersList);

                    // Store one out of three numbers
                    tempSession.RandomCode = randomNumbersList.
                        ElementAt(randomIndex).ToString();
                    tempSession.AdditionalValue = DTInternalConstants.pending;
                    tempSession.NotificationAdditionalValue = DTInternalConstants.pending;
                }
                catch (Exception error)
                {
                    _logger.LogError("GenerateRandomNumbers failed: {0}", error.Message);
                    var sendMobileNotificationRes = new SendMobileNotificationResponse();
                    sendMobileNotificationRes.Success = false;
                    errorMessage = _helper.GetErrorMsg(
                         ErrorCodes.GENERATE_RANDOM_CODES_FAILED);
                    sendMobileNotificationRes.Message = errorMessage;
                    return sendMobileNotificationRes;
                }
            }

            // Create temporary session
            var task = await _cacheClient.Add(CacheNames.TemporarySession,
                tempSession.TemporarySessionId,
                tempSession);
            if (0 != task.retValue)
            {
                _logger.LogError("TemporarySession Add failed");
                var sendMobileNotificationRes = new SendMobileNotificationResponse();
                sendMobileNotificationRes.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                    ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                sendMobileNotificationRes.Message = errorMessage;
                return sendMobileNotificationRes;
            }

            Client clientInDb = null;

            try
            {
                clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(
                    tempSession.Clientdetails.ClientId);
                if (null == clientInDb)
                {
                    _logger.LogError("GetClientByClientIdAsync failed, not found");
                    var sendMobileNotificationRes = new SendMobileNotificationResponse();
                    sendMobileNotificationRes.Success = false;
                    errorMessage = _helper.GetErrorMsg(
                         ErrorCodes.DB_CLIENT_GET_DETAILS_FAILED);
                    sendMobileNotificationRes.Message = errorMessage;
                    return sendMobileNotificationRes;
                }
            }
            catch (Exception)
            {
                errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                var sendMobileNotificationRes = new SendMobileNotificationResponse();
                sendMobileNotificationRes.Success = false;
                sendMobileNotificationRes.Message = errorMessage;
                return sendMobileNotificationRes;
            }

            authnNotification.AuthnScheme = AuthNSchemeConstants.PUSH_NOTIFICATION;
            authnNotification.RandomCodes = randomcodes;
            authnNotification.AuthnToken = tempSession.TemporarySessionId;
            authnNotification.ApplicationName = clientInDb.ApplicationName;

            if (tempSession.PrimaryAuthNSchemeList.Contains(
                AuthNSchemeConstants.PUSH_NOTIFICATION))
            {
                if (tempSession.Clientdetails.ClientId ==
                    DTInternalConstants.DTPortalClientId)
                {
                    var user = await _unitOfWork.Users.GetUserbyNameAsync
                        (tempSession.UserId);
                    if (null == user)
                    {
                        _logger.LogError("GetUserbyNameAsync failed, not found");
                        SendMobileNotificationResponse sendMobileNotificationRes =
                            new SendMobileNotificationResponse();
                        sendMobileNotificationRes.Success = false;
                        sendMobileNotificationRes.Message =
                            Constants.SubscriberNotFound;
                        return sendMobileNotificationRes;
                    }

                    if (user.Status == StatusConstants.SUSPENDED)
                    {
                        _logger.LogInformation("user account is SUSPENDED");
                        SendMobileNotificationResponse sendMobileNotificationRes =
                            new SendMobileNotificationResponse();
                        sendMobileNotificationRes.Success = false;
                        sendMobileNotificationRes.Message =
                            Constants.SubAccountSuspended;
                        return sendMobileNotificationRes;
                    }
                    authnNotification.RegistrationToken = string.Empty;
                }
                else
                {
                    SubscriberView raUser = null;

                    try
                    {
                        raUser = await _unitOfWork.Subscriber.GetSubscriberInfoBySUID
                        (tempSession.UserId);
                        if (null == raUser)
                        {
                            _logger.LogError("GetSubscriberInfo failed, not found");
                            var sendMobileNotificationRes = new SendMobileNotificationResponse();
                            sendMobileNotificationRes.Success = false;
                            errorMessage = _helper.GetErrorMsg(
                                 ErrorCodes.DB_USER_GET_DETAILS_FAILED);
                            sendMobileNotificationRes.Message = errorMessage;
                            return sendMobileNotificationRes;
                        }
                    }
                    catch (Exception)
                    {
                        var sendMobileNotificationRes = new SendMobileNotificationResponse();
                        sendMobileNotificationRes.Success = false;
                        errorMessage = _helper.GetErrorMsg(ErrorCodes.DB_ERROR);
                        sendMobileNotificationRes.Message = errorMessage;
                        return sendMobileNotificationRes;
                    }

                    if (raUser.SubscriberStatus == StatusConstants.SUSPENDED)
                    {
                        _logger.LogError("Subscriber account is suspended");
                        SendMobileNotificationResponse sendMobileNotificationRes =
                            new SendMobileNotificationResponse();
                        sendMobileNotificationRes.Success = false;
                        sendMobileNotificationRes.Message =
                            Constants.SubAccountSuspended;
                        return sendMobileNotificationRes;
                    }
                    authnNotification.RegistrationToken = raUser.FcmToken;
                }

                var isICPFace = configuration["UseICPForAuth"];

                var authSchema = isICPFace == "true" ? AuthNSchemeConstants.UAEKYCFACE : AuthNSchemeConstants.PUSH_NOTIFICATION;
                // Send notification to mobile
                _logger.LogInformation("authSchema for notification - " + authSchema);
                //var authnNotification = new AuthnNotification()
                //{
                //    AuthnScheme = authSchema,
                //    RandomCodes = randomcodes,
                //    AuthnToken = tempAuthNSessId,
                //    RegistrationToken = userInfo.DeviceToken,
                //    ApplicationName = clientInDb.ApplicationName,
                //    DeviceName = GetOSFromUserAgent(temporarySession.UserAgentDetails),
                //    Timestamp = DateTime.UtcNow.ToString("s")
                //};
                authnNotification.AuthnScheme = authSchema.ToString();
                authnNotification.DeviceName =
                    GetOSFromUserAgent(tempSession.UserAgentDetails);
                authnNotification.Timestamp = DateTime.UtcNow.ToString("s");

                try
                {
                    var result = await _pushNotificationClient.SendAuthnNotification(
                        authnNotification);
                    if (null == result)
                    {
                        _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                       " failed");
                        SendMobileNotificationResponse sendMobileNotificationRes =
                            new SendMobileNotificationResponse();
                        sendMobileNotificationRes.Success = false;
                        sendMobileNotificationRes.Message =
                            Constants.NotificationSendFailed;
                        return sendMobileNotificationRes;
                    }
                }
                catch
                {
                    _logger.LogError("_pushNotificationClient.SendAuthnNotification" +
                        " failed");
                    SendMobileNotificationResponse sendMobileNotificationRes =
                        new SendMobileNotificationResponse();
                    sendMobileNotificationRes.Success = false;
                    sendMobileNotificationRes.Message =
                        Constants.NotificationSendFailed;
                    return sendMobileNotificationRes;
                }
            }

            SendMobileNotificationResponse successResponse =
                new SendMobileNotificationResponse();
            successResponse.Success = true;
            successResponse.Message = "Notification sent";
            successResponse.RandomCode = tempSession.RandomCode;

            _logger.LogDebug("<---SendMobileNotification");
            return successResponse;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> VerifyUserAuthNData(
                   VerifyUserAuthNDataRequest request)
        {
            _logger.LogDebug("-->VerifyUserAuthNData");
            // Validate input
            if (null == request || null == request.allowedScopesAndClaims ||
                string.IsNullOrEmpty(request.authnToken) ||
                string.IsNullOrEmpty(request.authenticationScheme) ||
                string.IsNullOrEmpty(request.authenticationData) ||
                string.IsNullOrEmpty(request.approved))
            {
                _logger.LogError(Constants.InvalidArguments);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }

            // Check whether the temporary session exists
            var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                request.authnToken);
            if (CacheCodes.KeyExist != isExists.retValue)
            {
                _logger.LogError(Constants.TempSessionExpired);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.TempSessionExpired;
                return response;
            }

            // Get the temporary session object
            var tempSession = await _cacheClient.Get<TemporarySession>
                (CacheNames.TemporarySession,
                request.authnToken);
            if (null == tempSession)
            {
                _logger.LogError(Constants.TempSessionExpired);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.TempSessionExpired;
                return response;
            }

            var isAuthSchm = false;
            if (tempSession.PrimaryAuthNSchemeList.Contains(
                request.authenticationScheme))
            {
                isAuthSchm = true;
            }

            // If authScheme is not matched, return error response
            //if (false == isAuthSchm)
            //{
            //    _logger.LogError
            //        (Constants.AuthSchemeMisMatch);
            //    Response response = new Response();
            //    response.Success = false;
            //    response.Message = Constants.AuthSchemeMisMatch;
            //    return response;
            //}

            isAuthSchm = false;
            int duration = 30;
            // If already, there any authentication success count
            bool check = !tempSession.AuthNSuccessList.Any();
            if (check)
            {
                if (tempSession.AuthNSuccessList.Contains(
                    request.authenticationScheme))
                {
                    isAuthSchm = true;
                }
            }
            if (true == isAuthSchm)
            {
                _logger.LogError(Constants.AuthSchemeAlreadyAuthenticated);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.AuthSchemeAlreadyAuthenticated;
                return response;
            }

            _logger.LogInformation("TempSession ClientID is {0}",
                tempSession.Clientdetails.ClientId);

            var userId = new UserTable();
            UserLookupItem userInfo = new UserLookupItem();
            userInfo.DisplayName = tempSession.DisplayName;
            userInfo.Suid = tempSession.UserId;

            var wrongPin = false;
            var denyCount = false;
            var wrongFace = false;
            var isAuthNPassed = false;
            var wrongVoice = false;
            // Compare authdata
            // Password authnscheme
            if (request.authenticationScheme.Equals(AuthNSchemeConstants.PASSWORD))
            {
                // Verify Password
                var passwordRes = await VerifyPassword(request.authenticationData,
                    userInfo.Suid);
                if (passwordRes.Success == true)
                {
                    isAuthNPassed = true;
                }
            }
            // FIDO2 authentication scheme
            else if (request.authenticationScheme == AuthNSchemeConstants.FIDO2)
            {
                // Verify Fido2
                var fido2Res = await VerifyFido2(request.authenticationData,
                    userInfo.Suid);
                if (fido2Res.Success == true)
                {
                    isAuthNPassed = true;
                }
            }
            // Push notification authentication scheme
            else if (request.authenticationScheme ==
                AuthNSchemeConstants.PUSH_NOTIFICATION || request.authenticationScheme == "PIN")
            {
                // Check if subscriber denied authentication
                if (string.Equals(request.approved, DTInternalConstants.E_False))
                {
                    _logger.LogError(Constants.SubscriberNotApproved);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.SubscriberNotApproved;
                    denyCount = true;
                }
                else
                {
                    var verifyPinRequest = new VerifyPinRequest();
                    verifyPinRequest.subscriberDigitalID = tempSession.UserId;
                    verifyPinRequest.authenticationPin = request.authenticationData;

                    //VerifyPIN from registration authority
                    var verifyPinResponse = await VerifyPushNotification(tempSession,
                        verifyPinRequest);

                    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    // For Bypassing HSM Verification
                    //VerifyPinResponse verifyPinResponse = new VerifyPinResponse();
                    //verifyPinResponse.success = true;
                    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                    if (false == verifyPinResponse.success)
                    {
                        if (verifyPinResponse.message.Equals
                            (Constants.PinVerifyFailed))
                        {
                            wrongPin = true;
                            isAuthNPassed = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(verifyPinResponse.message))
                                _logger.LogError("PKI service error: {0}",
                                    verifyPinResponse.message);
                            var response = new Response();
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                    else
                    {
                        isAuthNPassed = true;
                        _logger.LogInformation("PKI service success: {0}",
                            verifyPinResponse.success);
                    }
                }
            }
            /*else if (request.authenticationScheme.Equals(AuthNSchemeConstants.VOICE))
            {
                var voiceRes = await VerifyVoice(request.authenticationData, tempSession.UserId);
                if (voiceRes.Success == false)
                {
                    if (voiceRes.Message.Equals
                            (Constants.VoiceVerifyFailed))
                    {
                        _logger.LogInformation("Wrong Face");
                        isAuthNPassed = false;
                        wrongVoice = true;
                    }
                    else
                    {
                        return voiceRes;
                    }
                }
                else
                {
                    isAuthNPassed = true;
                }

            }
            else if (request.authenticationScheme == AuthNSchemeConstants.MOBILE_TOTP)
            {
                // Check if subscriber denied authentication
                if (string.Equals(request.approved, DTInternalConstants.E_False))
                {
                    _logger.LogError(Constants.SubscriberNotApproved);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.SubscriberNotApproved;
                    denyCount = true;
                }

                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                    (userInfo.Suid, AuthNSchemeConstants.MOBILE_TOTP);
                if (null == userAuthData)
                {
                    Response VerifyUserAuthDataResponse = new Response();
                    VerifyUserAuthDataResponse.Success = false;
                    VerifyUserAuthDataResponse.Message = "User authentication data not found";
                    return VerifyUserAuthDataResponse;
                }

                // Verify TOTP
                var isSuccess = TOTPLibrary.VerifyTOTP(userAuthData.AuthData,
                    request.authenticationData);
                if (false == isSuccess)
                {
                    VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                    response.Success = false;
                    response.Message = Constants.RandomCodeNotMatched;
                    wrongPin = true;
                    isAuthNPassed = false;
                }
                else
                {
                    isAuthNPassed = true;
                }
            }*/
            else if (request.authenticationScheme == AuthNSchemeConstants.FACE)
            {
                if (string.Equals(request.approved, DTInternalConstants.E_False))
                {
                    _logger.LogError(Constants.SubscriberNotApproved);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.SubscriberNotApproved;
                    denyCount = true;
                }
                else
                {
                    _logger.LogInformation("Face Verification Started");

                    var verifyFaceRequest = new VerifyFaceRequest();

                    var response = new Response();

                    var faceVerificationStartTime = DateTime.Now.ToString("s");

                    var verifyfaceresponse = VerifyFace(request.authenticationData);

                    if (false == verifyfaceresponse.success)
                    {
                        if (verifyfaceresponse.message.Equals
                            (Constants.FaceVerifyFailed))
                        {
                            _logger.LogInformation("Wrong Face");
                            wrongFace = true;
                            isAuthNPassed = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(verifyfaceresponse.message))
                            {
                                _logger.LogError("PKI service error: {0}",
                                    verifyfaceresponse.message);

                                response.Success = false;
                                response.Message = verifyfaceresponse.message;
                                return response;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(verifyfaceresponse.message))
                                    _logger.LogError("PKI service error: {0}",
                                        verifyfaceresponse.message);
                                var response1 = new Response();
                                response.Success = false;
                                response.Message = Constants.InternalError;
                                return response1;
                            }
                        }
                    }
                    else
                    {
                        isAuthNPassed = true;
                        _logger.LogInformation("Pin Verified: {0}",
                            verifyfaceresponse.success);
                    }
                }
            }
            else
            {
                _logger.LogError(Constants.AuthSchemeMisMatch);
                var response = new Response();
                response.Success = false;
                response.Message = Constants.AuthSchemeMisMatch;
                return response;
            }

            // Failed case
            if ((isAuthNPassed == false) || (denyCount == true)
                || (wrongPin == true) || (wrongFace == true) || (wrongVoice = true))
            {
                // Get userpassword details
                if (userInfo.Id != 0)
                {
                    userInfo.Suid = userInfo.Id.ToString();
                }
                var userPwdDetailsinDB = await _unitOfWork.UserLoginDetail.
                    GetUserLoginDetailAsync(userInfo.Suid.ToString());
                if (null == userPwdDetailsinDB)
                {
                    _logger.LogError("GetUserPasswordDetailAsync failed, not found");
                    var response = new Response();
                    response.Success = false;
                    response.Message = Constants.InternalError;

                    userPwdDetailsinDB = new UserLoginDetail();

                    // Prepare user password object
                    userPwdDetailsinDB.UserId = userInfo.Suid.ToString();
                    userPwdDetailsinDB.BadLoginTime = DateTime.Now;
                    userPwdDetailsinDB.IsReversibleEncryption = false;
                    userPwdDetailsinDB.IsScrambled = false;
                    userPwdDetailsinDB.PriAuthSchId = 0;
                    if ((wrongPin == true) || (request.authenticationScheme == "PASSWORD") || (wrongFace == true) || (wrongVoice == true))
                    {
                        userPwdDetailsinDB.WrongPinCount = 1;
                    }
                    if (denyCount == true)
                    {
                        userPwdDetailsinDB.DeniedCount = 1;
                    }

                    try
                    {
                        await _unitOfWork.UserLoginDetail.AddAsync
                            (userPwdDetailsinDB);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("UserLoginDetail.AddAsync failed: {0}",
                            error.Message);
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }
                }
                else
                {
                    // Prepare user password object
                    userPwdDetailsinDB.UserId = userInfo.Suid.ToString();

                    userPwdDetailsinDB.BadLoginTime = DateTime.Now;
                    if ((wrongPin == true) || (request.authenticationScheme == "PASSWORD") || (wrongFace == true) || (wrongVoice == true))
                    {
                        userPwdDetailsinDB.WrongPinCount = userPwdDetailsinDB.
                            WrongPinCount + 1;
                    }
                    if (denyCount == true)
                    {
                        userPwdDetailsinDB.DeniedCount = userPwdDetailsinDB.
                            DeniedCount + 1;
                    }

                    // Update userpassword details
                    try
                    {
                        _unitOfWork.UserLoginDetail.Update(userPwdDetailsinDB);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("UserLoginDetail Update failed: {0}",
                            error.Message);
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.InternalError;

                        return response;
                    }
                }

                if ((userPwdDetailsinDB.WrongPinCount >=
                    ssoConfig.sso_config.wrong_pin)
                    || (userPwdDetailsinDB.WrongCodeCount >=
                    ssoConfig.sso_config.wrong_code)
                    || (userPwdDetailsinDB.DeniedCount >=
                    ssoConfig.sso_config.deny_count))
                {
                    var statusUpdateRequest =
                        new SubscriberStatusUpdateRequest();

                    statusUpdateRequest.description =
                        LogClientServices.SubscriberStatusUpdate;
                    statusUpdateRequest.subscriberStatus =
                        StatusConstants.SUSPENDED;
                    statusUpdateRequest.subscriberUniqueId = userInfo.Suid;

                    var statusResponse = await _raServiceClient.
                        SubscriberStatusUpdate(statusUpdateRequest);
                    if ((null == statusResponse) ||
                        (statusResponse.success == false))
                    {
                        _logger.LogError("SubscriberStatusUpdate failed, " +
                            "{0}", statusResponse.message);
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }

                    // Send central, service log message
                    var logResponse =await _LogClient.SendAuthenticationLogMessage(
                                tempSession,
                                userInfo.Suid,
                                LogClientServices.SubscriberStatusUpdate,
                                "Subscriber status changed to SUSPENDED",
                                LogClientServices.Success,
                                LogClientServices.Business,
                                true
                                );
                    if (false == logResponse.Success)
                    {
                        _logger.LogError("SendAuthenticationLogMessage failed: " +
                            "{0}", logResponse.Message);
                        //return logResponse;
                    }
                }
            }
            if (true == isAuthNPassed)
            {
                // All authN done
                _logger.LogInformation(DTInternalConstants.AuthNDone);
                tempSession.AllAuthNDone = true;
                tempSession.AdditionalValue = DTInternalConstants.S_True;
                tempSession.allowedScopesAndClaims = request.allowedScopesAndClaims;

                var clientDetails = tempSession.Clientdetails;

                List<string> approvedClaims = new List<string>();

                foreach (var allowedScope in tempSession.allowedScopesAndClaims)
                {
                    var profile = await _unitOfWork.Scopes.GetScopeByNameAsync(allowedScope.name);

                    if (profile == null)
                    {
                        return new Response()
                        {
                            Success = false,
                            Message = "Profile Not Found"
                        };
                    }

                    var saveConsent = await _scopeService.isScopehaveSaveConsent(profile.Id);
                    if (saveConsent)
                    {
                        var userConsentProfile = await _unitOfWork.UserProfilesConsent.GetUserProfilesConsentByProfileAsync(tempSession.UserId, clientDetails.ClientId, profile.Id.ToString());

                        if (userConsentProfile != null)
                        {
                            List<Attributes> attributesList = new List<Attributes>();

                            var approvedAttributesList = JsonConvert.DeserializeObject<List<Attributes>>(userConsentProfile.Attributes);

                            foreach (var claim in allowedScope.claims)
                            {
                                approvedClaims.Add(claim);

                                var claimObject = approvedAttributesList.FirstOrDefault(attr => attr.name == claim);

                                if (claimObject == null)
                                {
                                    Attributes attribute1 = new Attributes()
                                    {
                                        name = claim,
                                        created_time = DateTime.Now,
                                        expiry_time = DateTime.Now.AddDays(duration),
                                        duration = duration
                                    };
                                    approvedAttributesList.Add(attribute1);
                                }
                                else
                                {
                                    claimObject.expiry_time = DateTime.Now.AddDays(duration);
                                    claimObject.duration = duration;
                                }
                            }
                            userConsentProfile.Attributes = JsonConvert.SerializeObject(approvedAttributesList);

                            userConsentProfile.ModifiedDate = DateTime.Now;

                            _unitOfWork.UserProfilesConsent.Update(userConsentProfile);

                            await _unitOfWork.SaveAsync();
                        }
                        else
                        {
                            List<Attributes> attributesList = new List<Attributes>();
                            foreach (var claim in allowedScope.claims)
                            {
                                approvedClaims.Add(claim);

                                Attributes attribute1 = new Attributes()
                                {
                                    name = claim,
                                    created_time = DateTime.Now,
                                    expiry_time = DateTime.Now.AddDays(duration),
                                    duration = duration
                                };
                                attributesList.Add(attribute1);
                            }
                            UserProfilesConsent userProfiles = new UserProfilesConsent()
                            {
                                Suid = tempSession.UserId,
                                Profile = profile.Id.ToString(),
                                ClientId = clientDetails.ClientId,
                                Attributes = JsonConvert.SerializeObject(attributesList),
                                Status = "ACTIVE",
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };
                            await _unitOfWork.UserProfilesConsent.AddAsync(userProfiles);
                            await _unitOfWork.SaveAsync();
                        }
                    }
                }

                await UpdateTransactionProfileConsent(tempSession.TransactionId, string.Join(",", approvedClaims), "APPROVED");

            }
            if (false == isAuthNPassed)
            {
                var response = new Response();
                _logger.LogError(Constants.AuthNFailed);
                tempSession.AllAuthNDone = false;
                tempSession.AdditionalValue = DTInternalConstants.E_False;

                if (request.authenticationScheme.Equals(
                    AuthNSchemeConstants.PASSWORD))
                {
                    _logger.LogInformation(Constants.WrongCredentials);
                    response.Message = Constants.WrongCredentials;
                }

                if (denyCount == true)
                {
                    _logger.LogInformation(Constants.UserDeniedAuthN);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue =
                        DTInternalConstants.DeniedConsent;
                    response.Message = DTInternalConstants.DeniedConsent;
                    await UpdateTransactionProfileConsent(tempSession.TransactionId, "NA", "REJECTED");
                }
                else if (wrongPin == true)
                {
                    _logger.LogInformation(Constants.WrongPin);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongPin;
                    response.Message = Constants.WrongPin;
                    await UpdateTransactionProfileConsent(tempSession.TransactionId, "NA", "WRONG PIN");
                }
                else if (wrongFace == true)
                {
                    _logger.LogInformation(Constants.WrongFace);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongFace;
                    response.Message = Constants.WrongFace;
                    await UpdateTransactionProfileConsent(tempSession.TransactionId, "NA", "FACE NOT MATCHED");
                }
                /*else if (wrongVoice == true)
                {
                    _logger.LogInformation(Constants.WrongVoice);
                    tempSession.AllAuthNDone = false;
                    tempSession.AdditionalValue = Constants.WrongVoice;
                    response.Message = Constants.WrongVoice;
                }*/
                // Return failed response
                response.Success = false;

                // Modify temporary session
                var (retValue2, errorMsg2) = await _cacheClient.Add
                    (CacheNames.TemporarySession, tempSession.TemporarySessionId,
                    tempSession);
                if (0 != retValue2)
                {
                    _logger.LogError("TemporarySession add failed");
                    var cacheResponse = new Response();
                    cacheResponse.Success = false;
                    cacheResponse.Message = Constants.InternalError;
                    return cacheResponse;
                }

                var identifier = string.Empty;

                if (null != userInfo.Suid)
                {
                    identifier = userInfo.Suid;
                }
                else
                {
                    identifier = userInfo.DisplayName;
                }

                if (!configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
                {
                    var logResponse =await _LogClient.SendAuthenticationLogMessage(
                    tempSession,
                    identifier,
                    LogClientServices.AuthenticationFailed,
                    LogClientServices.AuthenticationFailed,
                    LogClientServices.Success,
                    LogClientServices.Business,
                    true
                    );
                    if (false == logResponse.Success)
                    {
                        _logger.LogError("SendAuthenticationLogMessage failed: " +
                            "{0}", logResponse.Message);
                        //return logResponse;
                    }
                }
                return response;
            }

            // Get UserDetails
            if (true == isAuthNPassed)
            {
                if (tempSession.Clientdetails.ClientId ==
                    DTInternalConstants.DTPortalClientId)
                {
                    userInfo.Suid = userInfo.Id.ToString();
                }

                var userLoginDetail = await _unitOfWork.UserLoginDetail.
                GetUserLoginDetailAsync(userInfo.Suid.ToString());
                if (null != userLoginDetail)
                {
                    _logger.LogError("GetUserPasswordDetailAsync failed," +
                        "not found");

                    if ((userLoginDetail.WrongCodeCount > 0)
                        || (userLoginDetail.WrongPinCount > 0)
                        || (userLoginDetail.DeniedCount > 0))
                    {
                        userLoginDetail.WrongPinCount = 0;
                        userLoginDetail.WrongCodeCount = 0;
                        userLoginDetail.DeniedCount = 0;

                        try
                        {
                            _unitOfWork.UserLoginDetail.Update(userLoginDetail);
                            await _unitOfWork.SaveAsync();
                        }
                        catch
                        {
                            _logger.LogError("UserLoginDetail update failed");
                            var response = new Response();
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                }

            }

            tempSession.AuthNSuccessList.Add(request.authenticationScheme);

            // Modify temporary session
            var (retValue, errorMsg) = await _cacheClient.Add(
                CacheNames.TemporarySession,
                tempSession.TemporarySessionId,
                tempSession);
            if (0 != retValue)
            {
                _logger.LogError("TemporarySession add failed");
                var response = new Response();
                response.Success = false;
                response.Message = Constants.InternalError;
                return response;
            }

            Response finalResponse = new Response();
            if (string.Equals(request.approved, DTInternalConstants.E_False))
            {
                finalResponse.Success = false;
                finalResponse.Message = "Verification Cancelled";
                finalResponse.Result = null;
            }
            else
            {
                finalResponse.Success = true;
                finalResponse.Message = "Verification Success";
                finalResponse.Result = null;
            }

            return finalResponse;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<VerifyUserAuthenticationDataResponse> VerifyUserAuthenticationData(
            VerifyUserAuthenticationDataRequest request)
        {

            _logger.LogDebug("-->VerifyUserAuthNData");
            if (request != null && request.authenticationScheme == "DeviceAuthentication")
            {
                var VerifyUserAuthenticationDataResult = new VerifyUserAuthenticationDataResult()
                {
                    ProvisionUrl = configuration["ProvisionUrl"]
                };
                return new VerifyUserAuthenticationDataResponse(VerifyUserAuthenticationDataResult);
            }
            // Validate input
            if (null == request ||
                string.IsNullOrEmpty(request.authenticationScheme) ||
                string.IsNullOrEmpty(request.authenticationData))
            {
                _logger.LogError(Constants.InvalidArguments);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return new VerifyUserAuthenticationDataResponse(Constants.InvalidArguments);
            }

            Accesstoken accessToken = null;
            try
            {
                accessToken = await _cacheClient.Get<Accesstoken>("AccessToken",
                    request.token);
                if (null == accessToken)
                {
                    _logger.LogError("Access token not received from cache." +
                        "Expired or Invalid access token");
                    return new VerifyUserAuthenticationDataResponse("Internal Error");
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Access Token Record");
                ErrorResponseDTO error = new ErrorResponseDTO();
                error.error = OIDCConstants.InternalError;
                error.error_description = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_ACCESS_TOKEN_GET_FAILED);
                return new VerifyUserAuthenticationDataResponse("Internal Error" + error.error_description);
            }
            if (request.authenticationScheme == AuthNSchemeConstants.FACE)
            {
                _logger.LogInformation("Face Verification Started");
                _logger.LogInformation("suid : " + request.suid);
                var verifyFaceRequest = new VerifyFaceRequest();

                var response = new Response();

                var face = await GetFaceById(request.suid);
                if (face == null)
                {
                    response.Success = false;
                    response.Message = "Failed to verify face";
                    return new VerifyUserAuthenticationDataResponse("Failed to verify face");
                }
                verifyFaceRequest.storedImage = face;

                verifyFaceRequest.capturedImage = request.authenticationData;

                var faceVerificationStartTime = DateTime.Now.ToString("s");

                var verifyFaceResponse = VerifyFace(request.authenticationData);

                if (false == verifyFaceResponse.success)
                {
                    if (verifyFaceResponse.message.Equals
                        (Constants.FaceVerifyFailed))
                    {
                        _logger.LogInformation("Wrong Face");
                        return new VerifyUserAuthenticationDataResponse(Constants.WrongFace);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyFaceResponse.message))
                        {
                            _logger.LogError("PKI service error: {0}",
                                verifyFaceResponse.message);

                            return new VerifyUserAuthenticationDataResponse(verifyFaceResponse.message);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(verifyFaceResponse.message))
                                _logger.LogError("PKI service error: {0}",
                                    verifyFaceResponse.message);
                            var response2 = new Response();
                            response2.Success = false;
                            response2.Message = Constants.InternalError;
                            return new VerifyUserAuthenticationDataResponse(Constants.InternalError);
                        }
                    }
                }
                else
                {
                    var VerifyUserAuthenticationDataResult = new VerifyUserAuthenticationDataResult()
                    {
                        ProvisionUrl = configuration["ProvisionUrl"]
                    };
                    return new VerifyUserAuthenticationDataResponse(VerifyUserAuthenticationDataResult);
                }
            }
            else if (request.authenticationScheme == AuthNSchemeConstants.PUSH_NOTIFICATION)
            {
                TemporarySession tempSession = new TemporarySession();
                tempSession.CoRelationId = Guid.NewGuid().ToString();
                var verifyPinRequest = new VerifyPinRequest();
                verifyPinRequest.subscriberDigitalID = request.suid;
                verifyPinRequest.authenticationPin = request.authenticationData;

                var verifyPinResponse = await VerifyPushNotification(tempSession,
                    verifyPinRequest);

                if (false == verifyPinResponse.success)
                {
                    if (verifyPinResponse.message.Equals
                        (Constants.PinVerifyFailed))
                    {
                        return new VerifyUserAuthenticationDataResponse(Constants.WrongPin);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyPinResponse.message))
                            _logger.LogError("PKI service error: {0}",
                                verifyPinResponse.message);
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return new VerifyUserAuthenticationDataResponse(Constants.InternalError);
                    }
                }
                else
                {
                    var VerifyUserAuthenticationDataResult = new VerifyUserAuthenticationDataResult()
                    {
                        ProvisionUrl = configuration["ProvisionUrl"]
                    };
                    return new VerifyUserAuthenticationDataResponse(VerifyUserAuthenticationDataResult);
                }
            }
            else
            {
                return new VerifyUserAuthenticationDataResponse("Invalid Auth Scheme");
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<VerifyUserAuthDataResponse> IsUserVerifiedQrCode
            (VerifyQrCodeRequest verifyQrCodeRequest)
        {
            var time = DateTime.Now.AddMinutes(2);

            var isVerified = false;

            var response = new APIResponse();

            while (DateTime.Now < time)
            {
                response = await VerifyQr(verifyQrCodeRequest.qrCode);

                if (response == null)
                {
                    return new VerifyUserAuthDataResponse()
                    {
                        Success = false,
                        Message = "Internal Error"
                    };
                }
                if (!response.Success)
                {
                    if (response.Message != "Data not yet posted")
                    {
                        return new VerifyUserAuthDataResponse()
                        {
                            Success = false,
                            Message = response.Message
                        };
                    }
                }
                else
                {
                    isVerified = true;
                    break;
                }
            }
            if (!isVerified)
            {
                return new VerifyUserAuthDataResponse()
                {
                    Success = false,
                    Message = "Request Timed out"
                };
            }
            JObject jsonObject = JObject.Parse(JsonConvert.SerializeObject(response));

            string documentNumber = jsonObject.SelectToken
                ("Result.attributesList.Document.idDocNumber")?.ToString();

            bool verified1 = jsonObject.SelectToken
                ("Result.verifyResult.presentationResult.verified").ToObject<bool>();
            if (!verified1)
            {
                return new VerifyUserAuthDataResponse()
                {
                    Success = false,
                    Message = "Verifiable Presentation Verification Failed"
                };
            }

            bool verified2 = jsonObject.SelectToken
                ("Result.verifyResult.credentialResults[0].verified").ToObject<bool>();

            if (!verified2)
            {
                return new VerifyUserAuthDataResponse()
                {
                    Success = false,
                    Message = "Verifiable Credential Verification Failed"
                };
            }
            bool verified3 = jsonObject.SelectToken
                ("Result.verifyResult.credentialResults[0].statusResult.verified").ToObject<bool>();

            if (!verified3)
            {
                return new VerifyUserAuthDataResponse()
                {
                    Success = false,
                    Message = "Verifiable Credential Revoked"
                };

            }
            VerifyUserAuthDataRequest verifyUserAuthDataRequest = new VerifyUserAuthDataRequest()
            {
                authnToken = verifyQrCodeRequest.tempSession,
                authenticationScheme = AuthNSchemeConstants.WALLET,
                authenticationData = documentNumber,
                documentNumber = documentNumber
            };

            var result = await VerifyUserAuthData(verifyUserAuthDataRequest);

            return result;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<VerifyUserResponse> ChangeAuthScheme(string authScheme, string temporarySession)
        {
            TemporarySession tempSession = null;
            var verifierUrl = string.Empty;
            var errorMessage = string.Empty;
            try
            {
                // Get the temporary session object
                tempSession = await _cacheClient.Get<TemporarySession>
                (CacheNames.TemporarySession,
                    temporarySession);
                if (null == tempSession)
                {
                    _logger.LogError(Constants.TempSessionExpired);
                    return new VerifyUserResponse(Constants.TempSessionExpired);
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Temporary Session Record");
                VerifyUserAuthDataResponse response = new VerifyUserAuthDataResponse();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_TEMP_SESS_GET_FAILED);
                return new VerifyUserResponse(errorMessage);
            }
            var authSchemeList = authScheme.Split(",").ToList();
            tempSession.PrimaryAuthNSchemeList = authSchemeList;
            tempSession.AuthNSuccessList = new List<string>();
            var (retValue1, errorMsg1) = await _cacheClient.Add
                    (CacheNames.TemporarySession, tempSession.TemporarySessionId,
                    tempSession);
            if (0 != retValue1)
            {
                _logger.LogError("TemporarySession add failed");
                errorMessage = _helper.GetRedisErrorMsg(retValue1,
                    ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                return new VerifyUserResponse(errorMessage);
            }

            verifyUserResult result = new verifyUserResult
            {
                AuthenticationSchemes = authSchemeList
            };
            if (authSchemeList.Contains(AuthNSchemeConstants.WALLET))
            {
                var verifierRequest = await GetVerifierUrl();
                if (verifierRequest == null || !verifierRequest.Success)
                {
                    return new VerifyUserResponse(verifierRequest.Message);
                }
                verifierUrl = verifierRequest.Result.ToString();
            }
            if (authSchemeList.Contains(AuthNSchemeConstants.WALLET))
            {
                result.VerifierUrl = verifierUrl;
                result.VerifierCode = verifierUrl.Substring(verifierUrl.LastIndexOf('/') + 1);
            }
            return new VerifyUserResponse(result);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> VerifyAgentConsent(
            VerifyAgentConsentRequest request)
        {
            _logger.LogDebug("-->VerifyUserAuthNData");

            if (null == request ||
                string.IsNullOrEmpty(request.authenticationScheme) ||
                string.IsNullOrEmpty(request.approved))
            {
                _logger.LogError(Constants.InvalidArguments);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }
            var isExists = await _cacheClient.Exists(CacheNames.TemporarySession,
                request.authnToken);
            if (CacheCodes.KeyExist != isExists.retValue)
            {
                _logger.LogError(Constants.TempSessionExpired);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.TempSessionExpired;
                return response;
            }

            var tempSession = await _cacheClient.Get<TemporarySession>
                (CacheNames.TemporarySession,
                request.authnToken);
            if (null == tempSession)
            {
                _logger.LogError(Constants.TempSessionExpired);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.TempSessionExpired;
                return response;
            }
            if (request.authenticationScheme == "DeviceAuthentication")
            {
                if (request.approved == "true")
                {
                    tempSession.AdditionalValue = DTInternalConstants.S_True;
                }
                else
                {
                    tempSession.AdditionalValue = "Agent Denied Authentication";
                }

                var (retValue2, errorMsg2) = await _cacheClient.Add
                    (CacheNames.TemporarySession, tempSession.TemporarySessionId,
                    tempSession);
                if (0 != retValue2)
                {
                    _logger.LogError("TemporarySession add failed");
                    var cacheResponse = new Response();
                    cacheResponse.Success = false;
                    cacheResponse.Message = Constants.InternalError;
                    return cacheResponse;
                }
                return new Response()
                {
                    Success = true,
                    Message = "Authentication Success",
                    Result = null
                };
            }
            else
            {
                return new Response()
                {
                    Success = false,
                    Message = "Invalid Authentication Scheme",
                    Result = null
                };
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> VerifyUserAuthentication(
            VerifyUserAuthenticationRequest request)
        {
            _logger.LogDebug("-->VerifyUserAuthNData");
            // Validate input
            if (null == request ||
                string.IsNullOrEmpty(request.authenticationScheme) ||
                string.IsNullOrEmpty(request.authenticationData)
                )
            {
                _logger.LogError(Constants.InvalidArguments);
                Response response = new Response();
                response.Success = false;
                response.Message = Constants.InvalidArguments;
                return response;
            }
            if (request.authenticationScheme == AuthNSchemeConstants.FACE)
            {
                var verifyFaceResponse = VerifyFace(request.authenticationData);
                if (false == verifyFaceResponse.success)
                {
                    if (verifyFaceResponse.message.Equals
                        (Constants.FaceVerifyFailed))
                    {
                        _logger.LogInformation("Wrong Pin");
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.WrongPin;
                        return response;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyFaceResponse.message))
                        {
                            _logger.LogError("PKI service error: {0}",
                                verifyFaceResponse.message);
                            var response = new Response();
                            response.Success = false;
                            response.Message = verifyFaceResponse.message;
                            return response;
                        }
                        else
                        {
                            _logger.LogError("PKI service error: {0}",
                                    Constants.InternalError);
                            var response = new Response();
                            response.Success = false;
                            response.Message = "Internal Error";
                            return response;
                        }
                    }
                }
                else
                {
                    var response = new Response();
                    response.Success = true;
                    response.Message = DTInternalConstants.AuthNDone;
                    return response;
                }
            }
            else if (request.authenticationScheme == AuthNSchemeConstants.PIN)
            {
                TemporarySession tempSession = new TemporarySession();
                tempSession.CoRelationId = Guid.NewGuid().ToString();
                var verifyPinRequest = new VerifyPinRequest();
                verifyPinRequest.subscriberDigitalID = request.Suid;
                verifyPinRequest.authenticationPin = request.authenticationData;

                var verifyPinResponse = await VerifyPushNotification(tempSession,
                    verifyPinRequest);

                if (false == verifyPinResponse.success)
                {
                    if (verifyPinResponse.message.Equals
                        (Constants.PinVerifyFailed))
                    {
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.WrongPin;
                        return response;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(verifyPinResponse.message))
                            _logger.LogError("PKI service error: {0}",
                                verifyPinResponse.message);
                        var response = new Response();
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }
                }
                else
                {
                    var response = new Response();
                    response.Success = true;
                    response.Message = DTInternalConstants.AuthNDone;
                    return response;
                }
            }
            else
            {
                var response = new Response();
                response.Success = false;
                response.Message = "Invalid Auth Scheme";
                return response;
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<IcpJourneyDetailsResponse> GetJourneyDetailsAsync(string journeyToken)
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{configuration["ICP:BaseUrl"]}/otk-service/v2/journey-details/{journeyToken}"
            );

            request.Headers.Add(
                "x-transaction-key",
                configuration["ICP:TransactionKey"]
            );

            using var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<IcpJourneyDetailsResponse>(json);
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private bool IsIcpFaceMatchSuccessful(IcpJourneyDetailsResponse journey)
        {
            return journey?.result?.data?.selfieAnalysis?.faceMatch != null
                && journey.result.data.selfieAnalysis.faceMatch.success
                && journey.result.data.selfieAnalysis.faceMatch.matched;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<ServiceResult> ICPVerify(ICPAuthRequest request)
        {
            try
            {

                TemporarySession tempSession = null;

                try
                {
                    // Get the temporary session object
                    tempSession = await _cacheClient.Get<TemporarySession>
                        (CacheNames.TemporarySession,
                        request.AuthnToken);
                    if (null == tempSession)
                    {
                        return new ServiceResult(false, "Temporary Session not found");
                    }
                }
                catch (CacheException)
                {
                    return new ServiceResult(false, "Failed to get Temporary Session");
                }


                SubscriberView subscriber = null;
                //if (request.DocumentType.ToLower() == "passport")
                //{
                //    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByPassportNumber(request.DocumentNumber);
                //}
                //else if (request.DocumentType.ToLower() == "emiratesid")
                //{
                //    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByEmiratesId(request.DocumentNumber);
                //}

                var suid = tempSession?.UserId;

                if (suid == null)
                {
                    return new ServiceResult(false, "Subscriber details not found");
                }

                subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsBySuid(suid);

                var userLookup = new UserLookupItem()
                {
                    DocumentId = subscriber.IdDocNumber,
                    DocumentType = subscriber.IdDocType,
                    NationalId = subscriber.NationalId,
                    Nationality = subscriber.Country
                };

                var icpResponse = await CreateJourney_FromConfig(userLookup);

                if (icpResponse.Error != null)
                {
                    _logger.LogError(
                        "ICP Error | Code:{0} | Message:{1}",
                        icpResponse.Error.code,
                        icpResponse.Error.message);

                    return new ServiceResult(false, icpResponse.Error.message);
                }

                var journeyToken = icpResponse.Success.result.journeyToken;

                tempSession.JourneyToken = journeyToken;


                var task = await _cacheClient.Add(CacheNames.TemporarySession,
                tempSession.TemporarySessionId, tempSession);
                if (DTInternalConstants.S_OK != task.retValue)
                {
                    _logger.LogError("_cacheClient.Add failed");
                    var errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                         ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                    return new ServiceResult(false, errorMessage);
                }

                return new ServiceResult(true, "Jorney Token Created", journeyToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<ServiceResult> ICPAppVerify(ICPAuthRequest request)
        {
            try
            {

                MobileAuthTemporarySession tempSession = null;

                try
                {
                    // Get the temporary session object
                    tempSession = await _cacheClient.Get<MobileAuthTemporarySession>
                        (CacheNames.MobileAuthTemporarySession,
                        request.AuthnToken);
                    if (null == tempSession)
                    {
                        return new ServiceResult(false, "Temporary Session not found");
                    }
                }
                catch (CacheException)
                {
                    return new ServiceResult(false, "Failed to get Temporary Session");
                }


                SubscriberView subscriber = null;
                if (request.DocumentType.ToLower() == "passport")
                {
                    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByPassportNumber(request.DocumentNumber);
                }
                else if (request.DocumentType.ToLower() == "emiratesid")
                {
                    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByEmiratesId(request.DocumentNumber);
                }

                var userLookup = new UserLookupItem()
                {
                    DocumentId = subscriber.IdDocNumber,
                    DocumentType = subscriber.IdDocType,
                    NationalId = subscriber.NationalId,
                    Nationality = subscriber.Country
                };

                var icpResponse = await CreateJourney_FromConfig(userLookup);

                if (icpResponse.Error != null)
                {
                    _logger.LogError(
                        "ICP Error | Code:{0} | Message:{1}",
                        icpResponse.Error.code,
                        icpResponse.Error.message);

                    return new ServiceResult(false, icpResponse.Error.message);
                }

                var journeyToken = icpResponse.Success.result.journeyToken;

                tempSession.JourneyToken = journeyToken;


                var task = await _cacheClient.Add(CacheNames.MobileAuthTemporarySession,
                tempSession.TemporarySessionId, tempSession);
                if (DTInternalConstants.S_OK != task.retValue)
                {
                    _logger.LogError("_cacheClient.Add failed");
                    var errorMessage = _helper.GetRedisErrorMsg(task.retValue,
                         ErrorCodes.REDIS_TEMP_SESS_ADD_FAILED);
                    return new ServiceResult(false, errorMessage);
                }

                return new ServiceResult(true, "Jorney Token Created", journeyToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<ServiceResult> ICPLoginVerify(ICPAuthRequest request)
        {
            try
            {

                SubscriberView subscriber = null;

                if (!string.IsNullOrEmpty(request.Suid))
                {
                    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsBySuid(request.Suid);
                    if (subscriber == null)
                    {
                        return new ServiceResult(false, "Subscriber details not found");
                    }
                    subscriber.IdDocType = "5";
                    subscriber.IdDocNumber = subscriber.UaeKycId;
                }

                else if (request.DocumentType.ToLower() == "passport")
                {
                    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByPassportNumber(request.DocumentNumber);
                }
                else if (request.DocumentType.ToLower() == "emiratesid")
                {
                    subscriber = await _unitOfWork.Subscriber.GetSubscriberDetailsByEmiratesId(request.DocumentNumber);
                }
                else
                {
                    return new ServiceResult(false, "Inavlid Document Type");
                }


                if (subscriber == null)
                {
                    return new ServiceResult(false, "Subscriber details not found");
                }

                var userLookup = new UserLookupItem()
                {
                    DocumentId = subscriber.IdDocNumber,
                    DocumentType = subscriber.IdDocType,
                    NationalId = subscriber.NationalId,
                    Nationality = subscriber.Country
                };

                var icpResponse = await CreateJourney_FromConfig(userLookup);

                if (icpResponse.Error != null)
                {
                    _logger.LogError(
                        "ICP Error | Code:{0} | Message:{1}",
                        icpResponse.Error.code,
                        icpResponse.Error.message);

                    return new ServiceResult(false, icpResponse.Error.message);
                }

                var journeyToken = icpResponse.Success.result.journeyToken;

                return new ServiceResult(true, "Jorney Token Created", journeyToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ServiceResult(false, ex.Message);
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> VerifyUAEKycFace(string journeyToken, string status, string message)
        {
            var journeyDetails = await GetJourneyDetailsAsync(journeyToken);
            if (journeyDetails == null)
            {
                _logger.LogError("Failed to retrieve ICP journey details for token: {0}", journeyToken);
                return new Response
                {
                    Success = false,
                    Message = "Failed to retrieve journey details"
                };
            }

            var isFaceMatchSuccessful = IsIcpFaceMatchSuccessful(journeyDetails);

            if (isFaceMatchSuccessful)
            {
                return new Response
                {
                    Success = true,
                    Message = DTInternalConstants.AuthNDone
                };
            }
            else
            {
                return new Response
                {
                    Success = false,
                    Message = Constants.FaceVerifyFailed
                };
            }

            //if (!string.IsNullOrEmpty(status) &&
            //    status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            //{
            //    return new Response
            //    {
            //        Success = true,
            //        Message = DTInternalConstants.AuthNDone
            //    };
            //}

            //// Real verification failures
            //if (status != null &&
            //    (status.Equals("faceverificationfailed", StringComparison.OrdinalIgnoreCase)
            //    || status.Equals("documentverificationfailed", StringComparison.OrdinalIgnoreCase)))
            //{
            //    return new Response
            //    {
            //        Success = false,
            //        Message = Constants.FaceVerifyFailed
            //    };
            //}

            //_logger.LogError("UAE KYC service error: {0}", message);
            //// Real system errors
            //return new Response
            //{
            //    Success = false,
            //    Message = Constants.FaceVerifyFailed
            //};
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> VerifyUaeKycAuthentication(
    string authenticationData)
        {
            try
            {

                int separator = authenticationData.IndexOf(':');
                if (-1 == separator)
                {
                    _logger.LogError("credentials not found");
                    return new Response()
                    {
                        Success = false,
                        Message = "Internal Error"
                    };
                }

                string status = authenticationData.Substring(0, separator);

                string journeyToken = authenticationData.Substring(separator + 1);

                bool isSuccess = status.Equals("success", StringComparison.OrdinalIgnoreCase);

                if (!isSuccess)
                {
                    _logger.LogError("UAE KYC authentication failed with status: {0}", status);
                    return new Response()
                    {
                        Success = false,
                        Message = Constants.FaceVerifyFailed
                    };
                }

                if (configuration.GetValue<bool>("VerifyJourneyToken") == true)
                {
                    var response = await VerifyUAEKycFace(journeyToken, status, "");

                    return response;
                }

                return new Response()
                {
                    Success = true,
                    Message = DTInternalConstants.AuthNDone
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Response()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Response> LogAttributes
            (string GlobalSessionId, LogAttributesRequest request)
        {
            GlobalSession globalSession = null;
            string errorMessage = string.Empty;

            try
            {
                // Get GlobalSession
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession,
                    GlobalSessionId);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    Response response = new Response();
                    response.Success = false;
                    response.Message = Constants.SessionMismatch;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                var response = new Response();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            List<string> AcceptedAttributes = new List<string>();

            foreach (var scope in request.ScopeDetail)
            {
                foreach (var attribute in scope.Attributes)
                {
                    if (!AcceptedAttributes.Contains(attribute.Name))
                    {
                        AcceptedAttributes.Add(attribute.Name);
                    }
                }
            }

            ClientAttributes clientAttributes = new ClientAttributes()
            {
                ClientId = request.clientId,
                Attributes = AcceptedAttributes
            };

            if (globalSession.AcceptedAttributes == null)
            {
                globalSession.AcceptedAttributes = new List<ClientAttributes>();
            }

            globalSession.AcceptedAttributes.Add(clientAttributes);

            var cacheAdd = await _cacheClient.Add(CacheNames.GlobalSession,
                    GlobalSessionId, globalSession);
            if (0 != cacheAdd.retValue)
            {
                _logger.LogError("GlobalSession Add failed");
                var response = new Response();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(cacheAdd.retValue,
                    ErrorCodes.REDIS_GLOBAL_SESS_ADD_FAILED);
                response.Message = errorMessage;
                return response;
            }

            string logMessage = "Consent Approved";
            string logServiceName = LogClientServices.Other;
            string logServiceStatus = LogClientServices.Success;
            bool centralLog = false;

            TemporarySession tempSession = new TemporarySession()
            {
                Clientdetails = new Clientdetails()
                {
                    ClientId = request.clientId,
                },
                CoRelationId = globalSession.CoRelationId,
                AuthNStartTime = DateTime.Now.ToString(),
            };

            if(globalSession.LoggedClients != null &&
                !globalSession.LoggedClients.Contains(request.clientId))
            {
                globalSession.LoggedClients.Add(request.clientId);
                var authLogMessage =await _LogClient.SendAuthenticationLogMessage(
                tempSession,
                globalSession.UserId,
                LogClientServices.AuthenticationSuccess,
                "Authentication Successfull",
                logServiceStatus,
                LogClientServices.Business,
                true,
                JsonConvert.SerializeObject(request.ScopeDetail)
            );

                if (false == authLogMessage.Success)
                {
                    _logger.LogError("SendAuthenticationLogMessage failed: " +
                        "{0}", authLogMessage.Message);
                }
            }

            var logResponse =await _LogClient.SendAuthenticationLogMessage(
                tempSession,
                globalSession.UserId,
                logServiceName,
                logMessage,
                logServiceStatus,
                LogClientServices.Business,
                centralLog,
                JsonConvert.SerializeObject(request.ScopeDetail)
            );

            if (false == logResponse.Success)
            {
                _logger.LogError("SendAuthenticationLogMessage failed: " +
                    "{0}", logResponse.Message);
            }


            return new Response()
            {
                Success = true,
                Message = "Attributes added to Global Session successfully"
            };
        }

        public async Task<VerifyConsentResponse> IsUserGivenConsent
            (string GlobalSessionId, string ClientId)
        {
            GlobalSession globalSession = null;
            string errorMessage = string.Empty;

            try
            {
                // Get GlobalSession
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession,
                    GlobalSessionId);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    VerifyConsentResponse response = new VerifyConsentResponse();
                    response.Success = false;
                    response.Message = Constants.SessionMismatch;
                    return response;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                var response = new VerifyConsentResponse();
                response.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                response.Message = errorMessage;
                return response;
            }

            var clientAttributes = globalSession.AcceptedAttributes;

            if (clientAttributes == null)
            {
                return new VerifyConsentResponse()
                {
                    Success = true,
                    Message = "Consent Not Given",
                    ConsentGiven = false,
                };
            }

            bool exists = clientAttributes.Any(c => c.ClientId == ClientId);

            return new VerifyConsentResponse()
            {
                Success = true,
                Message = "Consent Given",
                ConsentGiven = exists,
            };
        }

        public async Task<VerifyConsentResponse> SendConsentDeniedLogMessage
            (string GlobalSessionId,string ClientId)
        {
            GlobalSession globalSession = null;
            string errorMessage = string.Empty;

            try
            {
                // Get GlobalSession
                globalSession = await _cacheClient.Get<GlobalSession>
                    (CacheNames.GlobalSession,
                    GlobalSessionId);
                if (null == globalSession)
                {
                    _logger.LogError(Constants.SessionMismatch);
                    VerifyConsentResponse response2 = new VerifyConsentResponse();
                    response2.Success = false;
                    response2.Message = Constants.SessionMismatch;
                    return response2;
                }
            }
            catch (CacheException ex)
            {
                _logger.LogError("Failed to get Global Session Record");
                var response1 = new VerifyConsentResponse();
                response1.Success = false;
                errorMessage = _helper.GetRedisErrorMsg(
                    ex.ErrorCode, ErrorCodes.REDIS_GLOBAL_SESS_GET_FAILED);
                response1.Message = errorMessage;
                return response1;
            }

            string logMessage = "Consent Denied";
            string logServiceName = LogClientServices.Other;
            string logServiceStatus = LogClientServices.Success;
            bool centralLog = false;

            TemporarySession tempSession = new TemporarySession()
            {
                Clientdetails = new Clientdetails()
                {
                    ClientId = ClientId,
                },
                CoRelationId = globalSession.CoRelationId,
                AuthNStartTime = DateTime.Now.ToString(),
            };

            var logResponse =await _LogClient.SendAuthenticationLogMessage(
                tempSession,
                globalSession.UserId,
                logServiceName,
                logMessage,
                logServiceStatus,
                LogClientServices.Business,
                centralLog
            );

            if (false == logResponse.Success)
            {
                _logger.LogError("SendAuthenticationLogMessage failed: " +
                    "{0}", logResponse.Message);
            }

            var response = new VerifyConsentResponse();
            response.Success = true;
            response.Message = "Success";
            return response;
        }
    }
}
