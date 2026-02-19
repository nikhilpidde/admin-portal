using DTPortal.Core;
using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.EConsent
{
    public class EConsentClientListViewModel
    {
        public int Id { get; set; }

        public string ApplicationName { get; set; }

        public string Scopes { get; set; }

        public string PublicKeyCert { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public string Status { get; set; }

        public string OrganizationUid { get; set; }

        public string Purposes { get; set; }
    }
}
