using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum ServiceName
    {
        [Display(Name = "Cerificate Pair Issued")]
        [EnumMember(Value = "CERTIFICATE_PAIR_ISSUED")]
        CERTIFICATE_PAIR_ISSUED,

        [Display(Name = "Subscriber Authenticated")]
        [EnumMember(Value = "SUBSCRIBER_AUTHENTICATED")]
        SUBSCRIBER_AUTHENTICATED,

        [Display(Name = "Subscriber Authentication Failed")]
        [EnumMember(Value = "SUBSCRIBER_AUTHENTICATION_FAILED")]
        SUBSCRIBER_AUTHENTICATION_FAILED,

        [Display(Name = "Digitally Signed")]
        [EnumMember(Value = "DIGITALLY_SIGNED")]
        DIGITALLY_SIGNED,

        [Display(Name = "Key Pair Generated")]
        [EnumMember(Value = "KEY_PAIR_GENERATED")]
        KEY_PAIR_GENERATED,

        [Display(Name = "CSR Created")]
        [EnumMember(Value = "CSR_CREATED")]
        CSR_CREATED,

        [Display(Name = "Certificate Generated")]
        [EnumMember(Value = "CERTIFICATE_GENERATED")]
        CERTIFICATE_GENERATED,

        [Display(Name = "PKI Authenticated")]
        [EnumMember(Value = "PKI_AUTHENTICATED")]
        PKI_AUTHENTICATED,

        [Display(Name = "Signature Verified")]
        [EnumMember(Value = "SIGNATURE_VERIFIED")]
        SIGNATURE_VERIFIED,

        [Display(Name = "Service Provider Onboarded")]
        [EnumMember(Value = "SERVICE_PROVIDER_ONBOARDED")]
        SERVICE_PROVIDER_ONBOARDED,

        [Display(Name = "Subscriber Onboarded")]
        [EnumMember(Value = "SUBSCRIBER_ONBOARDED")]
        SUBSCRIBER_ONBOARDED,

        [Display(Name = "Other")]
        [EnumMember(Value = "OTHER")]
        OTHER
    }
}
