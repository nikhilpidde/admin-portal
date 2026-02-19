using DTPortal.Core.Domain.Services;
using DTPortal.Web.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using System.Security.Claims;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.Controllers
{
    //[Authorize(Roles = "Application Configuration")]
    //[ServiceFilter(typeof(SessionValidationAttribute))]
    public class ConfigurationController : BaseController
    {
        private readonly IConfigurationService _configurationService;
        private readonly IAuthSchemeSevice _authSchemeSevice;
        public ConfigurationController(ILogClient logClient, IConfigurationService configurationService, IAuthSchemeSevice authSchemeSevice) : base(logClient)
        {
            _configurationService = configurationService;
            _authSchemeSevice = authSchemeSevice;
        }

        public async Task<IActionResult> Index()
        {
            var configInDB = await _configurationService.GetConfigurationAsync<SSOConfig>("SSO_Config");
            if (configInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Get configuration details", LogMessageType.FAILURE.ToString(), "Fail to get SSO Application configuration");
                return NotFound();
            }
            var sso = new SSOConfigurationViewModel()
            {
                SSOTemporarySessionTimeout = configInDB.sso_config.temporary_session_timeout,
                SSOSessionTimeout = configInDB.sso_config.session_timeout,
                SSOIdealTimeout = configInDB.sso_config.ideal_timeout,
                SSOAuthorizationCodeTimeout = configInDB.sso_config.authorization_code_timeout,
                SSOAccessTokenTimeout = configInDB.sso_config.access_token_timeout,
                SSOAccountLockTime = configInDB.sso_config.account_lock_time,
                SSOActiveSessionsPerUser = configInDB.sso_config.active_sessions_per_user,
                SSOWrongPin = configInDB.sso_config.wrong_pin,
                SSOWrongCode = configInDB.sso_config.wrong_code,
                SSODenyCount = configInDB.sso_config.deny_count,
                CentralLogConnection = configInDB.log_config.central_log_config.connection_string,
                CentralLogQueueName = configInDB.log_config.central_log_config.queue_name,
                ServiceLogConnection = configInDB.log_config.service_log_config.connection_string,
                ServiceLogQueueName = configInDB.log_config.service_log_config.queue_name,
                AdminLogConnection = configInDB.log_config.admin_log_config.connection_string,
                AdminLogQueueName = configInDB.log_config.admin_log_config.queue_name,
                PKIServiceBaseAddress = configInDB.pki_service_config.base_address,
                PKIServiceGenerateSignatureUri = configInDB.pki_service_config.generate_signature_uri,
                PKIServiceVerifySignatureUri = configInDB.pki_service_config.verify_signature_uri,
                RAbaseAddress = configInDB.ra_service_config.base_address,
                RAstatusUpdateUri = configInDB.ra_service_config.status_update_uri,
                //RedisServerConnection = configInDB.redis_server_config.connection_string,
                // IDPDatabaseConnection = configInDB.database_config.idp_connection_string,
                // RADatabaseConnection = configInDB.database_config.ra_connection_string
            };

            var AdminPortalconfigInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (AdminPortalconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to get Application configuration in Update configuration");
                return NotFound();
            }

            var adminportalsso = new AdminPortalSSOConfiguratonViewModel()
            {
                AdminPortalSSO_session_timeout = AdminPortalconfigInDB.session_timeout,
                AdminPortalSSO_temporary_session_timeout = AdminPortalconfigInDB.temporary_session_timeout,
                AdminPortalSSO_ideal_timeout = AdminPortalconfigInDB.ideal_timeout,
                AdminPortalSSO_account_lock_time = AdminPortalconfigInDB.account_lock_time,
                AdminPortalSSO_active_sessions_per_user = AdminPortalconfigInDB.active_sessions_per_user,
                AdminPortalSSO_wrong_pin = AdminPortalconfigInDB.wrong_pin,
                AdminPortalSSO_email_domain = String.Join<string>(",", AdminPortalconfigInDB.allowed_domain_users)
            };

            var IDPconfigInDB = await _configurationService.GetConfigurationAsync<idp_configuration>("IDP_Configuration");
            if (IDPconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Get configuration details", LogMessageType.FAILURE.ToString(), "Fail to get IDP Application configuration");
                return NotFound();
            }
            dynamic oauth2 = IDPconfigInDB.openidconnect;
            dynamic saml2 = IDPconfigInDB.saml2;
            dynamic comman = IDPconfigInDB.common;

            var idp = new IDPConfigureationViewModel();
            idp.OAuth2_Issuer = oauth2["issuer"].ToString();
            idp.OAuth2_authorization_endpoint = oauth2["authorization_endpoint"].ToString();
            idp.OAuth2_token_endpoint = oauth2["token_endpoint"].ToString();
            idp.OAuth2_userinfo_endpoint = oauth2["userinfo_endpoint"].ToString();
            idp.OAuth2_introspection_Endpoint = oauth2["introspection_Endpoint"].ToString();
            idp.OAuth2_jwks_uri = oauth2["jwks_uri"].ToString();
            idp.OAuth2_response_types_supported = oauth2["response_types_supported"][0].ToString();
            idp.OAuth2_scopes_supported = JsonConvert.DeserializeObject<List<string>>(oauth2["scopes_supported"].ToString());
            idp.OAuth2_grant_types_supported = JsonConvert.DeserializeObject<List<string>>(oauth2["grant_types_supported"].ToString());
            idp.OAuth2_token_endpoint_auth_methods_supported = JsonConvert.DeserializeObject<List<string>>(oauth2["token_endpoint_auth_methods_supported"].ToString());
            idp.OAuth2_claims_supported = JsonConvert.DeserializeObject<List<string>>(oauth2["claims_supported"].ToString());
            idp.OAuth2_request_parameter_supported = oauth2["request_parameter_supported"].ToString();
            idp.OAuth2_algorithem_supported = oauth2["id_token_signing_alg_values_supported"][0].ToString();
            //idp.SAML2_entityID = saml2["entityID"].ToString();
            //idp.SAML2_singleSignOnService_hidden = saml2["singleSignOnService"].ToString();
            //idp.SAML2_singleLogoutService_hidden = saml2["singleLogoutService"].ToString();

            //dynamic signinUrl = JsonConvert.DeserializeObject(saml2["singleSignOnService"].ToString());
            //dynamic signoutUrl = JsonConvert.DeserializeObject(saml2["singleLogoutService"].ToString());


            //idp.SAML2_singleSignOnService = signinUrl[0]["Location"].ToString();
            //idp.SAML2_singleLogoutService = signoutUrl[0]["Location"].ToString();


            //idp.SAML2_Method_binding_Supported = new List<string>() { "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect" };
            idp.signCert = comman["signCertificate"].ToString();
            idp.enctCert = comman["encryptionCertificate"].ToString();

            var model = new ConfigrationViewModel()
            {
                idp = idp,
                sso = sso,
                adminPortalsso = adminportalsso
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Get configuration details", LogMessageType.SUCCESS.ToString(), "Get Application configuration success");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ConfigrationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", viewModel);
            }
          
            //update sso configuration
            var configInDB = await _configurationService.GetConfigurationAsync<SSOConfig>("SSO_Config");
            if (configInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to get Application configuration in Update configuration");
                return NotFound();
            }

            configInDB.sso_config.temporary_session_timeout = viewModel.sso.SSOTemporarySessionTimeout;
            configInDB.sso_config.ideal_timeout = viewModel.sso.SSOIdealTimeout;
            configInDB.sso_config.session_timeout = viewModel.sso.SSOSessionTimeout;
            configInDB.sso_config.authorization_code_timeout = viewModel.sso.SSOAuthorizationCodeTimeout;
            configInDB.sso_config.access_token_timeout = viewModel.sso.SSOAccessTokenTimeout;
            configInDB.sso_config.account_lock_time = viewModel.sso.SSOAccountLockTime;
            configInDB.sso_config.active_sessions_per_user = 0; // viewModel.SSOActiveSessionsPerUser;
            configInDB.sso_config.wrong_pin = viewModel.sso.SSOWrongPin;
            configInDB.sso_config.wrong_code = viewModel.sso.SSOWrongCode;
            configInDB.sso_config.deny_count = viewModel.sso.SSODenyCount;
            configInDB.log_config.central_log_config.connection_string = viewModel.sso.CentralLogConnection;
            configInDB.log_config.central_log_config.queue_name = viewModel.sso.CentralLogQueueName;
            configInDB.log_config.service_log_config.connection_string = viewModel.sso.ServiceLogConnection;
            configInDB.log_config.service_log_config.queue_name = viewModel.sso.ServiceLogQueueName;
            configInDB.log_config.admin_log_config.connection_string = viewModel.sso.AdminLogConnection;
            configInDB.log_config.admin_log_config.queue_name = viewModel.sso.AdminLogQueueName;
            configInDB.pki_service_config.base_address = viewModel.sso.PKIServiceBaseAddress;
            configInDB.pki_service_config.generate_signature_uri = viewModel.sso.PKIServiceGenerateSignatureUri;
            configInDB.pki_service_config.verify_signature_uri = viewModel.sso.PKIServiceVerifySignatureUri;
            configInDB.ra_service_config.base_address = viewModel.sso.RAbaseAddress;
            configInDB.ra_service_config.status_update_uri = viewModel.sso.RAstatusUpdateUri;
            // configInDB.redis_server_config.connection_string = viewModel.sso.RedisServerConnection;
            // configInDB.database_config.idp_connection_string = string.Empty; // viewModel.IDPDatabaseConnection;
            // configInDB.database_config.ra_connection_string = string.Empty; // viewModel.RADatabaseConnection;

            var response = await _configurationService.SetConfigurationAsync("SSO_Config", configInDB, UUID);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to update SSO Application configuration in Update configuration");
                Alert alert1 = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert1);
                return RedirectToAction("Index");
                //return View("Index", viewModel);
            }


            //update idp oauth configuration
            var IDPconfigInDB = await _configurationService.GetConfigurationAsync<idp_configuration>("IDP_Configuration");
            if (IDPconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to get IDP Application configuration");
                return NotFound();
            }
            dynamic oauth2 = IDPconfigInDB.openidconnect;
            //dynamic saml2 = IDPconfigInDB.saml2;
            dynamic common = IDPconfigInDB.common;

            oauth2["issuer"] = viewModel.idp.OAuth2_Issuer;
            oauth2["authorization_endpoint"] = viewModel.idp.OAuth2_authorization_endpoint;
            oauth2["token_endpoint"] = viewModel.idp.OAuth2_token_endpoint;
            oauth2["userinfo_endpoint"] = viewModel.idp.OAuth2_userinfo_endpoint;
            oauth2["introspection_Endpoint"] = viewModel.idp.OAuth2_introspection_Endpoint;
            oauth2["jwks_uri"] = viewModel.idp.OAuth2_jwks_uri;

            //saml2["entityID"] = viewModel.idp.SAML2_entityID;

            //dynamic signinUrl = JsonConvert.DeserializeObject(viewModel.idp.SAML2_singleLogoutService_hidden.ToString());
            //dynamic signoutUrl = JsonConvert.DeserializeObject(viewModel.idp.SAML2_singleSignOnService_hidden.ToString());

            //signinUrl[0]["Location"] = viewModel.idp.SAML2_singleSignOnService;
            //signinUrl[1]["Location"] = viewModel.idp.SAML2_singleSignOnService;
            //signoutUrl[0]["Location"] = viewModel.idp.SAML2_singleLogoutService;

            //saml2["singleLogoutService"] = JsonConvert.SerializeObject(signoutUrl);
            //saml2["singleSignOnService"] = JsonConvert.SerializeObject(signinUrl);

            if (viewModel.idp.signingCert != null)
                common["signCertificate"] = getCertificate(viewModel.idp.signingCert);

            if (viewModel.idp.encryptCert != null)
                common["encryptionCertificate"] = getCertificate(viewModel.idp.encryptCert);

            IDPconfigInDB.openidconnect = oauth2;
            // IDPconfigInDB.saml2 = saml2;
            IDPconfigInDB.common = common;

            var response1 = await _configurationService.SetConfigurationAsync("IDP_Configuration", IDPconfigInDB, UUID);
            if (response1 == null || !response1.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to update IDP Application configuration in Update configuration");
                Alert alert1 = new Alert { Message = (response1 == null ? "Internal error please contact to admin" : response1.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert1);
                return RedirectToAction("Index");
                // return View("Index", viewModel);
            }

            //update admin portal sso configuration
            var AdminPortalconfigInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (AdminPortalconfigInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to get Application configuration in Update configuration");
                return NotFound();
            }
            AdminPortalconfigInDB.session_timeout = viewModel.adminPortalsso.AdminPortalSSO_session_timeout;
            AdminPortalconfigInDB.temporary_session_timeout = viewModel.adminPortalsso.AdminPortalSSO_temporary_session_timeout;
            AdminPortalconfigInDB.ideal_timeout = viewModel.adminPortalsso.AdminPortalSSO_ideal_timeout;
            AdminPortalconfigInDB.account_lock_time = viewModel.adminPortalsso.AdminPortalSSO_account_lock_time;
            AdminPortalconfigInDB.active_sessions_per_user = viewModel.adminPortalsso.AdminPortalSSO_active_sessions_per_user;
            AdminPortalconfigInDB.wrong_pin = viewModel.adminPortalsso.AdminPortalSSO_wrong_pin;
            AdminPortalconfigInDB.allowed_domain_users = viewModel.adminPortalsso.AdminPortalSSO_email_domain.Split(",").ToList<string>();

            var response2 = await _configurationService.SetConfigurationAsync("AdminPortal_SSOConfig", AdminPortalconfigInDB, UUID);
            if (response2 == null || !response2.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.FAILURE.ToString(), "Fail to update AdminPortal SSOConfig in Update configuration");
                Alert alert1 = new Alert { Message = (response2 == null ? "Internal error please contact to admin" : response2.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert1);
                return RedirectToAction("Index");
                //return View("Index", viewModel);
            }

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ApplicationConfiguration, "Update configuration details", LogMessageType.SUCCESS.ToString(), "Update Application configuration status is : " + response2.Message);
            Alert alert = new Alert { IsSuccess = true, Message = response2.Message };
            TempData["Alert"] = JsonConvert.SerializeObject(alert);
            return RedirectToAction("Index");
            // return View("Index", viewModel);


        }

        public async Task<IActionResult> DefaultAuthenticationSchema()
        {
            var activeAuthSchemaId = await _configurationService.GetActiveAuthenticationId();
            if(activeAuthSchemaId == null)
            {
                return View("Error");
            }

            var authSchemeList = await _authSchemeSevice.ListAuthSchemesAsync();

            var list = new List<SelectListItem>();
            foreach (var authScheme in authSchemeList)
            {
                list.Add(new SelectListItem { Text = authScheme.DisplayName, Value = authScheme.Id.ToString() });
            }
            var viewModel = new DefaultAuthenticationSchemaViewModel()
            {
                AuthenticationSchemesList = list,
                AuthSchemeId = activeAuthSchemaId
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAuthScheme(string AuthSchemeId)
        {
            if (string.IsNullOrEmpty(AuthSchemeId))
            {
                return BadRequest(new { success = false, message = "AuthSchemeId cannot be null or empty." });
            }
            var result=await _configurationService.UpdateDefaultAuthScheme(AuthSchemeId);

            return Ok(result);
        }

        string getCertificate(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }

            return result.ToString().Replace("\r", "");
        }
    }
}
