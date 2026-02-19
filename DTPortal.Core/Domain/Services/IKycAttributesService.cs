using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IKycAttributesService
    {
        public Task<IEnumerable<KycAttribute>> ListKycAttributesAsync();
    }
}
