using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class DataPivot
{
    public int Id { get; set; }

    public string DataPivotUid { get; set; }

    public string Name { get; set; }

    public string AuthScheme { get; set; }

    public int ScopeId { get; set; }

    public string CategoryId { get; set; }

    public string OrgnizationId { get; set; }

    public string Description { get; set; }

    public string DataPivotLogo { get; set; }

    public string AttributeConfiguration { get; set; }

    public string ServiceConfiguration { get; set; }

    public string PublicKeyCert { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public string Status { get; set; }

    public string AllowedSubscriberTypes { get; set; }

    public virtual Scope Scope { get; set; }
}
