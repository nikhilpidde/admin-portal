using System.Collections.Generic;
using DTPortal.Core;
using DTPortal.Core.Domain.Models;


namespace DTPortal.Web.ViewModel.TimeBasedAccess
{
    public class TimeBasedAccessListViewModel
    {
        public IEnumerable<DTPortal.Core.Domain.Models.TimeBasedAccess> List { get; set; }

    }
}
