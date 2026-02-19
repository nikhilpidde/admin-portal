using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IClientsPurposeService
    {
        public Task<ClientPurposesResponse> AddPurposesToClientAsync(ClientsPurpose clientsPurpose);

        public Task<ClientPurposesResponse> UpdatePurposesToClientAsync(ClientsPurpose clientsPurpose);

        public Task<IEnumerable<string>> GetPurposeByClientId(string clientId);

        public Task<ClientsPurpose> GetClientsPurposesByClientId(string clientId);

        public Task<bool> IsClientExist(string clientId);

    }
}
