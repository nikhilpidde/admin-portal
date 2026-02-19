using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.TrustedSpoc
{
    public class TrustedUserNewList
    {
        public IEnumerable<TrustedSpocListUpdated> TrustedSpocList { get; set; } = new List<TrustedSpocListUpdated>();
    }
}
