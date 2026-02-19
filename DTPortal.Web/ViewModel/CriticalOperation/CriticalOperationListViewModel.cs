using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.CriticalOperation
{
    public class CriticalOperationListViewModel
    {
        public IEnumerable<OperationsAuthscheme> List { get; set; }
       
    }
}
