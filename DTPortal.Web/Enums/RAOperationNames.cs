using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum RAOperationNames
    {
        [Display(Name = "PKI Authenticated")]
        [EnumMember(Value = "PKI_AUTHENTICATED")]
        PKI_AUTHENTICATED,

        [Display(Name = "Key Pair Generated")]
        [EnumMember(Value = "KEY_PAIR_GENERATED")]
        KEY_PAIR_GENERATED,

        [Display(Name = "CSR Created")]
        [EnumMember(Value = "CSR_CREATED")]
        CSR_CREATED,

        [Display(Name = "Certificate Generated")]
        [EnumMember(Value = "CERTIFICATE_GENERATED")]
        CERTIFICATE_GENERATED,

        [Display(Name = "Cerificate Pair Issued")]
        [EnumMember(Value = "CERTIFICATE_PAIR_ISSUED")]
        CERTIFICATE_PAIR_ISSUED,

        [Display(Name = "Digitally Signed")]
        [EnumMember(Value = "DIGITALLY_SIGNED")]
        DIGITALLY_SIGNED,

        [Display(Name = "Signature Verified")]
        [EnumMember(Value = "SIGNATURE_VERIFIED")]
        SIGNATURE_VERIFIED,

        [Display(Name = "Other")]
        [EnumMember(Value = "OTHER")]
        OTHER
    }
}

