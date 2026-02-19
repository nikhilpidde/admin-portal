using System.Collections.Generic;

namespace DTPortal.Core.DTOs
{
    public class GetAgentDetailsDTO
    {
        public string Principal { get; set; }
        public List<string> DelegationPurpose { get; set; }
        public string NotaryInformation { get; set; }
        public string ValidityPeriod { get; set; }
        public string Agent { get; set; }
    }
}
