using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class TestCredentialRequest
    {
        public string UserId { get; set; }
        public string CredentialId { get; set; }
    }
    public class QrTestCredentialRequest
    {
        public string CredentialId { get; set; }
        public Dictionary<string, JsonElement> Data { get; set; }
    }
    public class ApprovalRequest
    {
        public string credentialId { get; set;}
        public string signedDocument { get; set;}
    }
}
