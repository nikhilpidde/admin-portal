using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DTPortal.Web.Enums
{
    public enum LogLevel
    {
        [Display(Name = "INFO")]
        [EnumMember(Value = "INFO")]
        Info,

        [Display(Name = "WARNING")]
        [EnumMember(Value = "WARNING")]
        Warning,

        [Display(Name = "ERROR")]
        [EnumMember(Value = "ERROR")]
        Error
    }
}
