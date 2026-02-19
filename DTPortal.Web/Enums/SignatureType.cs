using System.Runtime.Serialization;

namespace DTPortal.Web.Enums
{
    public enum SignatureType
    {
        [EnumMember(Value = "XADES")]
        XADES,

        [EnumMember(Value = "PADES")]
        PADES,

        [EnumMember(Value = "CADES")]
        CADES,

        [EnumMember(Value = "DATA")]
        DATA
    }
}
