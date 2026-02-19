using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class PortalSetting
{
    public int Id { get; set; }

    public string Name { get; set; }

    public byte[] Value { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
