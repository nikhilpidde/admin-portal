using System.Collections.Generic;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.ViewModel.WalletConfiguration
{
    public class WalletConfigurationViewModel
    {
        public List<CredentialFormats> CredentialFormats { get; set; }
        public List<BindingMethod> BindingMethods { get; set; }
    }

    //public class CredentialFormats
    //{
    //    public string Name { get; set; }
    //    public string DisplayName { get; set; }
    //    public bool isSelected { get; set; }
    //}
    //public class BindingMethods
    //{
    //    public string Name { get; set; }
    //    public string DisplayName { get; set; }
    //    public List<SupportedMethods> SupportedMethods { get; set; }
    //}
    //public class SupportedMethods
    //{
    //    public string Name { get; set; }
    //    public string DisplayName { get; set; }
    //    public bool isSelected { get; set; }
    //}
}
