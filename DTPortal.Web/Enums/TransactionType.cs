using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.Enums
{
    public enum TransactionType
    {
        //[Display(Name = "Authentication")]
        //Authentication = 1,

        //[Display(Name = "Signing")]
        //Signing = 2,

        //[Display(Name = "Both")]
        //Both = 3,

        [Display(Name = "All")]
        All = 1,

        //[Display(Name = "Signing")]
        //Signing = 11,

        [Display(Name = "Authentication")]
        Authentication = 12,

        [Display(Name = "Wallet")]
        Wallet = 2,
    }
}
