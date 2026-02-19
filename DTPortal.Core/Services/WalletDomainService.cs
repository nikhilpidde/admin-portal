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
using DTPortal.Core.DTOs;

namespace DTPortal.Core.Services
{
    public class WalletDomainService : IWalletDomainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IMCValidationService _mcValidationService;
        private readonly ICacheClient _cacheClient;

        public WalletDomainService(ILogger<ClientService> logger,
            IConfigurationService configurationService,
            IUnitOfWork unitOfWork, ILogClient logClient,
            IMCValidationService mcValidationService,
            ICacheClient cacheClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configurationService = configurationService;
            _LogClient = logClient;
            _mcValidationService = mcValidationService;
            _cacheClient = cacheClient;
        }

        public async Task<WalletDomainResponse> CreateDomainAsync(WalletDomain walletDomain)
        {
            _logger.LogDebug("--->CreateWalletDomainAsync");

            var isExists = await _unitOfWork.WalletDomain.IsScopeExistsWithNameAsync(
                walletDomain.Name);
            if (true == isExists)
            {
                _logger.LogError("Wallet Domain already exists with given name");
                return new WalletDomainResponse("Wallet Domain already exists with given Name");
            }



            walletDomain.CreatedDate = DateTime.Now;
            walletDomain.ModifiedDate = DateTime.Now;
            walletDomain.Status = StatusConstants.ACTIVE;
            try
            {
                await _unitOfWork.WalletDomain.AddAsync(walletDomain);
                await _unitOfWork.SaveAsync();
                return new WalletDomainResponse(walletDomain, "Wallet Domain created successfully");
            }
            catch
            {
                _logger.LogError("Wallet Domain AddAsync failed");
                return new WalletDomainResponse("An error occurred while creating the Profile." +
                    " Please contact the admin.");
            }
        }

        public async Task<WalletDomain> GetWalletDomainAsync(int id)
        {
            _logger.LogDebug("--->GetScopeAsync");

            var walletDomain = await _unitOfWork.WalletDomain.GetWalletDomainByIdWithPurposes(id);
            if (null == walletDomain)
            {
                _logger.LogError("Profile GetByIdAsync() Failed");
                return null;
            }

            return walletDomain;
        }

        public async Task<WalletDomainResponse> UpdateWalletDomainAsync(WalletDomain walletDomain)
        {
            _logger.LogDebug("--->Update Wallet Domain Async");

            // Check whether the scope exists or not
            var walletDomainInDb = _unitOfWork.WalletDomain.GetById(walletDomain.Id);
            if (null == walletDomainInDb)
            {
                _logger.LogError("Wallet Domain not found");
                return new WalletDomainResponse("Wallet Domain not found");
            }

            if (walletDomainInDb.Status == "DELETED")
            {
                _logger.LogError("Wallet Domain is already deleted");
                return new WalletDomainResponse("Wallet Domain is already deleted");
            }

            // Check wheter the scope already exists other than the given scope
            var allScopes = await _unitOfWork.WalletDomain.GetAllAsync();
            foreach (var item in allScopes)
            {
                if (item.Id != walletDomain.Id)
                {
                    if (item.Name == walletDomain.Name)
                    {
                        _logger.LogError("Wallet Domain already exists with given Name");
                        return new WalletDomainResponse("Wallet Domain already exists with given Name");
                    }
                }
            }



            walletDomainInDb.Name = walletDomain.Name;
            walletDomainInDb.DisplayName = walletDomain.DisplayName;
            walletDomainInDb.Description = walletDomain.Description;
            walletDomainInDb.UpdatedBy = walletDomain.UpdatedBy;
            walletDomainInDb.ModifiedDate = DateTime.Now;
            walletDomainInDb.Purposes = walletDomain.Purposes;
            //scopeInDb.IsClaimsPresent = scope.IsClaimsPresent;

            try
            {
                _unitOfWork.WalletDomain.Update(walletDomainInDb);
                await _unitOfWork.SaveAsync();
                return new WalletDomainResponse(walletDomainInDb, "Wallet Domain updated successfully");
            }
            catch
            {
                _logger.LogError("Wallet Domain Update failed");
                return new WalletDomainResponse("An error occurred while updating the Wallet Domain." +
                    " Please contact the admin.");
            }
        }

        public async Task<IEnumerable<WalletDomain>> ListWalletDomainAsync()
        {
            return await _unitOfWork.WalletDomain.ListAllScopeAsync();
        }

        public async Task<WalletDomainResponse> DeleteWalletDomainAsync(int id, string updatedBy)
        {
            var walletDomainInDb = await _unitOfWork.WalletDomain.GetByIdAsync(id);
            if (null == walletDomainInDb)
            {
                return new WalletDomainResponse("Wallet Domain not found");
            }

            try
            {
                _unitOfWork.WalletDomain.Remove(walletDomainInDb);
                await _unitOfWork.SaveAsync();

                return new WalletDomainResponse(walletDomainInDb, "Wallet Domain deleted successfully");
            }
            catch (Exception error)
            {
                _logger.LogError("DeleteScopeAsync failed : {0}", error.Message);
                return new WalletDomainResponse(
                    "An error occurred,Please contact the admin.");
            }
        }

        public async Task<ServiceResult> GetWalletDomainsList()
        {
            var walletDomainsList = await _unitOfWork.WalletDomain.GetAllAsync();
            var walletDomainList = new List<WalletDomainDTO>();
            foreach (var walletDomain in walletDomainsList)
            {
                string[] arr = walletDomain.Purposes.Split(" ");
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var item in arr)
                {
                    var purpose = await _unitOfWork.WalletPurpose.GetPurposeById(int.Parse(item));
                    dict[purpose.Id.ToString()] = purpose.DisplayName;
                }
                WalletDomainDTO walletDomainDTO = new WalletDomainDTO()
                {
                    displayName = walletDomain.DisplayName,
                    id = walletDomain.Id.ToString(),
                    purposes = dict
                };
                walletDomainList.Add(walletDomainDTO);
            }
            return new ServiceResult(true, "Success", walletDomainList);
        }

    }
}
