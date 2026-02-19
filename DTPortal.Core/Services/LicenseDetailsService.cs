using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;

namespace DTPortal.Core.Services
{
    public class LicenseDetailsService : ILicenseDetailsService
    {
        public LicenseDetailsService()
        {

        }

        public LicenseDetails GetLicenseDetailsAsync(string path)
        {
           var data = PKIMethods.Instance.PKIGetLicenseData(path);
           return new LicenseDetails { TotalSubscribersCertificates = data.Item1, TotalCertificates = data.Item2, TotalOnboardings = data.Item3 };
        }
    }
}
