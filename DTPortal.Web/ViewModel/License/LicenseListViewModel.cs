using DTPortal.Core.DTOs;
using System.Collections;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.License
{
    public class LicenseListViewModel
    {
        public IEnumerable<LicenseDTO> LicenseList { get; set; }
    }
}
