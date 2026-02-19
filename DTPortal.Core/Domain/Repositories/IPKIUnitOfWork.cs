namespace DTPortal.Core.Domain.Repositories
{
    public interface IPKIUnitOfWork : IGenericUnitOfWork
    {
        IHSMPluginRepository HSMPlugin { get; }

        ICAPluginRepository CAPlugin { get; }

        IHashAlgorithmRepository HashAlgorithm { get; }

        IKeyAlgorithmRepository KeyAlgorithm { get; }

        IKeySizeRepository KeySize { get; }

        IProcedureRepository Procedure { get; }

        IPKIServerConfigurationDataRepository PKIServerConfigurationData { get; }

        IPluginDataRepository PluginData { get; }

        ICADataRepository CAData { get; }

        IHSMDataRepository HSMData { get; }

        IPKIConfigurationRepository PKIConfiguration { get; }
    }
}
