using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.MakerChecker
{
    public class MakerCheckerListViewModel
    {
        //public IEnumerable<DTPortal.Core.Domain.Models.MakerChecker> ApprovalList { get; set; }

        public DTPortal.Core.Domain.Models.MakerChecker ApprovalList { get; set; }

        public string OperationName { get; set; }
    }
}
