using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.RoleManagement
{
    public class ActivityTreeItem
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public Object state { get; set; }
    }
}
