using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.PKIConfiguration;

using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Electronic Signature")]
    [Authorize(Roles = "Configuration")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class PKIConfigurationController : BaseController
    {
        private readonly IPKIConfigurationService _pkiConfigurationService;
        private readonly ILogger<PKIConfigurationController> _logger;

        public PKIConfigurationController(IPKIConfigurationService pkiConfigurationService,
            ILogger<PKIConfigurationController> logger,
            ILogClient logClient) : base(logClient)
        {
            _pkiConfigurationService = pkiConfigurationService;
            _logger = logger;
        }

        [HttpGet]
        //[Route("~/PKIConfigurations")]
        public async Task<IActionResult> List()
        {
            string logMessage;

            RemovePluginDataFromSession();
            var configurations = await _pkiConfigurationService.GetAllPluginConfigurationsAsync();
            if (configurations == null)
            {
                // Push the log to Admin Log Server
                logMessage = "Failed to get PKI Configuration list";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                    "Get all PKI Configuration list", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            PKIConfigurationListViewModel viewModel = new PKIConfigurationListViewModel
            {
                Configurations = configurations
            };

            // Push the log to Admin Log Server
            logMessage = "Successfully received PKI Configuration list";
            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                "Get all PKI Configuration list", LogMessageType.SUCCESS.GetValue(), logMessage);

            return View(viewModel);
        }

        [HttpGet]
        //[Route("Add")]
        public async Task<IActionResult> Add()
        {
            RemovePluginDataFromSession();
            var hsmSettingsAddViewModel = new HSMSettingsAddViewModel
            {
                HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync(),
                HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync(),
                KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync()
            };

            return View(hsmSettingsAddViewModel);
        }

        [HttpGet]
        //[Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            RemovePluginDataFromSession();
            var pkiPluginConfiguration = await _pkiConfigurationService.GetPluginConfigurationAsync(id);
            if (pkiPluginConfiguration == null)
            {
                return NotFound();
            }

            SetPluginDataInSession(pkiPluginConfiguration);

            HSMSettingsEditViewModel hsmSettingsEditViewModel = new HSMSettingsEditViewModel
            {
                CMAPIUrl = pkiPluginConfiguration.PkiHsmData.CmapiUrl,
                ClientPath = pkiPluginConfiguration.PkiHsmData.ClientPath,
                ClientEnvironmentPath = pkiPluginConfiguration.PkiHsmData.ClientEnvPath,
                AdminUserId = pkiPluginConfiguration.PkiHsmData.CmAdminUid,
                AdminPassword = pkiPluginConfiguration.PkiHsmData.CmAdminPwd,
                KeyGenerationTimeout = pkiPluginConfiguration.PkiHsmData.KeyGenerationTimeout,
                SlotId = pkiPluginConfiguration.PkiHsmData.SlotId,
                KeyAlgorithmId = pkiPluginConfiguration.PkiHsmData.KeyData.KeyAlgorithmId,
                KeySizeId = pkiPluginConfiguration.PkiHsmData.KeyData.KeySizeId,
                HashAlgorithmId = pkiPluginConfiguration.PkiHsmData.HashAlgorithmId,
                HSMPluginId = pkiPluginConfiguration.PkiHsmData.HsmPluginId,
                KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync(),
                HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync(),
                HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync()
            };

            return View(hsmSettingsEditViewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<JsonResult> GetKeysSize(int keyAlgorithmId)
        {
            var keysSize = await _pkiConfigurationService.GetAllKeysSizeForKeyAlgorithmIdAsync(keyAlgorithmId);

            List<SelectListItem> listItem = new List<SelectListItem>();

            foreach (var keySize in keysSize)
            {
                listItem.Add(new SelectListItem { Value = keySize.Id.ToString(), Text = keySize.Size });
            }
            return Json(keysSize);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GenerateConfiguration(int id)
        {
            string logMessage;

            var pluginData = await _pkiConfigurationService.GetCompletePluginConfigurationAsync(id);
            if (pluginData == null)
            {
                return Json(new { Status = "Failed", Title = "Generate Configuration", Message = "Configuration data not found" });
            }

            try
            {
                JObject pkiConfigData = CreatePKIConfigJsonData(pluginData);
                if (pkiConfigData == null)
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Failed to create json data for the configuration";
                    SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                        "Generate Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                    return Json(new { Status = "Failed", Title = "Generate Configuration", Message = "Failed to create json data for the configuration" });
                }
                else
                {
                    string wrappedData = _pkiConfigurationService.WrapData(pkiConfigData.ToString());
                    if (wrappedData == null)
                    {
                        // Push the log to Admin Log Server
                        logMessage = $"Failed to encrypt configuration JSON data";
                        SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                            "Generate Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                        return Json(new { Status = "Failed", Title = "Generate Configuration", Message = "Failed to encrypt the data" });
                    }
                    else
                    {
                        var response = await _pkiConfigurationService.UpdateEncryptedPluginConfigurationAsync(wrappedData, UUID);
                        if (!response.Success)
                        {
                            // Push the log to Admin Log Server
                            logMessage = $"Failed to generate PKI configuration";
                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Generate Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                            return Json(new { Status = "Failed", Title = "Generate Configuration", Message = response.Message });
                        }
                        else
                        {
                            // Push the log to Admin Log Server
                            logMessage = $"Successfully generated PKI configuration";
                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Generate Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                            return Json(new { Status = "Success", Title = "Generate Configuration", Message = response.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Json(new { Status = "Failed", Title = "Generate Configuration", Message = "Failed to generate configuration data" });
            }
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> AddHSMSettings(HSMSettingsAddViewModel hsmSettingsAddViewModel, string prevBtn, string nextBtn)
        {
            try
            {
                if (nextBtn != null)
                {
                    if (ModelState.IsValid)
                    {
                        PkiPluginDatum pluginData = GetPluginData();

                        PkiKeyDatum pkiKeyDatum = new PkiKeyDatum
                        {
                            KeyAlgorithmId = hsmSettingsAddViewModel.KeyAlgorithmId.Value,
                            KeySizeId = hsmSettingsAddViewModel.KeySizeId.Value
                        };

                        PkiHsmDatum pkiHsmDatum = new PkiHsmDatum
                        {
                            CmapiUrl = hsmSettingsAddViewModel.CMAPIUrl,
                            ClientPath = hsmSettingsAddViewModel.ClientPath,
                            ClientEnvPath = hsmSettingsAddViewModel.ClientEnvironmentPath,
                            CmAdminUid = hsmSettingsAddViewModel.AdminUserId,
                            CmAdminPwd = hsmSettingsAddViewModel.AdminPassword,
                            KeyGenerationTimeout = hsmSettingsAddViewModel.KeyGenerationTimeout.Value,
                            SlotId = hsmSettingsAddViewModel.SlotId.Value,
                            HashAlgorithmId = hsmSettingsAddViewModel.HashAlgorithmId.Value,
                            HsmPluginId = hsmSettingsAddViewModel.HSMPluginId.Value,
                            KeyData = pkiKeyDatum
                        };
                        pluginData.PkiHsmData = pkiHsmDatum;

                        CASettingsAddViewModel caSettingsAddViewModel;
                        if (pluginData.PkiCaData != null)
                        {
                            caSettingsAddViewModel = new CASettingsAddViewModel
                            {
                                Url = pluginData.PkiCaData.Url,
                                CertificateAuthority = pluginData.PkiCaData.CertificateAuthority,
                                EndEntityProfileName = pluginData.PkiCaData.EndEntityProfileName,
                                CertificateProfileName = pluginData.PkiCaData.CertificateProfileName,
                                ClientAuthenticationCertificate = pluginData.PkiCaData.ClientAuthCertificate,
                                ClientAuthenticationCertificatePassword = pluginData.PkiCaData.ClientAuthCertificatePassword,
                                IssuerDn = pluginData.PkiCaData.IssuerDn,
                                CertificateValidity = pluginData.PkiCaData.CertificateValidity,
                                ProcedureId = pluginData.PkiCaData.ProcedureId,
                                CAPluginId = pluginData.PkiCaData.CaPluginId,
                                StatgingCertificateProcedureRSA256 = pluginData.PkiCaData.StagingCertProcedureRsa256,
                                StatgingCertificateProcedureRSA512 = pluginData.PkiCaData.StagingCertProcedureRsa512,
                                StatgingCertificateProcedureEC256 = pluginData.PkiCaData.StagingCertProcedureEc256,
                                StatgingCertificateProcedureEC512 = pluginData.PkiCaData.StagingCertProcedureEc512,
                                TestCertificateProcedureRSA256 = pluginData.PkiCaData.TestCertProcedureRsa256,
                                TestCertificateProcedureRSA512 = pluginData.PkiCaData.TestCertProcedureRsa512,
                                TestCertificateProcedureEC256 = pluginData.PkiCaData.TestCertProcedureEc256,
                                TestCertificateProcedureEC512 = pluginData.PkiCaData.TestCertProcedureEc512,
                                Procedures = await _pkiConfigurationService.GetAllProceduresAsync(),
                                CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync()
                            };
                        }
                        else
                        {
                            caSettingsAddViewModel = new CASettingsAddViewModel()
                            {
                                Procedures = await _pkiConfigurationService.GetAllProceduresAsync(),
                                CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync()
                            };
                        }

                        SetPluginDataInSession(pluginData);
                        return PartialView("_AddCASettings", caSettingsAddViewModel);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }

            hsmSettingsAddViewModel.HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync();
            hsmSettingsAddViewModel.HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync();
            hsmSettingsAddViewModel.KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync();
            return PartialView("_AddHSMSettings", hsmSettingsAddViewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> AddCASettings(CASettingsAddViewModel caSettingsAddViewModel, string prevBtn, string nextBtn)
        {
            try
            {
                PkiPluginDatum pluginData = GetPluginData();
                if (prevBtn != null)
                {
                    HSMSettingsAddViewModel hsmSettingsAddViewModel = new HSMSettingsAddViewModel
                    {
                        CMAPIUrl = pluginData.PkiHsmData.CmapiUrl,
                        ClientEnvironmentPath = pluginData.PkiHsmData.ClientEnvPath,
                        ClientPath = pluginData.PkiHsmData.ClientPath,
                        AdminUserId = pluginData.PkiHsmData.CmAdminUid,
                        AdminPassword = pluginData.PkiHsmData.CmAdminPwd,
                        KeyGenerationTimeout = pluginData.PkiHsmData.KeyGenerationTimeout,
                        SlotId = pluginData.PkiHsmData.SlotId,
                        KeyAlgorithmId = pluginData.PkiHsmData.KeyData.KeyAlgorithmId,
                        KeySizeId = pluginData.PkiHsmData.KeyData.KeySizeId,
                        HashAlgorithmId = pluginData.PkiHsmData.HashAlgorithmId,
                        HSMPluginId = pluginData.PkiHsmData.HsmPluginId,
                        KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync(),
                        HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync(),
                        HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync()
                    };
                    return PartialView("_AddHSMSettings", hsmSettingsAddViewModel);
                }
                if (nextBtn != null)
                {
                    if (ModelState.IsValid)
                    {
                        PkiCaDatum pkiCaDatum = new PkiCaDatum
                        {
                            Url = caSettingsAddViewModel.Url,
                            CertificateAuthority = caSettingsAddViewModel.CertificateAuthority,
                            EndEntityProfileName = caSettingsAddViewModel.EndEntityProfileName,
                            CertificateProfileName = caSettingsAddViewModel.CertificateProfileName,
                            ClientAuthCertificate = caSettingsAddViewModel.ClientAuthenticationCertificate,
                            ClientAuthCertificatePassword = caSettingsAddViewModel.ClientAuthenticationCertificatePassword,
                            IssuerDn = caSettingsAddViewModel.IssuerDn,
                            CertificateValidity = caSettingsAddViewModel.CertificateValidity.Value,
                            ProcedureId = caSettingsAddViewModel.ProcedureId.Value,
                            CaPluginId = caSettingsAddViewModel.CAPluginId.Value,
                            StagingCertProcedureRsa256 = caSettingsAddViewModel.StatgingCertificateProcedureRSA256,
                            StagingCertProcedureRsa512 = caSettingsAddViewModel.StatgingCertificateProcedureRSA512,
                            StagingCertProcedureEc256 = caSettingsAddViewModel.StatgingCertificateProcedureEC256,
                            StagingCertProcedureEc512 = caSettingsAddViewModel.StatgingCertificateProcedureEC512,
                            TestCertProcedureRsa256 = caSettingsAddViewModel.TestCertificateProcedureRSA256,
                            TestCertProcedureRsa512 = caSettingsAddViewModel.TestCertificateProcedureRSA512,
                            TestCertProcedureEc256 = caSettingsAddViewModel.TestCertificateProcedureEC256,
                            TestCertProcedureEc512 = caSettingsAddViewModel.TestCertificateProcedureEC512
                        };

                        pluginData.PkiCaData = pkiCaDatum;
                        if (caSettingsAddViewModel.SigningCertificateIssuerFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.SigningCertificateIssuerFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateIssuer = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsAddViewModel.SigningCertificateRootFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.SigningCertificateRootFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateRoot = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsAddViewModel.OCSPSignerCertificateFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.OCSPSignerCertificateFile.CopyTo(stream);
                                pluginData.PkiCaData.OcspSignerCertificate = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsAddViewModel.SigningCertificateChainFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.SigningCertificateChainFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateChain = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsAddViewModel.TimestampingCertificateFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.TimestampingCertificateFile.CopyTo(stream);
                                pluginData.PkiCaData.TimestampingCertificate = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsAddViewModel.TimestampingCertificateChainFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsAddViewModel.TimestampingCertificateChainFile.CopyTo(stream);
                                pluginData.PkiCaData.TimestampingCertificateChain = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        OtherSettingsAddViewModel otherSettingsAddViewModel;
                        if (pluginData.PkiServerConfigurationData != null)
                        {
                            otherSettingsAddViewModel = new OtherSettingsAddViewModel()
                            {
                                LogPath = pluginData.PkiServerConfigurationData.LogPath,
                                LogLevel = pluginData.PkiServerConfigurationData.LogLevel,
                                ConfigDirectoryPath = pluginData.PkiServerConfigurationData.ConfigDirectoryPath,
                                LogQueueIP = pluginData.PkiServerConfigurationData.LogQueueIp,
                                LogQueuePort = pluginData.PkiServerConfigurationData.LogQueuePort,
                                LogQueueUserName = pluginData.PkiServerConfigurationData.LogQueueUsername,
                                LogQueuePassword = pluginData.PkiServerConfigurationData.LogQueuePassword,
                                Jre64BitDirectory = pluginData.PkiServerConfigurationData.Jre64Directory,
                                IDPUrl = pluginData.PkiServerConfigurationData.IdpUrl,
                                TSAUrl = pluginData.PkiServerConfigurationData.TsaUrl,
                                OCSPUrl = pluginData.PkiServerConfigurationData.OcspUrl,
                                PKIServiceUrl = pluginData.PkiServerConfigurationData.PkiServiceUrl,
                                SignatureServiceUrl = pluginData.PkiServerConfigurationData.SignatureServiceUrl,
                                ClientId = pluginData.PkiServerConfigurationData.ClientId,
                                ClientSecret = pluginData.PkiServerConfigurationData.ClientSecret,
                                EnableDss = pluginData.PkiServerConfigurationData.EnableDss,
                                DssClient = pluginData.PkiServerConfigurationData.DssClient,
                                SignLocally = pluginData.PkiServerConfigurationData.SignLocally,
                                LogCallStack = pluginData.PkiServerConfigurationData.LogCallstack,
                                StagingEnvironment = pluginData.PkiServerConfigurationData.StagingEnv,
                                Introspect = pluginData.PkiServerConfigurationData.Introspect,
                                HandSignature = pluginData.PkiServerConfigurationData.HandSignature,
                                SigningLogQueue = pluginData.PkiServerConfigurationData.SigningLogQueue,
                                RALogQueue = pluginData.PkiServerConfigurationData.RaLogQueue,
                                CentralLogQueue = pluginData.PkiServerConfigurationData.CentralLogQueue
                            };
                        }
                        else
                        {
                            otherSettingsAddViewModel = new OtherSettingsAddViewModel();
                        }

                        SetPluginDataInSession(pluginData);
                        return PartialView("_AddOtherSettings", otherSettingsAddViewModel);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }
            caSettingsAddViewModel.CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync();
            caSettingsAddViewModel.Procedures = await _pkiConfigurationService.GetAllProceduresAsync();
            return PartialView("_AddCASettings", caSettingsAddViewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOtherSettings(OtherSettingsAddViewModel otherSettingsAddViewModel, string prevBtn, string nextBtn)
        {
            string logMessage;

            try
            {
                PkiPluginDatum pluginData = GetPluginData();
                if (prevBtn != null)
                {
                    CASettingsAddViewModel caSettingsAddViewModel = new CASettingsAddViewModel
                    {
                        Url = pluginData.PkiCaData.Url,
                        CertificateAuthority = pluginData.PkiCaData.CertificateAuthority,
                        EndEntityProfileName = pluginData.PkiCaData.EndEntityProfileName,
                        CertificateProfileName = pluginData.PkiCaData.CertificateProfileName,
                        ClientAuthenticationCertificate = pluginData.PkiCaData.ClientAuthCertificate,
                        ClientAuthenticationCertificatePassword = pluginData.PkiCaData.ClientAuthCertificatePassword,
                        IssuerDn = pluginData.PkiCaData.IssuerDn,
                        CertificateValidity = pluginData.PkiCaData.CertificateValidity,
                        ProcedureId = pluginData.PkiCaData.ProcedureId,
                        CAPluginId = pluginData.PkiCaData.CaPluginId,
                        StatgingCertificateProcedureRSA256 = pluginData.PkiCaData.StagingCertProcedureRsa256,
                        StatgingCertificateProcedureRSA512 = pluginData.PkiCaData.StagingCertProcedureRsa512,
                        StatgingCertificateProcedureEC256 = pluginData.PkiCaData.StagingCertProcedureEc256,
                        StatgingCertificateProcedureEC512 = pluginData.PkiCaData.StagingCertProcedureEc512,
                        TestCertificateProcedureRSA256 = pluginData.PkiCaData.TestCertProcedureRsa256,
                        TestCertificateProcedureRSA512 = pluginData.PkiCaData.TestCertProcedureRsa512,
                        TestCertificateProcedureEC256 = pluginData.PkiCaData.TestCertProcedureEc256,
                        TestCertificateProcedureEC512 = pluginData.PkiCaData.TestCertProcedureEc512,
                        Procedures = await _pkiConfigurationService.GetAllProceduresAsync(),
                        CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync()
                    };

                    return PartialView("_AddCASettings", caSettingsAddViewModel);
                }
                if (nextBtn != null)
                {
                    ServiceResult response;
                    if (ModelState.IsValid)
                    {
                        PkiServerConfigurationDatum pkiServerConfigurationDatum = new PkiServerConfigurationDatum
                        {
                            LogPath = otherSettingsAddViewModel.LogPath,
                            LogLevel = otherSettingsAddViewModel.LogLevel,
                            ConfigDirectoryPath = otherSettingsAddViewModel.ConfigDirectoryPath,
                            LogQueueIp = otherSettingsAddViewModel.LogQueueIP,
                            LogQueuePort = otherSettingsAddViewModel.LogQueuePort.Value,
                            LogQueueUsername = otherSettingsAddViewModel.LogQueueUserName,
                            LogQueuePassword = otherSettingsAddViewModel.LogQueuePassword,
                            Jre64Directory = otherSettingsAddViewModel.Jre64BitDirectory,
                            IdpUrl = otherSettingsAddViewModel.IDPUrl,
                            TsaUrl = otherSettingsAddViewModel.TSAUrl,
                            OcspUrl = otherSettingsAddViewModel.OCSPUrl,
                            PkiServiceUrl = otherSettingsAddViewModel.PKIServiceUrl,
                            SignatureServiceUrl = otherSettingsAddViewModel.SignatureServiceUrl,
                            ClientId = otherSettingsAddViewModel.ClientId,
                            ClientSecret = otherSettingsAddViewModel.ClientSecret,
                            EnableDss = otherSettingsAddViewModel.EnableDss,
                            DssClient = otherSettingsAddViewModel.DssClient,
                            SignLocally = otherSettingsAddViewModel.SignLocally,
                            LogCallstack = otherSettingsAddViewModel.LogCallStack,
                            StagingEnv = otherSettingsAddViewModel.StagingEnvironment,
                            Introspect = otherSettingsAddViewModel.Introspect,
                            HandSignature = otherSettingsAddViewModel.HandSignature,
                            SigningLogQueue = otherSettingsAddViewModel.SigningLogQueue,
                            RaLogQueue = otherSettingsAddViewModel.RALogQueue,
                            CentralLogQueue = otherSettingsAddViewModel.CentralLogQueue
                        };

                        pluginData.PkiServerConfigurationData = pkiServerConfigurationDatum;
                        if (otherSettingsAddViewModel.SignatureImageFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                otherSettingsAddViewModel.SignatureImageFile.CopyTo(stream);
                                pluginData.PkiServerConfigurationData.SignatureImage = stream.ToArray();
                            }
                        }

                        SetPluginDataInSession(pluginData);

                        pluginData.CreatedBy = UUID;
                        response = await _pkiConfigurationService.AddPluginConfigurationAsync(pluginData);

                        if (!response.Success)
                        {
                            // Push the log to Admin Log Server
                            logMessage = $"Failed to create PKI configuration";
                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Create PKI Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                            ModelState.AddModelError("", response.Message);
                            return PartialView("_AddOtherSettings", otherSettingsAddViewModel);
                        }
                        else
                        {
                            // Push the log to Admin Log Server
                            if (response.Message == "Your request has sent for approval")
                                logMessage = $"Request for create PKI configuration has sent for approval";
                            else
                                logMessage = $"Successfully created PKI configuration";
                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Create PKI Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                            RemovePluginDataFromSession();
                            AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                            TempData["Alert"] = JsonConvert.SerializeObject(alert);
                            return Json(new { url = Url.Action("List", "PKIConfiguration") });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }
            return PartialView("_AddOtherSettings", otherSettingsAddViewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> EditHSMSettings(HSMSettingsEditViewModel hsmSettingsEditViewModel, string prevBtn, string nextBtn)
        {
            try
            {
                if (nextBtn != null)
                {
                    if (ModelState.IsValid)
                    {
                        PkiPluginDatum pluginData = GetPluginData();

                        pluginData.PkiHsmData.KeyData.KeyAlgorithmId = hsmSettingsEditViewModel.KeyAlgorithmId.Value;
                        pluginData.PkiHsmData.KeyData.KeySizeId = hsmSettingsEditViewModel.KeySizeId.Value;
                        pluginData.PkiHsmData.CmapiUrl = hsmSettingsEditViewModel.CMAPIUrl;
                        pluginData.PkiHsmData.ClientPath = hsmSettingsEditViewModel.ClientPath;
                        pluginData.PkiHsmData.ClientEnvPath = hsmSettingsEditViewModel.ClientEnvironmentPath;
                        pluginData.PkiHsmData.CmAdminUid = hsmSettingsEditViewModel.AdminUserId;
                        pluginData.PkiHsmData.CmAdminPwd = hsmSettingsEditViewModel.AdminPassword;
                        pluginData.PkiHsmData.KeyGenerationTimeout = hsmSettingsEditViewModel.KeyGenerationTimeout.Value;
                        pluginData.PkiHsmData.SlotId = hsmSettingsEditViewModel.SlotId.Value;
                        pluginData.PkiHsmData.HashAlgorithmId = hsmSettingsEditViewModel.HashAlgorithmId.Value;
                        pluginData.PkiHsmData.HsmPluginId = hsmSettingsEditViewModel.HSMPluginId.Value;

                        CASettingsEditViewModel caSettingsEditViewModel = new CASettingsEditViewModel
                        {
                            Url = pluginData.PkiCaData.Url,
                            CertificateAuthority = pluginData.PkiCaData.CertificateAuthority,
                            EndEntityProfileName = pluginData.PkiCaData.EndEntityProfileName,
                            CertificateProfileName = pluginData.PkiCaData.CertificateProfileName,
                            ClientAuthenticationCertificate = pluginData.PkiCaData.ClientAuthCertificate,
                            ClientAuthenticationCertificatePassword = pluginData.PkiCaData.ClientAuthCertificatePassword,
                            IssuerDn = pluginData.PkiCaData.IssuerDn,
                            CertificateValidity = pluginData.PkiCaData.CertificateValidity,
                            ProcedureId = pluginData.PkiCaData.ProcedureId,
                            CAPluginId = pluginData.PkiCaData.CaPluginId,
                            StatgingCertificateProcedureRSA256 = pluginData.PkiCaData.StagingCertProcedureRsa256,
                            StatgingCertificateProcedureRSA512 = pluginData.PkiCaData.StagingCertProcedureRsa512,
                            StatgingCertificateProcedureEC256 = pluginData.PkiCaData.StagingCertProcedureEc256,
                            StatgingCertificateProcedureEC512 = pluginData.PkiCaData.StagingCertProcedureEc512,
                            TestCertificateProcedureRSA256 = pluginData.PkiCaData.TestCertProcedureRsa256,
                            TestCertificateProcedureRSA512 = pluginData.PkiCaData.TestCertProcedureRsa512,
                            TestCertificateProcedureEC256 = pluginData.PkiCaData.TestCertProcedureEc256,
                            TestCertificateProcedureEC512 = pluginData.PkiCaData.TestCertProcedureEc512,
                            Procedures = await _pkiConfigurationService.GetAllProceduresAsync(),
                            CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync()
                        };

                        SetPluginDataInSession(pluginData);
                        return PartialView("_EditCASettings", caSettingsEditViewModel);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }

            hsmSettingsEditViewModel.HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync();
            hsmSettingsEditViewModel.HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync();
            hsmSettingsEditViewModel.KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync();
            return PartialView("_EditHSMSettings", hsmSettingsEditViewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> EditCASettings(CASettingsEditViewModel caSettingsEditViewModel, string prevBtn, string nextBtn)
        {
            try
            {
                PkiPluginDatum pluginData = GetPluginData();
                if (prevBtn != null)
                {
                    HSMSettingsEditViewModel hsmSettingsEditViewModel = new HSMSettingsEditViewModel
                    {
                        CMAPIUrl = pluginData.PkiHsmData.CmapiUrl,
                        ClientPath = pluginData.PkiHsmData.ClientPath,
                        ClientEnvironmentPath = pluginData.PkiHsmData.ClientEnvPath,
                        AdminUserId = pluginData.PkiHsmData.CmAdminUid,
                        AdminPassword = pluginData.PkiHsmData.CmAdminPwd,
                        KeyGenerationTimeout = pluginData.PkiHsmData.KeyGenerationTimeout,
                        SlotId = pluginData.PkiHsmData.SlotId,
                        KeyAlgorithmId = pluginData.PkiHsmData.KeyData.KeyAlgorithmId,
                        KeySizeId = pluginData.PkiHsmData.KeyData.KeySizeId,
                        HashAlgorithmId = pluginData.PkiHsmData.HashAlgorithmId,
                        HSMPluginId = pluginData.PkiHsmData.HsmPluginId,
                        KeyAlgorithms = await _pkiConfigurationService.GetAllKeyAlgorithmsAsync(),
                        HashAlgorithms = await _pkiConfigurationService.GetAllHashAlgorithmsAsync(),
                        HSMPlugins = await _pkiConfigurationService.GetAllHSMPluginsAsync()
                    };
                    return PartialView("_EditHSMSettings", hsmSettingsEditViewModel);
                }
                if (nextBtn != null)
                {
                    if (ModelState.IsValid)
                    {
                        pluginData.PkiCaData.Url = caSettingsEditViewModel.Url;
                        pluginData.PkiCaData.CertificateAuthority = caSettingsEditViewModel.CertificateAuthority;
                        pluginData.PkiCaData.EndEntityProfileName = caSettingsEditViewModel.EndEntityProfileName;
                        pluginData.PkiCaData.CertificateProfileName = caSettingsEditViewModel.CertificateProfileName;
                        pluginData.PkiCaData.ClientAuthCertificate = caSettingsEditViewModel.ClientAuthenticationCertificate;
                        pluginData.PkiCaData.ClientAuthCertificatePassword = caSettingsEditViewModel.ClientAuthenticationCertificatePassword;
                        pluginData.PkiCaData.IssuerDn = caSettingsEditViewModel.IssuerDn;
                        pluginData.PkiCaData.CertificateValidity = caSettingsEditViewModel.CertificateValidity.Value;
                        pluginData.PkiCaData.ProcedureId = caSettingsEditViewModel.ProcedureId.Value;
                        pluginData.PkiCaData.CaPluginId = caSettingsEditViewModel.CAPluginId.Value;
                        pluginData.PkiCaData.StagingCertProcedureRsa256 = caSettingsEditViewModel.StatgingCertificateProcedureRSA256;
                        pluginData.PkiCaData.StagingCertProcedureRsa512 = caSettingsEditViewModel.StatgingCertificateProcedureRSA512;
                        pluginData.PkiCaData.StagingCertProcedureEc256 = caSettingsEditViewModel.TestCertificateProcedureEC256;
                        pluginData.PkiCaData.StagingCertProcedureEc512 = caSettingsEditViewModel.TestCertificateProcedureEC512;
                        pluginData.PkiCaData.TestCertProcedureRsa256 = caSettingsEditViewModel.TestCertificateProcedureRSA256;
                        pluginData.PkiCaData.TestCertProcedureRsa512 = caSettingsEditViewModel.TestCertificateProcedureRSA512;
                        pluginData.PkiCaData.TestCertProcedureEc256 = caSettingsEditViewModel.TestCertificateProcedureEC256;
                        pluginData.PkiCaData.TestCertProcedureEc512 = caSettingsEditViewModel.TestCertificateProcedureEC512;

                        if (caSettingsEditViewModel.SigningCertificateIssuerFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.SigningCertificateIssuerFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateIssuer = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsEditViewModel.SigningCertificateRootFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.SigningCertificateRootFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateRoot = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsEditViewModel.OCSPSignerCertificateFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.OCSPSignerCertificateFile.CopyTo(stream);
                                pluginData.PkiCaData.OcspSignerCertificate = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsEditViewModel.SigningCertificateChainFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.SigningCertificateChainFile.CopyTo(stream);
                                pluginData.PkiCaData.SigningCertificateChain = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsEditViewModel.TimestampingCertificateFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.TimestampingCertificateFile.CopyTo(stream);
                                pluginData.PkiCaData.TimestampingCertificate = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        if (caSettingsEditViewModel.TimestampingCertificateChainFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                caSettingsEditViewModel.TimestampingCertificateChainFile.CopyTo(stream);
                                pluginData.PkiCaData.TimestampingCertificateChain = Convert.ToBase64String(stream.ToArray());
                            }
                        }

                        OtherSettingsEditViewModel otherSettingsEditViewModel = new OtherSettingsEditViewModel()
                        {
                            LogPath = pluginData.PkiServerConfigurationData.LogPath,
                            LogLevel = pluginData.PkiServerConfigurationData.LogLevel,
                            ConfigDirectoryPath = pluginData.PkiServerConfigurationData.ConfigDirectoryPath,
                            LogQueueIP = pluginData.PkiServerConfigurationData.LogQueueIp,
                            LogQueuePort = pluginData.PkiServerConfigurationData.LogQueuePort,
                            LogQueueUserName = pluginData.PkiServerConfigurationData.LogQueueUsername,
                            LogQueuePassword = pluginData.PkiServerConfigurationData.LogQueuePassword,
                            Jre64BitDirectory = pluginData.PkiServerConfigurationData.Jre64Directory,
                            IDPUrl = pluginData.PkiServerConfigurationData.IdpUrl,
                            TSAUrl = pluginData.PkiServerConfigurationData.TsaUrl,
                            OCSPUrl = pluginData.PkiServerConfigurationData.OcspUrl,
                            PKIServiceUrl = pluginData.PkiServerConfigurationData.PkiServiceUrl,
                            SignatureServiceUrl = pluginData.PkiServerConfigurationData.SignatureServiceUrl,
                            ClientId = pluginData.PkiServerConfigurationData.ClientId,
                            ClientSecret = pluginData.PkiServerConfigurationData.ClientSecret,
                            EnableDss = pluginData.PkiServerConfigurationData.EnableDss,
                            DssClient = pluginData.PkiServerConfigurationData.DssClient,
                            SignLocally = pluginData.PkiServerConfigurationData.SignLocally,
                            LogCallStack = pluginData.PkiServerConfigurationData.LogCallstack,
                            StagingEnvironment = pluginData.PkiServerConfigurationData.StagingEnv,
                            Introspect = pluginData.PkiServerConfigurationData.Introspect,
                            HandSignature = pluginData.PkiServerConfigurationData.HandSignature,
                            SigningLogQueue = pluginData.PkiServerConfigurationData.SigningLogQueue,
                            RALogQueue = pluginData.PkiServerConfigurationData.RaLogQueue,
                            CentralLogQueue = pluginData.PkiServerConfigurationData.CentralLogQueue
                        };

                        SetPluginDataInSession(pluginData);
                        return PartialView("_EditOtherSettings", otherSettingsEditViewModel);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }
            caSettingsEditViewModel.CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync();
            caSettingsEditViewModel.Procedures = await _pkiConfigurationService.GetAllProceduresAsync();
            return PartialView("_EditCASettings", caSettingsEditViewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOtherSettings(OtherSettingsEditViewModel otherSettingsEditViewModel, string prevBtn, string nextBtn)
        {
            string logMessage;

            try
            {
                PkiPluginDatum pluginData = GetPluginData();
                if (prevBtn != null)
                {
                    CASettingsEditViewModel caSettingsEditViewModel = new CASettingsEditViewModel
                    {
                        Url = pluginData.PkiCaData.Url,
                        CertificateAuthority = pluginData.PkiCaData.CertificateAuthority,
                        EndEntityProfileName = pluginData.PkiCaData.EndEntityProfileName,
                        CertificateProfileName = pluginData.PkiCaData.CertificateProfileName,
                        ClientAuthenticationCertificate = pluginData.PkiCaData.ClientAuthCertificate,
                        ClientAuthenticationCertificatePassword = pluginData.PkiCaData.ClientAuthCertificatePassword,
                        IssuerDn = pluginData.PkiCaData.IssuerDn,
                        CertificateValidity = pluginData.PkiCaData.CertificateValidity,
                        ProcedureId = pluginData.PkiCaData.ProcedureId,
                        CAPluginId = pluginData.PkiCaData.CaPluginId,
                        StatgingCertificateProcedureRSA256 = pluginData.PkiCaData.StagingCertProcedureRsa256,
                        StatgingCertificateProcedureRSA512 = pluginData.PkiCaData.StagingCertProcedureRsa512,
                        StatgingCertificateProcedureEC256 = pluginData.PkiCaData.StagingCertProcedureEc256,
                        StatgingCertificateProcedureEC512 = pluginData.PkiCaData.StagingCertProcedureEc512,
                        TestCertificateProcedureRSA256 = pluginData.PkiCaData.TestCertProcedureRsa256,
                        TestCertificateProcedureRSA512 = pluginData.PkiCaData.TestCertProcedureRsa512,
                        TestCertificateProcedureEC256 = pluginData.PkiCaData.TestCertProcedureEc256,
                        TestCertificateProcedureEC512 = pluginData.PkiCaData.TestCertProcedureEc512,
                        Procedures = await _pkiConfigurationService.GetAllProceduresAsync(),
                        CAPlugins = await _pkiConfigurationService.GetAllCAPluginsAsync()
                    };

                    return PartialView("_EditCASettings", caSettingsEditViewModel);
                }
                if (nextBtn != null)
                {
                    ServiceResult response;
                    if (ModelState.IsValid)
                    {
                        pluginData.PkiServerConfigurationData.LogPath = otherSettingsEditViewModel.LogPath;
                        pluginData.PkiServerConfigurationData.LogLevel = otherSettingsEditViewModel.LogLevel;
                        pluginData.PkiServerConfigurationData.ConfigDirectoryPath = otherSettingsEditViewModel.ConfigDirectoryPath;
                        pluginData.PkiServerConfigurationData.LogQueueIp = otherSettingsEditViewModel.LogQueueIP;
                        pluginData.PkiServerConfigurationData.LogQueuePort = otherSettingsEditViewModel.LogQueuePort.Value;
                        pluginData.PkiServerConfigurationData.LogQueueUsername = otherSettingsEditViewModel.LogQueueUserName;
                        pluginData.PkiServerConfigurationData.LogQueuePassword = otherSettingsEditViewModel.LogQueuePassword;
                        pluginData.PkiServerConfigurationData.Jre64Directory = otherSettingsEditViewModel.Jre64BitDirectory;
                        pluginData.PkiServerConfigurationData.IdpUrl = otherSettingsEditViewModel.IDPUrl;
                        pluginData.PkiServerConfigurationData.TsaUrl = otherSettingsEditViewModel.TSAUrl;
                        pluginData.PkiServerConfigurationData.OcspUrl = otherSettingsEditViewModel.OCSPUrl;
                        pluginData.PkiServerConfigurationData.PkiServiceUrl = otherSettingsEditViewModel.PKIServiceUrl;
                        pluginData.PkiServerConfigurationData.SignatureServiceUrl = otherSettingsEditViewModel.SignatureServiceUrl;
                        pluginData.PkiServerConfigurationData.ClientId = otherSettingsEditViewModel.ClientId;
                        pluginData.PkiServerConfigurationData.ClientSecret = otherSettingsEditViewModel.ClientSecret;
                        pluginData.PkiServerConfigurationData.EnableDss = otherSettingsEditViewModel.EnableDss;
                        pluginData.PkiServerConfigurationData.DssClient = otherSettingsEditViewModel.DssClient;
                        pluginData.PkiServerConfigurationData.SignLocally = otherSettingsEditViewModel.SignLocally;
                        pluginData.PkiServerConfigurationData.LogCallstack = otherSettingsEditViewModel.LogCallStack;
                        pluginData.PkiServerConfigurationData.StagingEnv = otherSettingsEditViewModel.StagingEnvironment;
                        pluginData.PkiServerConfigurationData.Introspect = otherSettingsEditViewModel.Introspect;
                        pluginData.PkiServerConfigurationData.HandSignature = otherSettingsEditViewModel.HandSignature;
                        pluginData.PkiServerConfigurationData.SigningLogQueue = otherSettingsEditViewModel.SigningLogQueue;
                        pluginData.PkiServerConfigurationData.RaLogQueue = otherSettingsEditViewModel.RALogQueue;
                        pluginData.PkiServerConfigurationData.CentralLogQueue = otherSettingsEditViewModel.CentralLogQueue;

                        if (otherSettingsEditViewModel.SignatureImageFile != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                otherSettingsEditViewModel.SignatureImageFile.CopyTo(stream);
                                pluginData.PkiServerConfigurationData.SignatureImage = stream.ToArray();
                            }
                        }

                        SetPluginDataInSession(pluginData);

                        pluginData.UpdatedBy = UUID;
                        response = await _pkiConfigurationService.UpdatePluginConfigurationAsync(pluginData);

                        if (!response.Success)
                        {
                            // Push the log to Admin Log Server
                            logMessage = $"Failed to update pki configuration";
                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Update PKI Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                            ModelState.AddModelError("", response.Message);
                            return PartialView("_EditOtherSettings", otherSettingsEditViewModel);
                        }
                        else
                        {
                            // Push the log to Admin Log Server
                            if (response.Message == "Your request has sent for approval")
                                logMessage = $"Request for update PKI configuration has sent for approval";
                            else
                                logMessage = $"Successfully updated PKI configuration";

                            SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                                "Update PKI Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                            RemovePluginDataFromSession();
                            AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                            TempData["Alert"] = JsonConvert.SerializeObject(alert);
                            return Json(new { url = Url.Action("List", "PKIConfiguration") });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please fill all the required fields");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", "Something went wrong, please reload the page and start again.");
            }
            return PartialView("_EditOtherSettings", otherSettingsEditViewModel);
        }

        [HttpPost]
        //[Route("Enable/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Enable(int id)
        {
            string logMessage;

            var response = await _pkiConfigurationService.EnablePluginConfigurationAsync(id, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to enable PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                    "Enable PKI Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Enable PKI Configuration", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for enable PKI configuration has sent for approval";
                else
                    logMessage = $"Successfully enabled PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                   "Enable PKI Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Enable PKI Configuration", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("Disable/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Disable(int id)
        {
            string logMessage;

            var response = await _pkiConfigurationService.DisablePluginConfigurationAsync(id, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to disable PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                    "Disable PKI Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Disable PKI Configuration", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for disable PKI configuration has sent for approval";
                else
                    logMessage = $"Successfully disable PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                   "Disable PKI Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Disable PKI Configuration", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            string logMessage;

            var response = await _pkiConfigurationService.DeletePluginConfigurationAsync(id, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to delete PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                    "Delete PKI Configuration", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Delete PKI Configuration", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for delete PKI configuration has sent for approval";
                else
                    logMessage = $"Successfully deleted PKI Configuration";
                SendAdminLog(ModuleNameConstants.ElectronicSignatures, ServiceNameConstants.Configuration,
                   "Delete PKI Configuration", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Delete PKI Configuration", Message = response.Message });
            }
        }

        [NonAction]
        private JObject CreatePKIConfigJsonData(PkiPluginDatum pluginData)
        {
            JObject root = null;
            try
            {
                root = new JObject {
                        new JProperty("log_path", pluginData.PkiServerConfigurationData.LogPath ?? ""),
                        new JProperty("log_level", pluginData.PkiServerConfigurationData.LogLevel ?? ""),
                        new JProperty("config_directory_path", pluginData.PkiServerConfigurationData.ConfigDirectoryPath ?? ""),
                        new JProperty("hsm_plugin", pluginData.PkiHsmData.HsmPlugin.Name ?? ""),
                        new JProperty("ca_plugin", pluginData.PkiCaData.CaPlugin.Name ?? ""),
                        new JProperty("log_queue_ip", pluginData.PkiServerConfigurationData.LogQueueIp ?? ""),
                        new JProperty("log_queue_port", pluginData.PkiServerConfigurationData.LogQueuePort),
                        new JProperty("log_queue_username", pluginData.PkiServerConfigurationData.LogQueueUsername ?? ""),
                        new JProperty("log_queue_password", pluginData.PkiServerConfigurationData.LogQueuePassword ?? ""),
                        new JProperty("jre64_directory", pluginData.PkiServerConfigurationData.Jre64Directory ?? ""),
                        new JProperty("enable_dss", pluginData.PkiServerConfigurationData.EnableDss),
                        new JProperty("dss_client", pluginData.PkiServerConfigurationData.DssClient),
                        new JProperty("sign_locally", pluginData.PkiServerConfigurationData.SignLocally),
                        new JProperty("idp_url", pluginData.PkiServerConfigurationData.IdpUrl ?? ""),
                        new JProperty("pki_service_url", pluginData.PkiServerConfigurationData.PkiServiceUrl ?? ""),
                        new JProperty("signature_service_url", pluginData.PkiServerConfigurationData.SignatureServiceUrl ?? ""),
                        new JProperty("tsa_url", pluginData.PkiServerConfigurationData.TsaUrl ?? ""),
                        new JProperty("ocsp_url", pluginData.PkiServerConfigurationData.OcspUrl ?? ""),
                        new JProperty("log_callstack", pluginData.PkiServerConfigurationData.LogCallstack),
                        new JProperty("client_id", pluginData.PkiServerConfigurationData.ClientId ?? ""),
                        new JProperty("client_secret", pluginData.PkiServerConfigurationData.ClientSecret ?? ""),
                        new JProperty("staging_env", pluginData.PkiServerConfigurationData.StagingEnv),
                        new JProperty("introspect", pluginData.PkiServerConfigurationData.Introspect),
                        new JProperty("hand_signature", pluginData.PkiServerConfigurationData.HandSignature),
                        new JProperty("signature_image", (pluginData.PkiServerConfigurationData.SignatureImage != null && pluginData.PkiServerConfigurationData.SignatureImage.Length > 0)
                        ? Convert.ToBase64String(pluginData.PkiServerConfigurationData.SignatureImage) : ""),
                        new JProperty("signing_log_queue", pluginData.PkiServerConfigurationData.SigningLogQueue ?? ""),
                        new JProperty("ra_log_queue", pluginData.PkiServerConfigurationData.RaLogQueue ?? ""),
                        new JProperty("central_log_queue", pluginData.PkiServerConfigurationData.CentralLogQueue ?? ""),
                    new JProperty("hsm_plugins",
                        new JObject(
                            new JProperty(pluginData.PkiHsmData.HsmPlugin.Name,
                                new JObject(
                                    new JProperty("cmapi_url", pluginData.PkiHsmData.CmapiUrl ?? ""),
                                    new JProperty("client_path", pluginData.PkiHsmData.ClientPath ?? ""),
                                    new JProperty("client_env_path", pluginData.PkiHsmData.ClientEnvPath ?? ""),
                                    new JProperty("cm_admin_uid", pluginData.PkiHsmData.CmAdminUid ?? ""),
                                    new JProperty("cm_admin_pwd", pluginData.PkiHsmData.CmAdminPwd ?? ""),
                                    new JProperty("key_gen_timeout", pluginData.PkiHsmData.KeyGenerationTimeout),
                                    new JProperty("key_algorithm", pluginData.PkiHsmData.KeyData.KeyAlgorithm.Name ?? ""),
                                    new JProperty("key_size", pluginData.PkiHsmData.KeyData.KeySize.Size ?? ""),
                                    new JProperty("hash_algorithm", pluginData.PkiHsmData.HashAlgorithm.Name ?? ""),
                                    new JProperty("slot_id", pluginData.PkiHsmData.SlotId),
                                    new JProperty("plugin_library_path", pluginData.PkiHsmData.HsmPlugin.HsmPluginLibraryPath ?? ""))))),
                    new JProperty("ca_plugins",
                        new JObject(
                            new JProperty(pluginData.PkiCaData.CaPlugin.Name,
                                new JObject(
                                    new JProperty("url", pluginData.PkiCaData.Url ?? ""),
                                    new JProperty("certificate_authority",pluginData.PkiCaData.CertificateAuthority ?? ""),
                                    new JProperty("end_entity_profile_name", pluginData.PkiCaData.EndEntityProfileName ?? ""),
                                    new JProperty("certificate_profile_name",pluginData.PkiCaData.CertificateProfileName ?? ""),
                                    new JProperty("client_auth_cert", pluginData.PkiCaData.ClientAuthCertificate ?? ""),
                                    new JProperty("client_auth_cert_pwd", pluginData.PkiCaData.ClientAuthCertificatePassword ?? ""),
                                    new JProperty("plugin_library_path", pluginData.PkiCaData.CaPlugin.CaPluginLibraryPath ?? ""),
                                    new JProperty("issuer_dn", pluginData.PkiCaData.IssuerDn ?? ""),
                                    new JProperty("procedure", pluginData.PkiCaData.Procedure.Name ?? ""),
                                    new JProperty("staging_cert_procedure_RSA256", pluginData.PkiCaData.StagingCertProcedureRsa256 ?? ""),
                                    new JProperty("staging_cert_procedure_RSA512", pluginData.PkiCaData.StagingCertProcedureRsa512 ?? ""),
                                    new JProperty("staging_cert_procedure_EC256", pluginData.PkiCaData.StagingCertProcedureEc256 ?? ""),
                                    new JProperty("staging_cert_procedure_EC512", pluginData.PkiCaData.StagingCertProcedureEc512 ?? ""),
                                    new JProperty("test_cert_procedure_RSA256", pluginData.PkiCaData.TestCertProcedureRsa256 ?? ""),
                                    new JProperty("test_cert_procedure_RSA512", pluginData.PkiCaData.TestCertProcedureRsa512 ?? ""),
                                    new JProperty("test_cert_procedure_EC256", pluginData.PkiCaData.TestCertProcedureEc256 ?? ""),
                                    new JProperty("test_cert_procedure_EC512", pluginData.PkiCaData.TestCertProcedureEc512 ?? ""),
                                    new JProperty("cert_validity", pluginData.PkiCaData.CertificateValidity),
                                    new JProperty("signing_cert_issuer", pluginData.PkiCaData.SigningCertificateIssuer ?? ""),
                                    new JProperty("signing_cert_root", pluginData.PkiCaData.SigningCertificateRoot ?? ""),
                                    new JProperty("ocsp_signer_cert", pluginData.PkiCaData.OcspSignerCertificate ?? ""),
                                    new JProperty("signing_cert_chain", pluginData.PkiCaData.SigningCertificateChain ?? ""),
                                    new JProperty("timestamping_cert", pluginData.PkiCaData.TimestampingCertificate ?? ""),
                                    new JProperty("timestamping_cert_chain", pluginData.PkiCaData.TimestampingCertificateChain ?? "")))))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return root;
        }

        [NonAction]
        private PkiPluginDatum GetPluginData()
        {
            if (HttpContext.Session.Get<PkiPluginDatum>("PKIPluginData") != null)
                return HttpContext.Session.Get<PkiPluginDatum>("PKIPluginData");

            return new PkiPluginDatum();
        }

        [NonAction]
        private void SetPluginDataInSession(PkiPluginDatum pluginConfiguration)
        {
            if (pluginConfiguration == null)
                pluginConfiguration = new PkiPluginDatum();

            HttpContext.Session.Set<PkiPluginDatum>("PKIPluginData", pluginConfiguration);
        }

        [NonAction]
        private void RemovePluginDataFromSession()
        {
            if (HttpContext.Session.Get<PkiPluginDatum>("PKIPluginData") != null)
            {
                HttpContext.Session.Remove("PKIPluginData");
            }
        }
    }
}
