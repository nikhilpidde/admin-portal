using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface ILicenseDetailsService
    {
        LicenseDetails GetLicenseDetailsAsync(string path);
    }
}