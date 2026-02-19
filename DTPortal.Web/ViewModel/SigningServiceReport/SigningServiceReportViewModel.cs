using System;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.CustomValidations;

using DTPortal.Core;
using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.SigningServiceReport
{
    public class SigningServiceReportViewModel
    {
        [Display(Name = "Subscriber Identifier")]
        public string Identifier { get; set; }

        [Display(Name = "Operation")]
        public string ServiceName { get; set; }

        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }

        [Display(Name = "Signature Type")]
        public string SignatureType { get; set; }

        [Display(Name = "ESeal Used?")]
        public bool ESealUsed { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        [DateGreaterThan(otherPropertyName = "StartDate", ErrorMessage = "End date cannot be less than start date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Show Records (Per Page)")]
        public int? PerPage { get; set; }

        public PaginatedList<LogReportDTO> Reports { get; set; }
    }
}
