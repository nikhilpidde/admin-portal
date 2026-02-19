using System;

namespace DTPortal.Web.ViewModel.QrCredentialApproval
{
    public class QrCredentialListViewModel
    {
        public int Id { get; set; }
        public string CredentialName { get; set; }

        public string organizationName { get; set; }
        public string authenticationScheme { get; set; }


        public string verificationDocType { get; set; }
        public string status { get; set; }

        public DateTime createdDate { get; set; }
    }
}
