using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.Configuration
{
    public class ConfigrationViewModel
    {
        public string ConfigName { get; set; }
        public IDPConfigureationViewModel idp { get; set; }

        public SSOConfigurationViewModel sso { get; set; }

        public AdminPortalSSOConfiguratonViewModel adminPortalsso { get; set; }
    }
}
