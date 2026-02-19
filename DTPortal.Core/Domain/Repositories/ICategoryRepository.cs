using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface ICategoryRepository:IGenericRepository<Category>
    {
        public Task<IEnumerable<Category>> ListAllCategoryAsync();
        public Task<bool> IsCategoryExistsWithNameAsync(string name);
        public Task<Category> GetCategoryByIdAsync(int catId);
        public Task<string> GetCatNameByCatUIDAsync(string name);

    }
}
