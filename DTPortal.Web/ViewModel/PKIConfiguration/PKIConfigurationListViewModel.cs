using System.Collections.Generic;

using DTPortal.Core.Domain.Models;

namespace DTPortal.Web.ViewModel.PKIConfiguration
{
    public class PKIConfigurationListViewModel
    {
        public IEnumerable<PkiPluginDatum> Configurations { get; set; }
    }
}
