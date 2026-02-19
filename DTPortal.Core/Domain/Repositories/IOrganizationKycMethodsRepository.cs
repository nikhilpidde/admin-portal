using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IOrganizationKycMethodsRepository 
        : IGenericRepository<OrganizationKycMethod>
    {
        public Task<OrganizationKycMethod> GetOrganizationKycMethodsByOrgIdAsync(string orgId);
    }
}
