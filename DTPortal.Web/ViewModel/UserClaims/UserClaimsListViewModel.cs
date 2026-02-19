using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserClaims
{
    public class UserClaimsListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool UserConsent { get; set; }
        public bool DefaultClaim { get; set; }
    }
}
