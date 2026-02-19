using DTPortal.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.OrganizationPaymentHistory
{
    public class AddOrganizationPaymentHistoryViewModel
    {
        [Required]
        public string OrganizationId { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "Enter organization name")]
        public string OrganizationName { get; set; }

        [Display(Name = "Payment Info")]
        //[Required(ErrorMessage = "Enter payment info")]
        public string PaymentInfo { get; set; } = "";

        [Display(Name = "Total Amount Paid")]
        [Required(ErrorMessage = "Enter toatl amount paid")]
        public double TotalAmountPaid { get; set; }

        [Display(Name = "Payment Channel")]
        [Required(ErrorMessage = "Enter payment channel")]
        public PaymentChannel? PaymentChannel { get; set; }

        [Display(Name = "Transaction Reference Id")]
        [Required(ErrorMessage = "Enter transaction reference id")]
        public string TransactionReferenceId { get; set; }

        [Display(Name = "Invoice Number")]
        [Required(ErrorMessage = "Enter invoice number")]
        public string InvoiceNumber { get; set; }
    }
}
