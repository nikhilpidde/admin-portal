using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Web.ViewModel.Clients
{
    public class ClientsListViewModel
    {
        public int Id { get; set; }
        public string ApplicationType { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationUri { get; set; }

        public string ClientID { get; set; }

        public string RedirectUri { get; set; }

        public string GrantTypes { get; set; }

        public string Scopes { get; set; }

        public string State { get; set; }
        public string OrgnizationId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
