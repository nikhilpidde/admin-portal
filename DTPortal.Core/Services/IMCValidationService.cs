using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public interface IMCValidationService
    {
        Task<bool> IsMCEnabled(int activityId);
        Task<BooleanResponse> IsCheckerApprovalRequired(
               int activityID, string operationType, string maker, string requestData);
    }
}
