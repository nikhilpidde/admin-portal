using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace DTPortal.Web.ViewModel.Configuration
{
    public class DefaultAuthenticationSchemaViewModel
    {
        public List<SelectListItem> AuthenticationSchemesList { get; set; }
        [DisplayName("Auth Scheme")]
        public string AuthSchemeId { get; set; }
    }
}
