using System;
using System.ComponentModel.DataAnnotations;

using DTPortal.Web.Enums;
using DTPortal.Web.CustomValidations;

using DTPortal.Core;
using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.AdminLogReports
{
    public class AdminLogReportViewModel
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Module")]
        public ModuleName? ModuleName { get; set; }

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
        public int? PerPage { get; set; }

        public PaginatedList<AdminLogReportDTO> Reports { get; set; }
    }
}
