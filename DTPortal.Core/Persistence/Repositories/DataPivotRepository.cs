using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class DataPivotRepository : GenericRepository<DataPivot, idp_dtplatformContext>, IDataPivotRepository

    {
        private readonly ILogger _logger;
        public DataPivotRepository(idp_dtplatformContext context,
            ILogger logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<DataPivot>> GetPivotByIdAsync(string orgid)
        {
            
           
            try
            {

               
                return await Context.DataPivots.AsNoTracking().Where(
                   u => u.OrgnizationId == orgid).ToListAsync();
            }

            catch (Exception error)
            {
                _logger.LogError("GetPivotByIdAsync::Database exception: {0}", error);
                throw;
            }
        }

        public async Task<DataPivot> GetByNameAsync(string name)
        {
            try
            {

                return await Context.DataPivots.AsNoTracking().SingleOrDefaultAsync(u => u.Name == name);
               
            }

            catch (Exception error)
            {
                _logger.LogError("GetByNameAsync::Database exception: {0}", error);
                throw;
            }
        }

        public async Task<bool> IsPivotExistsAsync(DataPivot dataPivot)
        {
            try
            {
                return await Context.DataPivots.AsNoTracking().AnyAsync(u => u.Id == dataPivot.Id);
            }
            catch (Exception error)
            {
                _logger.LogError("IsPivotExistsAsync::Database exception: {0}", error);
                return false;
            }
        }
        public async Task<IEnumerable<DataPivot>> GetAllPivotDataAsync()
        {
            try
            {
                //return await Context.DataPivots.AsNoTracking().ToListAsync();
                return await Context.DataPivots.Where(d=>d.Status!= "DELETED").ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetAllPivotDataAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<bool> IsUpdatePivotExistsAsync(DataPivot dataPivot)
        {
            try
            {
                return await Context.DataPivots.AsNoTracking().AnyAsync(u => u.Id == dataPivot.Id);
            }
            catch (Exception error)
            {
                _logger.LogError("IsUpdatePivotExistsAsync::Database exception: {0}", error);
                throw;
            }
        }
        public async Task<DataPivot> GetByUIDAsync(string UID)
        {
            try
            {

                return await Context.DataPivots.AsNoTracking().SingleOrDefaultAsync(u => u.DataPivotUid == UID);

            }

            catch (Exception error)
            {
                _logger.LogError("GetByNameAsync::Database exception: {0}", error);
                throw;
            }
        }
        public async Task<IEnumerable<DataPivot>> GetAllPivotDataByorgIdAsync(string orgId)
        {
            try
            {
                //return await Context.DataPivots.AsNoTracking().ToListAsync();
                return await Context.DataPivots.Where(d => d.Status != "DELETED" && d.OrgnizationId== orgId).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetAllPivotDataAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<IEnumerable<DataPivot>> GetDataPivotByCatIdAsync(string catId)
        {
            try
            {
                return await Context.DataPivots.Where(d => d.CategoryId == catId).ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetDataPivotByCatIdAsync::Database exception: {0}", error);
                return null;
            }
        }
    }
}
