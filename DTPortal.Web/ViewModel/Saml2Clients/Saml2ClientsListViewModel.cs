using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Saml2Clients
{
    public class Saml2ClientsListViewModel
    {
        public int Id { get; set; }
        public string ClientID { get; set; }
        public string ApplicationType { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationUri { get; set; }

        public string State { get; set; }
    }
}
