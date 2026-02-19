using Fido2NetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class GetUserProfileRequest
    {
        public string UserId { get; set; }
        public string UserIdType { get; set; }
        public string ProfileType { get; set; }
        public string Purpose { get; set; }
        public string ClientId { get; set; }
        public string Scopes { get; set; }
        public string Token { get; set; }
        public string Suid { get; set; }
    }
}
