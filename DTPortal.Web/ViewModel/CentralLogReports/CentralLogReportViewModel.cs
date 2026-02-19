using System;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;
using DTPortal.Web.CustomValidations;

using DTPortal.Core;
using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.CentralLogReports
{
    public class CentralLogReportViewModel
    {
        [Display(Name = "Subscriber Identifier")]
        public string Identifier { get; set; }

        [Display(Name = "Operation")]
        public ServiceName? ServiceName { get; set; }

        [Display(Name = "Transaction Type")]
        public TransactionType? TransactionType { get; set; }

        [Display(Name = "Signature Type")]
        public SignatureType? SignatureType { get; set; }

        [Display(Name = "ESeal Used?")]
        public bool ESealUsed { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        [DateGreaterThanAttribute(otherPropertyName = "StartDate", ErrorMessage = "End date must be greater than start date")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Show Records per page is required")]
        [Display(Name = "Show Records (Per Page)")]
        public int? PerPage { get; set; }

        public PaginatedList<LogReportDTO> Reports { get; set; }
    }
}
