using DTPortal.Core.DTOs;
using System.Collections.Generic;
using System.ComponentModel;

namespace DTPortal.Web.ViewModel.QrCredentialApproval
{
    public class QrCredentialDetailsViewModel
    {
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Credential UId")]
        public string credentialUId { get; set; }
        public string remarks { get; set; }

        [DisplayName("Credential Name ")]
        public string CredentialName { get; set; }

        [DisplayName("Organization Name")]
        public string organizationName { get; set; }
        [DisplayName("Data Attributes")]
        public QrAttributesDTO dataAttributes { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Created Date")]
        public string createdDate { get; set; }

    }
}
