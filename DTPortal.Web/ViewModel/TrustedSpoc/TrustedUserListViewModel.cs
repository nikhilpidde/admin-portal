using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.TrustedSpoc
{
    public class TrustedUserListViewModel
    {
        public IEnumerable<TrustedSpocListNewDTO> TrustedSpocList { get; set; } = new List<TrustedSpocListNewDTO>();
        //public IEnumerable<TrustedSpocListDTO> TrustedSpocList { get; set; } = new List<TrustedSpocListDTO>();
    }
}
