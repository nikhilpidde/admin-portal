using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.Beneficiary
{
    public class SponsorBeneficiaryListViewModel
    {
        public IEnumerable<SponsorBeneficiaryDTO> BeneficiaryList { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public IEnumerable<RawCsvDataDTO> CsvData { get; set; }
    }
}
