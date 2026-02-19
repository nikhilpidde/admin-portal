using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.Models;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Registration;
using DTPortal.Web.ViewModel.UserDashboard;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{

    public class UserDashboardController : BaseController
    {
        private readonly IFido2 _fido2;
        private readonly IUserConsoleService _userConsoleService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly IUserManagementService _userService;
        private readonly ILogger<UserDashboardController> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IConfiguration _configuration;
        public IConfiguration Configuration { get; }

        public UserDashboardController(ILogger<UserDashboardController> logger, 
            IConfiguration configuration, IFido2 fido2,
            IResetPasswordService resetPasswordService,
            IConfiguration configuration1,
            IConfigurationService configurationService,
            ILogClient logClient, IUserManagementService userService,
            IUserConsoleService userConsoleService) : base(logClient)
        {
            _fido2 = fido2;
            _userConsoleService = userConsoleService;
            _logger = logger;
            _userService = userService;
            _configuration = configuration1;
            Configuration = configuration;
            _resetPasswordService = resetPasswordService;
            _configurationService = configurationService;
        }

        [Authorize]
        [ServiceFilter(typeof(SessionValidationAttribute))]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);
            var userInDb = await _userConsoleService.GetUserAsync(id);
            if (userInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Get profile", LogMessageType.FAILURE.ToString(), "Fail to get User profile");
                return NotFound();
            }

            var roleLookups = await _userService.GetRoleLookupsAsync();
            if (roleLookups == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Get profile", LogMessageType.FAILURE.ToString(), "Fail to get User role in profile");
                return NotFound();
            }

            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Get profile", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }

            DateOnly dob = (DateOnly)userInDb.Dob;

            var email = userInDb.MailId.Split("@");
            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            var model = new ProfileViewModel
            {
                Id = userInDb.Id,
                Uuid = userInDb.Uuid,
                FullName = userInDb.FullName,
                MailId = email[0],
                EmailDomain = email[1],
                gender = userInDb.Gender,
                MobileNo = userInDb.MobileNo.Replace("+256", ""),
                Dob = dob.ToDateTime(TimeOnly.Parse("10:00 PM")),
                Role = roleLookups.FirstOrDefault(x => x.Id == userInDb.RoleId.Value)?.DisplayName,
                //AuthScheme = userInDb.AuthScheme,
                Status = userInDb.Status,
                EmailDomains = Domainlist
            };


            userInDb.Role = null;
            userInDb.MakerCheckers = null;
            userInDb.PasswordPolicyCreatedByNavigations = null;
            userInDb.PasswordPolicyUpdatedByNavigations = null;
            userInDb.Role = null;
            //userInDb.SecurityQueCreatedByNavigations = null;
            //userInDb.SecurityQueUpdatedByNavigations = null;
            
            //userInDb.UserAuthData = null;

            var Fidomodel = new RegistrationViewModel
            {
                userDetails = JsonConvert.SerializeObject(userInDb),
                suid = userInDb.Uuid,
                FullName = userInDb.FullName,
                MailID_MobileNO = userInDb.MailId + "/" + userInDb.MobileNo
            };

            model.fidoData = Fidomodel;

            var status = await _userService.GetUserFido2StatusAsync(userInDb.Uuid);
            model.FidoStatus = (status ? "1" : "0");

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Get profile", LogMessageType.SUCCESS.ToString(), "Get User profile success");
            return View(model);
        }

        [Authorize]
        [ServiceFilter(typeof(SessionValidationAttribute))]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel viewModel)
        {

            var Domainlist = GetDomainList().Result;
            if (Domainlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Get profile", LogMessageType.FAILURE.ToString(), "Fail to get Domain list");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                viewModel.EmailDomains = Domainlist;
                return View("Profile", viewModel);
            }

            var email = viewModel.MailId + "@" + viewModel.EmailDomain;

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){1,7})+)$");
            Match match = regex.Match(email);
            if (!match.Success)
            {
                ModelState.AddModelError("MailId", "Invalid MailId");
                viewModel.EmailDomains = Domainlist;
                return View("Profile", viewModel);
            }

            viewModel.MailId = email;

            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
            if (viewModel.MobileNo.StartsWith(mobileCountryCode) || viewModel.MobileNo.StartsWith("971"))
            {
                ModelState.AddModelError("MobileNo", "Write only mobile number without country code");
                viewModel.EmailDomains = Domainlist;
                return View("Edit", viewModel);
            }

            var userInDb = await _userConsoleService.GetUserAsync(viewModel.Id);
            if (userInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Update profile", LogMessageType.FAILURE.ToString(), "Fail to get User profile in update profile");
                return NotFound();
            }

            userInDb.Id = viewModel.Id;
            userInDb.Uuid = viewModel.Uuid;
            userInDb.FullName = viewModel.FullName;
            userInDb.MailId = viewModel.MailId;
            userInDb.Gender = viewModel.gender;
            userInDb.MobileNo = mobileCountryCode + viewModel.MobileNo;
            userInDb.Dob = DateOnly.FromDateTime((DateTime)viewModel.Dob);
            userInDb.UpdatedBy = UUID;
            //userInDb.AuthScheme = viewModel.AuthScheme;

            var response = await _userConsoleService.UpdateProfile(userInDb);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Update profile", LogMessageType.FAILURE.ToString(), "Fail to update User profile ");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                viewModel.EmailDomains = Domainlist;
                return View("Profile", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Update profile", LogMessageType.SUCCESS.ToString(), "Update User profile success");
                Alert alert = new Alert { IsSuccess = true, Message = "Profile Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("Profile");
            }
        }


        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword(int id, bool isFirstLogin = false)
        {
            var model = new ChangePasswordViewModel()
            {
                Id = id,
                isFirstLogin = isFirstLogin
            };

            if (isFirstLogin)
            {
                return View("FirstChangePassword", model);
            }
            else
            {
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    if (viewModel.isFirstLogin)
                    {
                        return View("FirstChangePassword", viewModel);
                    }
                    else
                    {
                        return View("ChangePassword", viewModel);
                    }
                }

                var response = await _userConsoleService.ChangePassword(viewModel.Id,
                    viewModel.OldPassword, viewModel.NewPassword);
                if (response == null || !response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Password", LogMessageType.FAILURE.ToString(), "Failed to change User Password");
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);

                    if (viewModel.isFirstLogin)
                    {
                        return View("FirstChangePassword", viewModel);
                    }
                    else
                    {
                        return View("ChangePassword", viewModel);
                    }
                }
                else
                {
                    if (viewModel.isFirstLogin)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Password", LogMessageType.SUCCESS.ToString(), "Change User Password success");
                        Alert alert = new Alert { IsSuccess = true, Message = "Change User Password success" };
                        TempData["Alert"] = JsonConvert.SerializeObject(alert);
                        return RedirectToAction("SetSecurityQuestion");
                    }
                    else
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Password", LogMessageType.SUCCESS.ToString(), "Change User Password success");
                        Alert alert = new Alert { IsSuccess = true, Message = "Change User Password success" };
                        TempData["Alert"] = JsonConvert.SerializeObject(alert);
                        return RedirectToAction("Profile");
                    }

                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("ChangePassword post Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }

        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangeSecurityQuestion()
        {
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);

            var UserSecQues = await _resetPasswordService.GetUserSecurityQuestions(id);
            if (UserSecQues == null || !UserSecQues.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change security quesstion", LogMessageType.FAILURE.ToString(), "Failed to change User security quesstion");
                return NotFound();
            }
            else
            {

                var model = new SequerityQuestionViewModel()
                {
                    Que1Id = UserSecQues.Result[0].Id,
                    Question1 = UserSecQues.Result[0].Question,

                    Que2Id = UserSecQues.Result[1].Id,
                    Question2 = UserSecQues.Result[1].Question,

                    Que3Id = UserSecQues.Result[2].Id,
                    Question3 = UserSecQues.Result[2].Question,

                    UserId = id,

                    SecurityQueList1 = getSecurityQuestionList(1),
                    SecurityQueList2 = getSecurityQuestionList(2),
                    SecurityQueList3 = getSecurityQuestionList(3)
                };

                _logger.LogInformation("<-- ChangeSecurityQuestion get");
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeSecurityQuestion(SequerityQuestionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                return View("ChangeSecurityQuestion", viewModel);
            }

            var QueAns1 = new UserSecurityQue
            {
                Id = viewModel.Que1Id,
                UserId = viewModel.UserId,
                Question = viewModel.Question1,
                Answer = viewModel.Answer1,
                CreatedBy = UUID,
                UpdatedBy = UUID
            };
            var response = await _userConsoleService.UpdateUserSecurityQnsAns(QueAns1);
            if (response == null || !response.Success)
            {
                viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Security Question", LogMessageType.FAILURE.ToString(), "Fail to update user security question ");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("ChangeSecurityQuestion", viewModel);
            }
            else
            {
                var QueAns2 = new UserSecurityQue
                {
                    Id = viewModel.Que2Id,
                    UserId = viewModel.UserId,
                    Question = viewModel.Question2,
                    Answer = viewModel.Answer2,
                    CreatedBy = UUID,
                    UpdatedBy = UUID
                };
                var response1 = await _userConsoleService.UpdateUserSecurityQnsAns(QueAns2);
                if (response1 == null || !response1.Success)
                {
                    viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                    viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Security Question", LogMessageType.FAILURE.ToString(), "Fail to update user security question ");
                    Alert alert = new Alert { Message = (response1 == null ? "Internal error please contact to admin" : response1.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("ChangeSecurityQuestion", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Change Security Question", LogMessageType.SUCCESS.ToString(), "Update user security question success");
                    Alert alert = new Alert { IsSuccess = true, Message = "Security question updated successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("Profile");

                }

            }
        }


        [HttpPost]
        public ActionResult CredentialOptions([FromBody] RegistrationViewModel model)
        {
            try
            {
                var user = new Fido2User
                {
                    DisplayName = $"{model.FullName}",
                    Name = model.MailID_MobileNO,
                    Id = Encoding.UTF8.GetBytes(model.suid)
                };

                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = false,
                    UserVerification = UserVerificationRequirement.Preferred
                };

                var Extention = new AuthenticationExtensionsClientInputs()
                {
                    Extensions = true,
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true,
                    BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                    {
                        FAR = float.MaxValue,
                        FRR = float.MaxValue
                    }
                };

                if (Configuration.GetValue<bool>("Fido2CrossPlatform"))
                {
                    authenticatorSelection.AuthenticatorAttachment =
                        AuthenticatorAttachment.CrossPlatform;
                }

                var options = _fido2.RequestNewCredential(user, new List<PublicKeyCredentialDescriptor>(), authenticatorSelection, AttestationConveyancePreference.Direct, Extention);

                // Remove all and keep ES256&RS256 Algorithm
                options.PubKeyCredParams.RemoveAll(x =>
                (x.Alg != COSE.Algorithm.ES256 && x.Alg != COSE.Algorithm.RS256));

                options.Timeout = 300000;

                TempData["fido2.attestationOptions"] = options.ToJson().ToString();
                //HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

                return Json(new { Status = "Success", option = options.ToJson().ToString() });
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail " + e.Message);
                return Json(new AssertionOptions { Status = "error", ErrorMessage = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveCredentials([FromBody] RegistrationDataViewModel Model)
        {
            try
            {
                if (string.IsNullOrEmpty(Model.attestationResponse) && string.IsNullOrEmpty(Model.user))
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail Getting assertion and username value null");

                    throw new Exception("Input Value Getting Null Something went wrong!");
                }
                var jsonOptions = TempData["fido2.attestationOptions"].ToString();
                var options = CredentialCreateOptions.FromJson(jsonOptions);

                var attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(Model.attestationResponse);
                var User = JsonConvert.DeserializeObject<UserTable>(Model.user);

                var fidoCredentials = await _fido2.MakeNewCredentialAsync(attestationResponse, options, IsCredentialUnique);
                if (fidoCredentials.Status == "ok")
                {
                    var storedCredential = new StoredCredential
                    {
                        Descriptor = new PublicKeyCredentialDescriptor(fidoCredentials.Result.CredentialId),
                        PublicKey = fidoCredentials.Result.PublicKey,
                        UserHandle = fidoCredentials.Result.User.Id,
                        SignatureCounter = fidoCredentials.Result.Counter,
                        CredType = fidoCredentials.Result.CredType,
                        RegDate = DateTime.Now,
                        AaGuid = fidoCredentials.Result.Aaguid
                    };

                    var responce = await _userService.RegisterUserFido2DeviceAsync(UUID, JsonConvert.SerializeObject(storedCredential), null);
                    if (responce == null || !responce.Success)
                    {
                        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration Intiate Fido2 DeviceAsync fail " + (responce == null ? "Getting response value null" : responce.Message));
                        return Json(new Fido2.CredentialMakeResult { Status = "error", ErrorMessage = (responce == null ? "Something went wrong! please Try again later" : responce.Message) });

                    }
                }
                return Json(fidoCredentials);

            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail " + e.Message);
                return Json(new Fido2.CredentialMakeResult { Status = "error", ErrorMessage = e.Message });
            }
        }

        private Task<bool> IsCredentialUnique(IsCredentialIdUniqueToUserParams userParams)
        {
            return Task.FromResult(true);
        }



        [Authorize]
        [HttpGet]
        public IActionResult SetSecurityQuestion()
        {
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);

            var model = new SequerityQuestionViewModel()
            {
                UserId = id,
                SecurityQueList1 = getSecurityQuestionList(1),
                SecurityQueList2 = getSecurityQuestionList(2),
                SecurityQueList3 = getSecurityQuestionList(3)
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SetSecurityQuestion(SequerityQuestionViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                    viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                    viewModel.SecurityQueList3 = getSecurityQuestionList(3);
                    return View("SetSecurityQuestion", viewModel);
                }

                // ---------- Question 1 ----------
                var QueAns1 = new UserSecurityQue
                {
                    UserId = viewModel.UserId,
                    Question = viewModel.Question1,
                    Answer = viewModel.Answer1,
                    CreatedBy = UUID,
                    UpdatedBy = UUID
                };

                var response = await _userConsoleService.CreateUserSecurityQnsAns(QueAns1);

                if (response == null || !response.Success)
                {
                    viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                    viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                    viewModel.SecurityQueList3 = getSecurityQuestionList(3);

                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile,
                        "Set Security Question", LogMessageType.FAILURE.ToString(),
                        "Fail to create user security question");

                    Alert alert = new Alert
                    {
                        Message = (response == null ? "Internal error please contact admin" : response.Message)
                    };

                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("SetSecurityQuestion", viewModel);
                }

                // ---------- Question 2 ----------
                var QueAns2 = new UserSecurityQue
                {
                    UserId = viewModel.UserId,
                    Question = viewModel.Question2,
                    Answer = viewModel.Answer2,
                    CreatedBy = UUID,
                    UpdatedBy = UUID
                };

                var response1 = await _userConsoleService.CreateUserSecurityQnsAns(QueAns2);

                if (response1 == null || !response1.Success)
                {
                    viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                    viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                    viewModel.SecurityQueList3 = getSecurityQuestionList(3);

                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile,
                        "Set Security Question", LogMessageType.FAILURE.ToString(),
                        "Fail to create user security question");

                    Alert alert = new Alert
                    {
                        Message = (response1 == null ? "Internal error please contact admin" : response1.Message)
                    };

                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("SetSecurityQuestion", viewModel);
                }

                // ---------- Question 3 ----------
                var QueAns3 = new UserSecurityQue
                {
                    UserId = viewModel.UserId,
                    Question = viewModel.Question3,
                    Answer = viewModel.Answer3,
                    CreatedBy = UUID,
                    UpdatedBy = UUID
                };

                var response2 = await _userConsoleService.CreateUserSecurityQnsAns(QueAns3);

                if (response2 == null || !response2.Success)
                {
                    viewModel.SecurityQueList1 = getSecurityQuestionList(1);
                    viewModel.SecurityQueList2 = getSecurityQuestionList(2);
                    viewModel.SecurityQueList3 = getSecurityQuestionList(3);

                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile,
                        "Set Security Question", LogMessageType.FAILURE.ToString(),
                        "Fail to create user security question");

                    Alert alert = new Alert
                    {
                        Message = (response2 == null ? "Internal error please contact admin" : response2.Message)
                    };

                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("SetSecurityQuestion", viewModel);
                }

                // ---------- Final Success Flow ----------
                var UserStatus = await _userService.GetUserStatusAsync(viewModel.UserId);

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile,
                    "Set Security Question", LogMessageType.SUCCESS.ToString(),
                    "Create user security question success");

                if (UserStatus == "SET_FIDO2")
                {
                    Alert alert = new Alert
                    {
                        IsSuccess = true,
                        Message = "Security Question Create Successfully"
                    };

                    TempData["Alert"] = JsonConvert.SerializeObject(alert);

                    return RedirectToAction("RegisterFido2", "Registration", new { id = viewModel.UserId });
                }
                else
                {
                    Alert alert = new Alert
                    {
                        IsSuccess = true,
                        Message = "Your Password and Security question set successfully"
                    };

                    TempData["Alert"] = JsonConvert.SerializeObject(alert);

                    return RedirectToAction("AccountActivateSuccess");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("SetSecurityQuestion post Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ValidateSecurityQuestion(string id, int type)
        {
            try
            {
                _logger.LogInformation("--> ValidateSecurityQuestion get");
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogInformation("ValidateSecurityQuestion get : id value getting null");
                    ViewBag.error = "Invalid Request";
                    ViewBag.error_description = "Input value getting null";
                    return View("Errorp");
                }
                UserTable Response;
                switch (type)
                {
                    case 1:
                        {
                            Response = await _userService.GetUserAsyncByPhone(id);
                            break;
                        }
                    case 2:
                        {
                            Response = await _userService.GetUserAsyncByEmail(id);
                            break;
                        }
                    default:
                        {
                            _logger.LogInformation("ValidateSecurityQuestion get : User Input Type Not Match {0}", type);
                            ViewBag.error = "Invalid Request";
                            ViewBag.error_description = "Invalid User Input Type";
                            return View("Errorp");
                        }
                }

                if (Response == null)
                {
                    _logger.LogInformation("ValidateSecurityQuestion get : getting user details response null");
                    ViewBag.error = "Internal Error";
                    ViewBag.error_description = "Something went wrong!";
                    return View("Errorp");
                }

                var UserStatus = await _userService.GetUserStatusAsync(Response.Id);
                if (UserStatus == "NEW" || UserStatus == "CHANGE_PASSWORD")
                {
                    _logger.LogInformation("ValidateSecurityQuestion get : User status is new fail to get security question details");
                    ViewBag.error = "Invalid Request";
                    ViewBag.error_description = "User security question not configured.";
                    return View("Errorp");
                }

                var UserSecQues = await _resetPasswordService.GetUserSecurityQuestions(Response.Id);
                if (UserSecQues == null || !UserSecQues.Success)
                {
                    _logger.LogInformation("ValidateSecurityQuestion get :" + (UserSecQues != null ? UserSecQues.Message : " getting user sequrity question details response null"));
                    ViewBag.error = (UserSecQues != null ? UserSecQues.Message : "");
                    ViewBag.error_description = (UserSecQues != null ? UserSecQues.Message : "Something went wrong!");
                    return View("Errorp");
                }

                var model = new SequerityQuestionViewModel()
                {
                    Que1Id = UserSecQues.Result[0].Id,
                    Que2Id = UserSecQues.Result[1].Id,
                    Que3Id = UserSecQues.Result[2].Id,

                    UserId = Response.Id,
                    Uuid = Response.Uuid,
                    Username = Response.FullName,

                    Question1 = UserSecQues.Result[0].Question,
                    Question2 = UserSecQues.Result[1].Question,
                    Question3 = UserSecQues.Result[2].Question,
                };

                _logger.LogInformation("<-- ValidateSecurityQuestion get");
                return View(model);
            }
            catch (Exception e)
            {
                _logger.LogInformation("ValidateSecurityQuestion get Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateSecurityQuestion(SequerityQuestionViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("--> ValidateSecurityQuestion post");
                if (!ModelState.IsValid)
                {
                    return View("ValidateSecurityQuestion", viewModel);
                }

                ValidateUserSecQueRequest data = new ValidateUserSecQueRequest();
                List<SecQuestionsAnswers> secQueList = new List<SecQuestionsAnswers>()
{
    new SecQuestionsAnswers
    {
        secQue = viewModel.Question1,
        answer = viewModel.Answer1
    },
    new SecQuestionsAnswers
    {
        secQue = viewModel.Question2,
        answer = viewModel.Answer2
    },
    new SecQuestionsAnswers
    {
        secQue = viewModel.Question3,
        answer = viewModel.Answer3
    }
};

                data.uuid = viewModel.Uuid;
                data.secQueAns = secQueList;

                var Response = await _resetPasswordService.ValidateUserSecurityQuestions(data);
                if (Response == null || !Response.Success)
                {
                    _logger.LogInformation("ValidateSecurityQuestion post : Security Question Validate Failed");
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Validate User Security Questions", LogMessageType.FAILURE.ToString(), "Fail to Validate User Security Questions of user " + viewModel.Username);
                    Alert alert = new Alert { Message = (Response == null ? "Internal error please contact to admin" : Response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("ValidateSecurityQuestion", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Validate User Security Questions", LogMessageType.SUCCESS.ToString(), "Validate User Security Questions of user " + viewModel.Username + " success");
                    Alert alert = new Alert { IsSuccess = true, Message = "Security Question Validate Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    _logger.LogInformation("<-- ValidateSecurityQuestion post");
                    TempData["TempSession"] = Response.TemporarySession;
                    return RedirectToAction("SetPassword", new { id = viewModel.UserId, Uuid = viewModel.Uuid });
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("ValidateSecurityQuestion post Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }
        }


        [HttpGet]
        public IActionResult SetPassword(int id, string Uuid)
        {
            try
            {
                _logger.LogInformation("--> SetPassword get");
                if (!TempData.ContainsKey("TempSession"))
                {
                    _logger.LogInformation("SetPassword get : TempSession value getting null");
                    ViewBag.error = "Internal Error";
                    ViewBag.error_description = "Something went wrong!";
                    return View("Errorp");
                }
                var model = new SetPasswordViewModel()
                {
                    Id = id,
                    Uuid = Uuid,
                    TempSession = TempData["TempSession"].ToString()
                };
                TempData.Keep("TempSession");
                _logger.LogInformation("<-- SetPassword get");
                return View(model);
            }
            catch (Exception e)
            {
                _logger.LogInformation("SetPassword get Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("--> SetPassword post");
                if (!ModelState.IsValid)
                {
                    return View("SetPassword", viewModel);
                }

                var passwordData = new ResetPasswordRequest()
                {
                    userId = viewModel.Id,
                    uuid = viewModel.Uuid,
                    newPassword = viewModel.NewPassword,
                    TemporarySession = viewModel.TempSession
                };

                var response = await _resetPasswordService.ResetPassword(passwordData);
                if (response == null || !response.Success)
                {
                    _logger.LogInformation("--> SetPassword post : reset user password fail");
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Reset Password", LogMessageType.FAILURE.ToString(), "Fail to Reset User Password ");
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("SetPassword", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Profile, "Reset Password", LogMessageType.SUCCESS.ToString(), "Reset User Password success");
                    Alert alert = new Alert { IsSuccess = true, Message = "Reset User Password Successfully" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    TempData["isForgotPassword"] = true;
                    _logger.LogInformation("<-- SetPassword post");
                    return RedirectToAction("AccountActivateSuccess");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("SetPassword post Exception : {0}", e.Message);
                ViewBag.error = "Internal Error";
                ViewBag.error_description = "Something went wrong, Please try again later";
                return View("Errorp");
            }
        }

        [HttpGet]
        public IActionResult AccountActivateSuccess()
        {
            if (TempData.ContainsKey("isForgotPassword"))
            {
                ViewBag.isForgotPassword = TempData["isForgotPassword"].ToString();
            }
            if (TempData.ContainsKey("Alert"))
            {
                var alert = TempData["Alert"].ToString();
                Alert alertData = JsonConvert.DeserializeObject<Alert>(alert);
                ViewBag.Message = alertData.Message;
            }
            return View("AccountActivateSuccess");
        }

        public List<SelectListItem> getSecurityQuestionList(int listNo)
        {
            var sequrity = new List<SelectListItem>();
            var names = Enum.GetNames(typeof(Core.Enums.SecurityQuesEnum));
            foreach (var name in names)
            {
                var field = typeof(Core.Enums.SecurityQuesEnum).GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                    if (listNo == 1)
                    {
                        var data = new SelectListItem();
                        data.Text = fd.Description;
                        data.Value = fd.Description;
                        if (name == "favouritecar")
                        {
                            data.Selected = true;
                        }
                        sequrity.Add(data);
                    }
                    else
                    {
                        var data = new SelectListItem();
                        data.Text = fd.Description;
                        data.Value = fd.Description;
                        if (name == "favoritecity")
                        {
                            data.Selected = true;
                        }
                        sequrity.Add(data);
                    }
                }
            }
            return sequrity;
        }

        public async Task<List<SelectListItem>> GetDomainList()
        {

            var AdminPortalconfigInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (AdminPortalconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "get AdminPortal_SSOConfig details", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal_SSOConfig configuration");
                return null;
            }

            List<SelectListItem> list = new List<SelectListItem>();

            foreach (string i in AdminPortalconfigInDB.allowed_domain_users)
            {
                list.Add(new SelectListItem { Value = i, Text = i });
            }

            return list;
        }

    }
}
