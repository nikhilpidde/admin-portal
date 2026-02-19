using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.Package
{
    public class PackageListViewModel
    {
        public IEnumerable<PackageDTO> Packages { get; set; }
    }
}
