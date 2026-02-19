using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class KycProfilesService : IKycProfilesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<KycProfilesService> _logger;
        public KycProfilesService(IUnitOfWork unitOfWork,
            ILogger<KycProfilesService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;

        }

        public async Task<ServiceResult> ListKycProfilesAsync()
        {
            try
            {
                var kycProfiles = await _unitOfWork.KycProfiles.ListAllKycProfilesAsync();
                return new ServiceResult(true, "ListKycProfilesAsync methods successfully", kycProfiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC profiles");
                return new ServiceResult(false, ex.Message);
            }
        }
        public async Task<KycProfile> GetKycProfileByIdAsync(int id)
        {
            try
            {
                var kycProfile = await _unitOfWork.KycProfiles.GetByIdAsync(id);
                if (kycProfile == null)
                {
                    _logger.LogWarning($"KYC Profile with ID {id} not found.");
                    return null;
                }
                return kycProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving KYC Profile with ID {id}");
                return null;
            }
        }
        public async Task<KycProfile> GetKycProfileByNameAsync(string name)
        {
            try
            {
                var kycProfile = await _unitOfWork.KycProfiles.GetKycProfileByNameAsync(name);
                if (kycProfile == null)
                {
                    _logger.LogWarning($"KYC Profile with Name {name} not found.");
                    return null;
                }
                return kycProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving KYC Profile with Name {name}");
                return null;
            }
        }
        public async Task<ServiceResult> CreateKycProfileAsync(KycProfile kycProfile)
        {
            try
            {
                if (kycProfile == null)
                {
                    return new ServiceResult(false, "KYC Profile cannot be null.");
                }
                await _unitOfWork.KycProfiles.AddAsync(kycProfile);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Created Profile Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating KYC Profile");
                return new ServiceResult(false, "An error occurred while creating the KYC Profile.");
            }
        }
        public async Task<ServiceResult> UpdateKycProfileAsync(KycProfile kycProfile)
        {
            try
            {
                if (kycProfile == null)
                {
                    return new ServiceResult(false, "KYC Profile cannot be null.");
                }
                var existingProfile = await _unitOfWork.KycProfiles.GetByIdAsync(kycProfile.Id);
                if (existingProfile == null)
                {
                    return new ServiceResult(false, "KYC Profile not found.");
                }
                _unitOfWork.KycProfiles.Update(kycProfile);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Updated Profile Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating KYC Profile");
                return new ServiceResult(false, "An error occurred while updating the KYC Profile.");
            }
        }
    }
}
