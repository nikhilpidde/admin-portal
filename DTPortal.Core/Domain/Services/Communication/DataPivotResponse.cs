using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class DataPivotResponse : BaseResponse<DataPivot>
    {
        public DataPivotResponse(DataPivot dataPivot) : base(dataPivot) { }

        public DataPivotResponse(string message) : base(message) { }

        public DataPivotResponse(DataPivot dataPivot, string message) : base(dataPivot, message) { }
    
    }
}
