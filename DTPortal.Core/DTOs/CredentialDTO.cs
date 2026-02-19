using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class CredentialDTO
    {
        public int Id { get; set; }

        public string credentialName { get; set; }
        public string displayName { get; set; }
        public string credentialId { get; set; }

        public string credentialUId { get; set; }
        public string remarks { get; set; }
        public List<int> categories {  get; set; }

        public string verificationDocType { get; set; }

        public List<DataAttributesDTO> dataAttributes { get; set; }

        public string authenticationScheme { get; set; }

        public string categoryId { get; set; }
        public int validity { get; set; }

        public string organizationId { get; set; }

        public string trustUrl { get; set; }

        public List<string> serviceDetails { get; set; }

        public string credentialOffer {  get; set; }

        public DateTime createdDate { get; set; }

        public string signedDocument { get; set; }
        public string logo { get; set; }
        public string status { get; set; }
    }
    public class DataAttributesDTO
    {

        public string displayName { get; set; }
        public string attribute { get; set; }
        public int dataType { get; set; }


    }
}
