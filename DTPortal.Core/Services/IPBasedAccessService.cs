using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class IPBasedAccessService : IIPBasedAccessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<IPBasedAccessService> _logger;

        public IPBasedAccessService(ILogger<IPBasedAccessService> logger,
            IUnitOfWork unitOfWork, ILogClient logClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _LogClient = logClient;
        }

        public async Task<IPBasedAccessResponse> CreateIPBasedAccessAsync(IpBasedAccess ipBasedAccess)
        {
            try
            {
                await _unitOfWork.IpBasedAccess.AddAsync(ipBasedAccess);
                await _unitOfWork.SaveAsync();
                return new IPBasedAccessResponse(ipBasedAccess);
            }
            catch(Exception error)
            {
                _logger.LogError("CreateIPBasedAccessAsync Failed: {0}",
                    error.Message);
                return new IPBasedAccessResponse("An error occurred while creating the ipbased access" +
                    "entry, please contact admin");
            }
        }

        public async Task<IpBasedAccess> GetIPBasedAccessAsync(int id)
        {
            return await _unitOfWork.IpBasedAccess.GetByIdAsync(id);
        }

        public async Task<IPBasedAccessResponse> UpdateIPBasedAccessAsync(IpBasedAccess ipBasedAccess)
        {
            try
            {
                _unitOfWork.IpBasedAccess.Update(ipBasedAccess);
                await _unitOfWork.SaveAsync();
                return new IPBasedAccessResponse(ipBasedAccess);
            }
            catch
            {
                return new IPBasedAccessResponse("An error occurred while updating the ipbased access" +
                    "entry, please contact admin");
            }
        }

        public async Task<IPBasedAccessResponse> DeleteIPBasedAccessAsync(int id)
        {
            var IPBasedAccessResponseInDb = await _unitOfWork.IpBasedAccess.GetByIdAsync(id);
            if (null == IPBasedAccessResponseInDb)
            {
                return new IPBasedAccessResponse("An error occurred while deleting the ipbased access" +
                    "entry, please contact admin");
            }

            try
            {
                _unitOfWork.IpBasedAccess.Remove(IPBasedAccessResponseInDb);
                await _unitOfWork.SaveAsync();
                return new IPBasedAccessResponse(IPBasedAccessResponseInDb);
            }
            catch
            {
                return new IPBasedAccessResponse("An error occurred while updating the ipbased access" +
                    "entry, please contact admin");
            }
        }

        public async Task<IEnumerable<IpBasedAccess>> ListIPBasedAccessAsync()
        {
            return await _unitOfWork.IpBasedAccess.GetAllAsync();
        }
    }
}
