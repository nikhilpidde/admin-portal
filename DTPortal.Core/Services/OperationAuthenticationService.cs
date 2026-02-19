using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Services
{
    public class OperationAuthenticationService: IOperationAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;
        private readonly ILogger<OperationAuthenticationService> _logger;
        private readonly IFido2 _fido2;
        private readonly SSOConfig ssoConfig;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly MessageConstants Constants;

        public OperationAuthenticationService(IUnitOfWork unitOfWork, ICacheClient cacheClient,
            ILogger<OperationAuthenticationService> logger, IFido2 fido2,
            IGlobalConfiguration globalConfiguration)
        {
            _unitOfWork = unitOfWork;
            _cacheClient = cacheClient;
            _logger = logger;
            _fido2 = fido2;
            _globalConfiguration = globalConfiguration;

            // Get SSO Configuration
            ssoConfig = _globalConfiguration.GetSSOConfiguration();
            if (null == ssoConfig)
            {
                _logger.LogError("Get SSO Configuration failed in operation auth service");
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
        }

        public async Task<ValidateOperationAuthNResponse> ValidateOperationAuthN(ValidateOperationAuthNRequest request)
        {
            ValidateOperationAuthNResponse response = new ValidateOperationAuthNResponse();
            var options = new AssertionOptions();

            // Validate input
            if (null == request.OperationName)
            {
                response.success = false;
                response.message = "No operation name received";

                return response;
            }

            // Get operation auth scheme
            var operationAuthScheme = await _unitOfWork.OperationsAuthScheme.
                GetOperationsAuthschemeByOperationName(request.OperationName);
            if (null == operationAuthScheme)
            {
                response.success = false;
                response.message = "Operation authenticationscheme not found";

                return response;
            }

            if(operationAuthScheme.AuthenticationRequired == false)
            {
                response.success = true;

                return response;
            }

            // Get usersession
            //var userSession = await _cacheClient.Get<List<string>>("UserSessions", request.userName);
            //if(null == userSession)
            //{
            //    response.success = false;
            //    response.message = "No user session found";

            //    return response;
            //}

            //// Get globalsession
            //var globalSession = await  _cacheClient.Get<GlobalSession>("GlobalSession", userSession.First());
            //if (null == userSession)
            //{
            //    response.success = false;
            //    response.message = "No user session found";

            //    return response;
            //}

            //var isExists = await _cacheClient.Exists("GlobalSession", globalSession.GlobalSessionId);
            //if(104 != isExists.retValue)
            //{
            //    response.success = false;
            //    response.message = "InvalidSession or Session Expired";

            //    return response;
            //}

            //foreach (var item in globalSession.OperationsDetails)
            //{
            //    if (item.OperationName.Contains(request.OperationName))
            //    {
            //        var ts = DateTime.Now - item.AuthenticatedTime;

            //        if(ts.TotalMinutes < ssoConfig.sso_config.operation_authn_timeout)
            //        {
            //            response.success = true;

            //            return response;
            //        }
            //    }
            //}

            var authScheme = operationAuthScheme.AuthenticationSchemeName;

            if (authScheme.Contains("FIDO2"))
            {
                var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(request.userName);
                if (null == userInDb)
                {
                    _logger.LogError("GetUserAuthDataAsync failed, not found");
                    response.success = false;
                    response.message = "User data not found, Please contact admin";

                    return response;
                }

                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                    (userInDb.Uuid, "FIDO2");
                if (null == userAuthData)
                {
                    _logger.LogError("GetUserAuthDataAsync failed, not found");
                    response.success = false;
                    response.message = "User is not provisioned for this authentication scheme";

                    return response;
                }

                // Get Encryption Key
                var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
                if (null == EncKey)
                {
                    _logger.LogError("EncDecKeys.GetByIdAsync failed, not found");
                    response.success = false;
                    response.message = "Internal server error";

                    return response;
                }


                string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
                var DecryptedPasswd = string.Empty;
                try
                {
                    // Decrypt Password
                    DecryptedPasswd = EncryptionLibrary.DecryptText(userAuthData.AuthData,
                        encryptionPassword, "appshield3.0");
                }
                catch (Exception error)
                {
                    _logger.LogError("DecryptText failed, found exception: {0}",
                        error.Message);
                    response.success = false;
                    response.message = "Internal server error";

                    return response;
                }

                var credential = JsonConvert.DeserializeObject<StoredCredential>
                    (DecryptedPasswd);

                var userVerification = "preferred";
                var extensions = new AuthenticationExtensionsClientInputs()
                {
                    SimpleTransactionAuthorization = "FIDO",
                    GenericTransactionAuthorization = new TxAuthGenericArg
                    {
                        ContentType = "text/plain",
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                    },
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true
                };

                // 3. Create options
                var userVerificationOptions = userVerification.ToEnum<UserVerificationRequirement>();

                try
                {
                    options = _fido2.GetAssertionOptions(new List<PublicKeyCredentialDescriptor>
                    { credential.Descriptor }, userVerificationOptions, extensions);
                }
                catch
                {

                    _logger.LogError("_fido2 GetAssertionOptions failed, not found");
                    response.success = false;
                    response.message = "Internal server error";

                    return response;
                }
            }

            // Generate sessionid
            var tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

            var authschmList = new List<string>
            {
                authScheme
            };

            // Prepare temporary session object
            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = request.userName,
                PrimaryAuthNSchemeList = authschmList,
                Clientdetails = new Clientdetails()
                {
                    ClientId = request.OperationName
                },
                AuthNSuccessList = new List<string>(),
                IpAddress = "NOT_AVAILABLE",
                MacAddress = "NOT_AVAILABLE",
                withPkce = false,
                AdditionalValue = "pending"
            };

            // Create temporary session
            var task = await _cacheClient.Add("TemporarySession", tempAuthNSessId,
                    temporarySession);
            if (0 != task.retValue)
            {
                response.success = false;
                response.message = "Internal server error";

                return response;
            }

            ValidateOperationAuthNResult result = new ValidateOperationAuthNResult()
            {
                authenticationScheme = authScheme,
                tempSession = temporarySession.TemporarySessionId
            };

            if(authScheme.Contains("FIDO2"))
            {
                result.Fido2Options = options.ToJson().ToString();
            }

            response.success = false;
            response.result = result;

            return response;
        }

        public async Task<Response> VerifyOperationAuthData(VerifyOperationAuthDataRequest request)
        {
            Response response = new Response();
            bool isAuthNPassed = false;

            var isExists = await _cacheClient.Exists("TemporarySession", request.tempSession);
            if (104 != isExists.retValue)
            {
                response.Success = false;
                response.Message = "InvalidSession or Session Expired";

                return response;
            }

            var tempSession = await _cacheClient.Get<TemporarySession>("TemporarySession", request.tempSession);
            if(null == tempSession)
            {
                response.Success = false;
                response.Message = "InvalidSession or Session Expired";

                return response;
            }

            if(!tempSession.PrimaryAuthNSchemeList.Contains(request.authNSchemeName))
            {
                response.Success = false;
                response.Message = "Authentication scheme mismatched";

                return response;
            }

            var userInfo = await _unitOfWork.Users.GetUserbyUuidAsync(tempSession.UserId);
            if (null == userInfo)
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            if(userInfo.Status == StatusConstants.SUSPENDED)
            {
                response.Success = false;
                response.Message = "User Account is Suspended";
                return response;
            }

            var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            if (null == EncKey)
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            if (request.authNSchemeName == "PASSWORD")
            {

                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userInfo.Uuid,"PASSWORD");
                if (null == userAuthData)
                {
                    response.Success = false;
                    response.Message = "User authentication data not found";
                    return response;
                }

                string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);

                var DecryptedPasswd = string.Empty;
                try
                {
                    // Decrypt Password
                    DecryptedPasswd = EncryptionLibrary.DecryptText(userAuthData.AuthData,
                        encryptionPassword, "appshield3.0");
                }
                catch (Exception error)
                {
                    _logger.LogError("DecryptText failed, found exception: {0}",
                        error.Message);
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }

                // Compare password
                if (request.authData != DecryptedPasswd)
                {
                    response.Success = false;
                    response.Message = "Wrong credentials";

                    isAuthNPassed = false;
                }
                else
                {
                    isAuthNPassed = true;
                }

            } // Password authnscheme

            else if (request.authNSchemeName == "FIDO2")
            {
                
                // Get UserAuthData
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync
                    (userInfo.Uuid, "FIDO2");
                if (null == userAuthData)
                {
                    _logger.LogError("GetUserAuthDataAsync failed, not found");
                    response.Success = false;
                    response.Message = "User authentication data not found";
                    return response;
                }


                string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
                var DecryptedPasswd = string.Empty;
                try
                {
                    // Decrypt Password
                    DecryptedPasswd = EncryptionLibrary.DecryptText(userAuthData.AuthData,
                        encryptionPassword, "appshield3.0");
                }
                catch (Exception error)
                {
                    _logger.LogError("DecryptText failed, found exception: {0}",
                        error.Message);
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }

                var fido2 = request.authData.Split(new char[] { '#' });
                if (fido2.Count() < 2)
                {
                    _logger.LogError("fido2 options not found");
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }

                var options = AssertionOptions.FromJson(fido2[1]);
                if (options == null)
                {
                    _logger.LogError("fido2 options not found");
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }

                var credential = JsonConvert.DeserializeObject<StoredCredential>
                    (DecryptedPasswd);
                if (credential == null)
                {
                    _logger.LogError("fido2 options not found");
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }

                var assertionData = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>
                    (fido2[0]);
                try
                {
                    var result = await _fido2.MakeAssertionAsync(assertionData,
                        options,
                        credential.PublicKey, credential.SignatureCounter,
                        args => Task.FromResult(credential.UserHandle.SequenceEqual(args.UserHandle)));
                    if (result.Status.Equals("ok"))
                    {
                        isAuthNPassed = true;
                    }
                    else
                    {
                        isAuthNPassed = false;
                        response.Success = false;
                        response.Message = result.Status;
                    }
                }
                catch (Exception error)
                {
                    isAuthNPassed = false;
                    _logger.LogError("VerifyOperationAuthData Failed: {0}",
                        error.Message);
                }
            }

            if(true == isAuthNPassed)
            {
                var userLoginDetail = await _unitOfWork.UserLoginDetail.
                    GetUserLoginDetailAsync(userInfo.Id.ToString());
                if (null != userLoginDetail)
                {
                    _logger.LogError("GetUserPasswordDetailAsync failed," +
                        "not found");

                    if (userLoginDetail.WrongPinCount > 0)
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
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }
                }
            }

            if (false == isAuthNPassed)
            {
                response.Success = false;
                response.Message = "Wrong credentials";

                var userPwdDetailsinDB = await _unitOfWork.UserLoginDetail.
                        GetUserLoginDetailAsync(userInfo.Id.ToString());
                if (null == userPwdDetailsinDB)
                {
                    _logger.LogError("GetUserPasswordDetailAsync failed, not found");
                    response.Success = false;
                    response.Message = Constants.InternalError;

                    userPwdDetailsinDB = new UserLoginDetail();

                    // Prepare user password object
                    userPwdDetailsinDB.UserId = userInfo.Id.ToString();
                    userPwdDetailsinDB.BadLoginTime = DateTime.Now;
                    userPwdDetailsinDB.IsReversibleEncryption = false;
                    userPwdDetailsinDB.IsScrambled = false;
                    userPwdDetailsinDB.PriAuthSchId = 0;
                    userPwdDetailsinDB.WrongPinCount = 1;

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
                    userPwdDetailsinDB.UserId = userInfo.Id.ToString();

                    userPwdDetailsinDB.BadLoginTime = DateTime.Now;
                    userPwdDetailsinDB.WrongPinCount = userPwdDetailsinDB.
                            WrongPinCount + 1;

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
                        response.Success = false;
                        response.Message = Constants.InternalError;

                        return response;
                    }
                }


                if ((userPwdDetailsinDB.WrongPinCount >=
                    ssoConfig.sso_config.wrong_pin))
                {
                    // If client is DTADMIN Portal, Change the user status in
                    // DT DataBase
                    var user = await _unitOfWork.Users.GetByIdAsync(userInfo.Id);
                    if (null == user)
                    {
                        _logger.LogError("Users GetByIdAsync failed, " +
                            "not found {0}",
                            userInfo.Id);
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }

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
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }

                    // Get usersession
                    var usersession = await _cacheClient.Get<List<string>>("UserSessions", tempSession.UserId);
                    if (null == usersession)
                    {
                        response.Success = false;
                        response.Message = "No user session found";

                        return response;
                    }

                    // Get globalsession
                    var globalsession = await _cacheClient.Get<GlobalSession>("GlobalSession", usersession.First());
                    if (null == usersession)
                    {
                        response.Success = false;
                        response.Message = "No user session found";

                        return response;
                    }

                    isExists = await _cacheClient.Exists("GlobalSession", globalsession.GlobalSessionId);
                    if (104 != isExists.retValue)
                    {
                        response.Success = false;
                        response.Message = "InvalidSession or Session Expired";

                        return response;
                    }

                    var res = await _cacheClient.Remove("GlobalSession", globalsession.GlobalSessionId);
                    if (0 != res.retValue)
                    {
                        response.Success = false;
                        response.Message = "Internal server error";

                        return response;
                    }

                    //var isRemoved = usersession.Remove(globalsession.GlobalSessionId);
                    //if (false == isRemoved)
                    //{
                    //    _logger.LogError(Constants.SessionMismatch);
                    //    response.Success = false;
                    //    response.Message = Constants.SessionMismatch;

                    //    return response;
                    //}

                    //var result = await _cacheClient.Add(CacheNames.UserSessions,
                    //    globalsession.UserId,
                    //    usersession);
                    //if (0 != result.retValue)
                    //{
                    //    _logger.LogError("UserSessions Add failed");
                    //    response.Success = false;
                    //    response.Message = Constants.InternalError;
                    //    return response;
                    //}

                    res = await _cacheClient.Remove(CacheNames.UserSessions,
                        tempSession.UserId);
                    if (0 != res.retValue)
                    {
                        _logger.LogError("UserSessions Remove failed");
                        response.Success = false;
                        response.Message = Constants.InternalError;
                        return response;
                    }
                }

                isAuthNPassed = false;

                return response;
            }

            // Get usersession
            //var userSession = await _cacheClient.Get<List<string>>("UserSessions", tempSession.UserId);
            //if (null == userSession)
            //{
            //    response.Success = false;
            //    response.Message = "No user session found";

            //    return response;
            //}

            // Get globalsession
            //var globalSession = await _cacheClient.Get<GlobalSession>("GlobalSession", userSession.First());
            //if (null == userSession)
            //{
            //    response.Success = false;
            //    response.Message = "No user session found";

            //    return response;
            //}

            //isExists = await _cacheClient.Exists("GlobalSession", globalSession.GlobalSessionId);
            //if (104 != isExists.retValue)
            //{
            //    response.Success = false;
            //    response.Message = "InvalidSession or Session Expired";

            //    return response;
            //}

            //OperationsDetails operationsDetails = new OperationsDetails()
            //{
            //    OperationName = tempSession.Clientdetails.ClientId,
            //    AuthenticatedTime = DateTime.Now
            //};

            //globalSession.OperationsDetails.Add(operationsDetails);

            //var cacheRes = await _cacheClient.Add("GlobalSession", globalSession.GlobalSessionId, globalSession);
            //if(0 != cacheRes.retValue)
            //{
            //    response.Success = false;
            //    response.Message = "Internal server error";

            //    return response;
            //}

            response.Success = true;
            return response;
        }

        public async Task<OperationsAuthscheme> GetOperationsAuthschemeById(int id)
        {
            return await _unitOfWork.OperationsAuthScheme.GetByIdAsync(id);
        }

        public async Task<OperationsAuthscheme> GetOperationsAuthschemeByName(string name)
        {
            return await _unitOfWork.OperationsAuthScheme.GetOperationsAuthschemeByOperationName(name);
        }
        public async Task<OperationAuthSchmesResponse> UpdateOperationsAuthscheme(OperationsAuthscheme perationsAuthscheme)
        {
            try
            {
                _unitOfWork.OperationsAuthScheme.Update(perationsAuthscheme);
                await _unitOfWork.SaveAsync();
                return new OperationAuthSchmesResponse(perationsAuthscheme);
            }
            catch (Exception error)
            {
                _logger.LogError("UpdateOperationsAuthschemeByName Failed: {0}",
                    error.Message);
                return new OperationAuthSchmesResponse("UpdateOperationsAuthschemeByName failed");
            }
        }

        public async Task<IEnumerable<OperationsAuthscheme>> ListAllOperationsAuthschemes()
        {
            return await _unitOfWork.OperationsAuthScheme.GetAllAsync();
        }
    }
}
