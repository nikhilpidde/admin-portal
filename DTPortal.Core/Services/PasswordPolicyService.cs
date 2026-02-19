using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Services
{
    public class PasswordPolicyService : IPasswordPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PasswordPolicyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PasswordPolicy> GetPasswordPolicyCriteria(int id)
        {

                return await _unitOfWork.PasswordPolicy.GetByIdAsync(id);
        }

        public async Task<PasswordPolicyResponse> UpdatePasswordPolicyCriteria(PasswordPolicy passwordPolicy)
        {

            var PasswordPolicyInDb = await _unitOfWork.PasswordPolicy.GetByIdAsync(passwordPolicy.Id);
            
            PasswordPolicyInDb.IsReversibleEncryption = passwordPolicy.IsReversibleEncryption;
            PasswordPolicyInDb.MaximumPwdAge = passwordPolicy.MaximumPwdAge;
            PasswordPolicyInDb.MaximumPwdLength = passwordPolicy.MaximumPwdLength;
            PasswordPolicyInDb.MinimumPwdAge = passwordPolicy.MinimumPwdAge;
            PasswordPolicyInDb.MinimumPwdLength = passwordPolicy.MinimumPwdLength;
            PasswordPolicyInDb.PasswordHistory = passwordPolicy.PasswordHistory;
            PasswordPolicyInDb.PwdContains = passwordPolicy.PwdContains;
            PasswordPolicyInDb.BadPwdCount = passwordPolicy.BadPwdCount;
            PasswordPolicyInDb.ModifiedDate = DateTime.Now;
            PasswordPolicyInDb.UpdatedBy = passwordPolicy.UpdatedBy;

            _unitOfWork.PasswordPolicy.Update(PasswordPolicyInDb);

            try
            {
                await _unitOfWork.SaveAsync();
                return new PasswordPolicyResponse(passwordPolicy);
            }
            catch
            {
                return new PasswordPolicyResponse("An error occurred while updating the password policy. Please contact the admin.");
            }

        }
    }
}
