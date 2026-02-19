using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Persistence.Repositories
{
    public class CategoryRepository : GenericRepository<Category, idp_dtplatformContext>,
        ICategoryRepository
    {
        private readonly ILogger _logger;
        public CategoryRepository(idp_dtplatformContext context, ILogger logger) :
            base(context, logger)
        {
            _logger = logger;
        }
        public async Task<IEnumerable<Category>> ListAllCategoryAsync()
        {
            try
            {
                return await Context.Categories
                    .Where(u => u.Status != "DELETED").ToListAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("ListAllCategoryAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int catId)
        {
            try
            {
                return await Context.Categories.FirstOrDefaultAsync(u => u.Status != "DELETED" && u.Id == catId);
            }
            catch (Exception error)
            {
                _logger.LogError("ListAllCategoryAsync::Database exception: {0}", error);
                return null;
            }
        }

        public async Task<bool> IsCategoryExistsWithNameAsync(string name)
        {
            try
            {
                return await Context.Categories.AsNoTracking().AnyAsync(u => u.Name == name);
            }
            catch (Exception error)
            {
                _logger.LogError("IsScopeExistsWithNameAsync::Database exception: {0}", error);
                return false;
            }
        }

        public async Task<string> GetCatNameByCatUIDAsync(string catId)
        {
            try
            {
                return await Context.Categories
                            .Where(d => d.CategoryUid == catId)
                            .Select(d => d.Name)
                            .FirstOrDefaultAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("GetCatNameByCatUID::Database exception: {0}", error);
                return null;
            }
        }
    }
}
