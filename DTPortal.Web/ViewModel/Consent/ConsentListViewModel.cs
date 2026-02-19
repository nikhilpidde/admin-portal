using System.Collections.Generic;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.Consent
{
    public class ConsentListViewModel
    {
        public IEnumerable<ConsentDTO> Consents { get; set; }
    }
}
