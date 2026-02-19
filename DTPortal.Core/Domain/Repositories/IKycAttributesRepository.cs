using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IKycAttributesRepository : IGenericRepository<KycAttribute>
    {
        public Task<IEnumerable<KycAttribute>> ListAllKycAttributesAsync();
    }
}
