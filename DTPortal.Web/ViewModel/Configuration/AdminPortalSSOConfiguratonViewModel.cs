using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Configuration
{
    public class AdminPortalSSOConfiguratonViewModel
    {
        [Required]
        [Display(Name = "Session Timeout (Minute)")]
        public int AdminPortalSSO_session_timeout { get; set; }

        [Required]
        [Display(Name = "Temporary Session Timeout (Minute)")]
        public int AdminPortalSSO_temporary_session_timeout { get; set; }

        [Required]
        [Display(Name = "Active Sessions Per User")]
        public int AdminPortalSSO_active_sessions_per_user { get; set; }

        [Required]
        [Display(Name = "Ideal Timeout (Minute)")]
        public int AdminPortalSSO_ideal_timeout { get; set; }

        [Required]
        [Display(Name = "Wrong pin count")]
        public int AdminPortalSSO_wrong_pin { get; set; }

        [Required]
        [Display(Name = "Account Lock Time (Hour)")]
        public int AdminPortalSSO_account_lock_time { get; set; }

        [Required]
        [Display(Name = "Domain")]
        [RegularExpression(@"^(([a-zA-Z]+[.][a-zA-z]+)*(,([a-zA-Z]+[.][a-zA-z]+)+)*?)$", ErrorMessage = "Please enter valid domain with comma (,) seprated")]
        public string AdminPortalSSO_email_domain { get; set; }
    }
}
