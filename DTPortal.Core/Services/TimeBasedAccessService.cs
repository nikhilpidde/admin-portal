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
    public class TimeBasedAccessService : ITimeBasedAccessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<TimeBasedAccessService> _logger;

        public TimeBasedAccessService(ILogger<TimeBasedAccessService> logger,
            IUnitOfWork unitOfWork, ILogClient logClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _LogClient = logClient;
        }

        public async Task<TimeBasedAccessResponse> CreateTimeBasedAccessAsync
            (TimeBasedAccess timeBasedAccessService)
        {
            timeBasedAccessService.Status = "ACTIVE";
            try
            {
                await _unitOfWork.TimeBasedAccess.AddAsync(timeBasedAccessService);
                await _unitOfWork.SaveAsync();
                return new TimeBasedAccessResponse(timeBasedAccessService);
            }
            catch
            {
                return new TimeBasedAccessResponse("An error occurred while creating the timebased access" +
                    "entry, please contact admin");
            }
        }

        public async Task<TimeBasedAccess> GetTimeBasedAccessAsync(int id)
        {
            return await _unitOfWork.TimeBasedAccess.GetByIdAsync(id);
        }
        public async Task<TimeBasedAccessResponse> UpdateTimeBasedAccessAsync
            (TimeBasedAccess timeBasedAccessService)
        {
            try
            {
                _unitOfWork.TimeBasedAccess.Update(timeBasedAccessService);
                await _unitOfWork.SaveAsync();
                return new TimeBasedAccessResponse(timeBasedAccessService);
            }
            catch
            {
                return new TimeBasedAccessResponse("An error occurred while updating the timebased access" +
                    "entry, please contact admin");
            }
        }

        public async Task<TimeBasedAccessResponse> DeleteTimeBasedAccessAsync
            (int id)
        {
            var timeBasedAccessInDb = await _unitOfWork.TimeBasedAccess.GetByIdAsync(id);
            if(null == timeBasedAccessInDb)
            {
                return new TimeBasedAccessResponse("An error occurred while deleting the timebased access" +
                    "entry, please contact admin");
            }

            timeBasedAccessInDb.Status = "DELETED";

            try
            {
                _unitOfWork.TimeBasedAccess.Update(timeBasedAccessInDb);
                await _unitOfWork.SaveAsync();
                return new TimeBasedAccessResponse(timeBasedAccessInDb);
            }
            catch
            {
                return new TimeBasedAccessResponse("An error occurred while updating the timebased access" +
                    "entry, please contact admin");
            }
        }
        public async Task<IEnumerable<TimeBasedAccess>> ListTimeBasedAccessAsync()
        {
            return await _unitOfWork.TimeBasedAccess.GetAllAsync();
        }

    }
}
