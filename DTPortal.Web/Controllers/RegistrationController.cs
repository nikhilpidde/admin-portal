using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel.Registration;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeterO.Cbor;
using System.IO;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace DTPortal.Web.Controllers
{
    public class RegistrationController : BaseController
    {
        private readonly IFido2 fido2;
        private readonly IUserManagementService _userService;
        private readonly IUserConsoleService _userConsoleService;
        public IConfiguration Configuration { get; }
        public RegistrationController(IUserConsoleService userConsoleService, ILogClient logClient, IConfiguration configuration, IFido2 fido2, IUserManagementService userService) : base(logClient)
        {
            _userService = userService;
            _userConsoleService = userConsoleService;
            this.fido2 = fido2;
            Configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int Request_type, string Request_code = null, string Request_for = null)
        {
            if (string.IsNullOrEmpty(Request_code))
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Registration fail Getting request code value null");
                ViewBag.error = "Invalid request";
                return View("Errorp");
            }
            UserResponse Response;

            if (Request_type == 0)
                Response = await _userService.VerifyDeviceRegistrationToken(Request_code);
            else
                Response = await _userService.VerifyDeviceRegistrationToken(Request_code);

            if (Response == null || !Response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Registration fail " + (Response == null ? "Getting response value null" : Response.Message));
                ViewBag.error = "Invalid request";
                return View("Errorp");
            }

            var model = new RegistrationViewModel
            {
                userDetails = JsonConvert.SerializeObject(Response.Result),
                suid = Response.Result.Uuid,
                FullName = Response.Result.FullName,
                MailID_MobileNO = Response.Result.MailId + "/" + Response.Result.MobileNo
            };

            TempData["RegistrationType"] = Request_type.ToString();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterFido2(int id)
        {
            if (id == 0)
            {
                id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);
            }
            var userInDb = await _userConsoleService.GetUserAsync(id);
            if (userInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Get profile", LogMessageType.FAILURE.ToString(), "Fail to get User profile");
                return NotFound();
            }

            userInDb.Role = null;
            userInDb.MakerCheckers = null;
            userInDb.PasswordPolicyCreatedByNavigations = null;
            userInDb.PasswordPolicyUpdatedByNavigations = null;
            userInDb.Role = null;
            //userInDb.SecurityQueCreatedByNavigations = null;
            //userInDb.SecurityQueUpdatedByNavigations = null;
            
            
            //userInDb.UserAuthData = null;

            var model = new RegistrationViewModel
            {
                userDetails = JsonConvert.SerializeObject(userInDb),
                suid = userInDb.Uuid,
                FullName = userInDb.FullName,
                MailID_MobileNO = userInDb.MailId + "/" + userInDb.MobileNo
            };

            TempData["RegistrationType"] = "0";
            TempData["FirstTimeLogin"] = true;
            return View("Index",model);
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

                var userVerification = "preferred";
                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = false,
                    UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
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

                var options = fido2.RequestNewCredential(user, new List<PublicKeyCredentialDescriptor>(), authenticatorSelection, AttestationConveyancePreference.Direct, Extention);

                // Remove all and keep ES256&RS256 Algorithm
                options.PubKeyCredParams.RemoveAll(x =>
                (x.Alg != COSE.Algorithm.ES256 && x.Alg != COSE.Algorithm.RS256));

                options.Timeout = 300000;

                TempData["fido2.attestationOptions"] = options.ToJson().ToString();
                //HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

                return Json(new { option = options.ToJson().ToString() });
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail " + e.Message);
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
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail Getting assertion and username value null");
                    ViewBag.error = "Internal error";
                    ViewBag.error_description = "Something went wrong..!";
                    return View("Errorp");
                }
                var RegistrationType = int.Parse(TempData["RegistrationType"].ToString());
                TempData.Keep("RegistrationType");
                var jsonOptions = TempData["fido2.attestationOptions"].ToString();
                var options = CredentialCreateOptions.FromJson(jsonOptions);

                var attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(Model.attestationResponse);
                var User = JsonConvert.DeserializeObject<UserTable>(Model.user);

                var fidoCredentials = await fido2.MakeNewCredentialAsync(attestationResponse, options, IsCredentialUnique);
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

                    if (RegistrationType == 0)
                    {
                        var Response = await _userService.SaveUserAsync(User, JsonConvert.SerializeObject(storedCredential));
                        if (Response == null || !Response.Success)
                        {
                            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail " + (Response == null ? "Getting response value null" : Response.Message));
                            return Json(new Fido2.CredentialMakeResult { Status = "error", ErrorMessage = (Response == null ? "Something went wrong! please contact admin" : Response.Message) });
                        }
                    }
                    else
                    {
                        var Response = await _userService.RegisterTempDeviceAsync(User.Uuid, JsonConvert.SerializeObject(storedCredential));
                        if (Response == null || !Response.Success)
                        {
                            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 temporary Authdata", LogMessageType.FAILURE.ToString(), "Temporary fido device Registration fail " + (Response == null ? "Getting response value null" : Response.Message));
                            return Json(new Fido2.CredentialMakeResult { Status = "error", ErrorMessage = (Response == null ? "Something went wrong! please contact admin" : Response.Message) });
                        }
                    }
                }
                return Json(fidoCredentials);

            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.UserFidoDeviceRegitration, "Register User fido2 Authdata", LogMessageType.FAILURE.ToString(), "Fido device Registration fail " + e.Message);
                return Json(new Fido2.CredentialMakeResult { Status = "error", ErrorMessage = e.Message });
            }
        }

        private Task<bool> IsCredentialUnique(IsCredentialIdUniqueToUserParams userParams)
        {
            return Task.FromResult(true);
        }

        [HttpGet]
        public IActionResult Error(string error = null, string error_description = null)
        {
            ViewBag.error = (string.IsNullOrEmpty(error)) ? "Internal error" : error;
            ViewBag.error_description = (string.IsNullOrEmpty(error_description)) ? "Internal server error" : error_description;
            return View("Errorp");
        }

        [HttpGet]
        public IActionResult RegistrationSuccess()
        {
            if (TempData.ContainsKey("FirstTimeLogin")){
                var RegistrationType = TempData["FirstTimeLogin"] as bool? ?? true;
                ViewBag.isFirstTimeLogin = RegistrationType;
            }
            return View();
           
        }
    }
}
