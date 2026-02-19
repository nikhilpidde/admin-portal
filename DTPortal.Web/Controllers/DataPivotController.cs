using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using DTPortal.Web.ViewModel;
using DTPortal.Core.Utilities;
using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Web.ViewModel.DataPivot;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using StackExchange.Redis;
using System;
using DTPortal.Core.Services;
using DTPortal.Web.ViewModel.Scopes;
using DTPortal.Core.DTOs;
using DTPortal.Core.Domain.Services.Communication;

namespace DTPortal.Web.Controllers
{
    //[Authorize(Roles = "Data Pivots")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class DataPivotController : BaseController
    {
        private readonly IDataPivotService _dataPivotService;
        private readonly IOrganizationService _organizationService;
        private readonly IConfigurationService _configurationService;
        private readonly ISessionService _sessionService;
        private readonly ICategoryService _categoryService;

        private readonly IScopeService _scopeService;
        public DataPivotController(ILogClient logClient,
            IOrganizationService organizationService,
            IDataPivotService dataPivotService,
            ISessionService sessionService,
            IConfigurationService configurationService,
            ICategoryService categoryService,
            IScopeService scopeService) : base(logClient)
        {
            _organizationService = organizationService;
            _dataPivotService = dataPivotService;
            _configurationService = configurationService;
            _sessionService = sessionService;
            _categoryService = categoryService;
            _scopeService = scopeService;
        }

        string getCertificate(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }

            return result.ToString().Replace("\r", "");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<DataPivotViewModel>();
            var pivotList = await _dataPivotService.GetAllPivotDataAsync();
            if (pivotList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Get all Data Pivots List", LogMessageType.FAILURE.ToString(), "Fail to get Data Pivots list");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Get all Data Pivots List", LogMessageType.SUCCESS.ToString(), "Get Data Pivots list success");

                foreach (var item in pivotList)
                {
                    var serviceConfigurationDeserialize = JsonConvert.DeserializeObject<ServiceConfiguration>(item.ServiceConfiguration);
                    viewModel.Add(new DataPivotViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DisplayName = item.Description,
                        OrgnizationId = item.OrgnizationId,
                        AttributeConfiguration = item.AttributeConfiguration,
                        Serviceurl = serviceConfigurationDeserialize.Serviceurl,
                        AuthScheme = item.AuthScheme

                    });
                }
            }
            return View(viewModel);
        }
        async Task<List<SelectListItem>> GetScopesList()
        {
            
            var result = await _scopeService.ListScopeAsync();
            var list = new List<SelectListItem>();
            if (result == null)
            {
                return list;
            }
            else
            {
                foreach (var org in result)
                {
                   
                    list.Add(new SelectListItem { Text = org.Name, Value =org.Id.ToString() });
                }

                return list;
            }
           
        }

        async Task<List<SelectListItem>> GetOrganizationList()
        {
            var result = await _organizationService.GetOrganizationNamesAndIdAysnc();
            var list = new List<SelectListItem>();
            if (result == null)
            {
                return list;
            }
            else
            {
                foreach (var org in result)
                {
                    var orgobj = org.Split(",");
                    list.Add(new SelectListItem { Text = orgobj[0], Value = orgobj[1] });
                }

                return list;
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateData(DataPivotViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var orgList = await GetOrganizationList();
                viewModel.OrganizatioList = orgList;

                return View("Create", viewModel);
            }
            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {
                var orgList = await GetOrganizationList();

                viewModel.OrganizatioList = orgList;

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("Create", viewModel);
            }
            ServiceConfiguration configuration = new ServiceConfiguration
            {
                Serviceurl = viewModel.Serviceurl,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret

            };
            var serviceConfiguration = JsonConvert.SerializeObject(configuration).ToString();
           
            DataPivot dataPivot = new DataPivot
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Description = viewModel.DisplayName,
                OrgnizationId = viewModel.OrgnizationId,
                AttributeConfiguration = viewModel.AttributeConfiguration,
                PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) : ""),
                ServiceConfiguration = serviceConfiguration,
                CreatedBy = UUID,
                UpdatedBy = UUID,
                ScopeId = int.Parse(viewModel.Scopes),
                DataPivotUid = Guid.NewGuid().ToString(),
                AuthScheme = viewModel.AuthScheme,
                //DataPivotLogo = viewModel.ResizedDataPivotLogo,
                DataPivotLogo = viewModel.DataPivotImage,
                CategoryId = viewModel.Category,
                AllowedSubscriberTypes = viewModel.AllowedSubscriberTypes
            };
            var response = await _dataPivotService.CreatePivotDataAsync(dataPivot);
            if (!response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "create Data Pivot details", LogMessageType.FAILURE.ToString(), "Failed to create Data Pivot");

                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "create Data Pivot details", LogMessageType.SUCCESS.ToString(), "Successfully created Data Pivot");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");

            }

        }
        //[HttpGet]
        //public async Task<IActionResult> GetDetailsByName(string name)
        //{
        //    var getDataPivotByName = await _dataPivotService.GetPivotByNameAsync(name);
        //    if (getDataPivotByName == null)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "View DataPivot details", LogMessageType.FAILURE.ToString(), "Fail to get  details");
        //        return NotFound();
        //    }
        //    var serviceConfigurationDeserialize = JsonConvert.DeserializeObject<ServiceConfiguration>(getDataPivotByName.ServiceConfiguration);
        //    var orgList = await GetOrganizationList();

        //    var viewModel = new DataPivotViewModel
        //    {
        //        Id = getDataPivotByName.Id,
        //        Name = getDataPivotByName.Name,
        //        DisplayName = getDataPivotByName.DisplayName,
        //        OrgnizationId = getDataPivotByName.OrgnizationId,
        //        AttributeConfiguration = getDataPivotByName.AttributeConfiguration,
        //        Serviceurl = serviceConfigurationDeserialize.Serviceurl,
        //        ClientId = serviceConfigurationDeserialize.ClientId,
        //        ClientSecret = serviceConfigurationDeserialize.ClientSecret,
        //        OrganizatioList = orgList,
        //        IsFileUploaded = !String.IsNullOrEmpty(getDataPivotByName.PublicKeyCert)
        //    };
        //    return View(viewModel);
        //}

        [HttpPost]
        public async Task<IActionResult> Update(DataPivotViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var orgList = await GetOrganizationList();
                viewModel.OrganizatioList = orgList;

                return View("Edit", viewModel);
            }
            if (viewModel.Cert != null && viewModel.Cert.ContentType != "application/x-x509-ca-cert")
            {

                var orgList = await GetOrganizationList();

                viewModel.OrganizatioList = orgList;

                ModelState.AddModelError("Cert", "invalid signing certificate");
                return View("Edit", viewModel);
            }
            var updateDataPivot = await _dataPivotService.GetPivotAsync(viewModel.Id);
            if (updateDataPivot == null)
            {
                var orgList = await GetOrganizationList();
                viewModel.OrganizatioList = orgList;

                return View("Edit", viewModel);
            }


            ServiceConfiguration configuration = new ServiceConfiguration
            {
                Serviceurl = viewModel.Serviceurl,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret

            };
            var serviceConfiguration = JsonConvert.SerializeObject(configuration).ToString();
            updateDataPivot.Id = viewModel.Id;
            updateDataPivot.Name = viewModel.Name;
            updateDataPivot.Description = viewModel.DisplayName;
            updateDataPivot.OrgnizationId = viewModel.OrgnizationId;
            updateDataPivot.AttributeConfiguration = viewModel.AttributeConfiguration;
            updateDataPivot.UpdatedBy = UUID;
            updateDataPivot.ServiceConfiguration = serviceConfiguration;
            updateDataPivot.AuthScheme = viewModel.AuthScheme;
            updateDataPivot.ScopeId = int.Parse(viewModel.Scopes);
            updateDataPivot.PublicKeyCert = (viewModel.Cert != null ? getCertificate(viewModel.Cert) : updateDataPivot.PublicKeyCert);
            updateDataPivot.CategoryId = viewModel.Category;
            //updateDataPivot.DataPivotLogo = viewModel.ResizedDataPivotLogo;
            updateDataPivot.DataPivotLogo = viewModel.DataPivotImage;
            updateDataPivot.AllowedSubscriberTypes = viewModel.AllowedSubscriberTypes;
            var response = await _dataPivotService.UpdatePivotDataAsync(updateDataPivot);
            if (!response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Update Data Pivot details", LogMessageType.FAILURE.ToString(), "Failed to Update Data Pivot");

                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Update Data Pivot details", LogMessageType.SUCCESS.ToString(), "Successfully Updated Data Pivot");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _dataPivotService.DeleteDatapivotAsync(id, UUID);
                if (response != null && response.Success)
                {

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Delete Data Pivot details", LogMessageType.SUCCESS.ToString(), "Delete  Data Pivot successfully");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.DataPivots, "Delete Data Pivot details", LogMessageType.FAILURE.ToString(), "Failed to Delete  Data Pivot");
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return new JsonResult(false);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

    }
}
