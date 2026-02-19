using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DTPortal.Web.Enums
{
    public enum PaymentChannel
    {
        [Display(Name = "Manual")]
        [EnumMember(Value = "MANUAL")]
        MANUAL
    }
}
