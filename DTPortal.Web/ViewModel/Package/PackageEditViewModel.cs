using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

namespace DTPortal.Web.ViewModel.Package
{
    public class PackageEditViewModel
    {
        [Required]
        [Display(Name = "Package Code")]
        public string PackageCode { get; set; }

        [Required]
        [Display(Name = "Package Description")]
        public string PackageDescription { get; set; }

        [Display(Name = "Service For")]
        [Required(ErrorMessage = "Select service for")]
        public UserType? ServiceFor { get; set; }

        [Required]
        [Display(Name = "Total Signing Transactions")]
        public int TotalSigningTransactions { get; set; }

        [Required]
        [Display(Name = "Total ESeal Transactions")]
        public int TotalESealTransactions { get; set; }

        [Required]
        [Display(Name = "Discount on Signing Transactions")]
        public int DiscounOnSigningTransactions { get; set; }

        [Required]
        [Display(Name = "Discount on ESeal Transactions")]
        public int DiscounOnESealTransactions { get; set; }

        [Required(ErrorMessage = "Please enter tax")]
        [Display(Name = "Tax (in percentage)")]
        public double TaxPercentage { get; set; }

        public string Status { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public string ApprovedBy { get; set; }
    }
}
