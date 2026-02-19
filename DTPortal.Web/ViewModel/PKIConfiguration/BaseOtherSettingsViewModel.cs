using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.PKIConfiguration
{
    public class BaseOtherSettingsViewModel
    {
        public BaseOtherSettingsViewModel()
        {
            LogLevels = new List<SelectListItem>
            {
                new SelectListItem { Text = "Info", Value = "INFO" },
                new SelectListItem { Text = "Warning", Value = "WARNING" },
                new SelectListItem { Text = "Error", Value = "ERROR" }
            };
        }

        public List<SelectListItem> LogLevels { get; set; }
    }
}
