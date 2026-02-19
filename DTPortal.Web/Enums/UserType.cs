using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum UserType
    {
        [Display(Name = "Subscriber")]
        [EnumMember(Value = "SUBSCRIBER")]
        SUBSCRIBER = 1,

        [Display(Name = "Organization")]
        [EnumMember(Value = "ORGANIZATION")]
        ORGANIZATION,
    }
}
