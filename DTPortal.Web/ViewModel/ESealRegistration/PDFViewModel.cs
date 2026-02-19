using System.Collections.Generic;

using DTPortal.Web.Enums;

using DTPortal.Core.DTOs;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class PDFViewModel
    {
        public string OrganizationName { get; set; }

        public string OrganizationEmail { get; set; }

        public string EmailDomain { get; set; }

        public string UniqueRegdNo { get; set; }

        public string TaxNo { get; set; }

        public string CorporateOfficeAddress1 { get; set; }

        public string CorporateOfficeAddress2 { get; set; }

        public string Country { get; set; }

        public string Pincode { get; set; }

        public List<OrganizationUser> OrganizationUsersList { get; set; }

        public List<string> DirectorsEmailList { get; set; }

        public List<DocumentListItem> DocumentListCheckbox { get; set; }

        public string UserName { get; set; }

        public string PdfLogo { get; set; }

        public string SPOCUgPassEmail { get; set; }

        public bool EnablePostPaidOption { get; set; }

        public string SignatureTemplate { get; set; }

        public string ESealTemplate { get; set; }
    }
}
