using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IKycProfilesRepository : IGenericRepository<KycProfile>
    {
        public Task<IEnumerable<KycProfile>> ListAllKycProfilesAsync();
        public Task<KycProfile> GetKycProfileByNameAsync(string Name);
    }
}
