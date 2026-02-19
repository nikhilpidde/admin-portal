using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class KycApplicationService: IKycApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<KycApplicationService> _logger;
        public KycApplicationService(IUnitOfWork unitOfWork,
            ILogger<KycApplicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<ClientResponse> CreateClientAsync(Client client)
        {
            _logger.LogInformation("--->CreateClientAsync");

            var isExists = await _unitOfWork.Client.IsClientExistsWithNameAsync(
                client);
            if (true == isExists)
            {
                _logger.LogError("Client already exists with given client id");
                return new ClientResponse("Application already exists with given" +
                    " Client Id");
            }

            isExists = await _unitOfWork.Client.IsClientExistsWithAppNameAsync(
                client);
            if (true == isExists)
            {
                _logger.LogError("Application already exists with given application name");
                return new ClientResponse("Application already exists with given" +
                    " Name");
            }

            isExists = await _unitOfWork.Client.IsClientExistsWithRedirecturlAsync(
                client);
            if (true == isExists)
            {
                _logger.LogError("Application already exists with given redirect url");
                return new ClientResponse("Application already exists with given" +
                    " Redirect url");
            }

            isExists = await _unitOfWork.Client.IsClientExistsWithAppUrlAsync(
                client);
            if (true == isExists)
            {
                _logger.LogError("Application already exists with given application url");
                return new ClientResponse("Application already exists with given" +
                    " Application url");
            }

            try
            {
                client.CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                client.ModifiedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                client.Status = "ACTIVE";
                client.Hash = "na";
                client.WithPkce = false;
                await _unitOfWork.Client.AddAsync(client);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("<---CreateClientAsync");

                return new ClientResponse(client, "Application created successfully");
            }
            catch
            {
                _logger.LogError("Application AddAsync failed");
                _logger.LogInformation("<---CreateApplicationAsync");
                return new ClientResponse("An error occurred while creating the Application." +
                    " Please contact the admin.");
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Client> GetClientAsync(int id)
        {
            _logger.LogInformation("--->GetClientAsync");
            var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
            if (null == clientInDb)
            {
                return null;
            }

            if (null != clientInDb.PublicKeyCert)
            {
                try
                {
                    clientInDb.PublicKeyCert = Encoding.UTF8.GetString(Convert.FromBase64String(
                        clientInDb.PublicKeyCert));
                }
                catch (Exception error)
                {
                    // do nothing
                    _logger.LogError("GetClientAsync Failed: {0}", error.Message);
                }
            }

            if (clientInDb.Type == "SAML2")
            {
                return await _unitOfWork.Client.GetClientByIdWithSaml2Async(id);
            }

            return clientInDb;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Client> GetClientByAppNameAsync(string appName)
        {
            _logger.LogInformation("--->GetClientByAppNameAsync");
            var clientInDb = await _unitOfWork.Client.GetClientByAppNameAsync(appName);
            if (null == clientInDb)
            {
                return null;
            }

            if (clientInDb.Type == "SAML2")
            {
                return await _unitOfWork.Client.GetClientByIdWithSaml2Async(clientInDb.Id);
            }

            return clientInDb;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<Client> GetClientByClientIdAsync(string clientId)
        {
            _logger.LogDebug("--->GetClientByClientIdAsync");
            var clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(clientId);
            if (null == clientInDb)
            {
                return null;
            }

            if (clientInDb.Type == "SAML2")
            {
                return await _unitOfWork.Client.GetClientByIdWithSaml2Async(clientInDb.Id);
            }

            return clientInDb;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<ClientResponse> UpdateClientAsync(Client ipClient)
        {
            _logger.LogInformation("--->UpdateClientAsync");

            Client client = ipClient;

            var isExists = await _unitOfWork.Client.IsClientExistsAsync(client.ClientId);
            if (false == isExists)
            {
                _logger.LogError("Application not found");
                return new ClientResponse("Application not found");
            }
            var allClients = await _unitOfWork.Client.GetAllAsync();

            foreach (var item in allClients)
            {
                if (item.ClientId != client.ClientId)
                {
                    if (item.RedirectUri == client.RedirectUri)
                    {
                        _logger.LogError("Application already exists with given redirect uri");
                        return new ClientResponse("Application already exists with given redirect uri");
                    }
                    if (item.ApplicationName == client.ApplicationName)
                    {
                        _logger.LogError("Application already exists with given application name");
                        return new ClientResponse("Application already exists with given application name");
                    }
                    if (item.ApplicationUrl == client.ApplicationUrl)
                    {
                        _logger.LogError("Application already exists with given application name");
                        return new ClientResponse("Application already exists with given application name");
                    }
                }
            }

            var clientInDb = _unitOfWork.Client.GetById(client.Id);
            if (null == clientInDb)
            {
                _logger.LogError("Application not found");
                return new ClientResponse("Application not found");
            }


            if (clientInDb.Status == "DELETED")
            {
                _logger.LogError("Application is already deleted");
                return new ClientResponse("Application is already deleted");
            }

            try
            {
                clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(client.ClientId);
                if (null == clientInDb)
                {
                    _logger.LogError("Application not found");
                    return new ClientResponse("Application not found");
                }

                //clientInDb.Id = client.Id;
                clientInDb.Scopes = client.Scopes;
                clientInDb.RedirectUri = client.RedirectUri;
                clientInDb.ClientSecret = client.ClientSecret;
                clientInDb.ClientId = client.ClientId;
                clientInDb.ResponseTypes = client.ResponseTypes;
                clientInDb.LogoutUri = client.LogoutUri;
                clientInDb.ModifiedDate =
                DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                clientInDb.EncryptionCert = client.EncryptionCert;
                clientInDb.GrantTypes = client.GrantTypes;
                clientInDb.ApplicationName = client.ApplicationName;
                clientInDb.ApplicationType = client.ApplicationType;
                clientInDb.ApplicationUrl = client.ApplicationUrl;
                clientInDb.OrganizationUid = client.OrganizationUid;


                _unitOfWork.Client.Update(clientInDb);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("<---UpdateClient");
                return new ClientResponse(client, "Application updated successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("Application Update failed : {0}", error.Message);
                return new ClientResponse("An error occurred while updating the Application." +
                    " Please contact the admin.");
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public async Task<ClientResponse> DeActivateClientAsync(int id)
        {
            var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
            if (null == clientInDb)
            {
                return new ClientResponse("Client not found");
            }

            clientInDb.Status = "DEACTIVATED";

            try
            {
                _unitOfWork.Client.Update(clientInDb);
                await _unitOfWork.SaveAsync();

                return new ClientResponse(clientInDb);
            }
            catch
            {
                return new ClientResponse("An error occurred while deleting the client." +
                    " Please contact the admin.");
            }

        }
        public async Task<ClientResponse> ActivateClientAsync(int id)
        {
            var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
            if (null == clientInDb)
            {
                return new ClientResponse("Client not found");
            }

            clientInDb.Status = "ACTIVE";

            try
            {
                _unitOfWork.Client.Update(clientInDb);
                await _unitOfWork.SaveAsync();

                return new ClientResponse(clientInDb);
            }
            catch
            {
                return new ClientResponse("An error occurred while deleting the client." +
                    " Please contact the admin.");
            }

        }
        public async Task<IEnumerable<Client>> ListClientAsync()
        {
            return await _unitOfWork.Client.ListAllClient();
        }
        public async Task<IEnumerable<Client>> ListOAuth2ClientAsync()
        {
            return await _unitOfWork.Client.ListOAuth2ClientAsync();
        }
        public async Task<IEnumerable<Client>> GetKycClientList()
        {
            return await _unitOfWork.Client.GetKycClientsList();
        }

    }
}
