using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class KycMethodsService:IKycMethodsService
    {
        private readonly ILogger<KycMethodsService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public KycMethodsService(
            ILogger<KycMethodsService> logger,
            IUnitOfWork unitOfWork
            ) 
        { 
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<ServiceResult> GetKycMethodsListAsync()
        {
            try
            {
                var kycMethods = await _unitOfWork.KycMethods.GetKycMethodsListAsync();
                return new ServiceResult(true, "GetKYC methods successfully", kycMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC methods");
                return new ServiceResult(false, ex.Message);
            }
        }
    }
}
