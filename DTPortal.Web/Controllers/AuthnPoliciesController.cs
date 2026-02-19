using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.AuthnPolicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    //[Authorize(Roles = "Authentication Policy")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class AuthnPoliciesController : BaseController
    {
        private readonly IAuthSchemeSevice _authSchemeSevice;
        private readonly IPrimaryAuthSchemeService _primaryAuthSchemeService;

        public AuthnPoliciesController(ILogClient logClient, IAuthSchemeSevice authSchemeService, IPrimaryAuthSchemeService primaryAuthSchemeService) : base(logClient)
        {
            _authSchemeSevice = authSchemeService;
            _primaryAuthSchemeService = primaryAuthSchemeService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var authlist = await _authSchemeSevice.ListAuthSchemesAsync();
            if (authlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get all authentication Policies List", LogMessageType.FAILURE.ToString(), "Fail to get authentication Policies list");
                return NotFound();
            }

            var model = new AuthnPoliciesListViewModel
            {
                authList = authlist
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get all Authentication Policies List", LogMessageType.SUCCESS.ToString(), "Get Authentication Policies list success");
            return View(model);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NewPrimary()
        {
            var model = new AuthnPoliciesNewPrimaryViewModel();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> NewMultiModel()
        {

            var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
            if (primaryAuthlist == null)
            {
                return NotFound();
            }
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in primaryAuthlist)
            {
                list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
            }

            var model = new AuthnPoliciesNewMultiModelViewModel
            {
                primaryAuthlist = list
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePrimary(AuthnPoliciesNewPrimaryViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("NewPrimary", viewModel);
            }

            var PriAuthScheme = new PrimaryAuthScheme()
            {
                Id = 0,
                Name = viewModel.Name,
                Description = viewModel.Description,
                DisplayName = viewModel.DisplayName,
                StrngMatch = viewModel.StrngMatch,
                ClientVerify = viewModel.ClientVerify,
                RandPresent = viewModel.RandPresent,
                Guid = Guid.NewGuid().ToString()
            };

            var response = await _primaryAuthSchemeService.CreatePrimaryAuthSchemeAsync(PriAuthScheme,viewModel.SupportsProvisioning);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Create new Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to create Authentication Policies");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("NewPrimary", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Create new Authentication Policies", LogMessageType.SUCCESS.ToString(), "Created New Authentication Policies with name " + viewModel.DisplayName + " Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMultiModel(AuthnPoliciesNewMultiModelViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;
                return View("NewMultiModel", viewModel);
            }

            var Authlist = viewModel.AuthSchemeSequence.Split(" ");
            if (Authlist.Count() <= 1)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;
                Alert alert = new Alert { Message = "Please select at least 2 Authentication schems" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return View("NewMultiModel", viewModel);
            }

            var AuthScheme = new AuthScheme()
            {
                Id = 0,
                Name = viewModel.AuthSchemeSequence.Replace(" ","_"),
                Description = viewModel.Description,
                DisplayName = viewModel.DisplayName,
                Guid = Guid.NewGuid().ToString(),
                IsPrimaryAuthscheme = false,
                PriAuthSchCnt = viewModel.AuthScheme.Count,
                SupportsProvisioning = viewModel.SupportsProvisioning,
            };

            var response = await _authSchemeSevice.CreateAuthSchemeAsync(AuthScheme, viewModel.AuthSchemeSequence.Split(" ").ToList<string>());
            if (response == null || !response.Success)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Create new MultiModel Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to create MultiModel Authentication Policies");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return View("NewMultiModel", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Create new MultiModel Authentication Policies", LogMessageType.SUCCESS.ToString(), "Created New MultiModel Authentication Policies with name " + viewModel.DisplayName + " Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPrimary(string id)
        {
            var PriAuth = await _primaryAuthSchemeService.GetPrimaryAuthSchemeAsync(id);
            if (PriAuth == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get Primary Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to get primary authentication policies details ");
                return NotFound();
            }

            var PriAuthProvisioningDetails = await _authSchemeSevice.GetAuthSchemeAsync(id);
            if (PriAuthProvisioningDetails == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get Primary Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to get primary authentication policies details ");
                return NotFound();
            }

            var model = new AuthnPoliciesEditPrimaryViewModel
            {
                AuthID = PriAuth.Id,
                Name = PriAuth.Name,
                Description = PriAuth.Description,
                DisplayName = PriAuth.DisplayName,
                StrngMatch = PriAuth.StrngMatch,
                ClientVerify = PriAuth.ClientVerify,
                RandPresent = PriAuth.RandPresent,
                SupportsProvisioning = (int)PriAuthProvisioningDetails.SupportsProvisioning
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get Primary Authentication Policies", LogMessageType.SUCCESS.ToString(), "Get primary authentication policies details of "+ model.DisplayName+ " success ");
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditMultiModel(string id)
        {

            var authScheme = await _authSchemeSevice.GetAuthSchemeAsync(id);
            if (authScheme == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get MultiModel Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to get MultiModel authentication policies details ");
                return NotFound();
            }

            var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
            if (primaryAuthlist == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get MultiModel Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to get primary authentication policies list ");
                return NotFound();
            }
            List<SelectListItem> list = new List<SelectListItem>();
            List<string> selected = new List<string>();
            foreach (var item in primaryAuthlist)
            {
                list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });//,Selected = (authScheme.Name.Contains(item.Name) ? true :false)});
                //if (authScheme.Name.Contains(item.Name))
                //{
                //    selected.Add(item.Name);
                //}
            }

            selected = (List<string>)await _authSchemeSevice.GetPrimaryAuthSchemesOfAuthScheme(authScheme.Id);

            var model = new AuthnPoliciesEditMultiModelViewModel
            {
                AuthID = authScheme.Id,
                Name = authScheme.Name,
                Description = authScheme.Description,
                DisplayName = authScheme.DisplayName,
                AuthSchemeSequence = authScheme.Name.Replace("_"," "),
                AuthScheme = selected,
                primaryAuthlist = list,
                SupportsProvisioning = (int)authScheme.SupportsProvisioning
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Get MultiModel Authentication Policies", LogMessageType.SUCCESS.ToString(), "Get MultiModel authentication policies details of " + model.DisplayName + " success ");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePrimary(AuthnPoliciesEditPrimaryViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("EditPrimary", viewModel);
            }

            var PriAuthScheme = new PrimaryAuthScheme()
            {
                Id = viewModel.AuthID,
                Name = viewModel.Name,
                Description = viewModel.Description,
                DisplayName = viewModel.DisplayName,
                StrngMatch = viewModel.StrngMatch,
                ClientVerify = viewModel.ClientVerify,
                RandPresent = viewModel.RandPresent
            };

            var response = await _primaryAuthSchemeService.UpdatePrimaryAuthSchemeAsync(PriAuthScheme,viewModel.SupportsProvisioning);
            if (response == null || !response.Success)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, " Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to Update Primary Authentication Policies");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal errUpdate Primaryor please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("NewPrimary", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Update Primary Authentication Policies", LogMessageType.SUCCESS.ToString(), "Update Primary Authentication Policies with name " + viewModel.DisplayName + " Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMultiModel(AuthnPoliciesEditMultiModelViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;
                return View("EditMultiModel", viewModel);
            }

            var Authlist = viewModel.AuthSchemeSequence.Split(" ");
            if (Authlist.Count() <= 1)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;
                Alert alert = new Alert { Message = "Please select at least 2 Authentication schems" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return View("EditMultiModel", viewModel);
            }
            

            var AuthScheme = new AuthScheme()
            {
                Id = viewModel.AuthID,
                Name = viewModel.AuthSchemeSequence.Replace(" ", "_"),
                Description = viewModel.Description,
                DisplayName = viewModel.DisplayName,
                IsPrimaryAuthscheme = false,
                PriAuthSchCnt = viewModel.AuthScheme.Count,
                SupportsProvisioning = viewModel.SupportsProvisioning
            };

            var response = await _authSchemeSevice.UpdateAuthSchemeAsync(AuthScheme, viewModel.AuthSchemeSequence.Split(" ").ToList<string>());
            if (response == null || !response.Success)
            {
                var primaryAuthlist = await _primaryAuthSchemeService.ListPrimaryAuthSchemesAsync();
                if (primaryAuthlist == null)
                {
                    return NotFound();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in primaryAuthlist)
                {
                    list.Add(new SelectListItem() { Value = item.Name, Text = item.Name });
                }

                viewModel.primaryAuthlist = list;

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Update MultiModel Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to Update MultiModel Authentication Policies");
                //ModelState.AddModelError(string.Empty, response.Message);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return View("NewMultiModel", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Update MultiModel Authentication Policies", LogMessageType.SUCCESS.ToString(), "Update MultiModel Authentication Policies with name " + viewModel.DisplayName + " Successfully");

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _authSchemeSevice.DeleteAuthSchemeAsync(id);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Delete Authentication Policies", LogMessageType.FAILURE.ToString(), "Fail to delete Authentication Policies");
                return null;
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Authentication_Policies, "Delete Authentication Policies", LogMessageType.SUCCESS.ToString(), "Delete Authentication Policies successfully");
                return new JsonResult(true);
            }
        }
    }
}
