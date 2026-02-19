using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IMakerCheckerRepository : IGenericRepository<MakerChecker>
    {
        Task<IEnumerable<MakerChecker>> GetAllRequestsByMakerId(int id);
        Task<IEnumerable<MakerChecker>> GetAllRequestsByCheckerRoleId(int id);
    }
}
