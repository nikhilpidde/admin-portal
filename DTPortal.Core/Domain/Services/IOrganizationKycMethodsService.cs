using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IOrganizationKycMethodsService
    {
        public Task<ServiceResult> GetOrganizationKycMethodsByOrgIdAsync
            (string organizationId);
        public Task<ServiceResult> GetOrganizationKycProfilesByOrgIdAsync
            (string organizationId);
        public Task<ServiceResult> GetOrganizationKycMethodsByClientIdAsync
            (string clientId);
        public Task<ServiceResult> GetOrganizationProfile(string organizationId);
        public Task<ServiceResult> AddOrganizationKycMethodsAsync(
            OrganizationKycMethod organizationKycMethod);
        public Task<ServiceResult> UpdateOrganizationKycMethodAsync(
            OrganizationKycMethod updatedKycMethod);
    }
}
