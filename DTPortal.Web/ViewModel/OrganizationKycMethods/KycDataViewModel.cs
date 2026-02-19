using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.OrganizationKycMethods
{
    public class KycDataViewModel
    {
        public List<KycMethodsViewModel> Methods { get; set; }
        public List<KycProfilesViewModel> Profiles { get; set; }
    }
}
