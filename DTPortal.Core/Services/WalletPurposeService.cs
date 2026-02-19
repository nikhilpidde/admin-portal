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
    public class WalletPurposeService : IWalletPurposeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        public WalletPurposeService(IUnitOfWork unitOfWork, ILogClient logClient, ILogger<ClientService> logger)
        {
            _unitOfWork = unitOfWork;
            _LogClient = logClient;
            _logger = logger;
        }
        public async Task<WalletPurposeResponse> CreatePurposeAsync(WalletPurpose purpose)
        {
            var isExist = await _unitOfWork.WalletPurpose.IsPurposeExistsWithNameAsync(purpose.Name);
            if (true == isExist)
            {
                _logger.LogError("Wallet Purpose with given name already exist");
                return null;
            }
            purpose.CreatedDate = DateTime.Now;
            purpose.ModifiedDate = DateTime.Now;
            purpose.Status = "ACTIVE";
            try
            {
                await _unitOfWork.WalletPurpose.AddAsync(purpose);
                await _unitOfWork.SaveAsync();
                return new WalletPurposeResponse(purpose, "Wallet Purpose added successfully");
            }
            catch (Exception)
            {
                _logger.LogError("Failed adding Wallet Purpose");
                return new WalletPurposeResponse("Error occured adding Wallet Purpose");
            }
        }


        public async Task<WalletPurpose> GetPurposeAsync(int id)
        {
            var Purpose = await _unitOfWork.WalletPurpose.GetPurposeById(id);
            if (Purpose == null)
            {
                return null;
            }
            return Purpose;
        }

        public async Task<int> GetPurposeIdByNameAsync(string name)
        {
            var purposeId = await _unitOfWork.Purpose.GetPurposeByNameAsync(name);
            if (purposeId == null)
            {
                return -1;
            }
            return purposeId.Id;
        }


        public async Task<WalletPurposeResponse> UpdatePurposeAsync(WalletPurpose purpose)
        {
            var PurposeInDb = await _unitOfWork.WalletPurpose.GetPurposeById(purpose.Id);
            if (PurposeInDb == null)
            {
                return new WalletPurposeResponse("Wallet Purpose not found");
            }
            if (purpose.Status == "DELETED")
            {
                return new WalletPurposeResponse("Wallet Purpose is deleted");
            }
            var PurposeList = await _unitOfWork.WalletPurpose.GetAllAsync();
            foreach (var item in PurposeList)
            {
                if (item.Id != purpose.Id)
                {
                    if (item.Name == purpose.Name)
                    {
                        _logger.LogError("Purpose Name already exist");
                        return new WalletPurposeResponse("Purpose name already exist");
                    }
                }
            }
            PurposeInDb.Name = purpose.Name;
            PurposeInDb.DisplayName = purpose.DisplayName;
            PurposeInDb.ModifiedDate = DateTime.Now;
            PurposeInDb.UpdatedBy = purpose.UpdatedBy;
            try
            {
                _unitOfWork.WalletPurpose.Update(PurposeInDb);
                await _unitOfWork.SaveAsync();
                return new WalletPurposeResponse(PurposeInDb, "Purpose updated Successfully");
            }
            catch (Exception)
            {
                _logger.LogError("Failed updating the Purpose");
                return new WalletPurposeResponse("Failed Updating the purpose");
            }

        }


        public async Task<IEnumerable<WalletPurpose>> GetPurposeListAsync()
        {
            var PurposeList = await _unitOfWork.WalletPurpose.ListAllPurposeAsync();
            return PurposeList;
        }

        public async Task<IEnumerable<string>> GetPurposesListAsync()
        {
            var list = await _unitOfWork.WalletPurpose.ListAllPurposeAsync();
            var purposelist = new List<string>();
            foreach (var purpose in list)
            {
                purposelist.Add(purpose.Name);
            }
            return purposelist;
        }

        public async Task<WalletPurposeResponse> DeletePurposeAsync(int id, string UUID)
        {
            var PurposeInDb = await _unitOfWork.WalletPurpose.GetByIdAsync(id);
            if (PurposeInDb == null)
            {
                return new WalletPurposeResponse("Wallet Purpose not found");
            }
            try
            {
                PurposeInDb.Status = "DELETED";
                PurposeInDb.ModifiedDate = DateTime.Now;
                PurposeInDb.UpdatedBy = UUID;
                _unitOfWork.WalletPurpose.Update(PurposeInDb);
                await _unitOfWork.SaveAsync();
                return new WalletPurposeResponse(PurposeInDb, "Wallet Purpose Deleted Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete the Wakket Purpose : {0}", ex.Message);
                return new WalletPurposeResponse("Error Occured while deleting the Wallet purpose");
            }
        }
    }
}
