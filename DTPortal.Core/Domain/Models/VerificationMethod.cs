using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class VerificationMethod
{
    public int Id { get; set; }

    public string MethodUid { get; set; }

    public string MethodCode { get; set; }

    public string MethodType { get; set; }

    public string MethodName { get; set; }

    public decimal Pricing { get; set; }

    public string ProcessingTime { get; set; }

    public string ConfidenceThreshold { get; set; }

    public string TargetSegments { get; set; }

    public string MandatoryAttributes { get; set; }

    public string OptionalAttributes { get; set; }

    public string PricingSlabDefinitions { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public virtual ICollection<OrganizationVerificationMethod> OrganizationVerificationMethods { get; set; } = new List<OrganizationVerificationMethod>();
}
