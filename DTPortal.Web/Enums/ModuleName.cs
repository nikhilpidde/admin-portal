using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum ModuleName
    {
        //[Display(Name = "Dashboard")]
        //[EnumMember(Value = "DASHBOARD")]
        //DASHBOARD,

        [Display(Name = "Registration Authority")]
        [EnumMember(Value = "REGISTRATION_AUTHORITY")]
        REGISTRATION_AUTHORITY,

        [Display(Name = "Digital Authentication")]
        [EnumMember(Value = "DIGITAL_AUTHENTICATION")]
        DIGITAL_AUTHENTICATION,

        [Display(Name = "Electronic Signatures")]
        [EnumMember(Value = "ELECTRONIC_SIGNATURES")]
        ELECTRONIC_SIGNATURES,

        //[Display(Name = "Price Model")]
        //[EnumMember(Value = "PRICE_MODEL")]
        //PRICE_MODEL,

        [Display(Name = "Activity Reports")]
        [EnumMember(Value = "ACTIVITY_REPORTS")]
        ACTIVITY_REPORTS,

        [Display(Name = "Portal Settings")]
        [EnumMember(Value = "PORTAL_SETTINGS")]
        PORTAL_SETTINGS
    }
}
