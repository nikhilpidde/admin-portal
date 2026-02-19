using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DTPortal.Web.ViewModel.CredentialApproval
{
    public class CredentialDetailsViewModel
    {
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Credential UId")]
        public string credentialUId { get; set; }
        public string remarks { get; set; }
        [DisplayName("Trust Gateway Credential Url")]
        public string trustUrl { get; set; }

        [DisplayName("Display Name ")]
        public string DisplayName { get; set; }

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

        [DisplayName("Logo")]
        public string logo { get; set; }

        [DisplayName("Created Date")]
        public string createdDate { get; set; }

        public string signedDocument { get; set; }
    }
}
