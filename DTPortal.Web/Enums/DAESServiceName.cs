using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum DAESServiceName
    {
        [Display(Name = "Digital Signature")]
        [EnumMember(Value = "DIGITAL_SIGNATURE")]
        DIGITAL_SIGNATURE,

        [Display(Name = "Eseal")]
        [EnumMember(Value = "E_SEAL")]
        E_SEAL,

        [Display(Name = "Digital Authentication")]
        [EnumMember(Value = "DIGITAL_AUTHENTICATION")]
        DIGITAL_AUTHENTICATION,

        [Display(Name = "Certificate Pair")]
        [EnumMember(Value = "CERTIFICATE_PAIR")]
        CERTIFICATE_PAIR,
    }
}
