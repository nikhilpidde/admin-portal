using DTPortal.Core.Domain.Models;
//using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Beneficiary;
using DTPortal.Web.ViewModel.Configuration;
using DTPortal.Web.ViewModel.ESealRegistration;
using DTPortal.Web.ViewModel.MakerChecker;
using DTPortal.Web.ViewModel.OfflinePayment;
using DTPortal.Web.ViewModel.OnboardingTemplate;
using DTPortal.Web.ViewModel.OrganizationPaymentHistory;
using DTPortal.Web.ViewModel.OrganizationPriceSlabDefinition;
using DTPortal.Web.ViewModel.Package;
using DTPortal.Web.ViewModel.PriceSlabDefinition;
using DTPortal.Web.ViewModel.RateCard;
using DTPortal.Web.ViewModel.RoleManagement;
using DTPortal.Web.ViewModel.Saml2Clients;
using DTPortal.Web.ViewModel.SelfServicePortal;
using DTPortal.Web.ViewModel.SigningCredits;
using DTPortal.Web.ViewModel.Subscriber;
using DTPortal.Web.ViewModel.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
//using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class MakerCheckerController : BaseController
    {
        private readonly IMakerCheckerService _makerCheckerService;
        private readonly IRoleManagementService _roleActivityService;
        private readonly IUserManagementService _userService;
        private readonly IPKIConfigurationService _pkiConfigurationService;
        private readonly ISubscriberService _subscriberService;
        private readonly IOrganizationService _organizationService;
        private readonly IClientService _clientService;
        private readonly IConfiguration _configuration;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly DataExportService _dataExportService;
        private readonly ISelfPortalService _selfPortalService;
        public MakerCheckerController(ILogClient logClient,
            IRazorRendererHelper razorRendererHelper,
            DataExportService dataExportService,
            IUserManagementService userService,
            IMakerCheckerService makerCheckerService,
            IRoleManagementService roleActivityService,
            IPKIConfigurationService pkiConfigurationService,
            ISubscriberService subscriberService,
            IOrganizationService organizationService,
            IClientService clientService,
            IConfiguration configuration,
            IBeneficiaryService beneficiaryService,
            ISelfPortalService selfPortalService) : base(logClient)
        {
            _makerCheckerService = makerCheckerService;
            _roleActivityService = roleActivityService;
            _userService = userService;
            _pkiConfigurationService = pkiConfigurationService;
            _subscriberService = subscriberService;
            _organizationService = organizationService;
            _clientService = clientService;
            _configuration = configuration;
            _beneficiaryService = beneficiaryService;
            _dataExportService = dataExportService;
            _razorRendererHelper = razorRendererHelper;
            _selfPortalService = selfPortalService;
        }

        public async Task<IActionResult> MakerCheckerActivities()
        {
            var activities = await _makerCheckerService.GetAllActivities();

            activities = activities.Where(a => !a.IsCritical);

            var viewModel = new MakerCheckerActivitiesListViewModel();

            //var activities = await _context.Activities.ToListAsync();
            foreach (var activity in activities)
            {
                viewModel.Activities.Add(new SelectActivityItem { Id = activity.Id, DisplayName = activity.DisplayName, IsSelected = activity.McEnabled });
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MakerCheckerActivities(MakerCheckerActivitiesListViewModel viewModel)
        {
            string logMessage;

            var activities = await _makerCheckerService.GetAllActivities();
            foreach (var item in activities)
            {
                item.McEnabled = viewModel.Activities.FirstOrDefault(x => x.Id == item.Id).IsSelected;
            }
            var response = await _makerCheckerService.UpdateMakerCheckerActivityConfiguration(activities);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update maker checker configuration";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MakerCheckerConfiguration,
                    "Update Maker Checker Configuration", LogMessageType.FAILURE.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully updated maker checker configuration";
                SendAdminLog(ModuleNameConstants.PortalSettings, ServiceNameConstants.MakerCheckerConfiguration,
                    "Update Maker Checker Configuration", LogMessageType.SUCCESS.ToString(), logMessage);

                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }

            var activitiesInDb = await _makerCheckerService.GetAllActivities();
            viewModel = new MakerCheckerActivitiesListViewModel();
            foreach (var activity in activitiesInDb)
            {
                viewModel.Activities.Add(new SelectActivityItem { Id = activity.Id, DisplayName = activity.DisplayName, IsSelected = activity.McSupported });
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Approvals()
        {
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserRoleID").Value);
            var ApprovalListItem = await _makerCheckerService.GetAllRequestsByCheckerRoleId(id);
            if (ApprovalListItem == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get all ApprovalsList", LogMessageType.FAILURE.ToString(), "Fail to get Approvals list");
                return NotFound();
            }

            var viewModel = await GetActivitiesName(ApprovalListItem);

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get all ApprovalsList", LogMessageType.SUCCESS.ToString(), "Get Approvals list success");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> RequestedOperations()
        {
            var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData).Value);
            var ApprovalListItem = await _makerCheckerService.GetAllRequestsByMakerId(id);
            if (ApprovalListItem == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get all RequestedOperationsList", LogMessageType.FAILURE.ToString(), "Fail to get Approvals list");
                return NotFound();
            }

            var viewModel = await GetActivitiesName(ApprovalListItem);

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get all RequestedOperationsList", LogMessageType.SUCCESS.ToString(), "Get Approvals list success");

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isApproved, string reason = null)
        {
            var response = await _makerCheckerService.UpdateState(id, isApproved, AccessToken, reason);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "UpdateStatus", LogMessageType.FAILURE.ToString(), "fail to update status of id(" + id + ")");
                return new JsonResult(new { Message = (response == null ? "Internal error please contact to admin" : response.Message), Success = response.Success });
            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "UpdateStatus", LogMessageType.SUCCESS.ToString(), (isApproved ? "Request Approved Successfully" : "Request Denied Successfully"));

            return new JsonResult(new { Message = response.Message, Success = response.Success });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusWithRequsetBodyChange(int id, string requestBody, bool isApproved, string reason = null)
        {
            var response = await _makerCheckerService.UpdateStateWithRequetBodyChange(id, requestBody, isApproved, reason);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "UpdateStatus", LogMessageType.FAILURE.ToString(), "fail to update status of id(" + id + ")");
                return new JsonResult(new { Message = (response == null ? "Internal error please contact to admin" : response.Message), Success = response.Success });
            }
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "UpdateStatus", LogMessageType.SUCCESS.ToString(), (isApproved ? "Request Approved Successfully" : "Request Denied Successfully"));

            return new JsonResult(new { Message = response.Message, Success = response.Success });
        }

        //[HttpGet]
        //public async Task<IActionResult> ViewData(string operation, string data)
        //{
        //    ViewResult view = null;

        //    if (operation == "Oauth OpenID" || operation == "Saml2")
        //    {
        //        var model = JsonConvert.DeserializeObject<Client>(data);
        //        if (model.Type == "OAUTH2")
        //        {
        //            view = View("OAuth2ClientView", model);
        //        }
        //        else
        //        {
        //            var viewModel = new Saml2ClientsNewViewModel();
        //            viewModel.ApplicationType = model.ApplicationType;
        //            viewModel.ApplicationName = model.ApplicationName;
        //            viewModel.ApplicationUrl = model.ApplicationUrl;
        //            viewModel.assertionConsumerServiceUrl = model.RedirectUri;
        //            viewModel.singleLogoutService = model.LogoutUri;

        //            ClientsSaml2 Saml2Cilent = model.ClientsSaml2s.FirstOrDefault();
        //            /*parse spconfig and get url*/
        //            dynamic spconfig = JsonConvert.DeserializeObject(Saml2Cilent.Config);

        //            viewModel.entityID = Saml2Cilent.EntityId;
        //            var nameIdFormat = spconfig["nameIDFormat"].ToString();
        //            var nameID = JsonConvert.DeserializeObject(nameIdFormat);
        //            viewModel.nameIDFormat = nameID[0].ToString();
        //            //viewModel.requestSignatureAlgorithm = spconfig["requestSignatureAlgorithm"].ToString();
        //            //viewModel.dataEncryptionAlgorithm = spconfig["dataEncryptionAlgorithm"].ToString();
        //            //viewModel.keyEncryptionAlgorithm = spconfig["keyEncryptionAlgorithm"].ToString();

        //            viewModel.RequestSigned = spconfig["authnRequestsSigned"].ToString();
        //            viewModel.assertionSignature = spconfig["wantAssertionsSigned"].ToString();
        //            viewModel.assertionEncryption = spconfig["isAssertionEncrypted"].ToString();
        //            viewModel.ResponceSigned = spconfig["wantMessageSigned"].ToString();

        //            view = View("Saml2ClientView", viewModel);
        //        }
        //    }

        //    if (operation == "Roles")
        //    {
        //        var RoleDetails = JsonConvert.DeserializeObject<roleRequest>(data);
        //        var viewModel = new RoleManagementNewViewModel
        //        {
        //            Id = RoleDetails.role.Id,
        //            RoleName = RoleDetails.role.Name,
        //            DisplayName = RoleDetails.role.DisplayName,
        //            Description = RoleDetails.role.Description,
        //            CheckerActivitie = await GetCkeckerList(RoleDetails.role.RoleActivities),
        //            Activities = JsonConvert.DeserializeObject(await GetActivitiesList(RoleDetails.role.RoleActivities))
        //        };
        //        view = View("RoleView", viewModel);
        //    }

        //    if (operation == "Users")
        //    {
        //        var roleLookups = await _userService.GetRoleLookupsAsync();
        //        if (roleLookups == null)
        //        {
        //            SendAdminLog("Fail to get User role in Approvals", LogMessageType.FAILURE.ToString(), "Digital Authentication",
        //                "MakerChecker", "ViewData");
        //            return NotFound();
        //        }

        //        var UserDetails = JsonConvert.DeserializeObject<UserTable>(data);
        //        var viewModel = new UserManagementNewViewModel
        //        {

        //            FullName = UserDetails.FullName,
        //            MailId = UserDetails.MailId,
        //            MobileNo = UserDetails.MobileNo,
        //            RoleName = roleLookups.FirstOrDefault(x => x.Id == UserDetails.RoleId)?.DisplayName,
        //            Dob = UserDetails.Dob,
        //            gender = UserDetails.Gender,
        //        };
        //        view = View("UserView", viewModel);
        //    }

        //    return view;
        //}


        [HttpGet]
        public async Task<IActionResult> ViewRequest(string operation, int id, string activity)
        {
            try
            {
                ViewResult view = null;

                var data = await _makerCheckerService.GetRequestDataById(id);
                if (string.IsNullOrEmpty(data))
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Request data get empty string or null");
                    Alert alert = new Alert { Message = "Request data not found" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return RedirectToAction("Approvals");
                }

                switch (activity)
                {
                    //case "Oauth OpenID":
                    case "Service Provider":
                        {
                            var orgList = await GetOrganizationList();

                            if (operation == "CREATE")
                            {
                                Client client = JsonConvert.DeserializeObject<Client>(data);
                                if (orgList.Count != 0 && !string.IsNullOrEmpty(client.OrganizationUid))
                                {
                                    client.OrganizationUid = orgList.FirstOrDefault(x => x.Value == client.OrganizationUid).Text;
                                }
                                view = View("OAuth2ClientView", client);
                            }
                            else
                            {
                                ClientRequest client = JsonConvert.DeserializeObject<ClientRequest>(data);
                                if (orgList.Count != 0 && !string.IsNullOrEmpty(client.client.OrganizationUid))
                                {
                                    client.client.OrganizationUid = orgList.FirstOrDefault(x => x.Value == client.client.OrganizationUid).Text;
                                }
                                view = View("OAuth2ClientView", client.client);
                            }
                        }
                        break;

                    case "Saml2":
                        {
                            Client client;
                            ClientsSaml2 Saml2Cilent;
                            if (operation == "CREATE")
                            {
                                client = JsonConvert.DeserializeObject<Client>(data);
                                Saml2Cilent = client.ClientsSaml2s.FirstOrDefault();
                                view = View("OAuth2ClientView", client);
                            }
                            else
                            {
                                ClientRequest clientdata = JsonConvert.DeserializeObject<ClientRequest>(data);
                                client = clientdata.client;
                                Saml2Cilent = clientdata.ClientSaml2;
                            }

                            var viewModel = new Saml2ClientsNewViewModel();
                            viewModel.ApplicationType = client.ApplicationType;
                            viewModel.ApplicationName = client.ApplicationName;
                            viewModel.ApplicationUrl = client.ApplicationUrl;
                            viewModel.assertionConsumerServiceUrl = client.RedirectUri;
                            viewModel.singleLogoutService = client.LogoutUri;

                            /*parse spconfig and get url*/
                            dynamic spconfig = JsonConvert.DeserializeObject(Saml2Cilent.Config);

                            viewModel.entityID = Saml2Cilent.EntityId;
                            var nameIdFormat = spconfig["nameIDFormat"].ToString();
                            var nameID = JsonConvert.DeserializeObject(nameIdFormat);
                            viewModel.nameIDFormat = nameID[0].ToString();

                            viewModel.RequestSigned = spconfig["authnRequestsSigned"].ToString();
                            viewModel.assertionSignature = spconfig["wantAssertionsSigned"].ToString();
                            viewModel.assertionEncryption = spconfig["isAssertionEncrypted"].ToString();
                            viewModel.ResponceSigned = spconfig["wantLogoutResponseSigned"].ToString();

                            view = View("Saml2ClientView", viewModel);
                        }
                        break;

                    case "Roles":
                        {
                            var viewModel = new RoleManagementNewViewModel();
                            roleRequest roleRequest = JsonConvert.DeserializeObject<roleRequest>(data);
                            if (operation == "CREATE")
                            {

                                viewModel.Id = roleRequest.role.Id;
                                viewModel.RoleName = roleRequest.role.Name;
                                viewModel.DisplayName = roleRequest.role.DisplayName;
                                viewModel.Description = roleRequest.role.Description;
                                viewModel.CheckerActivitie = await GetCkeckerList(roleRequest.role.RoleActivities);
                                viewModel.Activities = JsonConvert.DeserializeObject(await GetActivitiesList(roleRequest.role.RoleActivities));

                            }
                            else
                            {
                                viewModel.Id = roleRequest.role.Id;
                                viewModel.RoleName = roleRequest.role.Name;
                                viewModel.DisplayName = roleRequest.role.DisplayName;
                                viewModel.Description = roleRequest.role.Description;
                                viewModel.CheckerActivitie = await GetCkeckerListForUpdate(roleRequest.selectedActivityIds);
                                viewModel.Activities = JsonConvert.DeserializeObject(await GetActivitiesListForUpdate(roleRequest.selectedActivityIds));
                            }
                            view = View("RoleView", viewModel);
                        }
                        break;

                    case "Users":
                        {
                            var roleLookups = await _userService.GetRoleLookupsAsync();
                            if (roleLookups == null)
                            {
                                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "ViewData", LogMessageType.FAILURE.ToString(), "Fail to get User role in Approvals");
                                return NotFound();
                            }


                            var viewModel = new UserManagementNewViewModel();
                            if (operation == "CREATE")
                            {
                                UserRequest user = JsonConvert.DeserializeObject<UserRequest>(data);

                                DateOnly dob = (DateOnly)user.user.Dob;
                                viewModel.Uuid = user.user.Uuid;
                                viewModel.FullName = user.user.FullName;
                                viewModel.MailId = user.user.MailId;
                                viewModel.MobileNo = user.user.MobileNo;
                                viewModel.RoleName = roleLookups.FirstOrDefault(x => x.Id == user.user.RoleId)?.DisplayName;
                                viewModel.Dob = dob.ToDateTime(TimeOnly.Parse("10:00 PM"));
                                viewModel.gender = user.user.Gender;
                            }
                            else
                            {
                                UserTable user = JsonConvert.DeserializeObject<UserTable>(data);
                                DateOnly dob = (DateOnly)user.Dob;
                                viewModel.Uuid = user.Uuid;
                                viewModel.FullName = user.FullName;
                                viewModel.MailId = user.MailId;
                                viewModel.MobileNo = user.MobileNo;
                                viewModel.RoleName = roleLookups.FirstOrDefault(x => x.Id == user.RoleId)?.DisplayName;
                                viewModel.Dob = dob.ToDateTime(TimeOnly.Parse("10:00 PM"));
                                viewModel.gender = user.Gender;
                            }

                            view = View("UserView", viewModel);
                        }
                        break;

                    case "Application Configuration":
                        {
                            var idp = new IDPConfigureationViewModel();
                            var sso = new SSOConfigurationViewModel();
                            var adminportalsso = new AdminPortalSSOConfiguratonViewModel();

                            var Config = JsonConvert.DeserializeObject<configurationMCRequest>(data);
                            if (Config.configName == "SSO_Config")
                            {
                                dynamic configInDB = Config.requestData;

                                sso.SSOTemporarySessionTimeout = configInDB.sso_config.temporary_session_timeout;
                                sso.SSOSessionTimeout = configInDB.sso_config.session_timeout;
                                sso.SSOAuthorizationCodeTimeout = configInDB.sso_config.authorization_code_timeout;
                                sso.SSOAccessTokenTimeout = configInDB.sso_config.access_token_timeout;
                                sso.SSOAccountLockTime = configInDB.sso_config.account_lock_time;
                                sso.SSOActiveSessionsPerUser = configInDB.sso_config.active_sessions_per_user;
                                sso.SSOWrongPin = configInDB.sso_config.wrong_pin;
                                sso.SSOWrongCode = configInDB.sso_config.wrong_code;
                                sso.SSODenyCount = configInDB.sso_config.deny_count;
                                sso.CentralLogConnection = configInDB.log_config.central_log_config.connection_string;
                                sso.CentralLogQueueName = configInDB.log_config.central_log_config.queue_name;
                                sso.ServiceLogConnection = configInDB.log_config.service_log_config.connection_string;
                                sso.ServiceLogQueueName = configInDB.log_config.service_log_config.queue_name;
                                sso.AdminLogConnection = configInDB.log_config.admin_log_config.connection_string;
                                sso.AdminLogQueueName = configInDB.log_config.admin_log_config.queue_name;
                                sso.PKIServiceBaseAddress = configInDB.pki_service_config.base_address;
                                sso.PKIServiceGenerateSignatureUri = configInDB.pki_service_config.generate_signature_uri;
                                sso.PKIServiceVerifySignatureUri = configInDB.pki_service_config.verify_signature_uri;
                                sso.RAbaseAddress = configInDB.ra_service_config.base_address;
                                sso.RAstatusUpdateUri = configInDB.ra_service_config.status_update_uri;
                                // sso.RedisServerConnection = configInDB.redis_server_config.connection_string;
                                // IDPDatabaseConnection = configInDB.database_config.idp_connection_string,
                                // RADatabaseConnection = configInDB.database_config.ra_connection_string


                            }
                            if (Config.configName == "IDP_Configuration")
                            {
                                dynamic IDPconfigInDB = Config.requestData;

                                dynamic oauth2 = IDPconfigInDB.openidconnect;
                                //  dynamic saml2 = IDPconfigInDB.saml2;
                                dynamic comman = IDPconfigInDB.common;


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
                                //idp.SAML2_singleSignOnService = saml2["singleSignOnService"].ToString();
                                //idp.SAML2_singleLogoutService = saml2["singleLogoutService"].ToString();
                                //idp.SAML2_Method_binding_Supported = new List<string>() { "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect" };
                                idp.signCert = comman["signCertificate"].ToString();
                                idp.enctCert = comman["encryptionCertificate"].ToString();
                            }

                            if (Config.configName == "AdminPortal_SSOConfig")
                            {
                                dynamic configInDB = Config.requestData;

                                adminportalsso.AdminPortalSSO_session_timeout = configInDB.session_timeout;
                                adminportalsso.AdminPortalSSO_temporary_session_timeout = configInDB.temporary_session_timeout;
                                adminportalsso.AdminPortalSSO_ideal_timeout = configInDB.ideal_timeout;
                                adminportalsso.AdminPortalSSO_account_lock_time = configInDB.account_lock_time;
                                adminportalsso.AdminPortalSSO_active_sessions_per_user = configInDB.active_sessions_per_user;
                                adminportalsso.AdminPortalSSO_wrong_pin = configInDB.wrong_pin;
                                adminportalsso.AdminPortalSSO_email_domain = configInDB["allowed_domain_users"].ToString(); // String.Join<string>(",", configInDB.allowed_domain_users);
                            }

                            var model = new ConfigrationViewModel()
                            {
                                idp = idp,
                                sso = sso,
                                adminPortalsso = adminportalsso,
                                ConfigName = Config.configName
                            };
                            view = View("Configuration", model);
                        }
                        break;

                    //case "OnboardingTemplate":
                    //    {
                    //        OnboardingTemplateDTO onboardingTemplate = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(data);
                    //        OnboardingTemplateDetailsViewModel viewModel = new OnboardingTemplateDetailsViewModel
                    //        {
                    //            OnboardingTemplate = onboardingTemplate
                    //        };

                    //        view = View("OnboardingTemplate", viewModel);
                    //    }
                    //    break;

                    case "Configuration":
                        {
                            PkiPluginDatum pluginData = JsonConvert.DeserializeObject<PkiPluginDatum>(data);
                            pluginData.PkiHsmData.HashAlgorithm = await _pkiConfigurationService.GetHashAlgorithmAsync(pluginData.PkiHsmData.HashAlgorithmId);
                            pluginData.PkiHsmData.HsmPlugin = await _pkiConfigurationService.GetHSMPluginAsync(pluginData.PkiHsmData.HsmPluginId);
                            pluginData.PkiHsmData.KeyData.KeyAlgorithm = await _pkiConfigurationService.GetKeyAlgorithmAsync(pluginData.PkiHsmData.KeyData.KeyAlgorithmId);
                            pluginData.PkiHsmData.KeyData.KeySize = await _pkiConfigurationService.GetKeySizeAsync(pluginData.PkiHsmData.KeyData.KeySizeId);
                            pluginData.PkiCaData.Procedure = await _pkiConfigurationService.GetProcedureAsync(pluginData.PkiCaData.ProcedureId);
                            pluginData.PkiCaData.CaPlugin = await _pkiConfigurationService.GetCAPluginAsync(pluginData.PkiCaData.CaPluginId);

                            view = View("PKIConfiguration", pluginData);
                        }
                        break;

                    case "Rate Card":
                        {
                            RateCardDTO rateCard = JsonConvert.DeserializeObject<RateCardDTO>(data);
                            RateCardDetailsViewModel viewModel = new RateCardDetailsViewModel
                            {
                                RateCard = rateCard
                            };

                            view = View("RateCard", viewModel);
                        }
                        break;

                    case "Package":
                        {
                            PackageDTO package = JsonConvert.DeserializeObject<PackageDTO>(data);
                            PackageDetailsViewModel viewModel = new PackageDetailsViewModel
                            {
                                Package = package
                            };

                            view = View("Package", viewModel);
                        }
                        break;

                    case "Generic Price Slab":
                        {
                            IList<PriceSlabDefinitionDTO> priceSlabs = JsonConvert.DeserializeObject<IList<PriceSlabDefinitionDTO>>(data);
                            PriceSlabDefinitionDTO priceSlab = priceSlabs[0];
                            PriceSlabDefinitionDetailsViewModel viewModel = new PriceSlabDefinitionDetailsViewModel();
                            viewModel.ServiceDisplayName = priceSlab.ServiceDefinitions.ServiceDisplayName;
                            viewModel.Stakeholder = priceSlab.Stakeholder;
                            viewModel.CreatedBy = priceSlab.CreatedBy;
                            viewModel.Updatedby = priceSlab.UpdatedBy;
                            for (int i = 0; i < priceSlabs.Count(); i++)
                            {
                                viewModel.DiscountVolumeRanges.Add(new DiscountVolumeRangeDTO
                                {
                                    Id = priceSlabs[i].Id,
                                    VolumeRangeFrom = priceSlabs[i].VolumeRangeFrom,
                                    VolumeRangeTo = priceSlabs[i].VolumeRangeTo,
                                    Discount = priceSlabs[i].Discount
                                });
                            }
                            view = View("PriceSlab", viewModel);
                        }
                        break;

                    case "Organization Price Slab":
                        {
                            IList<OrganizationPriceSlabDefinitionDTO> priceSlabs = JsonConvert.DeserializeObject<IList<OrganizationPriceSlabDefinitionDTO>>(data);
                            OrganizationPriceSlabDefinitionDTO priceSlab = priceSlabs[0];
                            OrganizationPriceSlabDefinitionDetailsViewModel viewModel = new OrganizationPriceSlabDefinitionDetailsViewModel();
                            viewModel.ServiceDisplayName = priceSlab.ServiceDefinitions.ServiceDisplayName;
                            viewModel.OrganizationName = priceSlab.OrganizationName;
                            viewModel.CreatedBy = priceSlab.CreatedBy;
                            viewModel.Updatedby = priceSlab.UpdatedBy;
                            for (int i = 0; i < priceSlabs.Count(); i++)
                            {
                                viewModel.DiscountVolumeRanges.Add(new DiscountVolumeRangeDTO
                                {
                                    Id = priceSlabs[i].Id,
                                    VolumeRangeFrom = priceSlabs[i].VolumeRangeFrom,
                                    VolumeRangeTo = priceSlabs[i].VolumeRangeTo,
                                    Discount = priceSlabs[i].Discount
                                });
                            }

                            view = View("OrganizationPriceSlab", viewModel);
                        }
                        break;

                    case "Template Management":
                        {
                            OnboardingTemplateDTO template = JsonConvert.DeserializeObject<OnboardingTemplateDTO>(data);
                            OnboardingTemplateDetailsViewModel viewModel = new OnboardingTemplateDetailsViewModel
                            {
                                OnboardingTemplate = template
                            };

                            view = View("OnboardingTemplate", viewModel);
                        }
                        break;
                    case "Subscriber":
                        {
                            var definition = new { SubscriberUniqueId = "", RevokeReasonId = 0, Remarks = "", Subscriber = "" };
                            var details = JsonConvert.DeserializeAnonymousType(data, definition);
                            SubscriberCheckerViewModel viewModel = new SubscriberCheckerViewModel
                            {
                                SubscriberUniqueId = details.SubscriberUniqueId,
                                RevokeReasonId = details.RevokeReasonId,
                                Remarks = details.Remarks,
                                Subscriber = details.Subscriber
                            };

                            view = View("Subscriber", viewModel);
                        }
                        break;
                    case "Organizations":
                        {
                            string remarks = null;
                            string logMessage;
                            OrganizationDTO organizaion;
                            if (operation == "REVOKE_CERTIFICATE")
                            {
                                var definition = new { Organization = new OrganizationDTO(), RevokeReasonId = 0, Remarks = "" };
                                var details = JsonConvert.DeserializeAnonymousType(data, definition);
                                organizaion = details.Organization;
                                remarks = details.Remarks;
                            }
                            else
                            {
                                organizaion = JsonConvert.DeserializeObject<OrganizationDTO>(data);
                            }
                            string[] address = organizaion.CorporateOfficeAddress.Split(';');
                            ViewModel.ESealRegistration.OrganizationDetailsViewModel viewModel = new ViewModel.ESealRegistration.OrganizationDetailsViewModel
                            {
                                OrganizationId = organizaion.OrganizationId,
                                OrganizationUID = organizaion.OrganizationUid,
                                OrganizationName = organizaion.OrganizationName,
                                OrganizationEmail = organizaion.OrganizationEmail,
                                UniqueRegdNo = organizaion.UniqueRegdNo,
                                TaxNo = organizaion.TaxNo,
                                CorporateOfficeAddress1 = address[0],
                                CorporateOfficeAddress2 = address[1],
                                Country = address[2],
                                Pincode = address[3],
                                OrganizationUsersList = organizaion.OrgUserList,
                                DirectorsEmailList = organizaion.DirectorsEmailList,
                                AuthorizedLetterForSignatories = organizaion.AuthorizedLetterForSignatories,
                                ESealImage = organizaion.ESealImage,
                                Incorporation = organizaion.Incorporation,
                                Tax = organizaion.Tax,
                                AdditionalLegalDocument = organizaion.OtherLegalDocument,
                                Status = organizaion.Status,
                                Remarks = remarks,
                                SpocUgpassEmail = organizaion.SpocUgpassEmail,
                                AgentUrl = organizaion.AgentUrl,
                                EnablePostPaidOption = organizaion.EnablePostPaidOption,
                                SignedPdf = organizaion.SignedPdf,
                                SignatureTemplate = organizaion.TemplateId[0],
                                ESealTemplate = organizaion.TemplateId[1]
                            };

                            var templateList = await _organizationService.GetSignatureTemplateListAsyn();
                            if (templateList == null)
                            {
                                logMessage = $"Failed to get signature templates";
                                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                                    "Get Signature Templates", LogMessageType.FAILURE.ToString(), logMessage);

                                return NotFound();
                            }
                            viewModel.TemplateList = templateList;

                            viewModel.DocumentListCheckbox = GetCheckedDocuments(viewModel.DocumentListCheckbox, organizaion.DocumentListCheckbox);
                            //if (operation == "GENERATE_E_SEAL")
                            //{
                            //    view = View("GenerateEseal", viewModel);
                            //}
                            //else
                            view = View("Organization", viewModel);
                        }
                        break;
                    case "Organization Payment History":
                        {
                            OrganizationPaymentHistoryDTO organizationPaymentHistory = JsonConvert.DeserializeObject<OrganizationPaymentHistoryDTO>(data); ;

                            OrganizationPaymentHistoryDetailsViewModel organizationPaymentHistoryDetails = new OrganizationPaymentHistoryDetailsViewModel
                            {
                                OrganizationPaymentHistory = organizationPaymentHistory
                            };

                            view = View("OrganizationPaymentHistory", organizationPaymentHistoryDetails);
                        }
                        break;
                    case "Controlled Onboarding":
                        {
                            ControlledOnboardingDTO controlledOnboardingData = JsonConvert.DeserializeObject<ControlledOnboardingDTO>(data); ;

                            view = View("ControlledOnboarding", controlledOnboardingData);
                        }
                        break;
                    case "Offline Payment History":
                        {
                            CreditAllocationListDTO creditAllocationListDTO = JsonConvert.DeserializeObject<CreditAllocationListDTO>(data);
                            OfflinePaymentViewModel offlinePaymentViewModel = new OfflinePaymentViewModel
                            {
                                Id = creditAllocationListDTO.Id,
                                OrgId = creditAllocationListDTO.OrgId,
                                OrgName = creditAllocationListDTO.OrgName,
                                TransactionRefId = creditAllocationListDTO.TransactionRefId,
                                AmountReceived = creditAllocationListDTO.AmountReceived,
                                PaymentChannel = creditAllocationListDTO.PaymentChannel,
                                TotalEsealCredits = creditAllocationListDTO.TotalEsealCredits,
                                TotalSigningCredits = creditAllocationListDTO.TotalSigningCredits,
                                OnboardingCredits = creditAllocationListDTO.OnboardingCredits,
                                OnlinePaymentGateway = creditAllocationListDTO.OnlinePaymentGateway,
                                OnlinePaymentGatewayReferenceNo = creditAllocationListDTO.OnlinePaymentGatewayReferenceNo,
                                InvoiceNo = creditAllocationListDTO.InvoiceNo,
                            };
                            view = View("OfflinePaymentHistory", offlinePaymentViewModel);
                        }
                        break;
                    case "Exclusive Apps":
                        {
                            UpdateBucketConfigDTO bucketConfig = JsonConvert.DeserializeObject<UpdateBucketConfigDTO>(data);
                            var clientName = "";

                            var clientinDb = await _clientService.GetClientByClientIdAsync(bucketConfig.appId);

                            if (clientinDb != null)
                            {
                                clientName = clientinDb.ApplicationName;
                            }
                            var model = new UpdateBucketViewModel()
                            {
                                OrganizationName = bucketConfig.orgName,
                                closingMessage = bucketConfig.bucketClosingMessage,
                                AdditionalDs = bucketConfig.additionalDs,
                                AdditionalEDs = bucketConfig.additionalEds,
                                label = bucketConfig.label,
                                status = bucketConfig.status,
                                ApplicationName = clientName,
                                OrgId = bucketConfig.orgId,
                                appId = bucketConfig.appId
                            };
                            view = View("ExclusiveApps", model);
                        }
                        break;
                    case "Beneficiaries":
                        {
                            if (operation == "CREATEMANY")
                            {
                                var beneficiary = JsonConvert.DeserializeObject<IList<BeneficiaryAddDTO>>(data);

                                AddMultipleBeneficiariesViewModel viewModel = new AddMultipleBeneficiariesViewModel()
                                {
                                    MultipleBeneficiaries = beneficiary,
                                };
                                //var partialName = "/Views/MakerChecker/BeneficiaryCsvUploadPdf.cshtml";
                                //var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, viewModel);
                                //byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);
                                //string base64String = Convert.ToBase64String(pdfBytes);
                                var fileName = _dataExportService.ExportBeneficiariesToFile(beneficiary);
                                viewModel.PdfFile = fileName;
                                view = View("CSVuploadBeneficiaries", viewModel);

                            }
                            else
                            {
                                var response1 = await _beneficiaryService.GetBeneficiaryPrivilegesAsync();
                                if (response1 == null || !response1.Success)
                                {
                                    return NotFound();
                                }
                                var beneficiaries = (IEnumerable<BeneficiaryPrivilegeDTO>)response1.Resource;
                                List<BeneficiaryPrivilegeDTO> servicePrivilages = beneficiaries.ToList();

                                BeneficiaryUpdateDTO beneficiaryAddDTO = JsonConvert.DeserializeObject<BeneficiaryUpdateDTO>(data);
                                var model = new BeneficiaryEditViewModel()
                                {
                                    id = id,
                                    orgName = beneficiaryAddDTO.SponsorName,
                                    OrganizationUid = beneficiaryAddDTO.SponsorDigitalId,
                                    EmployeeEmail = beneficiaryAddDTO.BeneficiaryOfficeEmail,
                                    Designation = beneficiaryAddDTO.Designation,
                                    SignaturePhoto = beneficiaryAddDTO.SignaturePhoto,
                                    UgpassEmail = beneficiaryAddDTO.BeneficiaryUgpassEmail,
                                    PassportNumber = beneficiaryAddDTO.BeneficiaryPassport,
                                    NationalIdNumber = beneficiaryAddDTO.BeneficiaryNin,
                                    MobileNumber = beneficiaryAddDTO.BeneficiaryMobileNumber,
                                    SponsorType = "ORGANIZATION",
                                    BeneficiaryType = "INDIVIDUAL",
                                    ServicePrevilage = beneficiaryAddDTO.BeneficiaryValidities,
                                    Service = servicePrivilages,
                                };
                                if (!string.IsNullOrEmpty(beneficiaryAddDTO.BeneficiaryMobileNumber))
                                {
                                    var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
                                    if (beneficiaryAddDTO.BeneficiaryMobileNumber.StartsWith(mobileCountryCode))
                                    {
                                        string[] mobileParts = new string[2];
                                        mobileParts[0] = mobileCountryCode;
                                        mobileParts[1] = beneficiaryAddDTO.BeneficiaryMobileNumber.Substring(4);
                                        model.CountryCode = mobileParts[0];
                                        model.MobileNumber = mobileParts[1];
                                    }
                                    else if (beneficiaryAddDTO.BeneficiaryMobileNumber.StartsWith("+91"))
                                    {
                                        string[] mobileParts = new string[2];
                                        mobileParts[0] = "+91";
                                        mobileParts[1] = beneficiaryAddDTO.BeneficiaryMobileNumber.Substring(3);
                                        model.CountryCode = mobileParts[0];
                                        model.MobileNumber = mobileParts[1];
                                    }
                                    else
                                    {
                                        model.CountryCode = "";
                                        model.MobileNumber = "";
                                    }
                                }
                                view = View("Beneficiary", model);
                            }
                        }
                        break;

                    case "Onboarding Approval Requests":
                        {

                            SelfServiceOrganizationDTO orgdetails1 = JsonConvert.DeserializeObject<SelfServiceOrganizationDTO>(data);

                            var orgdetails = await _selfPortalService.GetSelfServiceOrganizationDetailsAsync(orgdetails1.OrgOnboardingFormsId);

                            if (orgdetails == null)
                            {
                                return NotFound();
                            }

                            string[] address = orgdetails.OrgCorporateAddress.Split(';');

                            ViewModel.SelfServicePortal.OrganizationDetailsViewModel viewModel = new ViewModel.SelfServicePortal.OrganizationDetailsViewModel()
                            {
                                OrgOnboardingFormsId = orgdetails.OrgOnboardingFormsId,
                                OrgRegIdNumber = orgdetails.OrgRegIdNumber,
                                OrganizationName = orgdetails.OrgName,
                                OrgOfficialContactNumber = orgdetails.OrgOfficialContactNumber,
                                CorporateOfficeAddress1 = address[0],
                                CorporateOfficeAddress2 = address[1],
                                Country = address[2],
                                Pincode = address[3],
                                OrgWebUrl = orgdetails.OrgWebUrl,
                                OrgTanTaxNumber = orgdetails.OrgTanTaxNumber,
                                UrsbCertificate = orgdetails.UrsbCertificate,
                                ApprovalLetter = orgdetails.ApprovalLetter,
                                ObFormStatus = orgdetails.ObFormStatus,
                                OrgApprovalStatus = orgdetails.OrgApprovalStatus,
                                RejectedReason = orgdetails1.OrgObRejectedReason,
                                SignApprByBrmStaff = orgdetails.SignApprByBrmStaff,
                                OrgUid = orgdetails.OrgUid,
                                OrgCategory = orgdetails.OrgCategory,
                                SpocSuid = orgdetails.SpocSuid,
                                OtpVerification = orgdetails.OtpVerification
                            };

                            if (orgdetails.OrgFinancialAuditorDetailsDTO != null)
                            {
                                viewModel.FinancialAuditorName = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorName;
                                viewModel.FinancialAuditorLicenseNum = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorLicenseNum;
                                //viewModel.FinancialAuditorNin = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorNin;
                                viewModel.FinancialAuditorIdDocNumber = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorIdDocumentNumber;
                                viewModel.OrgFinancialAuditorDetailsId = orgdetails.OrgFinancialAuditorDetailsDTO.OrgFinancialAuditorDetailsId;
                                viewModel.AuditorUgPassEmail = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorUgPassEmail;
                                viewModel.financialAuditorTinNumber = orgdetails.OrgFinancialAuditorDetailsDTO.financialAuditorTinNumber;
                            }

                            if (orgdetails.OrganisationSpocDetailsDTO != null)
                            {
                                viewModel.spocSuid = orgdetails.OrganisationSpocDetailsDTO.SpocSuid;
                                viewModel.SpocName = orgdetails.OrganisationSpocDetailsDTO.SpocName;
                                viewModel.SpocFaceCaptured = orgdetails.OrganisationSpocDetailsDTO.Spoc_face_captured;
                                viewModel.SpocFaceFromUgpass = orgdetails.OrganisationSpocDetailsDTO.SpocFaceFromUgpass;
                                viewModel.SpocFaceMatchStatus = true;
                                viewModel.SpocIdDocNo = orgdetails.OrganisationSpocDetailsDTO.SpocIdDocumentNumber;
                                viewModel.SpocOfficeEmail = orgdetails.OrganisationSpocDetailsDTO.SpocOfficeEmail;
                                viewModel.SpocOtpVerfyStatus = orgdetails.OrganisationSpocDetailsDTO.SpocOtpVerfyStatus;
                                //viewModel.SpocPassport = orgdetails.OrganisationSpocDetailsDTO.SpocPassport;
                                viewModel.SpocSocialSecurityNum = orgdetails.OrganisationSpocDetailsDTO.SpocSocialSecurityNum;
                                viewModel.SpocTaxNum = orgdetails.OrganisationSpocDetailsDTO.SpocTaxNum;
                                viewModel.SpocUgpassEmail = orgdetails.OrganisationSpocDetailsDTO.SpocUgpassEmail;
                                viewModel.SpocUgpassMobNum = orgdetails.OrganisationSpocDetailsDTO.SpocUgpassMobNum;
                                viewModel.OrgSpocDetailsId = orgdetails.OrganisationSpocDetailsDTO.OrgSpocDetailsId;

                                //here 3 is for email
                                var subscriberDetails = await _subscriberService.GetSubscriberDetailsAsync(3, orgdetails.OrganisationSpocDetailsDTO.SpocUgpassEmail);
                                if (subscriberDetails != null)
                                {
                                    viewModel.SpocRAFaceCaptured = subscriberDetails.SubscriberPhoto;
                                }
                            }

                            if (orgdetails.OrgCeoDetailsDTO != null)
                            {
                                viewModel.CeoPanTaxNum = orgdetails.OrgCeoDetailsDTO.CeoPanTaxNum;
                                viewModel.OrgCeoDetailsiId = orgdetails.OrgCeoDetailsDTO.OrgCeoDetailsiId;
                                viewModel.CeoName = orgdetails.OrgCeoDetailsDTO.CeoName;
                                viewModel.CeoEmail = orgdetails.OrgCeoDetailsDTO.CeoEmail;
                                viewModel.ceoIdDocumentNumber = orgdetails.OrgCeoDetailsDTO.ceoIdDocumentNumber;
                            }

                            if (orgdetails.UraReportsDTO != null)
                            {
                                viewModel.auditorUraPdf = orgdetails.UraReportsDTO.auditorUraPdf;
                                viewModel.spocUraPdf = orgdetails.UraReportsDTO.spocUraPdf;
                                viewModel.ceoUraPdf = orgdetails.UraReportsDTO.ceoUraPdf;
                                viewModel.orgUraPdf = orgdetails.UraReportsDTO.orgUraPdf;
                            }
                            TempData["ApprovalRecordID"] = id;
                            view = View("OnboardingApprovalRequests", viewModel);
                        }
                        break;
                }

                ViewBag.ApprovalRecordID = id;
                ViewBag.Operation = operation;
                return view;
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), e.Message);
                Alert alert = new Alert { Message = "Something went wrong! please contact to admin or try again." };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("Approvals");
            }
        }

        public async Task<IEnumerable<MakerCheckerListViewModel>> GetActivitiesName(IEnumerable<MakerChecker> List = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<MakerCheckerListViewModel>();
            if (List != null && List.Count() != 0)
            {
                foreach (var operation in List)
                {
                    //if (operation.State != "APPROVED")
                    //{
                    nodes.Add(new MakerCheckerListViewModel()
                    {
                        ApprovalList = operation,
                        OperationName = activityLookupItems.FirstOrDefault(x => x.Id == operation.ActivityId).DisplayName
                    });
                    // }
                }
            }

            return nodes;
        }


        public async Task<string> GetActivitiesList(IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<ActivityTreeItem>();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetState(roleActivities, activity.Id)) //Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id) })
                    });
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetState(null, -1))//Json(new {selected = false})
                    });
                }
            }

            var data = new JsonResult(nodes);
            return JsonConvert.SerializeObject(data.Value);
        }

        public async Task<List<CheckerListItem>> GetCkeckerList(IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<CheckerListItem>();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerState(roleActivities, activity.Id)// Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id && x.IsChecker == true) })
                        }); ;
                    }
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerState(roleActivities, activity.Id)//Json(new {selected = false})
                        });
                    }
                }
            }

            return nodes;
        }

        [NonAction]
        private string GetState(IEnumerable<RoleActivity> roleActivity = null, int parentId = -1)
        {
            JsonResult data;
            if (roleActivity != null)
            {
                data = Json(new { selected = roleActivity.Any(x => x.ActivityId == parentId && (bool)x.IsEnabled) });
            }
            else
            {
                data = Json(new { selected = false });
            }
            return JsonConvert.SerializeObject(data.Value);
        }

        [NonAction]
        private bool GetCheckerState(IEnumerable<RoleActivity> roleActivity = null, int parentId = -1)
        {
            bool data;
            if (roleActivity != null)
                data = roleActivity.Any(x => x.ActivityId == parentId && x.IsChecker);
            else
                data = false;

            return data;
        }


        public async Task<string> GetActivitiesListForUpdate(IDictionary<int, bool> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<ActivityTreeItem>();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetStateForUpdate(roleActivities, activity.Id)) //Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id) })
                    });
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    nodes.Add(new ActivityTreeItem()
                    {
                        id = activity.Id.ToString(),
                        parent = (activity.ParentId == 0 ? "#" : activity.ParentId.ToString()),
                        text = activity.DisplayName,
                        state = JsonConvert.DeserializeObject(GetStateForUpdate(null, -1))//Json(new {selected = false})
                    });
                }
            }

            var data = new JsonResult(nodes);
            return JsonConvert.SerializeObject(data.Value);
        }

        public async Task<List<CheckerListItem>> GetCkeckerListForUpdate(IDictionary<int, bool> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.MakerChecker, "Get operation data", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var nodes = new List<CheckerListItem>();

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerStateForUpdate(roleActivities, activity.Id)// Json(new { selected = roleActivities.Any(x => x.ActivityId == activity.Id && x.IsChecker == true) })
                        }); ;
                    }
                }
            }
            else
            {
                foreach (var activity in activityLookupItems)
                {
                    if (activity.McSupported)
                    {
                        nodes.Add(new CheckerListItem()
                        {
                            id = "IsChecker_" + activity.Id.ToString(),
                            Display = activity.DisplayName + " Checker",
                            IsSelected = GetCheckerStateForUpdate(roleActivities, activity.Id)//Json(new {selected = false})
                        });
                    }
                }
            }

            return nodes;
        }



        [NonAction]
        private string GetStateForUpdate(IDictionary<int, bool> roleActivity = null, int parentId = -1)
        {
            JsonResult data;
            if (roleActivity != null)
            {
                data = Json(new { selected = roleActivity.ContainsKey(parentId) });
            }
            else
            {
                data = Json(new { selected = false });
            }
            return JsonConvert.SerializeObject(data.Value);
        }

        [NonAction]
        private bool GetCheckerStateForUpdate(IDictionary<int, bool> roleActivity = null, int parentId = -1)
        {
            bool data = false;
            bool flag;
            if (roleActivity != null)
            {
                if (roleActivity.ContainsKey(parentId) && roleActivity.TryGetValue(parentId, out flag))
                    data = flag;
            }
            else
            {
                data = false;
            }

            return data;
        }

        [NonAction]
        private List<DocumentListItem> GetCheckedDocuments(List<DocumentListItem> documentListItems, List<string> checkedDocumentList = null)
        {
            if (checkedDocumentList != null)
            {
                foreach (var document in documentListItems)
                {
                    if (checkedDocumentList.Contains(document.DisplayName))
                    {
                        document.IsSelected = true;
                    }
                }
            }

            return documentListItems;
        }

        [NonAction]
        async Task<List<SelectListItem>> GetOrganizationList()
        {
            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            var list = new List<SelectListItem>();
            if (result == null)
            {
                return list;
            }
            else
            {
                foreach (var org in result)
                {
                    var orgobj = org.Split(",");
                    list.Add(new SelectListItem { Text = orgobj[0], Value = orgobj[1] });
                }

                return list;
            }
        }

        [HttpGet]
        public IActionResult ApproveOrganization(int id)
        {
            ApproveOrganizationConsentViewModel viewModel = new ApproveOrganizationConsentViewModel()
            {
                OrganizationFormId = id
            };
            return PartialView("_ApproveOrganizationConsent", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int orgFormId)
        {
            RejectOrganizationViewModel viewModel = new RejectOrganizationViewModel()
            {
                OrgFormId = orgFormId
            };

            var rejectReason = await _selfPortalService.GetRejectedReasonAsync();
            if (rejectReason.Success)
            {
                viewModel.RejectedReasonList = (IList<string>)rejectReason.Resource;
            }

            return PartialView("_RejectOrganization", viewModel);
        }
    }
}
