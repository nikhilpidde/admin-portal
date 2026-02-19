using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DTPortal.Web.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "All")]
        [EnumMember(Value = "All")]
        ALL = 1,

        [Display(Name = "Success")]
        [EnumMember(Value = "Success")]
        SUCCESS,

        [Display(Name = "Failed")]
        [EnumMember(Value = "Failed")]
        FAILED,

        [Display(Name = "Pending")]
        [EnumMember(Value = "Pending")]
        PENDING
    }
}
