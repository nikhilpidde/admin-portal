using DTPortal.Core.DTOs;
using DTPortal.Core;
using DTPortal.Web.CustomValidations;
using DTPortal.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System;

namespace DTPortal.Web.ViewModel.SigningFailedLogReports
{
    public class SigningFailedLogReportsViewModel
    {
        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        [DateGreaterThanAttribute(otherPropertyName = "StartDate", ErrorMessage = "End date cannot be less than start date")]
        public DateTime? EndDate { get; set; }

        public string TransactionStatus { get; set; }

        public string TransactionType { get; set; }


        [Display(Name = "Show Records (Per Page)")]
        public int? PerPage { get; set; }

       

        public PaginatedList<LogReportDTO> Reports { get; set; }
    }
}
