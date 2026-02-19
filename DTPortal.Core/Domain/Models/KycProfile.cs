using System;
using System.Collections.Generic;

namespace DTPortal.Core.Domain.Models;

public partial class KycProfile
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string AttributesList { get; set; }
}
