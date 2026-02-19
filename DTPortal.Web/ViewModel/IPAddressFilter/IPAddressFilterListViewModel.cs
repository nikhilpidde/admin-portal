using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.IPAddressFilter
{
    public class IPAddressFilterListViewModel
    {
        public IEnumerable<IpBasedAccess> List { get; set; }
    }
}
