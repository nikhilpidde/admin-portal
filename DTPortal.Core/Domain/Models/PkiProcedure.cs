using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiProcedure
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PkiCaDatum> PkiCaData { get; set; } = new List<PkiCaDatum>();
}
