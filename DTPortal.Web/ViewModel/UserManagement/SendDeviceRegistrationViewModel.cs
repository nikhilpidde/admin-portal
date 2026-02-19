using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class SendDeviceRegistrationViewModel
    {
        public string Uuid { get; set; }

        public string RegistrationType { get; set; }

        public int id { get; set; }

        public DateTime? Expiry { get; set; }
    }
}
