using System.Threading.Tasks;
using System.Collections.Generic;

using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IPackageService
    {
        Task<IEnumerable<PackageDTO>> GetAllPackagesAsync();

        Task<PackageDTO> GetPackageAsync(int id);

        Task<bool> IsPackageExists(string packageCode);

        Task<ServiceResult> AddPackageAsync(PackageDTO package, bool makerCheckerFlag = false);

        Task<ServiceResult> EnablePackageAsync(int id, string uuid, bool makerCheckerFlag = false);

        Task<ServiceResult> DisablePackageAsync(int id, string uuid, bool makerCheckerFlag = false);

        Task<ServiceResult> DeletePackageAsync(int id, string uuid, bool makerCheckerFlag = false);
    }
}
