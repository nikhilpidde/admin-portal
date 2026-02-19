using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum DigitalAuthenticationOperationNames
    {
        [Display(Name = "Subscriber Authenticated")]
        [EnumMember(Value = "SUBSCRIBER_AUTHENTICATED")]
        SUBSCRIBER_AUTHENTICATED,

        [Display(Name = "Subscriber Authentication Failed")]
        [EnumMember(Value = "SUBSCRIBER_AUTHENTICATION_FAILED")]
        SUBSCRIBER_AUTHENTICATION_FAILED,

        [Display(Name = "Service Provider Onboarded")]
        [EnumMember(Value = "SERVICE_PROVIDER_ONBOARDED")]
        SERVICE_PROVIDER_ONBOARDED
    }
}
