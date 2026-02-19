using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.MobileVersionConfiguration
{
    public class BaseMobileVersionConfigurationViewModel
    {
        public BaseMobileVersionConfigurationViewModel()
        {
            OSVersions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Android", Value = "ANDROID" },
                new SelectListItem { Text = "iOS", Value = "IOS" }
            };
        }

        public List<SelectListItem> OSVersions { get; set; }
    }
}
