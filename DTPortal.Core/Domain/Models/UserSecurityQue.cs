using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class UserSecurityQue
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Question { get; set; }

    public string Answer { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }
}
