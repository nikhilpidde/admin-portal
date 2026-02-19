using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class KycMethod
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }
}
