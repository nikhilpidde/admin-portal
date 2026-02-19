using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class AuthenticateUserRequest
    {
        public string SessionId { get; set; }
        public string AuthenticationScheme { get; set; }
        public string AuthenticationData { get; set; }
        public string Response { get; set; }
        public bool Approved { get; set; }
        public string UserId { get; set; }
        public List<ProfileInfo> scopes { get; set; }
    }
}
