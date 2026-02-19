using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Web.ViewModel.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class UserManagementSessionListViewModel
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public IEnumerable<SessionListViewModel> list { get; set; }
       
    }
}
