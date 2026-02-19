using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiKeySize
{
    public int Id { get; set; }

    public string Size { get; set; }

    public int KeyAlgorithmId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual PkiKeyAlgorithm KeyAlgorithm { get; set; }

    public virtual ICollection<PkiKeyDatum> PkiKeyData { get; set; } = new List<PkiKeyDatum>();
}
