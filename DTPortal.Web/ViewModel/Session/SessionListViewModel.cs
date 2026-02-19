using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Session
{
    public class SessionListViewModel
    {
        public string GlobalSessionId { get; set; }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string AuthenticationScheme { get; set; }
        public string LoggedInTime { get; set; }
        public string LastAccessTime { get; set; }
        public string TypeOfDevice { get; set; }
        public string AdditionalValue { get; set; }
        public string UserAgentDetails { get; set; }
        public string ClientId { get; set; }

        public List<SelectListItem> Clientlist { get; set; }
    }
}
