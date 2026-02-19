using System.Collections.Generic;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class ConsentRequest
    {
        public string SessionId { get; set; }
    }

    public class ConsentResponse
    {
        public string clientId { get; set; }
        public string clientName { get; set; }
        public bool consentRequired { get; set; }
        public List<ScopeDetail> scopes { get; set; }
    }

    public class ScopeDetail
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<AttributeInfo> Attributes { get; set; }

    }

    public class AttributeInfo
    {
        public string Name { get; set; }
        public bool Mandatory { get; set; }
        public string DisplayName { get; set; }
    }

    public class ConsentApprovalRequest
    {
        public string clientId { get; set; }
        public string suid { get; set; }
        public List<ScopeObject> scopes { get; set; }

    }

    public class ScopeObject
    {
        public string Name { get; set; }
        public List<string> Attributes { get; set; }
    }

    public class CredentialDetail
    {
        public string CredentialId { get; set; }
        public string DisplayName { get; set; }
        public List<ClaimsDetail> Attributes { get; set; }
    }

    public class ProfileInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<ClaimsDetail> Attributes { get; set; }
    }

    public class ClaimsDetail
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
