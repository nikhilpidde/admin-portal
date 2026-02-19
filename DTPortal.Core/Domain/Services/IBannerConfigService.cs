using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IBannerConfigService
    {
        Task<List<BannerTextData>> GetLatestBannerTextsAsync(string bannerTextId);
        Task<ServiceResult> UpdateBannerTextsAsync(UpdateBannerTextRequestDTO request);
    }

}
