using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum UserIdentifier
    {
        [Display(Name = "Email")]
        Email = 3,

        [Display(Name = "Mobile Number")]
        MobileNumber = 4
    }
}
