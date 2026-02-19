using DTPortal.Core.DTOs;
using DTPortal.Core;
using DTPortal.Web.CustomValidations;
using DTPortal.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.AttributeServiceTransactions
{
    public class AttributeServiceLogReportViewModel
    {
        [Display(Name = "User Id")]
        public string UserId { get; set; }

        [Display(Name = "User Id Type")]
        public UserIdentifier? UserIdType { get; set; }

        [Display(Name = "Application Name")]
        public string ApplicationName { get; set; }

        [Display(Name = "Profile")]
        public string Profile { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        [DateGreaterThanAttribute(otherPropertyName = "StartDate", ErrorMessage = "End date cannot be less than start date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Show Records (Per Page)")]
        public int PerPage { get; set; }

        public PaginatedList<AttributeServiceTransactionListDTO> Reports { get; set; }
    }
}
