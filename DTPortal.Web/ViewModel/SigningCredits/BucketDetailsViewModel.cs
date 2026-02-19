using System;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SigningCredits
{
    public class BucketDetailsViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Bid Id")]
        public string BucketId { get; set; }
        public int BucketConfigurationId { get; set; }
        [Display(Name = "Total Digital Signatures")]
        public int TotalDigitalSignatures { get; set; }
        [Display(Name = "Total Eseal Signatures")]
        public int TotalESeal { get; set; }
        [Display(Name = "Created Date")]
        public string CreatedOn { get; set; }
        [Display(Name = "Updated Date")]
        public string UpdatedOn { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "Last Signatory Id")]
        public string LastSignatoryId { get; set; }
        [Display(Name = "Sponsor Id")]
        public string SponsorId { get; set; }
        [Display(Name = "Payment Received Date")]
        public string PaymentReceivedOn { get; set; }
        [Display(Name = "Payment Received")]
        public string PaymentReceived { get; set; }
        [Display(Name = "Additional DS Remaining")]
        public int AdditionalDSRemaining { get; set; }
        [Display(Name = "Additional EDS Remaining")]
        public int AdditionalEDSRemaining { get; set; }
        public string appName { get; set; }
        public string Label { get; set; }
    }
}
