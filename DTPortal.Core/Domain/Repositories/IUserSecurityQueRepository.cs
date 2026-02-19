using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IUserSecurityQueRepository:IGenericRepository<UserSecurityQue>
    {
        Task<IEnumerable<UserSecurityQue>> GetAllUserSecQueAnsAsync(int userId);
    }
}
