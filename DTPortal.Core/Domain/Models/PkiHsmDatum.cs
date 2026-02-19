using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiHsmDatum
{
    public int Id { get; set; }

    public string CmapiUrl { get; set; }

    public string ClientPath { get; set; }

    public string ClientEnvPath { get; set; }

    public string CmAdminUid { get; set; }

    public string CmAdminPwd { get; set; }

    public int KeyGenerationTimeout { get; set; }

    public int SlotId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public int HsmPluginId { get; set; }

    public int KeyDataId { get; set; }

    public int HashAlgorithmId { get; set; }

    public virtual PkiHashAlgorithm HashAlgorithm { get; set; }

    public virtual PkiHsmPlugin HsmPlugin { get; set; }

    public virtual PkiKeyDatum KeyData { get; set; }

    public virtual ICollection<PkiPluginDatum> PkiPluginData { get; set; } = new List<PkiPluginDatum>();
}
