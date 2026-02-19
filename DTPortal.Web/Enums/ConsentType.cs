using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum ConsentType
    {
        [Display(Name = "Subscriber")]
        [EnumMember(Value = "SUBSCRIBER")]
        Subscriber
    }
}
