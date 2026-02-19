using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IProvisionStatusRepository : IGenericRepository<ProvisionStatus>
    {
        public Task<ProvisionStatus> GetProvisionStatus(string suid, string credentialId);
        public Task<ProvisionStatus> GetProvisionStatusByDocumentId(string credentialId, string documentId);
    }
}
