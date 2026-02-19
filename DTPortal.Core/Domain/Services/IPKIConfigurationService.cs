using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IPKIConfigurationService
    {
        Task<IEnumerable<PkiHsmPlugin>> GetAllHSMPluginsAsync();

        Task<IEnumerable<PkiCaPlugin>> GetAllCAPluginsAsync();

        Task<IEnumerable<PkiHashAlgorithm>> GetAllHashAlgorithmsAsync();

        Task<IEnumerable<PkiKeyAlgorithm>> GetAllKeyAlgorithmsAsync();

        Task<IEnumerable<PkiKeySize>> GetAllKeysSizeAsync();

        Task<IEnumerable<PkiProcedure>> GetAllProceduresAsync();

        Task<PkiHsmPlugin> GetHSMPluginAsync(int id);

        Task<PkiHashAlgorithm> GetHashAlgorithmAsync(int id);

        Task<PkiKeyAlgorithm> GetKeyAlgorithmAsync(int id);

        Task<PkiKeySize> GetKeySizeAsync(int id);

        Task<PkiProcedure> GetProcedureAsync(int id);

        Task<PkiCaPlugin> GetCAPluginAsync(int id);

        Task<IEnumerable<PkiPluginDatum>> GetAllPluginConfigurationsAsync();

        Task<PkiPluginDatum> GetCompletePluginConfigurationAsync(int id);

        Task<PkiPluginDatum> GetPluginConfigurationAsync(int id);

        Task<PkiConfiguration> GetConfigurationDataAsync(string name);

        Task<IEnumerable<PkiKeySize>> GetAllKeysSizeForKeyAlgorithmIdAsync(int id);

        Task<ServiceResult> AddPluginConfigurationAsync(PkiPluginDatum pluginData, bool makerCheckerFlag = false);

        Task<ServiceResult> UpdatePluginConfigurationAsync(PkiPluginDatum pkiPlugindata, bool makerCheckerFlag = false);

        Task<ServiceResult> EnablePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false);

        Task<ServiceResult> DisablePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false);

        Task<ServiceResult> DeletePluginConfigurationAsync(int pluginDataId, string updatedBy, bool makerCheckerFlag = false);

        Task<ServiceResult> UpdateEncryptedPluginConfigurationAsync(string wrappedData, string updatedBy);

        string WrapData(string data);

        byte[] GenerateTimestamp(byte[] data);

        byte[] POSDigiTimeStamp(byte[] data);
    }
}
