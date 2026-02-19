using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiCaPlugin
{
    public int Id { get; set; }

    public string Guid { get; set; }

    public string Name { get; set; }

    public string CaPluginLibraryPath { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PkiCaDatum> PkiCaData { get; set; } = new List<PkiCaDatum>();
}
