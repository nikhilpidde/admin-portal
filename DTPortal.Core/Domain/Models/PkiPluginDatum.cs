using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiPluginDatum
{
    public int Id { get; set; }

    public string Status { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public string ApprovedBy { get; set; }

    public string BlockedReason { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public int PkiCaDataId { get; set; }

    public int PkiHsmDataId { get; set; }

    public int PkiServerConfigurationDataId { get; set; }

    public virtual PkiCaDatum PkiCaData { get; set; }

    public virtual PkiHsmDatum PkiHsmData { get; set; }

    public virtual PkiServerConfigurationDatum PkiServerConfigurationData { get; set; }
}
