using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.CriticalOperation
{
    public class CriticalOperationEditViewModel
    {
        public int OperationId { get; set; }

        [Required]
        [Display(Name = "Operation Name ")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description ")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Authentication Policies")]
        public string AuthScheme { get; set; }
        public List<SelectListItem> Authlist { get; set; }

      
        [Display(Name = "Authentication Required ")]
        public int IsEnable { get; set; }
    }
}
