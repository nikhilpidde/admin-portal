using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class KycAttribute
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }
}
