using DTPortal.Web.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.AllPaymentHistory
{
    public class AllPaymentHistoryViewModel
    {
        [Required(ErrorMessage ="Select a date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? PaymentHistoryDate { get; set; }

        [Required(ErrorMessage = "Select a status")]
        public PaymentStatus Status { get; set; }
    }
}
