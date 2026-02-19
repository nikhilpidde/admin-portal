using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum OnboardingOperationNames
    {
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

