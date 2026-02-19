using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiKeyAlgorithm
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PkiKeyDatum> PkiKeyData { get; set; } = new List<PkiKeyDatum>();

    public virtual ICollection<PkiKeySize> PkiKeySizes { get; set; } = new List<PkiKeySize>();
}
