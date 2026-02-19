using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.Subscriber
{
    public class SubscriberSearchViewModel
    {
        [Display(Name ="Search By")]
        [Required(ErrorMessage ="Select type")]
        public SubscriberIdentifier IdentifierType { get; set; }

        [Display(Name = "Value")]
        [Required(ErrorMessage = "Value is required")]
        public string IdentifierValue { get; set; }

        public SubscriberDetailsDTO SubscriberDetails { get; set; }
    }
}
