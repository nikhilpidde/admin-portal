using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class OrganizationKycMethod
{
    public int Id { get; set; }

    public string OrganizationId { get; set; }

    public string KycMethods { get; set; }

    public string KycProfiles { get; set; }
}
