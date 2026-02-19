using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class OrganizationKycMethodsService:IOrganizationKycMethodsService
    {
        private readonly ILogger<OrganizationKycMethodsService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public OrganizationKycMethodsService(
            ILogger<OrganizationKycMethodsService> logger,
            IUnitOfWork unitOfWork
            ) 
        { 
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResult> GetOrganizationKycMethodsByOrgIdAsync
            (string organizationId)
        {
            try
            {
                var kycDetails = await _unitOfWork.OrganizationKycMethods.
                    GetOrganizationKycMethodsByOrgIdAsync(organizationId);
                List<string> selectedMethods;
                if (kycDetails == null || kycDetails.KycMethods == null)
                {
                    selectedMethods = new List<string>();
                }
                else
                {
                    string methods=(string)kycDetails.KycMethods;
                    selectedMethods=JsonConvert.DeserializeObject<List<string>>(methods);
                }

                return new ServiceResult(true, "Get Organization KYC Methods successfully", selectedMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Organization KYC Methods for organization {OrganizationId}",
                    organizationId);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetOrganizationKycProfilesByOrgIdAsync
            (string organizationId)
        {
            try
            {
                var kycDetails = await _unitOfWork.OrganizationKycMethods.
                    GetOrganizationKycMethodsByOrgIdAsync(organizationId);

                List<string> selectedProfiles;
                if (kycDetails == null || kycDetails.KycProfiles == null)
                {
                    selectedProfiles = new List<string>();
                }
                else
                {
                    string profiles = (string)kycDetails.KycProfiles;
                    selectedProfiles = JsonConvert.DeserializeObject<List<string>>(profiles);
                }
                return new ServiceResult(true, "Get Organization KYC profiles successfully", selectedProfiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Organization KYC Profiles for organization {OrganizationId}",
                    organizationId);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> GetOrganizationKycMethodsByClientIdAsync
            (string clientId)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                {
                    return new ServiceResult(false, "Invalid request parameter");
                }

                var client = await _unitOfWork.Client.
                    GetClientByClientIdAsync(clientId);
                if(client == null)
                {
                    return new ServiceResult(false, "Client not found");
                }
                if (string.IsNullOrEmpty(client.OrganizationUid))
                {
                    return new ServiceResult(false, "Organization ID is not associated with the client");
                }
                return await GetOrganizationKycMethodsByOrgIdAsync(client.OrganizationUid);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC methods for organization {OrganizationId}",
                    clientId);
                return new ServiceResult(false, ex.Message);
            }
        }

        public async Task<ServiceResult> AddOrganizationKycMethodsAsync(
            OrganizationKycMethod organizationKycMethod)
        {
            try
            {
                await _unitOfWork.OrganizationKycMethods.AddAsync(organizationKycMethod);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Successfully Added KYC methods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Adding KYC methods for organization");
                return new ServiceResult(false, ex.Message);
            }
        }
        public async Task<ServiceResult> UpdateOrganizationKycMethodAsync(
                    OrganizationKycMethod updatedKycMethod)
        {
            try
            {
                var organizationKycMethod = await _unitOfWork.OrganizationKycMethods
                    .GetOrganizationKycMethodsByOrgIdAsync(updatedKycMethod.OrganizationId);

                if (organizationKycMethod == null)
                {
                    return await AddOrganizationKycMethodsAsync(updatedKycMethod);
                }

                organizationKycMethod.KycMethods = updatedKycMethod.KycMethods;
                organizationKycMethod.KycProfiles = updatedKycMethod.KycProfiles;
                _unitOfWork.OrganizationKycMethods.Update(organizationKycMethod);
                await _unitOfWork.SaveAsync();

                return new ServiceResult(true, "Successfully updated KYC method.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating KYC method for organization");
                return new ServiceResult(false, ex.Message);
            }
        }
        public async Task<ServiceResult> GetOrganizationProfile(string organizationId)
        {
            try
            {
                var kycMethods = await _unitOfWork.OrganizationKycMethods.
                    GetOrganizationKycMethodsByOrgIdAsync(organizationId);
                List<string> selectedProfiles;
                if (kycMethods == null)
                {
                    return new ServiceResult(false, "No KYC methods found for the organization");
                }
                string profiles = (string)kycMethods.KycProfiles;
                selectedProfiles = JsonConvert.DeserializeObject<List<string>>(profiles);
                return new ServiceResult(true, "Get KYC Profiles successfully", selectedProfiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC methods for organization {OrganizationId}",
                    organizationId);
                return new ServiceResult(false, ex.Message);
            }
        }
    }
}
