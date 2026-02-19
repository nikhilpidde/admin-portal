using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum State
    {
        [Display(Name = "New")]
        [EnumMember(Value = "NEW")]
        New,

        [Display(Name = "Active")]
        [EnumMember(Value = "ACTIVE")]
        Active,

        [Display(Name = "Modified")]
        [EnumMember(Value = "MODIFIED")]
        Modified,

        [Display(Name = "Declined")]
        [EnumMember(Value = "DECLINED")]
        Declined
    }
}
