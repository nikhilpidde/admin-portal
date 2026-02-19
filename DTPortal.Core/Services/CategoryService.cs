using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Exceptions;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DTPortal.Core.Services
{
    public class CategoryService:ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(IUnitOfWork unitOfWork,
            ILogger<CategoryService> logger
            )
        { 
            _unitOfWork= unitOfWork;
            _logger= logger;

        }
        public async Task<CategoryResponse> CreateCategoryAsync(Category category)
        {
            _logger.LogDebug("--->CreateCategoryAsync");

            var isExists = await _unitOfWork.Category.IsCategoryExistsWithNameAsync(
                category.Name);
            if (true == isExists)
            {
                _logger.LogError("Category already exists with given name");
                return new CategoryResponse("Category already exists with given Name");
            }
            category.Status = StatusConstants.ACTIVE;
            try
            {
                await _unitOfWork.Category.AddAsync(category);
                await _unitOfWork.SaveAsync();
                return new CategoryResponse(category, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Category AddAsync failed" + ex.Message);
                return new CategoryResponse("An error occurred while creating the Category." +
                    " Please contact the admin.");
            }
        }
        public async Task<IEnumerable<Category>> ListCategoryAsync()
        {
            return await _unitOfWork.Category.ListAllCategoryAsync();
        }
        public async Task<ServiceResult> GetCategoryListAsync()
        {
            var categorylist = await _unitOfWork.Category.ListAllCategoryAsync();
            if(categorylist == null)
            {
                return new ServiceResult(false, "Failed to get category List");
            }
            return new ServiceResult(true, "Successfully retrieved category list", categorylist);
        }

        public async Task<ServiceResult> GetCategoryNameAndIdListAsync()
        {
            var categoryList = await _unitOfWork.Category.ListAllCategoryAsync();

            if (categoryList == null)
            {
                return new ServiceResult(false, "Failed to get category List");
            }
            List<string> Categories = new List<string>();

            foreach (var category in categoryList)
            {
                Categories.Add(category.Name+","+category.CategoryUid);
            }

            return new ServiceResult(true, "Successfully retrieved category list", Categories);
        }

        public async Task<ServiceResult> GetCategorybyIdAsync(int catId)
        {
            var categorylist = await _unitOfWork.Category.GetCategoryByIdAsync(catId);
            if (categorylist == null)
            {
                return new ServiceResult(false, $"No category present with {catId} ID");
            }
            return new ServiceResult(true, "Successfully retrieved category details", categorylist);
        }

        public async Task<ServiceResult> DeleteCategorybyIdAsync(int catId)
        {
            try
            {
                var category = await _unitOfWork.Category.GetCategoryByIdAsync(catId);
                if (category == null)
                {
                    return new ServiceResult(false, $"No category present with {catId} ID");
                }
                _unitOfWork.Category.Remove(category);
                await _unitOfWork.SaveAsync();
                return new ServiceResult(true, "Category Deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete Category : "+ex);
                return new ServiceResult(false, "Failed to delete Category");
            }
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(Category category)
        {
            var categoryinDb = await _unitOfWork.Category.GetByIdAsync(category.Id);
            categoryinDb.Name = category.Name;
            categoryinDb.Description = category.Description;
            categoryinDb.ModifiedDate = DateTime.Now;
            categoryinDb.ModifiedBy = category.ModifiedBy;
            var allCategories = await _unitOfWork.Category.GetAllAsync();
            foreach (var item in allCategories)
            {
                if (item.Id != category.Id)
                {
                    if (item.Name == category.Name)
                    {
                        _logger.LogError("Category already exists with given Name");
                        return new CategoryResponse("Category already exists with given Name");
                    }
                }
            }
            try
            {
                _unitOfWork.Category.Update(category);
                await _unitOfWork.SaveAsync();
                return new CategoryResponse(category, "Category Updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Category AddAsync failed" + ex.Message);
                return new CategoryResponse("An error occurred while creating the scope." +
                    " Please contact the admin.");
            }
        }

        public async Task<string> GetCategoryNamebyUIdAsync(string catId)
        {
            var categoryName = await _unitOfWork.Category.GetCatNameByCatUIDAsync(catId);
            if (categoryName == null)
            {
                return null;
            }
            return categoryName;
        }

        public async Task<CategoryResponse> DeleteCategoryAsync(int id, string UUID)
        {
            var dataPivotList = await _unitOfWork.Datapivots.GetAllAsync();
            var categoryinDb = await _unitOfWork.Category.GetByIdAsync(id);
            foreach (var item in dataPivotList)
            {
                if (item.CategoryId == categoryinDb.CategoryUid)
                {
                    _logger.LogError("Category depends on another data pivot");
                    return new CategoryResponse("Category linked to Datapivot");
                }
            }
            categoryinDb.ModifiedDate = DateTime.Now;
            categoryinDb.ModifiedBy = UUID;
            categoryinDb.Status = "DELETED";
            try
            {
                _unitOfWork.Category.Update(categoryinDb);
                await _unitOfWork.SaveAsync();
                return new CategoryResponse(categoryinDb, "Category Deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Category AddAsync failed" + ex.Message);
                return new CategoryResponse("An error occurred while creating the Category." +
                    " Please contact the admin.");
            }
        }

        public async Task<Dictionary<string,string>> GetCategoryNameAndIdPairAsync()
        {
            var categoryList = await _unitOfWork.Category.ListAllCategoryAsync();

            if (categoryList == null)
            {
                return null;
            }
            Dictionary<string,string> Categories = new Dictionary<string, string>();

            foreach (var category in categoryList)
            {
                Categories[category.CategoryUid] = category.Name;
            }
            return Categories;
        }
        public async Task<Category> GetCategoryAsync(int id)
        {
            _logger.LogDebug("--->GetCategoryAsync");

            var Category = await _unitOfWork.Category.GetByIdAsync(id);
            if (null == Category)
            {
                _logger.LogError("Category GetByIdAsync() Failed");
                return null;
            }

            return Category;
        }
    }
}
