using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using static DTPortal.Common.CommonResponse;
using DTPortal.Common;
using static DTPortal.Common.EncryptionLibrary;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using DTPortal.Core.Constants;

namespace DTPortal.Core.Services
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly ILogger<ResetPasswordService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;
        private readonly IEmailSender _emailSender;
        public ResetPasswordService(ILogger<ResetPasswordService> logger,
        IUnitOfWork unitOfWork, ICacheClient cacheClient,
            IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _cacheClient = cacheClient;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
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
/*
        public async Task<bool> CheckPasswordComplexity(string password, PasswordPolicy passwordPolicy)
        {

            if (password.Length < passwordPolicy.MinimumPwdLength
                || password.Length > passwordPolicy.MaximumPwdLength)
            {
                return false;
            }

            switch (passwordPolicy.PwdContains)
            {
                case 1:
                    {
                        return Regex.IsMatch(password, @"^[a-zA-Z]+$");
                    }
                case 2:
                    {
                        return Regex.IsMatch(password, @"^[0-9]+$");
                    }
                case 3:
                    {
                        return Regex.IsMatch(password, @"^[a-zA-Z0-9]+$");
                    }
                case 4:
                    {
                        return Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-zA-Z])(?!.*\s)[0-9a-zA-Z]*$");
                    }
                case 5:
                    {
                        return Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[!@#$%_&.+=*])(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9!@#$%_&.+=*]{1,106}$");
                    }

                default:
                    {
                        return true;
                    }

            }
        }

*/
        public async Task<GetAllUserSecurityQueResponse> GetUserSecurityQuestions(int userId)
        {

            // Variable declaration
            GetAllUserSecurityQueResponse response = new GetAllUserSecurityQueResponse();
            //var isAuthSchm = false;

            var user =await  _unitOfWork.Users.GetByIdAsync(userId);
            if (null == user)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            var userSecQues = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync(user.Id);
            if (userSecQues.Count() == 0)
            {
                response.Success = false;
                response.Message = "User Security Questions not found";
                return response;
            }

            var SecQueList = new List<SecurityQuestions>();
            foreach (var item in userSecQues)
            {
                var secQue = new SecurityQuestions()
                {
                    Id = item.Id,
                    Question = item.Question
                };

                SecQueList.Add(secQue);
            }

            response.Success = true;
            response.Result = SecQueList;
            return response;
        }

        public async Task<ValidateUserSecQueResponse> ValidateUserSecurityQuestions(ValidateUserSecQueRequest request)
        {
            ValidateUserSecQueResponse response = new ValidateUserSecQueResponse();

            // Get user by UUID
            var userId = await _unitOfWork.Users.GetUserbyUuidAsync(request.uuid);
            if (userId == null)
            {
                response.Success = false;
                response.Message = "User Security Questions/Answers not matched";
                return response;
            }

            // Get stored security Q&A
            var userSecQueAns = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync(userId.Id);

            // ❗ Must have exactly 3 configured
            if (userSecQueAns.Count() < 3)
            {
                response.Success = false;
                response.Message = "Security questions not configured properly";
                return response;
            }

            int matchedCount = 0;

            foreach (var req in request.secQueAns)
            {
                var dbMatch = userSecQueAns.FirstOrDefault(x => x.Question == req.secQue);

                if (dbMatch == null || dbMatch.Answer != req.answer)
                {
                    response.Success = false;
                    response.Message = "User Security Questions/Answers not matched";
                    return response;
                }

                matchedCount++;
            }

            // ❗ Ensure all 3 matched
            if (matchedCount != 3)
            {
                response.Success = false;
                response.Message = "Security Questions/Answers not matched with User";
                return response;
            }

            // Generate temporary session
            var tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = request.uuid,
                PrimaryAuthNSchemeList = new List<string>() { "PASSWORD" },
                AuthNSuccessList = new List<string>(),
                IpAddress = "NOT_AVAILABLE",
                MacAddress = "NOT_AVAILABLE"
            };

            var cacheResult = await _cacheClient.Add("TemporarySession", tempAuthNSessId, temporarySession);

            if (cacheResult.retValue != 0)
            {
                _logger.LogError("_cacheClient.Add failed");
                response.Success = false;
                response.Message = cacheResult.errorMsg;
                return response;
            }

            response.Success = true;
            response.TemporarySession = tempAuthNSessId;

            return response;
        }

        public async Task<ValidateUserSecQueResponse> SendEmailOTP(string uuid, string tempsession)
        {
            var response = new ValidateUserSecQueResponse();

            var user = await _unitOfWork.Users.GetUserbyUuidAsync(uuid);
            if(null == user)
            {
                _logger.LogError("GetUserbyUuidAsync failed, not found");
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            if(null != tempsession)
            {
                var isExists = _cacheClient.KeyExists("TemporarySession", tempsession);
                if(104 == isExists.retValue)
                {
                    var res = await _cacheClient.Remove("TemporarySession", tempsession);
                    if(0 != res.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed");
                        response.Success = false;
                        response.Message ="Internal server error";
                        return response;
                    }
                }
            }

            var otp = GenerateRandomOTP(6);

            // Generate sessionid
            var tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

            // Prepare temporary session object
            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = uuid,
                PrimaryAuthNSchemeList = new List<string>() { "EMAIL_OTP" },
                AuthNSuccessList = new List<string>(),
                IpAddress = "NOT_AVAILABLE",
                MacAddress = "NOT_AVAILABLE",
                AdditionalValue = otp
            };

            // Create temporary session
            var task = await _cacheClient.Add("TemporarySession", tempAuthNSessId,
                    temporarySession);
            if (0 != task.retValue)
            {
                _logger.LogError("_cacheClient.Add failed");
                response.Success = false;
                if (!string.IsNullOrEmpty(task.errorMsg))
                    response.Message = task.errorMsg;
                return response;
            }

            // Send Success response
            response.Success = true;
            response.TemporarySession = tempAuthNSessId;
            //response.AuthenticationSchemes = new List<string>() { "PASSWORD" };

            //var mailBody = string.Format("Hi {0},\nBelow is the OTP for email verification\nOTP: {1}\n",
            //    user.FullName, otp);
            var mailBody = "<p>Hi " + user.FullName + ",</p>" +
               "<p>Below is the OTP for email verification:</p>" +
               "<p>OTP: " + otp + "</p>";

            var message = new Message(new string[]
            {
            user.MailId
            },
            "IDP OTP",
            mailBody
            );

            try
            {
                await _emailSender.SendEmail(message);
            }
            catch
            {
                response.Success = false;
                response.Message = "Internal server error";
            }

            return response;
        }

        public async Task<Response> ResetPassword(ResetPasswordRequest request)
        {

            _logger.LogDebug("-->ResetPassword");

            // Variable declaration
            Response response = new Response();
            //var isAuthSchm = false;

            // Check whether the temporary session exists
            var isExists = await _cacheClient.Exists("TemporarySession",
                request.TemporarySession);
            if (104 != isExists.retValue)
            {
                response.Success = false;
                response.Message = "Temporary session expired/does not exists";
                return response;
            }

            // Get the temporary session object
            var tempSession = await _cacheClient.Get<TemporarySession>("TemporarySession",
                request.TemporarySession);
            if (null == tempSession)
            {
                response.Success = false;
                response.Message = "Temporary session expired/does not exists";
                return response;
            }

            if(tempSession.PrimaryAuthNSchemeList.Count == 0 && 
                tempSession.PrimaryAuthNSchemeList[0].Equals("EMAIL_OTP"))
            {
                if(request.otp != tempSession.AdditionalValue)
                {
                    response.Success = false;
                    response.Message = "Incorrect OTP";
                    return response;
                }
            }

            // Get Encryption Key
            var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            if (null == EncKey)
            {
                _logger.LogError(" _unitOfWork.EncDecKey.GetByIdAsync failed: Internal error occurred");
                response.Success = false;
                response.Message = "Internal error occurred";

                return response;
            }

            // Get user id
            var userId =await _unitOfWork.Users.GetUserbyUuidAsync(request.uuid);
            if(null == userId)
            {
                response.Success = false;
                response.Message = "No user found with username";
                return response;
            }

            var passwordPolicy = await _unitOfWork.PasswordPolicy.GetByIdAsync(1);
            if (passwordPolicy == null)
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            var isAccept = PasswordValidation.CheckPasswordComplexity(request.newPassword,
                passwordPolicy);
            if (false == isAccept)
            {
                response.Success = false;
                response.Message = String.Format(DTInternalConstants.PasswordPolicyMismatch,
                    passwordPolicy.MinimumPwdLength, passwordPolicy.MaximumPwdLength);
                return response;
            }

            string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
            var DecryptedPasswd = string.Empty;

            if (!string.IsNullOrEmpty(userId.AuthData))
            {
                var passwordsInDb = userId.AuthData.Split(',');
                if (passwordsInDb.Count() > 0)
                {
                    if (passwordPolicy.PasswordHistory > 0)
                    {
                        for (int i = 0; i < passwordPolicy.PasswordHistory && i < passwordsInDb.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(passwordsInDb[i]))
                            {
                                try
                                {
                                    // Decrypt Password
                                    DecryptedPasswd = EncryptionLibrary.DecryptText(passwordsInDb[i],
                                        encryptionPassword, "appshield3.0");
                                }
                                catch (Exception error)
                                {
                                    _logger.LogError("DecryptText failed, found exception: {0}",
                                        error.Message);
                                    // Log the exception 
                                    response.Success = false;
                                    response.Message = "DecryptText failed, found exception";
                                    return response;
                                }

                                if (DecryptedPasswd.Equals(request.newPassword))
                                {
                                    response.Success = false;
                                    response.Message = String.Format("New password matches one of the last {0} passwords",
                                        passwordPolicy.PasswordHistory);
                                    return response;
                                }
                            }
                        }

                    }
                }
            }

            // Encrypt Password
            var EncryptedPasswd = EncryptionLibrary.EncryptText(request.newPassword,
                encryptionPassword, "appshield3.0");

            var userAuthDatainDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userId.Uuid, "PASSWORD");
            if(null == userAuthDatainDb)
            {

            }

            try
            {
                // Decrypt Password
                DecryptedPasswd = EncryptionLibrary.DecryptText(userAuthDatainDb.AuthData,
                    encryptionPassword, "appshield3.0");
            }
            catch (Exception error)
            {
                _logger.LogError("DecryptText failed, found exception: {0}",
                    error.Message);
                // Log the exception 
                response.Success = false;
                response.Message = "DecryptText failed, found exception";
                return response;
            }


            if (DecryptedPasswd == request.newPassword)
            {
                response.Success = false;
                response.Message = "New password and Old password is same";
                return response;
            }

            userAuthDatainDb.AuthData = EncryptedPasswd;

            _unitOfWork.UserAuthData.Update(userAuthDatainDb);

            int res = await _unitOfWork.SaveAsync();
            if (1 != res)
            {
                response.Success = false;
                response.Message = "Internal error occurred";

                return response;
            }

            var UserpasswordDetail = await _unitOfWork.UserLoginDetail.GetUserLoginDetailAsync(
                userId.Id.ToString());
            if(null == UserpasswordDetail)
            {
                // User Login Details
                var userPasswordDetail = new UserLoginDetail
                {
                    UserId = userId.Id.ToString(),
                    IsReversibleEncryption = false,
                    WrongPinCount = 0,
                    WrongCodeCount = 0,
                    DeniedCount = 0,
                    IsScrambled = false,
                    PriAuthSchId = 64
                };

                try
                {
                    await _unitOfWork.UserLoginDetail.AddAsync(userPasswordDetail);
                    await _unitOfWork.SaveAsync();
                }
                catch
                {
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }
            }

            UserpasswordDetail.LastAuthData = EncryptedPasswd;
            UserpasswordDetail.WrongPinCount = 0;

            // Add entry in db
            _unitOfWork.UserLoginDetail.Update(UserpasswordDetail);

            res = await _unitOfWork.SaveAsync();
            if (1 != res)
            {
                response.Success = false;
                response.Message = "Internal error occurred";

                return response;
            }

            if (string.IsNullOrEmpty(userId.AuthData))
            {
                userId.AuthData = EncryptedPasswd;
            }
            else
            {
                var password = userId.AuthData.Split(',').ToList();
                if(password.Count()==passwordPolicy.PasswordHistory)
                {
                    password.Remove(password[0]);
                    userId.AuthData = string.Join(',', password);
                }
                userId.AuthData = string.Format("{0},{1}", userId.AuthData,
                    EncryptedPasswd);
            }

            try
            {
                _unitOfWork.Users.Update(userId);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                response.Success = false;
                response.Message = "Internal server error";

                return response;
            }

            response.Success = true;
            response.Message = "User password reset successfully";

            return response;
        }
    }
}
