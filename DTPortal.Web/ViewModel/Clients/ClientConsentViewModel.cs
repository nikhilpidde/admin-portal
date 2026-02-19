using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Services.Communication;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.Clients
{
    public class ClientConsentViewModel
    {
        public List<ProfilesModel> ProfilesList { get; set; } 
        public List<PurposesModel> PurposesList { get; set; } 
    }
}
