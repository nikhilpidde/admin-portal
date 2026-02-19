using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PkiConfiguration
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
