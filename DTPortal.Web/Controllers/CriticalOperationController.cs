using DTPortal.Core.Domain.Services;
using DTPortal.Web.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.CriticalOperation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Critical Operation")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class CriticalOperationController : BaseController
    {
        private readonly IOperationAuthenticationService _operationsAuthSchemeService;

        public CriticalOperationController(ILogClient logClient, IOperationAuthenticationService operationsAuthSchemeService) : base(logClient)
        {
            _operationsAuthSchemeService = operationsAuthSchemeService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var operationList = await _operationsAuthSchemeService.ListAllOperationsAuthschemes();
            if (operationList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "Get all Critical operation", LogMessageType.FAILURE.ToString(), "Fail to get Critical operations list");
                return NotFound();
            }
            var viewModel = new CriticalOperationListViewModel()
            {
               List = operationList 
            };

            SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "Get all Critical operation", LogMessageType.SUCCESS.ToString(), "Get Critical operations list success");
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Update(CriticalOperationEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                //var Authlist = await _authSchemeSevice.ListAuthSchemesAsync();
                //if (Authlist == null)
                //{
                //    return NotFound();
                //}

                //List<SelectListItem> list = new List<SelectListItem>();
                //foreach (var item in Authlist)
                //{
                //    list.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
                //}

                //viewModel.Authlist = list;
                return View("Edit", viewModel);
            }

            var operationDetails = await _operationsAuthSchemeService.GetOperationsAuthschemeById(viewModel.OperationId);
            if (operationDetails == null)
            {
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "Update Critical operation", LogMessageType.FAILURE.ToString(), "Fail to Get Critical operations details of" + viewModel.Name );
                return NotFound();
            }

            operationDetails.Id = viewModel.OperationId;
            operationDetails.DisplayName = viewModel.Name;
            operationDetails.Description = viewModel.Description;
            operationDetails.AuthenticationSchemeName = viewModel.AuthScheme;
            operationDetails.AuthenticationRequired = (viewModel.IsEnable == 1 ? true : false);
            operationDetails.ModifiedBy = UUID;

            var response = await _operationsAuthSchemeService.UpdateOperationsAuthscheme(operationDetails);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "Update Critical operation", LogMessageType.FAILURE.ToString(), "Fail to Update Critical operations details of" + viewModel.Name);
                Alert alert = new Alert { Message = "Operation Update failed" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                //var Authlist = await _authSchemeSevice.ListAuthSchemesAsync();
                //if (Authlist == null)
                //{
                //    return NotFound();
                //}

                //List<SelectListItem> list = new List<SelectListItem>();
                //foreach (var item in Authlist)
                //{
                //    list.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
                //}

                //viewModel.Authlist = list;
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "Update Critical operation", LogMessageType.SUCCESS.ToString(), "Updated Critical operation " + viewModel.Name + " successfully");

                Alert alert = new Alert { IsSuccess = true, Message = "Operation Updated Successfully" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);


                return RedirectToAction("Edit", new { id = response.Result.Id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var OperationDetails = await _operationsAuthSchemeService.GetOperationsAuthschemeById(id);
            if (OperationDetails == null)
            {
                SendAdminLog( ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "View Critical operation", LogMessageType.FAILURE.ToString(), "Fail to get Critical operation details");
                return NotFound();
            }

            //var Authlist = await _authSchemeSevice.ListAuthSchemesAsync();
            //if (Authlist == null)
            //{
            //    return NotFound();
            //}

            //List<SelectListItem> list = new List<SelectListItem>();
            //foreach (var item in Authlist)
            //{
            //    list.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            //}

            var model = new CriticalOperationEditViewModel
            {
                OperationId = OperationDetails.Id,
                Name = OperationDetails.DisplayName,
                AuthScheme = OperationDetails.AuthenticationSchemeName,
                Description = OperationDetails.Description,
                IsEnable = ((bool)OperationDetails.AuthenticationRequired ? 1 : 0)
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.CriticalOperation, "View Critical operation", LogMessageType.SUCCESS.ToString(), "Get Critical operation details of " + model.Name + " successfully");
            return View(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> UpdateStatus(int id, bool isApproved, string reason = null)
        //{
        //    var response = await _operationsAuthSchemeService.UpdateOperationsAuthSchemeState(id, isApproved, reason);
        //    if (!response.Success)
        //    {
        //        return new JsonResult(response);
        //    }
        //    return new JsonResult(response);
        //}

    }
}
