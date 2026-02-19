using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.OrganizationUsageReport
{
    public class OrganizationUsageReportUsageViewModel
    {
        [Required]
        public string OrganizationUid { get; set; }

        [Required(ErrorMessage = "Organization required")]
        [Display(Name = "Organization")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Select year")]
        [Display(Name = "Year")]
        public string Year { get; set; }

        public IEnumerable<OrganizationUsageReportDTO> OrganizationUsageReports { get; set; }
    }
}
