using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum SubscriberIdentifier
    {
        [Display(Name = "Emirates Id Number")]
        NIN = 1,

        [Display(Name = "Passport")]
        Passport = 2,

        [Display(Name = "Email")]
        Email = 3,

        [Display(Name = "Mobile Number")]
        MobileNumber = 4
    }
}
