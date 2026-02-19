using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class CredentialVerifierDTO
    {
        public int id { get; set; }
        public string credentialId {  get; set; }
        public string organizationId { get; set; }
        public string credentialName { get; set; }
        public string organizationName { get; set; }
        public List<DataAttributes> attributes { get; set; }
        public List<CredentialConfig> configuration { get; set; }
        public List<string> emails { get; set; }
        public string status { get; set; }
        public DomainConfig domainConfig { get; set; }
        public int validity { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? updatedDate { get; set; }
        public string remarks { get; set; }
    }
    public class DataAttributes
    {

        public string displayName { get; set; }
        public string attribute { get; set; }
        public int dataType { get; set; }
        public bool mandatory { get; set; }

    }
    public class CredentialConfig
    {
        public string format { get; set; }
        public string bindingMethod { get; set; }
        public string supportedMethod { get; set; }
    }
    public class DomainConfig
    {
        public string domain { get; set; }
        public List<string> purposesList { get; set; }
    }
}
