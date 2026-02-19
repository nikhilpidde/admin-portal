using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services
{
    public interface IUserClaimService
    {
        public Task<UserClaimResponse> CreateUserClaimAsync(UserClaim userClaim,
            bool makerCheckerFlag = false);

        public Task<UserClaim> GetUserClaimAsync(int id);

        public Task<UserClaimResponse> UpdateUserClaimAsync(UserClaim userClaim,
            bool makerCheckerFlag = false);

        public Task<IEnumerable<UserClaim>> ListUserClaimAsync();

        public Task<UserClaimResponse> DeleteUserClaimAsync(int id, string updatedBy,
        bool makerCheckerFlag = false);

        public Task<Dictionary<string, string>> GetAttributes();

        public Task<Dictionary<string, string>> GetAttributeNameDisplayNameAsync();
        public Task<Dictionary<string, bool>> GetAttributeNameMandatoryAsync();
    }
}
