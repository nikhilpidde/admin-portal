using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiServerConfigurationDatum
{
    public int Id { get; set; }

    public string LogPath { get; set; }

    public string LogLevel { get; set; }

    public string ConfigDirectoryPath { get; set; }

    public string LogQueueIp { get; set; }

    public int LogQueuePort { get; set; }

    public string LogQueueUsername { get; set; }

    public string LogQueuePassword { get; set; }

    public string Jre64Directory { get; set; }

    public string IdpUrl { get; set; }

    public string TsaUrl { get; set; }

    public string OcspUrl { get; set; }

    public string PkiServiceUrl { get; set; }

    public string SignatureServiceUrl { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public bool LogCallstack { get; set; }

    public bool EnableDss { get; set; }

    public bool DssClient { get; set; }

    public bool SignLocally { get; set; }

    public bool StagingEnv { get; set; }

    public bool Introspect { get; set; }

    public bool HandSignature { get; set; }

    public byte[] SignatureImage { get; set; }

    public string SigningLogQueue { get; set; }

    public string RaLogQueue { get; set; }

    public string CentralLogQueue { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PkiPluginDatum> PkiPluginData { get; set; } = new List<PkiPluginDatum>();
}
