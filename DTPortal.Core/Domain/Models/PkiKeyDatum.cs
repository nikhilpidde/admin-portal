using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiKeyDatum
{
    public int Id { get; set; }

    public int KeyAlgorithmId { get; set; }

    public int KeySizeId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual PkiKeyAlgorithm KeyAlgorithm { get; set; }

    public virtual PkiKeySize KeySize { get; set; }

    public virtual ICollection<PkiHsmDatum> PkiHsmData { get; set; } = new List<PkiHsmDatum>();
}
