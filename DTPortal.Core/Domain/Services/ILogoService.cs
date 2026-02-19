using System.Threading.Tasks;

using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface ILogoService
    {
        Task<PortalSetting> GetLogoPrimary();

        Task<ServiceResult> UpdateLogoPrimary(string base64Image, string updatedBy);
    }
}
