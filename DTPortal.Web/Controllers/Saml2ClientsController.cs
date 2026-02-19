using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Web.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Saml2Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class Saml2ClientsController : BaseController
    {
        private readonly IClientService _clientsSaml2Service;
        private readonly IConfigurationService _configurationService;
        private readonly ISessionService _sessionService;
        private readonly IAuthenticationService _authenticationService;
        public IConfiguration Configuration { get; }
        public Saml2ClientsController(IAuthenticationService authenticationService, ISessionService sessionService, IConfigurationService configurationService,IConfiguration configuration, ILogClient logClient, IClientService clientsSaml2Service) : base(logClient)
        {
            _clientsSaml2Service = clientsSaml2Service;
            _configurationService = configurationService;
            Configuration = configuration;
            _sessionService = sessionService;
            _authenticationService = authenticationService;
        }


        /*function get certificate in string format form IFormFile object*/
        string getCertificate(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }

            return result.ToString().Replace("\r","");
        }

        /*function for create json url array eith binding method*/
        JArray getJsonArray(string url)
        {
            JArray array = new JArray();
            array.Add(url);
            return array;
        }

        /*function for create client id*/
        string get_unique_string(int string_length)
        {
            const string src = "ABCDEFGHIJKLMNOPQRSTUVWXYS0123456789";
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < string_length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }


        /*function for new sp json config*/
        string getNewSpConfig(Saml2ClientsNewViewModel viewModel)
        {
                dynamic config = new JObject();

                dynamic signatureConfig = new JObject();
                signatureConfig.prefix = "ds";
                signatureConfig.location = new JObject();

                config.wantMessageSigned = false;// (viewModel.ResponceSigned == "true" && viewModel.assertionEncryption == "true" ? true : false); 
                config.signatureConfig = signatureConfig;
                config.messageSigningOrder = "sign-then-encrypt";
                config.nameIDFormat = getJsonArray(viewModel.nameIDFormat);
                config.authnRequestsSigned = (viewModel.RequestSigned == "true" ? true : false);
                config.requestSignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                config.dataEncryptionAlgorithm = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
                config.keyEncryptionAlgorithm = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p";
                config.wantLogoutRequestSigned = (viewModel.RequestSigned == "true" ? true : false);
                config.wantLogoutResponseSigned = (viewModel.ResponceSigned == "true" ? true : false);
                config.isAssertionEncrypted = false;// (viewModel.assertionEncryption == "true" ? true : false);
                config.wantAssertionsSigned = (viewModel.assertionSignature == "true" ? true : false);
                config.relayState = "";

            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<Saml2ClientsListViewModel>();
            var Saml2ClientsList = await _clientsSaml2Service.ListSaml2ClientAsync();
            if (Saml2ClientsList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Get all saml2 client List", LogMessageType.FAILURE.ToString(), "Fail to get SAML2 clients list");
                return NotFound();
            }
            else
            {
                foreach (var item in Saml2ClientsList)
                {
                    viewModel.Add(new Saml2ClientsListViewModel
                    {
                        Id = item.Id,
                        ApplicationName = item.ApplicationName,
                        ApplicationType = item.ApplicationType,
                        ApplicationUri = item.ApplicationUrl,
                        ClientID = item.ClientId,
                        State = item.Status
                    });
                }
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Get all saml2 client List", LogMessageType.SUCCESS.ToString(), "Get SAML2 clients list success");
            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult New()
        {
            var viewModel = new Saml2ClientsNewViewModel();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var samlclient = await _clientsSaml2Service.GetClientAsync(id);
            if (samlclient == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Edit saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to get SAML2 client info of id(" + id + ")");
                return NotFound();
            }
            

            var viewModel = new Saml2ClientsEditViewModel();
            viewModel.id = samlclient.Id;
            viewModel.ApplicationType = samlclient.ApplicationType;
            viewModel.ApplicationName = samlclient.ApplicationName;
            viewModel.ApplicationUrl = samlclient.ApplicationUrl;
            viewModel.ClientId = samlclient.ClientId;
            viewModel.assertionConsumerServiceUrl = samlclient.RedirectUri;
            viewModel.singleLogoutService = samlclient.LogoutUri;

            ClientsSaml2 Saml2Cilent = samlclient.ClientsSaml2s.FirstOrDefault();
            /*parse spconfig and get url*/
            dynamic spconfig = JsonConvert.DeserializeObject(Saml2Cilent.Config);

            viewModel.entityID = Saml2Cilent.EntityId;
            var nameIdFormat = spconfig["nameIDFormat"].ToString();
            var nameID = JsonConvert.DeserializeObject(nameIdFormat);
            viewModel.nameIDFormat = nameID[0].ToString();
            viewModel.requestSignatureAlgorithm = spconfig["requestSignatureAlgorithm"].ToString();
            viewModel.dataEncryptionAlgorithm = spconfig["dataEncryptionAlgorithm"].ToString();
            viewModel.keyEncryptionAlgorithm = spconfig["keyEncryptionAlgorithm"].ToString();

            viewModel.RequestSigned = spconfig["authnRequestsSigned"].ToString();
            viewModel.assertionSignature = spconfig["wantAssertionsSigned"].ToString();
            viewModel.assertionEncryption = spconfig["isAssertionEncrypted"].ToString();
            viewModel.ResponceSigned = spconfig["wantLogoutResponseSigned"].ToString();


            var IDPconfigInDB = await _configurationService.GetConfigurationAsync<idp_configuration>("IDP_Configuration");
            if (IDPconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Edit saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to get IDP saml2 configuration");
                return NotFound();
            }
            dynamic saml2 = IDPconfigInDB.saml2;
            dynamic comman = IDPconfigInDB.common;

            dynamic signinUrl = JsonConvert.DeserializeObject(saml2["singleSignOnService"].ToString());
            dynamic signoutUrl = JsonConvert.DeserializeObject(saml2["singleLogoutService"].ToString());

            viewModel.entityIDIDP = saml2["entityID"].ToString().Replace("<client_ID>", samlclient.ClientId);
            viewModel.singleSignOnServiceIDP = signinUrl[0]["Location"].ToString().Replace("<client_ID>", samlclient.ClientId);
            viewModel.singleLogoutServiceIDP = signoutUrl[0]["Location"].ToString().Replace("<client_ID>", samlclient.ClientId);
            viewModel.signingCertIDP = comman["signCertificate"].ToString();


            viewModel.State = samlclient.Status;
        

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Edit saml2 client details", LogMessageType.FAILURE.ToString(), "Get SAML2 client info of id(" + id + ") success");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Saml2ClientsNewViewModel viewModel)
        {
            
            if (!ModelState.IsValid)
            {
                return View("New", viewModel);
            }
            if (viewModel.RequestSigned == "true" || viewModel.assertionSignature == "true")
            {
                if (viewModel.signingCert == null)
                {
                    ModelState.AddModelError("signingCert", "required signing certificate");
                    return View("New", viewModel);
                }
                if (viewModel.signingCert.ContentType != "application/x-x509-ca-cert")
                {
                    ModelState.AddModelError("signingCert", "invalid signing certificate");
                    return View("New", viewModel);
                }
            }
            //if (viewModel.assertionEncryption == "true")
            //{
            //    if (viewModel.encryptCert == null)
            //    {
            //        ModelState.AddModelError("encryptCert", "required encryptCert certificate");
            //        return View("New", viewModel);
            //    }
            //    if (viewModel.encryptCert.ContentType != "application/x-x509-ca-cert")
            //    {
            //        ModelState.AddModelError("encryptCert", "invalid encryptCert certificate");
            //        return View("New", viewModel);
            //    }
            //}

            var client = new Client()
            {
                ClientId = get_unique_string(48),
                ClientSecret = get_unique_string(64),
                ApplicationName = viewModel.ApplicationName,
                ApplicationType = viewModel.ApplicationType,
                ApplicationUrl = viewModel.ApplicationUrl,
                RedirectUri = viewModel.assertionConsumerServiceUrl,
                GrantTypes = "authorization_code",
                Scopes = "urn:idp:digitalid:profile email urn:idp:digitalid:verifytoken openid profile",
                LogoutUri = viewModel.singleLogoutService,
                //WithPkce = viewModel.WithPkce,
                ResponseTypes = "code",
                Type = "SAML2",
                PublicKeyCert = (viewModel.signingCert != null && viewModel.RequestSigned == "true" || viewModel.assertionSignature == "true" || viewModel.ResponceSigned == "true" ? getCertificate(viewModel.signingCert) : ""),
                EncryptionCert = string.Empty,// (viewModel.encryptCert != null && viewModel.assertionEncryption == "true" ? getCertificate(viewModel.encryptCert) : ""),
                CreatedBy = UUID,
                UpdatedBy = UUID
            };

            var samlclient = new ClientsSaml2()
            {
                EntityId = viewModel.entityID,
                AssertionConsumerServiceBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST",
                SingleLogoutServiceBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST,urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect",
                Config = getNewSpConfig(viewModel)
            };

            client.ClientsSaml2s.Add(samlclient);

            var response = await _clientsSaml2Service.CreateClientAsync(client);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Create saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to create SAML2 client");
                Alert alert = new Alert {  Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Create saml2 client details", LogMessageType.SUCCESS.ToString(), "Create SAML2 client success");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(Saml2ClientsEditViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View("Edit", viewModel);
            }

            var dbsamlclient = await _clientsSaml2Service.GetClientAsync(viewModel.id);
            if (dbsamlclient == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Update saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to get SAML2 client info of id(" + viewModel.id + ") in update" );
                return NotFound();
            }

            if (viewModel.RequestSigned == "true" || viewModel.assertionSignature == "true")
            {
                if (viewModel.signingCert == null && String.IsNullOrEmpty(dbsamlclient.PublicKeyCert) )
                {
                    ModelState.AddModelError("signingCert", "required signing certificate");
                    return View("Edit", viewModel);
                }
            }

            //if (viewModel.assertionEncryption == "true")
            //{
            //    if (viewModel.encryptCert == null &&  String.IsNullOrEmpty(dbsamlclient.EncryptionCert))
            //    {
            //        ModelState.AddModelError("encryptCert", "required encryption certificate");
            //        return View("Edit", viewModel);
            //    }
            //}

            if (viewModel.signingCert != null)
            {
                if (viewModel.signingCert.ContentType != "application/x-x509-ca-cert")
                {
                    ModelState.AddModelError("signingCert", "invalid signing certificate");
                    return View("Edit", viewModel);
                }
            }

            //if (viewModel.encryptCert != null)
            //{
            //    if (viewModel.encryptCert.ContentType != "application/x-x509-ca-cert")
            //    {
            //        ModelState.AddModelError("encryptCert", "invalid encryption certificate");
            //        return View("Edit", viewModel);
            //    }
            //}

            ClientsSaml2 Saml2Cilent = dbsamlclient.ClientsSaml2s.FirstOrDefault();
            /*parse spconfig and get url*/
            JObject spconfig = JObject.Parse(Saml2Cilent.Config);

            spconfig["wantMessageSigned"] = false;// (viewModel.ResponceSigned == "true" && viewModel.assertionEncryption == "true" ? true : false);
            spconfig["nameIDFormat"] = getJsonArray(viewModel.nameIDFormat);
            spconfig["authnRequestsSigned"] = (viewModel.RequestSigned == "true" ? true : false);
            spconfig["wantLogoutRequestSigned"] = (viewModel.RequestSigned == "true" ? true : false);
            spconfig["wantLogoutResponseSigned"] = (viewModel.ResponceSigned == "true" ? true : false);
            spconfig["isAssertionEncrypted"] = false;// (viewModel.assertionEncryption == "true" ? true : false);
            spconfig["wantAssertionsSigned"] = (viewModel.assertionSignature == "true" ? true : false);

            Saml2Cilent.EntityId = viewModel.entityID;
            Saml2Cilent.Config = JsonConvert.SerializeObject(spconfig, Formatting.Indented);

            dbsamlclient.ApplicationName = viewModel.ApplicationName;
            dbsamlclient.ApplicationType = viewModel.ApplicationType;
            dbsamlclient.ApplicationUrl = viewModel.ApplicationUrl;
            dbsamlclient.RedirectUri = viewModel.assertionConsumerServiceUrl;
            dbsamlclient.LogoutUri = viewModel.singleLogoutService;
            dbsamlclient.PublicKeyCert =(viewModel.RequestSigned=="true" || viewModel.assertionSignature=="true" || viewModel.ResponceSigned == "true" ? (viewModel.signingCert != null ? getCertificate(viewModel.signingCert) : dbsamlclient.PublicKeyCert) :"");
            dbsamlclient.EncryptionCert = string.Empty;// (viewModel.assertionEncryption == "true" ? (viewModel.encryptCert != null ? getCertificate(viewModel.encryptCert) : dbsamlclient.EncryptionCert) :"");
            dbsamlclient.ClientsSaml2s.Clear();
            //dbsamlclient.ClientsSaml2s.Add(Saml2Cilent);
            dbsamlclient.UpdatedBy = UUID;

            var response = await _clientsSaml2Service.UpdateClientAsync(dbsamlclient, Saml2Cilent);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Update saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to update SAML2 client info of id(" + viewModel.id + ") in update");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Update saml2 client details", LogMessageType.SUCCESS.ToString(), "Update SAML2 client info of id(" + viewModel.id + ") in update success");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _clientsSaml2Service.DeleteClientAsync(id, UUID);
                if (response == null || !response.Success)
                {
                    Alert alert = new Alert {  Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Delete saml2 client details", LogMessageType.FAILURE.ToString(), "Fail to delete SAML2 client info of id(" + id + ")");
                    return null;
                }
                else
                {
                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Delete saml2 client details", LogMessageType.SUCCESS.ToString(), "Delete SAML2 client info of id(" + id + ") success");
                    return new JsonResult(true);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Session(string id, string name)
        {
            var sessionInDb = await _sessionService.GetAllClientSessions(id);
            if (sessionInDb==null || sessionInDb.Success == false)
            {
                Alert alert = new Alert { Message = (sessionInDb == null ? "Internal error please contact to admin" : sessionInDb.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Session SAML2 client", LogMessageType.FAILURE.ToString(), "Fail to get client session in OAuth2/OpenID clients");
                var model = new Saml2ClientsSessionListViewModel();
                model.ClientName = name;
                return View(model); 
            }
            else
            {
                var model = new Saml2ClientsSessionListViewModel
                {
                    ClientName = name,
                    session = sessionInDb.GlobalSessions
                };

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Session SAML2 client", LogMessageType.SUCCESS.ToString(), "Get client session in OAuth2/OpenID clients success");
                return View(model);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Logout(string id)
        //{
        //    var data = new LogoutUserRequest
        //    {
        //        GlobalSession = id
        //    };

        //    var response = await _authenticationService.LogoutUser(data);
        //    if (response == null)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Logout SAML2 client", LogMessageType.FAILURE.ToString(), "Fail to logout all session in SAML2 clients getting response value null");
        //        return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
        //    }
        //    if (response.Success)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Logout SAML2 client", LogMessageType.FAILURE.ToString(), "Fail to logout session of id(" + id + ") in OAuth2/OpenID clients");
        //        return new JsonResult(response);
        //    }

        //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "Logout SAML2 client", LogMessageType.SUCCESS.ToString(), "Logout session of id(" + id + ") in OAuth2/OpenID clients success");
        //    return new JsonResult(response);


        //}

        //[HttpPost]
        //public async Task<IActionResult> LogoutAll(List<string> id)
        //{
        //    foreach (var session in id)
        //    {
        //        var data = new LogoutUserRequest
        //        {
        //            GlobalSession = session
        //        };

        //        var response = await _authenticationService.LogoutUser(data);
        //        if (response == null)
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "LogoutAll SAML2 client", LogMessageType.FAILURE.ToString(), "Fail to logout all session in SAML2 clients getting response value null");
        //            return StatusCode(500, "somethig went wrong! please contact to admin or try again later");
        //        }
        //        if (response.Success)
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "LogoutAll SAML2 client", LogMessageType.FAILURE.ToString(), "Fail to logout all session in OAuth2/OpenID clients");
        //            return new JsonResult(response);
        //        }

        //    }

        //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Saml2, "LogoutAll SAML2 client", LogMessageType.SUCCESS.ToString(), "Logout all session in OAuth2/OpenID clients success");
        //    return new JsonResult(new { success = true });
        //}

    }
}
