using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class KycDevice
{
    public int Id { get; set; }

    public string OrganizationId { get; set; }

    public string ClientId { get; set; }

    public string DeviceId { get; set; }

    public string Status { get; set; }

    public DateTime? CreatedTime { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string UpdatedBy { get; set; }
}
