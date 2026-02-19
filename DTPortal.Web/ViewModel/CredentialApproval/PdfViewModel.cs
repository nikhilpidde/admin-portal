using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.ComponentModel;

namespace DTPortal.Web.ViewModel.CredentialApproval
{
    public class PdfViewModel
    {

        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Credential UId")]
        public string credentialUId { get; set; }
        public string remarks { get; set; }
        [DisplayName("Credential Name ")]
        public string CredentialName { get; set; }

        [DisplayName("Authentication Scheme ")]
        public string authenticationScheme { get; set; }

        [DisplayName("Verification Document Type")]
        public string verificationDocType { get; set; }

        [DisplayName("Organization Name")]
        public string organizationName { get; set; }

        [DisplayName("Category Name")]
        public string categoryName { get; set; }
        [DisplayName("Data Attributes")]
        public List<DataAttributesDTO> dataAttributes { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        public string UserName { get; set; }


        public string PdfLogo {  get; set; }
    }
}
