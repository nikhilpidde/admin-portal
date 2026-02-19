using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum PublishStatus
    {
        [Display(Name = "Unpublished")]
        [EnumMember(Value = "UNPUBLISHED")]
        Unpublished,

        [Display(Name = "Published")]
        [EnumMember(Value = "PUBLISHED")]
        Published
    }
}
