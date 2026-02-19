using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace DTPortal.Web.ViewModel.Purposes
{
    public class PurposeListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool UserConsent { get; set; }
    }
}
