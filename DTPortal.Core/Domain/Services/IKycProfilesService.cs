using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IKycProfilesService
    {
        public Task<ServiceResult> ListKycProfilesAsync();
        public Task<KycProfile> GetKycProfileByIdAsync(int id);
        public Task<KycProfile> GetKycProfileByNameAsync(string name);
        public Task<ServiceResult> CreateKycProfileAsync(KycProfile kycProfile);
        public Task<ServiceResult> UpdateKycProfileAsync(KycProfile kycProfile);
    }
}
