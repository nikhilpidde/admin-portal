using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class WalletConsentService:IWalletConsentService
    {
        private readonly ILogger<WalletConsentService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public WalletConsentService(ILogger<WalletConsentService> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResult> GetAllConsentAsync()
        {
            var list = await _unitOfWork.WalletConsent.GetAllConsentsAsync();
            if(list == null)
            {
                return new ServiceResult(false, "Failed to get Consents List");
            }
            return new ServiceResult(true, "Get Consents List Success", list);
        }

        public async Task<ServiceResult> GetActiveConsentAsync()
        {
            var list = await _unitOfWork.WalletConsent.GetActiveConsentsAsync();
            if (list == null)
            {
                return new ServiceResult(false, "Failed to get Consents List");
            }
            return new ServiceResult(true, "Get Consents List Success", list);
        }

        public async Task<ServiceResult> GetConsentsByUserIdAsync(string Id)
        {
            var list = await _unitOfWork.WalletConsent.GetConsentsByUserIdAsync(Id);
            if (list == null)
            {
                return new ServiceResult(false, "Failed to get Consents List");
            }
            return new ServiceResult(true, "Get Consents List Success", list);
        }

        public async Task<ServiceResult> GetActiveConsentsByUserIdAsync(string Id)
        {
            var list = await _unitOfWork.WalletConsent.GetActiveConsentsByUserIdAsync(Id);
            if (list == null)
            {
                return new ServiceResult(false, "Failed to Get Consent");
            }
            return new ServiceResult(true, "Get Consents List Success", list);
        }

        public async Task<ServiceResult> AddConsent(WalletConsentDTO walletConsentDTO)
        {
            WalletConsent walletConsent = new WalletConsent()
            {
                ApplicationId = walletConsentDTO.applicationId,
                Suid = walletConsentDTO.suid,
                CredentialId = walletConsentDTO.credentialId,
                ConsentData = walletConsentDTO.consentData,
                Status = "ACTIVE",
                CreatedDate=DateTime.Now
            };
            try
            {
                await _unitOfWork.WalletConsent.AddAsync(walletConsent);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Add Wallet Consent Success", walletConsent);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, ex.Message);
            }
        }
        public async Task<ServiceResult> RevokeConsent(int id)
        {
            var consent=await _unitOfWork.WalletConsent.GetConsentByIdAsync(id);
            if(consent == null)
            {
                return new ServiceResult(false, "Consent Not Found");
            }
            consent.Status = "REVOKED";
            consent.UpdatedDate = DateTime.Now;
            try
            {
                _unitOfWork.WalletConsent.Update(consent);
                _unitOfWork.Save();
                return new ServiceResult(true, "Successfully Revoked Consent Status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, ex.Message);
            }

        }
    }
}
