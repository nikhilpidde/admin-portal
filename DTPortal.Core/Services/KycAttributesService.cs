using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class KycAttributesService : IKycAttributesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<KycProfilesService> _logger;
        public KycAttributesService(IUnitOfWork unitOfWork,
            ILogger<KycProfilesService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<IEnumerable<KycAttribute>> ListKycAttributesAsync()
        {
            return await _unitOfWork.KycAttributes.ListAllKycAttributesAsync();
        }
        
    }
}
