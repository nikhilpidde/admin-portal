using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IMakerCheckerService
    {
        Task<IEnumerable<Activity>> GetAllActivities();
        Task<MakerCheckerResponse> UpdateMakerCheckerActivityConfiguration(IEnumerable<Activity> activities);
        Task<bool> IsMCEnabled(int activityId);
        Task<BooleanResponse> IsCheckerApprovalRequired(
               int activityID, string operationType, string maker, string requestData);
        Task<IEnumerable<MakerChecker>> GetAllRequestsByMakerId(int id);
        Task<IEnumerable<MakerChecker>> GetAllRequestsByCheckerRoleId(int id);
        Task<MakerCheckerResponse> UpdateState(int id, bool isApproved,string token,
            string reason = null);
        Task<MakerCheckerResponse> UpdateStateWithRequetBodyChange(int id, string requetBody, bool isApproved, string reason = null);

        Task<string> GetRequestDataById(int id);
    }

}
