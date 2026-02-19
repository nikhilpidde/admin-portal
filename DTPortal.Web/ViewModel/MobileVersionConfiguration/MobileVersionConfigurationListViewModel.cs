using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.MobileVersionConfiguration
{
    public class MobileVersionConfigurationListViewModel
    {
        public IEnumerable<MobileVersionDTO> MobileVersions { get; set; }
    }
}
