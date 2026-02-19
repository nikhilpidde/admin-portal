using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.Dashboard
{
    public class ServiceHealthHistoryViewModel
    {
        public string DisplayName { get; set; }

        public string ServiceName { get; set; }

        public IEnumerable<ServiceHealthHistory> ServiceHealthHistory { get; set; }
    }
}
