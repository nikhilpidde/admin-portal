using System;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;
using DTPortal.Web.CustomValidations;
using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.RateCard
{
    public class RateCardAddViewModel
    {
        public RateCardAddViewModel()
        {
            Services = new List<ServiceDefinitionDTO>();
        }

        public IEnumerable<ServiceDefinitionDTO> Services { get; set; }

        [Display(Name = "Service Name")]
        [Required(ErrorMessage = "Select service name")]
        public int? ServiceId { get; set; }

        [Display(Name = "Stakeholder")]
        [Required(ErrorMessage = "Select stakeholder")]
        public UserType? Stakeholder { get; set; }

        [Required]
        [Display(Name = "Fee (AED)")]
        [Range(0.0, Double.MaxValue, ErrorMessage ="Fee must be greater than 0")]
        public double FeePerTransaction { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Tax must be between 0 to 100")]
        [Display(Name = "Tax(%)")]
        public double Tax { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Rate Effective From")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? RateEffectiveFrom { get; set; }
    }
}
