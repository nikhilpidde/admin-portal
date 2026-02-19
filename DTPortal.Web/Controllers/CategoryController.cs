using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.category;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ILogClient logClient, 
            ICategoryService categoryService) : base(logClient)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var ViewModel = new List<CategoryViewModel>();

            var categoryList = await _categoryService.ListCategoryAsync();

            if (categoryList == null)
            {
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Get all Category List", LogMessageType.FAILURE.ToString(), "Fail to get Category list");
                return NotFound();
            }

            foreach (var category in categoryList)
            {
                var CategoryViewModel = new CategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedDate = category.CreatedDate,
                    Description = category.Description,
                    Status = category.Status,

                };
                ViewModel.Add(CategoryViewModel);
            }
            SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Get all Category List", LogMessageType.SUCCESS.ToString(), "Get Category list success");
            return View(ViewModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var ViewModel = new CategoryAddViewModel();
            return View(ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(CategoryAddViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Add", ViewModel);
            }
            var category = new Category()
            {
                Name = ViewModel.Name,
                Description = ViewModel.Description,
                CreatedBy = UUID,
                ModifiedBy = UUID,
                CategoryUid = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,

            };
            var response = await _categoryService.CreateCategoryAsync(category);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Create new Category", LogMessageType.FAILURE.ToString(), "Fail to create Category");
                return View("New", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Create new Category", LogMessageType.SUCCESS.ToString(), "Created New Category with name " + ViewModel.Name + " Successfully");
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoryinDb = await _categoryService.GetCategoryAsync(id);

            if (categoryinDb == null)
            {
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "View Category details", LogMessageType.FAILURE.ToString(), "Fail to get Category details");
                return NotFound();
            }
            var ViewModel = new CatecoryEditViewModel
            {
                Id = categoryinDb.Id,
                Name = categoryinDb.Name,

                Description = categoryinDb.Description,
                CreatedDate = categoryinDb.CreatedDate,
                Status = categoryinDb.Status
            };
            SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "View Category details", LogMessageType.SUCCESS.ToString(), "Get Category details of " + categoryinDb.Name + " successfully ");
            return View(ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CatecoryEditViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", ViewModel);
            }
            var categoryinDb = await _categoryService.GetCategoryAsync(ViewModel.Id);
            if (categoryinDb == null)
            {
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Update Category", LogMessageType.FAILURE.ToString(), "Fail to get Category details");
                return View("Edit", ViewModel);
            }
            categoryinDb.Id = ViewModel.Id;
            categoryinDb.Name = ViewModel.Name;
            categoryinDb.Description = ViewModel.Description;
            categoryinDb.ModifiedBy = UUID;
            categoryinDb.ModifiedDate = DateTime.Now;

            var response = await _categoryService.UpdateCategoryAsync(categoryinDb);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "Update Category", LogMessageType.FAILURE.ToString(), "Fail to update Category details of  name " + ViewModel.Name);
                return View("Edit", ViewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.Category, ServiceNameConstants.Category, "UpdateCategory", LogMessageType.SUCCESS.ToString(), "Updated Category details of  name " + ViewModel.Name + " successfully");
                return RedirectToAction("List");
            }
        }

    }
}
