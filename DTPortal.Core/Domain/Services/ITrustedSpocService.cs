using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ITrustedSpocService
    {
        Task<APIResponse> AddTrustedSpocAsync(TrustedUserRequestDTO request);
        Task<IEnumerable<TrustedSpocListUpdated>> GetTrustedSpocList();
        Task<IEnumerable<TrustedSpocListNewDTO>> GetTrustedSpocList1();
        Task<ServiceResult> AddTrustedSpocAsync1(TrustedSpocAddDTO trustedSpocAddDTO);
        Task<ServiceResult> VerifyOrganizationTin(string organizationTin);
        Task<ServiceResult> VerifyCeoTin(string ceoTin);
        Task<TrustedSpocListDTO> GetSpocDetailsByIDasync(int id);
        Task<ServiceResult> SuspendSpoc(int id);
        Task<ServiceResult> RemoveSuspensionSpoc(int id);
        Task<ServiceResult> ReInviteSpoc(int id);
    }
}
