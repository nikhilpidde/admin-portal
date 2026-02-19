using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DTPortal.Core.Constants;
using Newtonsoft.Json.Linq;

namespace DTPortal.Core.Services
{
    public class ClientsPurposeService:IClientsPurposeService
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        public ClientsPurposeService(IUnitOfWork unitOfWork, ILogClient logClient, ILogger<ClientService> logger)
        {
            _unitOfWork = unitOfWork;
            _LogClient = logClient;
            _logger = logger;
        }


        public async Task<ClientPurposesResponse> AddPurposesToClientAsync(ClientsPurpose clientsPurpose)
        {
            try
            {
                await _unitOfWork.ClientsPurpose.AddAsync(clientsPurpose);
                await _unitOfWork.SaveAsync();
                return new ClientPurposesResponse(clientsPurpose, "purposes added to client successfully");
            }
            catch(Exception)
            {
                _logger.LogError("Failed to add Purposes to client");
                return new ClientPurposesResponse("Error occured adding purposes to client");
            }
        }


        public async Task<ClientPurposesResponse> UpdatePurposesToClientAsync(ClientsPurpose clientsPurpose)
        {
            try
            {
                var clientpurposeInDb = await GetClientsPurposesByClientId(clientsPurpose.ClientId);

                if (clientpurposeInDb == null)
                {
                    _logger.LogError("Failed to get clientpurpose with clientid");

                    return new ClientPurposesResponse("Error occured updating client purposes");
                }
                clientpurposeInDb.PurposesAllowed = clientsPurpose.PurposesAllowed;

                _unitOfWork.ClientsPurpose.Update(clientpurposeInDb);

                await _unitOfWork.SaveAsync();

                return new ClientPurposesResponse(clientsPurpose, "client purposes updated successfully");
            }
            catch(Exception)
            {
                _logger.LogError("Failed to add purposes");
                return new ClientPurposesResponse("Error occured updating client purposes");
            }
        }


        public async Task<IEnumerable<string>> GetPurposeByClientId(string clientId)
        {
            try
            {
                IEnumerable<string> purposeList=new List<string>();

                var allowedPurposes = await _unitOfWork.ClientsPurpose.GetPurposesByClientIdAsync(clientId);
                if(allowedPurposes == null)
                {
                    return purposeList;
                }
                IEnumerable<string> allowedPurposesList = allowedPurposes.Split(',');
                return allowedPurposesList;
            }
            catch(Exception)
            {
                _logger.LogError("Failed to get clientPurposesList");
                return null;
            }

        }


        public async Task<ClientsPurpose> GetClientsPurposesByClientId(string clientId)
        {
            try
            {
                var clientPurposeInDb = await _unitOfWork.ClientsPurpose.GetByClientIdAsync(clientId);
                if (clientPurposeInDb == null)
                {
                    _logger.LogError("Failed to get clientpurpose with clientid");
                    return null;
                }
                return clientPurposeInDb;
            }
            catch(Exception)
            {
                _logger.LogError("Failed to get clientpurpose with clientid");
                return null;
            }
        }

        public async Task<bool> IsClientExist(string clientId)
        {
            try
            {
                return await _unitOfWork.ClientsPurpose.IsClientExist(clientId);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
