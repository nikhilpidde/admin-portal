using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IKycApplicationService
    {
        public Task<ClientResponse> CreateClientAsync(Client client);

        public Task<Client> GetClientAsync(int id);

        public Task<Client> GetClientByAppNameAsync(string appName);
        public Task<Client> GetClientByClientIdAsync(string clientId);
        public Task<ClientResponse> UpdateClientAsync(Client ipClient);
        public Task<ClientResponse> DeActivateClientAsync(int id);
        public Task<ClientResponse> ActivateClientAsync(int id);
        public Task<IEnumerable<Client>> ListClientAsync();
        public Task<IEnumerable<Client>> ListOAuth2ClientAsync();
        public Task<IEnumerable<Client>> GetKycClientList();
    }
}
