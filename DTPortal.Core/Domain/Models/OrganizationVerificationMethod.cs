using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class OrganizationVerificationMethod
{
    public int Id { get; set; }

    public string OrganizationId { get; set; }

    public string VerificationMethodUid { get; set; }

    public string Status { get; set; }

    public string Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public virtual VerificationMethod VerificationMethodU { get; set; }
}
