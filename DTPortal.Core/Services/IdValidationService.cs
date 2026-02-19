using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DTPortal.Core.Services
{
    public class IdValidationService : IIdValidationService
    {
        private readonly ILogger<IdValidationService> _logger;
        private readonly ECDsaSecurityKey _securityKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public IdValidationService(IConfiguration configuration,
            ILogger<IdValidationService> logger,
            IHttpClientFactory httpClientFactory,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
            string publicKeyPem = configuration["PublicKeyPem"];

            if (_configuration.GetValue<bool>("EncryptionEnabled"))
            {
                publicKeyPem = PKIMethods.Instance.
                                PKIDecryptSecureWireData(publicKeyPem);
            }

            try
            {
                byte[] keyBytes = ExtractDerBytesFromPem(publicKeyPem, "PUBLIC KEY");

                var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

                _securityKey = new ECDsaSecurityKey(ecdsa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load or parse the public key.");
                throw;
            }
        }

        public ServiceResult ValidateSignedDataAsync(string signedData, string serviceName)
        {
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey
                };
                var handler = new JwtSecurityTokenHandler
                {
                    MaximumTokenSizeInBytes = 1024 * 1024
                };
                handler.ValidateToken(signedData, validationParameters, out _);

                var payLoad = handler.ReadJwtToken(signedData).Payload;

                Dictionary<string, object> data = new Dictionary<string, object>();

                foreach (var claim in payLoad)
                {
                    data[claim.Key] = claim.Value;
                }
                string[] parts = signedData.Split('.');

                string plainText = DecodeBase64Payload(parts[1]);

                //var options = new JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true
                //};

                //Dictionary<string, object> payloadDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(plainText, options);



                if(serviceName == "CARD_STATUS_WITH_OCR" || serviceName == "BATCH_CARD_STATUS" || serviceName == "CARD_STATUS_WITH_MANUAL" || serviceName == "PASSPORT_STATUS_WITH_OCR" || serviceName == "PASSPORT_STATUS_WITH_MANUAL_ENTRY")
                {
                    var callStack = JsonConvert.DeserializeObject<IdValidationCallStackResponseDTO>(plainText);
                    string genderCode = callStack?.idCardData?.nonModifiableData?.gender ?? string.Empty;
                    string gender = genderCode switch
                    {
                        "M" => "MALE",
                        "F" => "FEMALE",
                        _ => string.Empty
                    };

                    var idValidationReport = new VerifiedIdValidationResponseDTO
                    {
                        name = callStack?.name ?? string.Empty,
                        idNumber = callStack?.idNumber ?? string.Empty,
                        nationality = callStack?.nationality ?? string.Empty,
                        issueDate = callStack?.issueDate ?? string.Empty,
                        photo = callStack?.idCardData?.photography?.cardHolderPhoto ?? string.Empty,
                        expiryDate = callStack?.expiryDate ?? string.Empty,
                        gender = gender,
                        dob = callStack?.dateOfBirth ?? string.Empty,
                        documentStatus = callStack?.documentStatus ?? string.Empty,
                    };

                    return new ServiceResult(true, "Token is valid", idValidationReport);
                }
                //else if(serviceName == "PASSPORT_STATUS_WITH_OCR" || serviceName == "PASSPORT_STATUS_WITH_MANUAL_ENTRY")
                //{
                //    var callStack = JsonConvert.DeserializeObject<EmiratesIdResponse>(plainText);

                //    // Get first binaryBase64String
                //    string base64 = string.Empty;
                //    if (callStack?.Data?.BinaryObjects != null && callStack.Data.BinaryObjects.Count > 0)
                //    {
                //        base64 = callStack.Data.BinaryObjects[0].BinaryBase64String;
                //    }

                //    var idValidationReport = new VerifiedIdValidationResponseDTO
                //    {
                //        name = callStack?.Data?.FullNameEn ?? string.Empty,
                //        idNumber = callStack?.Data?.EmiratesId ?? string.Empty,
                //        issueDate = string.Empty,
                //        gender = callStack?.Data?.Gender?.DescriptionEn ?? string.Empty,
                //        dob = callStack?.Data?.DateOfBirth ?? string.Empty,
                //        nationality = callStack?.Data?.CurrentNationality?.DescriptionEn ?? string.Empty,
                //        photo = base64 ?? string.Empty,
                //        expiryDate = string.Empty
                //    };

                //    return new ServiceResult(true, "Token is valid", idValidationReport);
                //}
                else
                {
                    var callStack = JsonConvert.DeserializeObject<EmiratesIdResponse>(plainText);

                    // Get first binaryBase64String
                    string base64 = string.Empty;
                    if (callStack?.Data?.BinaryObjects != null && callStack.Data.BinaryObjects.Count > 0)
                    {
                        base64 = callStack.Data.BinaryObjects[0].BinaryBase64String;
                    }

                    var idValidationReport = new VerifiedIdValidationResponseDTO
                    {
                        name = callStack?.Name ?? string.Empty,
                        idNumber = callStack?.IdNumber ?? string.Empty,
                        gender = callStack?.Data?.Gender?.DescriptionEn ?? string.Empty,
                        dob = callStack?.DateOfBirth ?? string.Empty,
                        nationality = callStack?.Nationality ?? string.Empty,
                        photo = base64 ?? string.Empty,
                        expiryDate = callStack?.ExpiryDate ?? string.Empty,
                        issueDate = callStack?.Data?.ActivePassport?.IssueDate ?? string.Empty,
                        documentStatus = callStack?.DocumentStatus ?? string.Empty
                    };

                    return new ServiceResult(true, "Token is valid", idValidationReport);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT validation failed.");
                return new ServiceResult(false, $"Token validation failed: {ex.Message}");
            }
        }

        public async Task<IList<IdValidationSummaryResponse>> GetAllServiceProvidersDetails()
        {
            HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:KycLogServiceBaseAddress"]);
            client.Timeout = TimeSpan.FromSeconds(30);


            List<IdValidationSummaryResponse> logReportsDTO = new List<IdValidationSummaryResponse>();

            var orgKycDictionary = await GetOrgKycMethodsDict();

            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/records/org-summary");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var resultToken = JToken.FromObject(apiResponse.Result);

                        //logReportsDTO = resultToken.ToObject<List<IdValidationSummaryResponse>>();

                        logReportsDTO = resultToken
                                            .ToObject<List<IdValidationSummaryResponse>>()
                                            ?.Where(x => !string.IsNullOrWhiteSpace(x.OrgId))
                                            .ToList();

                        foreach (var log in logReportsDTO)
                        {
                            if(log.OrgId != null)
                            {
                                var orgId = log.OrgId.ToString();

                                if (orgKycDictionary.TryGetValue(orgId, out var kycMethods))
                                {
                                    log.KycMethods = kycMethods;
                                }
                                else
                                {
                                    log.KycMethods = new List<string>();
                                }
                            }
                            string lastKycSuccessfulTimestampFormatted = "";
                            if (!string.IsNullOrWhiteSpace(log.LastKycSuccessfulTimestamp))
                            {
                                string cleanedTimestamp = log.LastKycSuccessfulTimestamp.Replace("T", " ");

                                string[] formats = {
                                    "MM/dd/yyyy HH:mm:ss.fff", // with milliseconds
                                    "MM/dd/yyyy HH:mm:ss"      // without milliseconds
                                };

                                if (DateTime.TryParseExact(
                                    cleanedTimestamp,
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AssumeLocal,
                                    out DateTime parsedDateTime
                                ))
                                {
                                    lastKycSuccessfulTimestampFormatted = parsedDateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }

                            log.LastKycSucessfulTimestampDate = lastKycSuccessfulTimestampFormatted;

                            //var timestamp = log.LastKycSuccessfulTimestamp;

                            //if (!string.IsNullOrWhiteSpace(timestamp))
                            //{
                            //    var datePart = timestamp.Split(' ')[0];
                            //    if (DateTime.TryParseExact(datePart, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            //    {
                            //        log.LastKycSucessfulTimestampDate = parsedDate.ToString("yyyy-MM-dd");
                            //    }
                            //    else
                            //    {
                            //        log.LastKycSucessfulTimestampDate = "";
                            //    }
                            //}
                            //else
                            //{
                            //    log.LastKycSucessfulTimestampDate = "";
                            //}
                        }


                        return logReportsDTO;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);

                        return logReportsDTO;
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");

                    return logReportsDTO;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return logReportsDTO;

        }


        public async Task<Dictionary<string, List<string>>> GetOrgKycMethodsDict()
        {
            var orgDetails = await _unitOfWork.OrganizationKycMethods.GetAllAsync();

            var dict = new Dictionary<string, List<string>>();

            foreach (var org in orgDetails)
            {
                if (!dict.ContainsKey(org.OrganizationId))
                {
                    try
                    {
                        var parsedList = JsonConvert.DeserializeObject<List<string>>(org.KycMethods)
                                         ?? new List<string>();

                        dict.Add(org.OrganizationId, parsedList);
                    }
                    catch
                    {
                        dict.Add(org.OrganizationId, new List<string>());
                    }
                }
            }


            return dict;
        }


        public async Task<PaginatedList<IdValidationResponseDTO>> GetIdValidationLogReportAsync(
    string identifier = "",
    string logMessageType = "",
    string orgId = "",
    List<string> kycMethods = null,
    string fromDate = "",
    string toDate = "",
    int page = 1,
    int perPage = 10)
        {
            HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:KycLogServiceBaseAddress"]);
            client.Timeout = TimeSpan.FromSeconds(200);

            IList<IdValidationResponseDTO> idValidationReports = new List<IdValidationResponseDTO>();

            try
            {
                string apiUrl = "api/records/by-identifier";

                var requestBody = new
                {
                    identifier,
                    logMessageType,
                    orgId,
                    serviceNames = kycMethods ?? new List<string>(),
                    fromDate,
                    toDate,
                    page,
                    perPage
                };

                string json = JsonConvert.SerializeObject(
                    requestBody,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<KycLogResponseDTO>(await response.Content.ReadAsStringAsync());

                    if (apiResponse != null && apiResponse.Success)
                    {
                        var resultToken = JToken.FromObject(apiResponse.Result);
                        List<LogReportDTO> logReportsDTO = resultToken.ToObject<List<LogReportDTO>>();

                        foreach (var log in logReportsDTO)
                        {
                            string lastKycSuccessfulTimestampFormatted = "";
                            if (!string.IsNullOrWhiteSpace(log.EndTime))
                            {
                                string cleanedTimestamp = log.EndTime.Replace("T", " ");

                                string[] formats = {
                                    "yyyy-MM-dd HH:mm:ss.fff", // with milliseconds
                                    "yyyy-MM-dd HH:mm:ss"      // without milliseconds
                                };

                                if (DateTime.TryParseExact(
                                    cleanedTimestamp,
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AssumeLocal,
                                    out DateTime parsedDateTime
                                ))
                                {
                                    lastKycSuccessfulTimestampFormatted = parsedDateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }

                            try
                            {
                                if (log.ServiceName == "CARD_STATUS_WITH_OCR" || log.ServiceName == "BATCH_CARD_STATUS" || log.ServiceName == "CARD_STATUS_WITH_MANUAL" || log.ServiceName == "PASSPORT_STATUS_WITH_OCR" || log.ServiceName == "PASSPORT_STATUS_WITH_MANUAL_ENTRY")
                                {
                                    CallStackDTO callStack = null;
                                    CallStackRequestDTO callStackRequestDTO = null;
                                    IdValidationCallStackResponseDTO callStackResponseDTO = null;

                                    if (!string.IsNullOrWhiteSpace(log.CallStack))
                                    {
                                        try
                                        {
                                            callStack = JsonConvert.DeserializeObject<CallStackDTO>(log.CallStack);
                                            if (callStack != null && callStack.response != null && callStack.request != null)
                                            {
                                                callStackRequestDTO = callStack.request;
                                                callStackResponseDTO = JsonConvert.DeserializeObject<IdValidationCallStackResponseDTO>(callStack.response.ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, $"Failed to deserialize IdValidationCallStackDTO. Identifier: {log.Identifier}");
                                        }
                                    }

                                    idValidationReports.Add(new IdValidationResponseDTO
                                    {
                                        identifier = log.Identifier ?? string.Empty,
                                        deviceId = log.DeviceId ?? string.Empty,
                                        name = callStackResponseDTO?.name ?? string.Empty,
                                        identifierName = callStackResponseDTO?.name ?? string.Empty,
                                        orgName = log.ServiceProviderName ?? string.Empty,
                                        applicationName = log.ServiceProviderAppName ?? string.Empty,
                                        date = log.EndTime?.Contains("T") == true ? log.EndTime.Split('T')[0] : string.Empty,
                                        time = log.EndTime?.Contains("T") == true ? log.EndTime.Split('T')[1] : string.Empty,
                                        status = log.LogMessageType ?? string.Empty,
                                        kycMethod = log.ServiceName ?? string.Empty,
                                        signedResponse = callStackResponseDTO?.signedResponse?.Trim() ?? string.Empty,
                                        nationality = callStackResponseDTO?.nationality ?? string.Empty,
                                        photo = callStackResponseDTO?.idCardData?.photography?.cardHolderPhoto ?? string.Empty,
                                        expiryDate = callStackResponseDTO?.expiryDate ?? string.Empty,
                                        validationDateTime = lastKycSuccessfulTimestampFormatted,
                                        issueDate = callStackResponseDTO?.issueDate ?? string.Empty,
                                        documentStatus = callStackResponseDTO?.documentStatus ?? string.Empty,
                                        request = callStackRequestDTO
                                    });                                    
                                }
                                else
                                {
                                    CallStackDTO callStack = null;
                                    CallStackRequestDTO callStackRequestDTO = null;
                                    EmiratesIdResponse callStackResponseDTO = null;

                                    if (!string.IsNullOrWhiteSpace(log.CallStack))
                                    {
                                        try
                                        {
                                            callStack = JsonConvert.DeserializeObject<CallStackDTO>(log.CallStack);
                                            if (callStack != null && callStack.response != null && callStack.request != null)
                                            {
                                                callStackRequestDTO = callStack.request;
                                                callStackResponseDTO = JsonConvert.DeserializeObject<EmiratesIdResponse>(callStack.response.ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogWarning(ex, $"Failed to deserialize IdValidationCallStackDTO. Identifier: {log.Identifier}");
                                        }
                                    }

                                    string base64 = string.Empty;
                                    if (callStackResponseDTO?.Data?.BinaryObjects?.Count > 0)
                                    {
                                        base64 = callStackResponseDTO.Data.BinaryObjects[0]?.BinaryBase64String ?? string.Empty;
                                    }

                                    idValidationReports.Add(new IdValidationResponseDTO
                                    {
                                        identifier = log.Identifier ?? string.Empty,
                                        deviceId = log.DeviceId ?? string.Empty,
                                        name = callStackResponseDTO?.Name ?? string.Empty,
                                        identifierName = callStackResponseDTO?.Name ?? string.Empty,
                                        orgName = log.ServiceProviderName ?? string.Empty,
                                        applicationName = log.ServiceProviderAppName ?? string.Empty,
                                        date = log.EndTime?.Contains("T") == true ? log.EndTime.Split('T')[0] : string.Empty,
                                        time = log.EndTime?.Contains("T") == true ? log.EndTime.Split('T')[1] : string.Empty,
                                        status = log.LogMessageType ?? string.Empty,
                                        kycMethod = log.ServiceName ?? string.Empty,
                                        signedResponse = callStackResponseDTO?.SignedResponse?.Trim() ?? string.Empty,
                                        nationality = callStackResponseDTO?.Nationality ?? string.Empty,
                                        photo = base64,
                                        validationDateTime = lastKycSuccessfulTimestampFormatted,
                                        expiryDate = callStackResponseDTO?.ExpiryDate ?? string.Empty,
                                        request = callStackRequestDTO,
                                        issueDate = callStackResponseDTO?.Data?.ActivePassport?.IssueDate ?? string.Empty,
                                        documentStatus = callStackResponseDTO?.DocumentStatus,
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to process log for identifier: {log.Identifier}");
                            }
                        }

                        return new PaginatedList<IdValidationResponseDTO>(
                            idValidationReports,
                            apiResponse.CurrentPage,
                            apiResponse.PerPage,
                            apiResponse.TotalPages,
                            apiResponse.TotalCount);
                    }
                    else
                    {
                        _logger.LogError(apiResponse?.Message ?? "Unknown error occurred.");
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new PaginatedList<IdValidationResponseDTO>(new List<IdValidationResponseDTO>(), page, perPage, 0, 0);
        }




        public async Task<KycSummaryDTO> GetIdValidationSummaryAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:KycLogServiceBaseAddress"]);
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/records/kyc-overall-stats");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var resultToken = JToken.FromObject(apiResponse.Result);
                        return resultToken.ToObject<KycSummaryDTO>();
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
            return new KycSummaryDTO();
        }


        public async Task<OrgKycSummaryDTO> GetOrganizationIdValidationSummaryAsyncOld(string orgId)
        {
            HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:KycLogServiceBaseAddress"]);
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/records/org-summary");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var resultToken = JToken.FromObject(apiResponse.Result);
                        var summaryList = resultToken.ToObject<List<OrgKycSummaryDTO>>();

                        var orgSummary = summaryList.FirstOrDefault(x => x.orgId == orgId);
                        if(orgSummary == null)
                        {
                            return null;
                        }

                        var orgKycDictionary = await GetOrgKycMethodsDict();

                        if (orgKycDictionary.TryGetValue(orgId, out var kycMethods))
                        {
                            orgSummary.KycMethods = kycMethods;
                        }
                        else
                        {
                            orgSummary.KycMethods = new List<string>();
                        }

                        return orgSummary;
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
            return new OrgKycSummaryDTO();
        }

        public async Task<OrgKycSummaryDTO> GetOrganizationIdValidationSummaryAsync(string orgId)
        {
            HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
            client.BaseAddress = new Uri(_configuration["APIServiceLocations:KycLogServiceBaseAddress"]);
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/kyc-summary/organization/{orgId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var resultToken = JToken.FromObject(apiResponse.Result);
                        var orgSummary = resultToken.ToObject<OrgKycSummaryDTO>();

                        orgSummary.totalKycDone = orgSummary.totalKycCountSuccessful + orgSummary.totalKycCountFailed;
                        orgSummary.totalKycDoneCurrentMonth = orgSummary.totalKycCountSuccessfulCurrentMonth + orgSummary.totalKycCountFailedCurrentMonth;

                        var orgKycDictionary = await GetOrgKycMethodsDict();

                        if (orgKycDictionary.TryGetValue(orgId, out var kycMethods))
                        {
                            orgSummary.KycMethods = kycMethods;
                        }
                        else
                        {
                            orgSummary.KycMethods = new List<string>();
                        }

                        return orgSummary;
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
            return new OrgKycSummaryDTO();
        }

        private static byte[] ExtractDerBytesFromPem(string pem, string label)
        {
            string header = $"-----BEGIN {label}-----";
            string footer = $"-----END {label}-----";

            int start = pem.IndexOf(header, StringComparison.Ordinal);
            int end = pem.IndexOf(footer, StringComparison.Ordinal);

            if (start < 0 || end < 0)
                throw new FormatException($"PEM format is invalid. Missing {label} headers.");

            string base64 = pem.Substring(start + header.Length, end - start - header.Length)
                .Replace("\r", "").Replace("\n", "").Trim();

            return Convert.FromBase64String(base64);
        }

        public static string DecodeBase64Payload(string base64Payload)
        {
            int padding = 4 - (base64Payload.Length % 4);
            if (padding != 4)
            {
                base64Payload = base64Payload.PadRight(base64Payload.Length + padding, '=');
            }

            base64Payload = base64Payload.Replace('-', '+').Replace('_', '/');

            byte[] bytes = Convert.FromBase64String(base64Payload);

            return Encoding.UTF8.GetString(bytes);
        }

    }
}
