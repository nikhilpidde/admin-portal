using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.Beneficiary
{
    public class AddMultipleBeneficiariesViewModel
    {
        public IList<BeneficiaryAddDTO> MultipleBeneficiaries { get; set; } = new List<BeneficiaryAddDTO>();

        public string PdfFile { get; set; }
    }
}
