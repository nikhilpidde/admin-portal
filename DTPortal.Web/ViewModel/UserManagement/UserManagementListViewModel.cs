using System.Collections.Generic;
using DTPortal.Core;
using DTPortal.Core.Domain.Models;


namespace DTPortal.Web.ViewModel.UserManagement
{
    public class UserManagementListViewModel
    {
        public PaginatedList<UserTable> Users { get; set; }

      //  public IEnumerable<UserTable> Users { get; set; }
    }
}
