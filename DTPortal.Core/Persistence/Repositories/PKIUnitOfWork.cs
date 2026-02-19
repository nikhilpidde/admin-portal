using Microsoft.Extensions.Logging;

using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Persistence.Contexts;

namespace DTPortal.Core.Persistence.Repositories
{
    public class PKIUnitOfWork : GenericUnitOfWork<PKIDbContext>, IPKIUnitOfWork
    {
        private IHSMPluginRepository _hsmPlugin;
        private ICAPluginRepository _caPlugin;
        private IHashAlgorithmRepository _timeStampingHashAlgorithm;
        private IKeyAlgorithmRepository _keyAlgorithm;
        private IKeySizeRepository _keySize;
        private IProcedureRepository _procedure;
        private IPKIServerConfigurationDataRepository _pkiServerConfigurationData;
        private IPluginDataRepository _pluginData;
        private IPKIConfigurationRepository _pkiConfiguration;
        private ICADataRepository _caData;
        private IHSMDataRepository _hsmData;
        private ILogger _logger;

        public PKIUnitOfWork(PKIDbContext context,
            ILogger logger) : base(context)
        {
            _logger = logger;
        }

        public IHSMPluginRepository HSMPlugin
        {
            get { return _hsmPlugin ??= new HSMPluginRepository(Context, _logger); }
        }

        public ICAPluginRepository CAPlugin
        {
            get { return _caPlugin ??= new CAPluginRepository(Context, _logger); }
        }

        public IHashAlgorithmRepository HashAlgorithm
        {
            get
            {
                return _timeStampingHashAlgorithm ??= new HashAlgorithmRepository(Context, _logger);
            }
        }

        public IKeyAlgorithmRepository KeyAlgorithm
        {
            get
            {
                return _keyAlgorithm ??= new KeyAlgorithmRepository(Context, _logger);
            }
        }

        public IKeySizeRepository KeySize
        {
            get
            {
                return _keySize ??= new KeySizeRepository(Context, _logger);
            }
        }

        public IProcedureRepository Procedure
        {
            get
            {
                return _procedure ??= new ProcedureRepository(Context, _logger);
            }
        }

        public IPKIServerConfigurationDataRepository PKIServerConfigurationData
        {
            get
            {
                return _pkiServerConfigurationData ??= new PKIServerConfigurationDataRepository(Context, _logger);
            }
        }

        public ICADataRepository CAData
        {
            get
            {
                return _caData ??= new CADataRepository(Context, _logger);
            }
        }

        public IPluginDataRepository PluginData
        {
            get
            {
                return _pluginData ??= new PluginDataRepository(Context, _logger);
            }
        }

        public IHSMDataRepository HSMData
        {
            get
            {
                return _hsmData ??= new HSMDataRepository(Context, _logger);
            }
        }

        public IPKIConfigurationRepository PKIConfiguration
        {
            get
            {
                return _pkiConfiguration ??= new PKIConfigurationRepository(Context, _logger);
            }
        }
    }
}
