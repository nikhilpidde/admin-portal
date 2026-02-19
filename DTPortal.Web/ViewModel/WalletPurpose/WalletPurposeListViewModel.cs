using System;

namespace DTPortal.Web.ViewModel.WalletPurpose
{
    public class WalletPurposeListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }

    }
}
