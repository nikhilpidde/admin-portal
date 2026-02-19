using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface ISigningCreditsService
    {
        Task<ServiceResult> GetBucketList();
        Task<ServiceResult> GetBucketDetailsById(string Id);
        Task<ServiceResult> SaveBucket(SaveBucketConfigDTO saveBucketConfigDTO,string UUID, bool makerCheckerFlag = false);
        Task<ServiceResult> UpdateBucket(UpdateBucketConfigDTO updateBucketConfigDTO, string createdBy, bool makerCheckerFlag = false);
        Task<ServiceResult> GetBucketConfigById(string Id);
        Task<ServiceResult> BucketHistoryList(string Id);
    }
}
