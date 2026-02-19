using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class MakerChecker
{
    public int Id { get; set; }

    public string OperationType { get; set; }

    public string OperationPriority { get; set; }

    public int ActivityId { get; set; }

    public string RequestData { get; set; }

    public int MakerId { get; set; }

    public int MakerRoleId { get; set; }

    public string State { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual Activity Activity { get; set; }

    public virtual UserTable Maker { get; set; }
}
