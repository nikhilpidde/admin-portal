using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IDataPivotRepository : IGenericRepository<DataPivot>
    {
        Task<IEnumerable<DataPivot>> GetAllPivotDataAsync();

         Task<bool> IsPivotExistsAsync(DataPivot dataPivot);

         Task<bool> IsUpdatePivotExistsAsync(DataPivot dataPivot);

        Task<IEnumerable<DataPivot>> GetPivotByIdAsync(string orgid);

        Task<DataPivot> GetByNameAsync(string name);
        Task<DataPivot> GetByUIDAsync(string UID);
        Task<IEnumerable<DataPivot>> GetAllPivotDataByorgIdAsync(string orgId);
        Task<IEnumerable<DataPivot>> GetDataPivotByCatIdAsync(string catId);

    }
}
