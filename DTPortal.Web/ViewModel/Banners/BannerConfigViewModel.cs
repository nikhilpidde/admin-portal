using System.Collections.Generic;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace DTPortal.Web.ViewModel.Banners
{
    public class BannerConfigViewModel
    {
        public List<BannerTextData> BannerTextList { get; set; } = new();
    }



}
