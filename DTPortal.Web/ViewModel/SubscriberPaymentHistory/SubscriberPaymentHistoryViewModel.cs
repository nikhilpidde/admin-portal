using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Core.DTOs;
using DTPortal.Web.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SubscriberPaymentHistory
{
    public class SubscriberPaymentHistoryViewModel
    {
        [Display(Name = "Search By")]
        [Required(ErrorMessage = "Select type")]
        public UserIdentifier IdentifierType { get; set; }

        [Display(Name = "Value")]
        [Required(ErrorMessage = "Value is required")]
        public string IdentifierValue { get; set; }

        public IEnumerable<SubscriberPaymentHistoryDTO> SubscriberPaymentHistory { get; set; }
    }
}
