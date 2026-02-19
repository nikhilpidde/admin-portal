using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class user
    {
        public int Id { get; set; }
        public string Uuid { get; set; }

        public string FullName { get; set; }
        public string MailId { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}
