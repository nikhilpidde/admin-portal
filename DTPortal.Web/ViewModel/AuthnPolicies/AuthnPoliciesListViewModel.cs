using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.AuthnPolicies
{
    public class AuthnPoliciesListViewModel
    {
        public IEnumerable<AuthScheme> authList { get; set; }
    }
}
