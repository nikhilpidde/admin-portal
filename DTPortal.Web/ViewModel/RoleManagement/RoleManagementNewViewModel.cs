using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DTPortal.Web.ViewModel.RoleManagement
{
    public class RoleManagementNewViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Role Name ")]
        public string RoleName { get; set; }

        [Required]
        [Display(Name = "Display Name ")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Role Description ")]
        public string Description { get; set; }

        // public List<ActivityListItem> Activities { get; set; }

        public Object Activities { get; set; }

        public string Activitie { get; set; }

        public IEnumerable<CheckerListItem> CheckerActivitie { get; set; }

        public string Status { get; set; }
    }
}
