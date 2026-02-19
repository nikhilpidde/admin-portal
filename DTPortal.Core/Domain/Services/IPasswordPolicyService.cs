using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Core.Domain.Services
{
    public interface IPasswordPolicyService
    {
        Task<PasswordPolicy> GetPasswordPolicyCriteria(int id);
        Task<PasswordPolicyResponse> UpdatePasswordPolicyCriteria(PasswordPolicy passwordPolicy);
    }
}
