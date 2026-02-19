using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserDashboard
{
    public class SequerityQuestionViewModel
    {
        public int Que1Id { get; set; }
        public int Que2Id { get; set; }
        public int Que3Id { get; set; }
        public int UserId { get; set; }
       
        public string Username { get; set; }
        public string Uuid { get; set; }
        public List<SelectListItem> SecurityQueList1 { get; set; }
        public List<SelectListItem> SecurityQueList2 { get; set; }
        public List<SelectListItem> SecurityQueList3 { get; set; }
        [Required]
        [Display(Name = "Security Question One")]
        public string Question1 { get; set; }

        [Required]
        [Display(Name = "Answer for Question One")]
        public string Answer1 { get; set; }

        [Required]
        [Display(Name = "Security Question Two")]
        public string Question2 { get; set; }

        [Required]
        [Display(Name = "Answer for Question Two")]
        public string Answer2 { get; set; }

        [Required]
        [Display(Name = "Security Question Three")]
        public string Question3 { get; set; }

        [Required]
        [Display(Name = "Answer for Question Three")]
        public string Answer3 { get; set; }

    }
}
