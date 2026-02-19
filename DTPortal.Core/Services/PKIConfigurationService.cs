using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using DTPortal.Core.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Core.Constants;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.ExtensionMethods;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class PKIConfigurationService : IPKIConfigurationService
    {
        private readonly IMCValidationService _mcValidationService;
        private readonly IPKIUnitOfWork _pkiUnitOfWork;
        private readonly ILogger<PKIConfigurationService> _logger;

        public PKIConfigurationService(IMCValidationService mcValidationService,
            IPKIUnitOfWork pkiUnitOfWork,
            ILogger<PKIConfigurationService> logger)
        {
            _mcValidationService = mcValidationService;
            _pkiUnitOfWork = pkiUnitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<PkiHsmPlugin>> GetAllHSMPluginsAsync()
        {
            return await _pkiUnitOfWork.HSMPlugin.GetAllAsync();
        }

        public async Task<IEnumerable<PkiCaPlugin>> GetAllCAPluginsAsync()
        {
            return await _pkiUnitOfWork.CAPlugin.GetAllAsync();
        }

        public async Task<IEnumerable<PkiHashAlgorithm>> GetAllHashAlgorithmsAsync()
        {
            return await _pkiUnitOfWork.HashAlgorithm.GetAllAsync();
        }

        public async Task<IEnumerable<PkiKeyAlgorithm>> GetAllKeyAlgorithmsAsync()
        {
            return await _pkiUnitOfWork.KeyAlgorithm.GetAllAsync();
        }

        public async Task<IEnumerable<PkiKeySize>> GetAllKeysSizeAsync()
        {
            return await _pkiUnitOfWork.KeySize.GetAllAsync();
        }

        public async Task<IEnumerable<PkiKeySize>> GetAllKeysSizeForKeyAlgorithmIdAsync(int id)
        {
            return await _pkiUnitOfWork.KeySize.GetAllKeysSizeByKeyAlgorithmIdAsync(id);
        }

        public async Task<IEnumerable<PkiProcedure>> GetAllProceduresAsync()
        {
            return await _pkiUnitOfWork.Procedure.GetAllAsync();
        }

        public async Task<IEnumerable<PkiPluginDatum>> GetAllPluginConfigurationsAsync()
        {
            return await _pkiUnitOfWork.PluginData.GetAllPluginsDataAsync();
        }

        public async Task<PkiHsmPlugin> GetHSMPluginAsync(int id)
        {
            return await _pkiUnitOfWork.HSMPlugin.GetByIdAsync(id);
        }

        public async Task<PkiHashAlgorithm> GetHashAlgorithmAsync(int id)
        {
            return await _pkiUnitOfWork.HashAlgorithm.GetByIdAsync(id);
        }

        public async Task<PkiKeyAlgorithm> GetKeyAlgorithmAsync(int id)
        {
            return await _pkiUnitOfWork.KeyAlgorithm.GetByIdAsync(id);
        }

        public async Task<PkiKeySize> GetKeySizeAsync(int id)
        {
            return await _pkiUnitOfWork.KeySize.GetByIdAsync(id);
        }

        public async Task<PkiProcedure> GetProcedureAsync(int id)
        {
            return await _pkiUnitOfWork.Procedure.GetByIdAsync(id);
        }

        public async Task<PkiCaPlugin> GetCAPluginAsync(int id)
        {
            return await _pkiUnitOfWork.CAPlugin.GetByIdAsync(id);
        }

        public async Task<PkiConfiguration> GetConfigurationDataAsync(string name)
        {
            return await _pkiUnitOfWork.PKIConfiguration.GetConfigurationByNameAsync(name);
        }

        public async Task<PkiPluginDatum> GetPluginConfigurationAsync(int id)
        {
            return await _pkiUnitOfWork.PluginData.GetPluginDataAsync(id);
        }

        public async Task<PkiPluginDatum> GetCompletePluginConfigurationAsync(int id)
        {
            return await _pkiUnitOfWork.PluginData.GetCompletePluginDataAsync(id);
        }

        public async Task<ServiceResult> AddPluginConfigurationAsync(PkiPluginDatum pkiPlugindata, bool makerCheckerFlag = false)
        {
            try
            {
                var isExists = await _pkiUnitOfWork.PluginData.IsPluginExistsAsync(pkiPlugindata.PkiHsmData.HsmPluginId, pkiPlugindata.PkiCaData.CaPluginId);
                if (isExists == true)
                {
                    _logger.LogError("Plugin configuration already exists");
                    return new ServiceResult(false, "Plugin configuration already exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PKIConfigurationActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PKIConfigurationActivityId, OperationTypeConstants.Create, pkiPlugindata.CreatedBy,
                        JsonConvert.SerializeObject(pkiPlugindata));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                DateTime dateTime = DateTime.Now;
                pkiPlugindata.PkiCaData.CreatedDate = dateTime;
                pkiPlugindata.PkiHsmData.CreatedDate = dateTime;
                pkiPlugindata.PkiHsmData.KeyData.CreatedDate = dateTime;
                pkiPlugindata.PkiServerConfigurationData.CreatedDate = dateTime;
                pkiPlugindata.CreatedDate = dateTime;
                if ((await GetAllPluginConfigurationsAsync()).Any(x => x.Status.ToLower() == "active"))
                    pkiPlugindata.Status = Status.Inactive.GetValue();
                else
                    pkiPlugindata.Status = Status.Active.GetValue();
                await _pkiUnitOfWork.PluginData.AddAsync(pkiPlugindata);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Plugin configuration created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while creating plugin configuration. Please try later.");
        }

        public async Task<ServiceResult> UpdatePluginConfigurationAsync(PkiPluginDatum pkiPlugindata, bool makerCheckerFlag = false)
        {
            try
            {
                var pluginData = await GetPluginConfigurationAsync(pkiPlugindata.Id);
                if (pluginData == null)
                {
                    return new ServiceResult(false, "Plugin configuration doesn't exists");
                }

                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PKIConfigurationActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PKIConfigurationActivityId, OperationTypeConstants.Update, pkiPlugindata.UpdatedBy,
                        JsonConvert.SerializeObject(pkiPlugindata));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                DateTime dateTime = DateTime.Now;
                pkiPlugindata.PkiCaData.ModifiedDate = dateTime;
                pkiPlugindata.PkiHsmData.ModifiedDate = dateTime;
                pkiPlugindata.PkiHsmData.KeyData.ModifiedDate = dateTime;
                pkiPlugindata.PkiServerConfigurationData.ModifiedDate = dateTime;
                pkiPlugindata.ModifiedDate = dateTime;
                _pkiUnitOfWork.PluginData.Update(pkiPlugindata);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Plugin configuration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while updating plugin configuration. Please try later.");
        }

        public async Task<ServiceResult> EnablePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false)
        {
            try
            {
                var pluginData = await GetPluginConfigurationAsync(pluginDataId);
                if (pluginData == null)
                {
                    return new ServiceResult(false, "Plugin configuration doesn't exists");
                }

                // Check if any plugin is active already
                bool isTrue = (await GetAllPluginConfigurationsAsync())
                .Any(x => x.Status == Status.Active.GetValue());
                if (isTrue)
                {
                    return new ServiceResult(false, "One of the plugin configuration is already enabled. " +
                           "Please disable it to enable this.");
                }

                pluginData.UpdatedBy = updatedBy;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PKIConfigurationActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PKIConfigurationActivityId, OperationTypeConstants.Enable, updatedBy,
                        JsonConvert.SerializeObject(pluginData, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                DateTime dateTime = DateTime.Now;
                pluginData.PkiCaData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.KeyData.ModifiedDate = dateTime;
                pluginData.PkiServerConfigurationData.ModifiedDate = dateTime;
                pluginData.ModifiedDate = dateTime;
                pluginData.Status = Status.Active.GetValue();
                _pkiUnitOfWork.PluginData.Update(pluginData);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Plugin configuration enabled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while enabling the plugin configuration. Please try later.");
        }

        public async Task<ServiceResult> DisablePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false)
        {
            try
            {
                var pluginData = await GetPluginConfigurationAsync(pluginDataId);
                if (pluginData == null)
                {
                    return new ServiceResult(false, "Plugin configuration doesn't exists");
                }

                pluginData.UpdatedBy = updatedBy;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PKIConfigurationActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PKIConfigurationActivityId, OperationTypeConstants.Disable, updatedBy,
                        JsonConvert.SerializeObject(pluginData, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                DateTime dateTime = DateTime.Now;
                pluginData.PkiCaData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.KeyData.ModifiedDate = dateTime;
                pluginData.PkiServerConfigurationData.ModifiedDate = dateTime;
                pluginData.ModifiedDate = dateTime;
                pluginData.Status = Status.Inactive.GetValue();
                _pkiUnitOfWork.PluginData.Update(pluginData);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Plugin configuration disabled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while disabling the plugin configuration. Please try later.");
        }

        public async Task<ServiceResult> DeletePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false)
        {
            try
            {
                var pluginData = await GetPluginConfigurationAsync(pluginDataId);
                if (pluginData == null)
                {
                    return new ServiceResult(false, "Plugin configuration doesn't exists");
                }

                if (pluginData.Status == Status.Active.GetValue())
                {
                    return new ServiceResult(false, "Cannot delete the plugin configuration as it is in active state. " +
                        "Please disable it first to delete.");
                }

                pluginData.UpdatedBy = updatedBy;
                var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.PKIConfigurationActivityId);
                if (false == makerCheckerFlag && true == isEnabled)
                {
                    // Check whether checker approval is required for this operation
                    var isApprovalRequired = await _mcValidationService.IsCheckerApprovalRequired(
                        ActivityIdConstants.PKIConfigurationActivityId, OperationTypeConstants.Delete, updatedBy,
                        JsonConvert.SerializeObject(pluginData, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }));
                    if (!isApprovalRequired.Success)
                    {
                        _logger.LogError("Checker approval required failed");
                        return new ServiceResult(false, isApprovalRequired.Message);
                    }
                    if (isApprovalRequired.Result)
                    {
                        return new ServiceResult(true, "Your request has sent for approval");
                    }
                }

                DateTime dateTime = DateTime.Now;
                pluginData.PkiCaData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.ModifiedDate = dateTime;
                pluginData.PkiHsmData.KeyData.ModifiedDate = dateTime;
                pluginData.PkiServerConfigurationData.ModifiedDate = dateTime;
                pluginData.ModifiedDate = dateTime;
                pluginData.Status = Status.Deleted.GetValue();
                _pkiUnitOfWork.PluginData.Update(pluginData);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Plugin configuration deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while deleting the plugin configuration. Please try later.");
        }

        public async Task<ServiceResult> UpdateEncryptedPluginConfigurationAsync(string wrappedData, string updatedBy)
        {
            try
            {
                var configuration = await _pkiUnitOfWork.PKIConfiguration.GetConfigurationByNameAsync("configuration");
                if (configuration == null)
                {
                    //return new PKIServiceResponse("Configuration doesn't exists");
                    _logger.LogInformation("Configuration doesn't exists. Creating new configuration...");

                    configuration = new PkiConfiguration
                    {
                        Name = "configuration",
                        Value = wrappedData,
                        CreatedBy = updatedBy,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    await _pkiUnitOfWork.PKIConfiguration.AddAsync(configuration);
                    await _pkiUnitOfWork.SaveAsync();

                    return new ServiceResult(true, "Configuration generated successfully");
                }
                configuration.Value = wrappedData;
                configuration.UpdatedBy = updatedBy;
                configuration.ModifiedDate = DateTime.Now;

                _pkiUnitOfWork.PKIConfiguration.Update(configuration);
                await _pkiUnitOfWork.SaveAsync();

                return new ServiceResult(true, "Configuration generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return new ServiceResult(false, "An error occured while updating the configuration. Please try later.");
        }

        public string WrapData(string data)
        {
            try
            {
                return PKIMethods.Instance.PKIWrapSecureData(data);
            }
            catch (PKIException pkiEx)
            {
                _logger.LogError(pkiEx.Message, pkiEx);
            }

            return null;
        }

        public byte[] GenerateTimestamp(byte[] data)
        {
            try
            {
                return PKIMethods.Instance.GenerateTimestamp(data);
            }
            catch (PKIException pkiEx)
            {
                _logger.LogError(pkiEx.Message, pkiEx);
            }

            return null;
        }

        public byte[] POSDigiTimeStamp(byte[] data)
        {
            try
            {
                return PKIMethods.Instance.POSDigiTimeStamp(data);
            }
            catch (PKIException pkiEx)
            {
                _logger.LogError(pkiEx.Message, pkiEx);
            }

            return null;
        }
    }
}
