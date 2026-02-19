using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DTPortal.Core.Domain.Lookups;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.TimeBasedAccess
{
    public class TimeBasedAccessNewViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name ")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Display(Name = "Activate")]
        public int isActive { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? sDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime? eDate { get; set; }

        
        [Required]
        [Display(Name = "Start Time")]
        public TimeSpan? sTime { get; set; }

        
        [Required]
        [Display(Name = "End Time")]
        public TimeSpan? eTime { get; set; }

        public string attrValues { get; set; }
        public string ApplicableRole { get; set; }

        [Required(ErrorMessage = "Select atleast one role.")]
        [Display(Name = "Applicable role ")]
        public List<SelectListItem> RolesList { get; set; }
    }
}
