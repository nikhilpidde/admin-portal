using AspNetCoreGeneratedDocument;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.KycProfles;
using DTPortal.Web.ViewModel.OrganizationKycMethods;
using DTPortal.Web.ViewModel.Scopes;
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
    [Authorize]
    public class KycProfilesController : BaseController
    {
        private readonly IKycMethodsService _kycMethodsService;
        private readonly IOrganizationKycMethodsService _organizationKycMethodsService;
        private readonly IOrganizationService _organizationService;
        private readonly IKycProfilesService _kycProfilesService;
        private readonly IKycAttributesService _kycAttributesService;
        public KycProfilesController(
            IKycMethodsService kycMethodsService,
            IOrganizationKycMethodsService organizationKycMethodsService,
            IOrganizationService organizationService,
            IKycProfilesService kycProfilesService,
            IKycAttributesService kycAttributesService,
            ILogClient logClient) : base(logClient)
        {
            _kycMethodsService = kycMethodsService;
            _organizationKycMethodsService = organizationKycMethodsService;
            _organizationService = organizationService;
            _kycProfilesService = kycProfilesService;
            _kycAttributesService = kycAttributesService;
        }
        public async Task<IActionResult> List()
        {
            var kycProfiles = await _kycProfilesService.ListKycProfilesAsync();
            if(kycProfiles == null || !kycProfiles.Success)
            {
                return NotFound();
            }
            var KycProfileList = (IEnumerable<KycProfile>)kycProfiles.Resource;
            var kycProfilesViewModel = KycProfileList.Select(k => new KycProfileListViewModel
            {
                Id = k.Id,
                Name = k.Name,
                DisplayName = k.DisplayName,
            }).ToList();
            return View(kycProfilesViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var attributesList = await GetAttributesList(false);
            if (attributesList == null)
            {
                return NotFound();
            }
            var viewModel = new KycProfileAddViewModel
            {
                AttributesList = attributesList,
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(KycProfileAddViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var AttributesList = await GetAttributesList(true, viewModel.Attributes);
                if (AttributesList == null)
                {
                    return NotFound();
                }
                viewModel.AttributesList = AttributesList;
                return View("New", viewModel);
            }

            var profile = new KycProfile()
            {
                Name = viewModel.Name,
                DisplayName = viewModel.DisplayName,
                AttributesList = viewModel.Attributes
            };

            var response = await _kycProfilesService.CreateKycProfileAsync(profile);
            if (response == null || !response.Success)
            {
                var Claimslist = await GetAttributesList(true, viewModel.Attributes);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.AttributesList = Claimslist;
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("New", viewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var kycProfile = await _kycProfilesService.GetKycProfileByIdAsync(id);
            if (kycProfile == null)
            {
                return NotFound();
            }

            var AttributesList = await GetAttributesList(true, kycProfile.AttributesList);
            if (AttributesList == null)
            {
                return NotFound();
            }

            var ScopeEditViewModel = new KycProfileEditViewModel
            {
                Id = kycProfile.Id,
                Name = kycProfile.Name,
                DisplayName = kycProfile.DisplayName,
                AttributesList = AttributesList,
                Attributes = kycProfile.AttributesList
            };

            return View(ScopeEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(KycProfileEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var AttributesList = await GetAttributesList(true, viewModel.Attributes);
                if (AttributesList == null)
                {
                    return NotFound();
                }
                viewModel.AttributesList = AttributesList;
                return View("Edit", viewModel);
            }
            var kycProfilinDb = await _kycProfilesService.GetKycProfileByIdAsync(viewModel.Id);
            if (kycProfilinDb == null)
            {
                var AttributesList = await GetAttributesList(true, viewModel.Attributes);
                if (AttributesList == null)
                {
                    return NotFound();
                }
                viewModel.AttributesList = AttributesList;
                return View("Edit", viewModel);
            }

            kycProfilinDb.Id = viewModel.Id;
            kycProfilinDb.Name = viewModel.Name;
            kycProfilinDb.DisplayName = viewModel.DisplayName;
            kycProfilinDb.AttributesList = viewModel.Attributes;

            var response = await _kycProfilesService.UpdateKycProfileAsync(kycProfilinDb);
            if (response == null || !response.Success)
            {
                var AttributesList = await GetAttributesList(true, viewModel.Attributes);
                if (AttributesList == null)
                {
                    return NotFound();
                }
                viewModel.AttributesList = AttributesList;
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Edit", viewModel);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        public async Task<List<AttributesListItem>> GetAttributesList
            (bool isAttributesPresent, string ScopeClaims = null)
        {
            var AttributesList = await _kycAttributesService.ListKycAttributesAsync();
            if (AttributesList == null)
            {
                throw new Exception("Fail to get user clamis");
            }
            var nodes = new List<AttributesListItem>();

            if (isAttributesPresent && ScopeClaims != null)
            {
                var ScopeClaimslist = ScopeClaims.Split(",");
                foreach (var attribute in AttributesList)
                {
                    nodes.Add(new AttributesListItem()
                    {
                        name = attribute.Name,
                        Display = attribute.DisplayName,
                        IsSelected = ScopeClaimslist.Any(x => x == attribute.Name)
                    });
                }
            }
            else
            {
                foreach (var claim in AttributesList)
                {

                    nodes.Add(new AttributesListItem()
                    {
                        name = claim.Name,
                        Display = claim.DisplayName,
                        IsSelected = false
                    });
                }
            }

            return nodes;
        }
    }
}
