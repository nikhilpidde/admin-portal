using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class MakerCheckerResponse : BaseResponse<MakerChecker>
    {
        public MakerCheckerResponse(MakerChecker category) : base(category) { }

        public MakerCheckerResponse(string message) : base(message) { }

        public MakerCheckerResponse(MakerChecker category, string message) :
            base(category, message) { }
    }

}
