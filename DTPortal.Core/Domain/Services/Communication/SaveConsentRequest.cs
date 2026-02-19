using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class SaveConsentRequest
    {
        public string sessionId { get; set; }
        public string suid { get; set; }
        public List<ProfileInfo> scopes { get; set; }
    }
}
