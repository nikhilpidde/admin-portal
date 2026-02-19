using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.OfflinePayment
{
    public class OfflinePaymentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organization Name is required.")]
        [Display(Name = "Organization Name")]
        public string OrgName { get; set; }

        [Required(ErrorMessage = "Amount Received is required.")]
        [Display(Name = "Amount Received")]
        public double AmountReceived { get; set; }


        [Display(Name = "Transaction Reference Id")]
        public string TransactionRefId { get; set; }
        [MinLength(5)]
        [MaxLength(16)]
        [Required(ErrorMessage = "Invoice Number should be greater than 5.")]
        [Display(Name = "Invoice Number")]
        public string InvoiceNo { get; set; }

        [Required(ErrorMessage = "Payment Channel is required.")]
        [Display(Name = "Payment Channel")]
        public string PaymentChannel { get; set; }


        [Display(Name = "Online Payment Gateway")]
        public string OnlinePaymentGateway { get; set; }


        [Display(Name = "Online Payment Gateway Reference Number")]
        public string OnlinePaymentGatewayReferenceNo { get; set; }

        [Required(ErrorMessage = "Total Signing Credits is required.")]
        [Display(Name = "Total Signing Credits")]
        public double? TotalSigningCredits { get; set; }

        [Required(ErrorMessage = "Total Eseal Credits is required.")]
        [Display(Name = "Total Eseal Credits")]
        public double? TotalEsealCredits { get; set; }

        [Required(ErrorMessage = "User Subscription Credits is required.")]
        [Display(Name = "Total User Subscription Credits")]
        public double? TotalUserSubscriptionCredits { get; set; }

        public double? OnboardingCredits { get; set; }


        [Display(Name = "Organization Id")]
        public string OrgId { get; set; }

        //public string CreatedOn { get; set; }
        //public string AllocationStatus { get; set; }
    }
}
