using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

namespace DTPortal.Web.ViewModel.Subscriber
{
    public class SubscriberReportsSearchViewModel
    {
        public string Identifier { get; set; }

        public string SubscriberFullname { get; set; }

        [Required(ErrorMessage = "Select Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Select Transaction Status")]
        public string TransactionStatus { get; set; }

         [Required(ErrorMessage = "Select Date Range")]
        public string DateRange { get; set; }
    }
}
