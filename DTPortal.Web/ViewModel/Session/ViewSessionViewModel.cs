using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Session
{
    public class ViewSessionViewModel
    {
        [Display(Name = "Session Id")]
        public string GlobalSessionId { get; set; }
        

        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Display(Name = "UUID")]
        public string UserId { get; set; }

        [Display(Name = "Ip Address")]
        public string IpAddress { get; set; }

        [Display(Name = "Mac Address")]
        public string MacAddress { get; set; }

        [Display(Name = "Authentication Scheme")]
        public string AuthenticationScheme { get; set; }

        [Display(Name = "Logged In Time")]
        public string LoggedInTime { get; set; }
        [Display(Name = "Last Access Time")]
        public string LastAccessTime { get; set; }
        [Display(Name = "Type Of Device")]
        public string TypeOfDevice { get; set; }
        public string AdditionalValue { get; set; }
        [Display(Name = "User Agent Details")]
        public string UserAgentDetails { get; set; }
        [Display(Name = "Client Id")]

        public string ClientId { get; set; }
        public List<SelectListItem> Clientlist { get; set; }
    }
}
