using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel.Scopes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel.WalletDomain;
using DTPortal.Core.Services;
using System.Linq;
using System;
using DTPortal.Core.Domain.Models;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class WalletDomainController : BaseController
    {
        private readonly IWalletDomainService _walletDomainService;
        private readonly IWalletPurposeService _walletPurposeService;
        public WalletDomainController(ILogClient logClient, IWalletDomainService walletDomainService,IWalletPurposeService walletPurposeService) : base(logClient)
        {
            _walletDomainService = walletDomainService;
            _walletPurposeService = walletPurposeService;
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<WalletDomainListViewModel>();

            var DomainList = await _walletDomainService.ListWalletDomainAsync();
            if (DomainList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all Wallet Domains Configuration List", LogMessageType.FAILURE.ToString(), "Fail to get Wallet Domains Configuration list");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all Wallet Domains Configuration List", LogMessageType.SUCCESS.ToString(), "Get Wallet Domains Configuration list success");

                foreach (var item in DomainList)
                {
                    viewModel.Add(new WalletDomainListViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DisplayName = item.DisplayName,
                        CreatedDate=item.CreatedDate,
                        Status = item.Status,
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var Purposelist = await GetPurposeList(false);
            if (Purposelist == null)
            {
                return NotFound();
            }
            var viewModel = new WalletDomainNewViewModel
            {
                PurposeLists = Purposelist,
            };
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Save(WalletDomainNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var Purposeslist = await GetPurposeList(true, viewModel.Purposes);
                if (Purposeslist == null)
                {
                    return NotFound();
                }
                viewModel.PurposeLists = Purposeslist;
                return View("New", viewModel);
            }

            var WalletDomain = new WalletDomain()
            {
                Name = viewModel.Name,
                DisplayName = viewModel.DisplayName,
                Description = viewModel.Description,

                CreatedBy = UUID,
                UpdatedBy = UUID,
                Purposes = viewModel.Purposes,
                //IsClaimsPresent = true,
            };
                

            //var CliamList = viewModel.claims.Split(",")
            //      .Select(x => new { claimId = int.Parse(x.Split(":")[0]), isSelected = x.Split(":")[1].Equals("true") })
            //      .ToDictionary(x => x.claimId, x => x.isSelected);

            //foreach (var item in CliamList)
            //{
            //    var ScopeClaims = new ScopeClaim
            //    {
            //        ClaimId = item.Key,
            //        IsEnabled = item.Value
            //    };
            //    scope.ScopeClaims.Add(ScopeClaims);
            //}

            var response = await _walletDomainService.CreateDomainAsync(WalletDomain);
            if (response == null || !response.Success)
            {
                var Purposelist = await GetPurposeList(true, viewModel.Purposes);
                if (Purposelist == null)
                {
                    return NotFound();
                }
                viewModel.PurposeLists = Purposelist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to create Scopes Configuration");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Created New Scopes Configuration with name " + viewModel.DisplayName + " Successfully");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var walletDomainInDb = await _walletDomainService.GetWalletDomainAsync(id);
            if (walletDomainInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
                return NotFound();
            }

            var Purposeslist = await GetPurposeList(false, walletDomainInDb.Purposes);
            if (Purposeslist == null)
            {
                return NotFound();
            }

            var WalletDomainEditViewModel = new WalletDomainEditViewModel
            {
                Id = walletDomainInDb.Id,
                Name = walletDomainInDb.Name,
                DisplayName = walletDomainInDb.DisplayName,
                Description = walletDomainInDb.Description,
                PurposeLists = Purposeslist,
                Purposes = walletDomainInDb.Purposes,
                //isClaimPresent = scopeInDb.IsClaimsPresent,
                //SaveConsent = scopeInDb.SaveConsent,
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration details", LogMessageType.SUCCESS.ToString(), "Get Scopes Configuration details of " + walletDomainInDb.DisplayName + " successfully ");

            return View(WalletDomainEditViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Update(WalletDomainEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var Purposeslist = await GetPurposeList(false, viewModel.Purposes);
                if (Purposeslist == null)
                {
                    return NotFound();
                }
                viewModel.PurposeLists = Purposeslist;
                return View("Edit", viewModel);
            }



            var walletDomainInDb = await _walletDomainService.GetWalletDomainAsync(viewModel.Id);
            if (walletDomainInDb == null)
            {
                var Purposeslist = await GetPurposeList(false, viewModel.Purposes);
                if (Purposeslist == null)
                {
                    return NotFound();
                }
                viewModel.PurposeLists = Purposeslist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
                ModelState.AddModelError(string.Empty, "Scopes Configuration not found");

                return View("Edit", viewModel);
            }


            walletDomainInDb.Id = viewModel.Id;
            walletDomainInDb.Name = viewModel.Name;
            walletDomainInDb.DisplayName = viewModel.DisplayName;
            walletDomainInDb.Description = viewModel.Description;
            walletDomainInDb.UpdatedBy = UUID;
            walletDomainInDb.Purposes = viewModel.Purposes;
            //scopeInDb.IsClaimsPresent = true;
            //scopeInDb.SaveConsent = viewModel.SaveConsent;

            //var CliamList = viewModel.claims.Split(",")
            //      .Select(x => new { claimId = int.Parse(x.Split(":")[0]), isSelected = x.Split(":")[1].Equals("true") })
            //      .ToDictionary(x => x.claimId, x => x.isSelected);

            //foreach (var item in scopeInDb.ScopeClaims)
            //{
            //    if (CliamList.ContainsKey(item.ClaimId))

            //    {
            //        CliamList.TryGetValue(item.ClaimId, out var val);

            //        item.IsEnabled = val;
            //    }
            //    else
            //    {
            //        item.IsEnabled = false;
            //    }
            //}

            var response = await _walletDomainService.UpdateWalletDomainAsync(walletDomainInDb);
            if (response == null || !response.Success)
            {
                var Purposeslist = await GetPurposeList(false, viewModel.Purposes);
                if (Purposeslist == null)
                {
                    return NotFound();
                }
                viewModel.PurposeLists = Purposeslist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to update Scopes Configuration details of  name " + viewModel.DisplayName);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Scopes Configuration details of  name " + viewModel.DisplayName + " successfully" : "Request for Update Scopes Configuration details of application name " + viewModel.DisplayName + " has send for approval "));

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
                var response = await _walletDomainService.DeleteWalletDomainAsync(id, UUID);
                if (response != null || response.Success)
                {

                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Delete Scopes Configuration successfully");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration");
                    return new JsonResult(true);
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration " + e.Message);
                return StatusCode(500, e);
            }
        }

        public async Task<List<PurposeListItem>> GetPurposeList(bool isPurposePresent, string WalletPurposes = null)
        {
            var PurposeList = await _walletPurposeService.GetPurposeListAsync();
            if (PurposeList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all user clamis list", LogMessageType.FAILURE.ToString(), "Fail to get all user clamis list");
                throw new Exception("Fail to get user clamis");
            }
            var nodes = new List<PurposeListItem>();

            if (isPurposePresent || WalletPurposes != null)
            {
                var PurposesList = WalletPurposes.Split(" ");
                foreach (var purpose in PurposeList)
                {
                    nodes.Add(new PurposeListItem()
                    {
                        name = purpose.Id.ToString(),
                        Display = purpose.DisplayName,
                        IsSelected = PurposesList.Any(x => x == purpose.Id.ToString())
                    });
                }
            }
            else
            {
                foreach (var claim in PurposeList)
                {

                    nodes.Add(new PurposeListItem()
                    {
                        name = claim.Id.ToString(),
                        Display = claim.DisplayName,
                        IsSelected = false
                    });
                }
            }

            return nodes;
        }
    }
}
