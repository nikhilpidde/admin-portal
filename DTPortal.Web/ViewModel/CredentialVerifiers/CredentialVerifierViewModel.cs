using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.CredentialVerifiers
{
    public class CredentialVerifierViewModel
    {
        public int Id { get; set; }
        public string credentialName { get; set; }
        public string organizationName { get; set; }
        public List<DataAttributes> attributes { get; set; }
        public List<CredentialConfig> configuration { get; set; }
        public string status { get; set; }
        public List<string> emails { get; set; }
        public string remarks { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

}
