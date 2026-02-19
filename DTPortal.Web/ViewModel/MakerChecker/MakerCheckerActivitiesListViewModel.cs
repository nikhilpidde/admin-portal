using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.MakerChecker
{
    public class MakerCheckerActivitiesListViewModel
    {
        public MakerCheckerActivitiesListViewModel()
        {
            Activities = new List<SelectActivityItem>();
        }
        public IList<SelectActivityItem> Activities { get; set; }
    }

    public class SelectActivityItem
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public bool IsSelected { get; set; }
    }
}
