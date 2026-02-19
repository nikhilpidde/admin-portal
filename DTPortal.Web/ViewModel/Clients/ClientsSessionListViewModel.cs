using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Clients
{
    public class ClientsSessionListViewModel
    {
        public string ClientName { get; set; }
        public IList<GlobalSession> session { get; set; }
    }
}
