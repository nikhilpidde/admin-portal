using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.SelfServicePortal
{
    public class OrgNewDetailsViewModel
    {
        public Guid Ouid { get; set; }
        public int OrgDetailsId { get; set; }

        [Display(Name = "Organization Name")]
        public string OrgName { get; set; }
        public string OrgNo { get; set; }
        [Display(Name = "Organization Tin Number")]
        public string TaxNumber { get; set; }
        [Display(Name = "Organization Category")]
        public string OrgType { get; set; }
        [Display(Name = "Organization Registration ID Number")]
        public string RegNo { get; set; }

        public string OrgEmail { get; set; }
        [Display(Name = "Corporate Office Address")]
        public string Address { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "SPOC Name")]
        public string SpocName { get; set; }
        public string SpocOfficialEmail { get; set; }
        [Display(Name = "SPOC Id Document Number")]
        public string SpocDocumentNumber { get; set; }
        [Display(Name = "Auditor Name")]
        public string AuditorName { get; set; }
        [Display(Name = "Auditor ID Document Number")]
        public string AuditorDocumentNumber { get; set; }
        [Display(Name = "Auditor  Email")]
        public string AuditorOfficialEmail { get; set; }
        [Display(Name = "Created On")]
        public string CreatedOn { get; set; }
        public List<OrganizationDocumentDto> Documents { get; set; }

    }
}
