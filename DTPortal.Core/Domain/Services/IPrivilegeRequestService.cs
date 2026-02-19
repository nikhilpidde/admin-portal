using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IPrivilegeRequestService
    {
        public Task<ServiceResult> GetAllUniquePreviligesAsync();
        public Task<ServiceResult> GetAllPrivilegesAsync();

        public Task<ServiceResult> GetPrivilegeByIdAsync(int id);

        public Task<ServiceResult> UpdatePrivilegeAsync(UpdatePrivilegeDTO updatePrivilegeModel);

        public Task<ServiceResult> GetPrivilegesByOrganizationIdAsync(string organizationId);

        public Task<ServiceResult> UpdateOrganizationPrivilegesAsync(UpdateOrganizationPrivilegesDTO updateOrgPrivilegesModel);

    }
}
