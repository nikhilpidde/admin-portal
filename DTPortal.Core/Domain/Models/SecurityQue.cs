using System;
using System.Collections.Generic;

#nullable disable

namespace DTPortal.Core.Domain.Models
{
    public partial class SecurityQue
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string Hash { get; set; }

        public virtual UserTable CreatedByNavigation { get; set; }
        public virtual UserTable UpdatedByNavigation { get; set; }
    }
}
