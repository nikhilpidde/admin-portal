using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IDataPivotService
    {
         Task<IEnumerable<DataPivot>> GetPivotUserAsync(string orgid);

         Task<DataPivotResponse> CreatePivotDataAsync(DataPivot dataPivot);

         Task<DataPivotResponse> UpdatePivotDataAsync(DataPivot dataPivot);

         Task<IEnumerable<DataPivot>> GetAllPivotDataAsync();

        Task<DataPivot> GetPivotAsync(int id);

        Task<DataPivot> GetPivotByNameAsync(string name);

        Task<DataPivotResponse> DeleteDatapivotAsync(int id, string UUID);
        Task<ServiceResult> GetDocumentsListAsync(string AccessToken);
        Task<DataPivot> GetPivotByUidAsync(string Uid);
        Task<IEnumerable<DataPivot>> GetAllPivotDataByOrgIdAsync(string orgId);
        Task<ServiceResult> GetDataPivotByCatIdAsync(string id, string AccessToken);
        Task<ServiceResult> GetUserSpecificList(string suid, string AccessToken,string CategoryId);
        Task<ServiceResult> GetDataPivotById(string Id, string AccessToken, string suid);
    }
}
