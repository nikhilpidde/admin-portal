using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Web.ViewModel.OrganizationKycMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class OrganizationKycMethodsController : Controller
    {
        private readonly IKycMethodsService _kycMethodsService;
        private readonly IOrganizationKycMethodsService _organizationKycMethodsService;
        private readonly IOrganizationService _organizationService;
        private readonly IKycProfilesService _kycProfilesService;
        public OrganizationKycMethodsController(
            IKycMethodsService kycMethodsService,
            IOrganizationKycMethodsService organizationKycMethodsService,
            IOrganizationService organizationService,
            IKycProfilesService kycProfilesService)
        {
            _kycMethodsService = kycMethodsService;
            _organizationKycMethodsService = organizationKycMethodsService;
            _organizationService = organizationService;
            _kycProfilesService = kycProfilesService;
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
        public async Task<IActionResult> Index()
        {
            var organizations = await GetOrganizationList();
            var viewModel = new OrganizationKycMethodsViewModel
            {
                OrganizationList = organizations
            };
            return View(viewModel);
        }

        public async Task<IActionResult> GetKycMethodsByOrgId(string orgId)
        {
            // Methods
            var kycMethodsResponse = await _kycMethodsService.GetKycMethodsListAsync();
            if (!kycMethodsResponse.Success)
                return Ok(kycMethodsResponse);
            var kycMethodsList = (List<KycMethod>)kycMethodsResponse.Resource;

            // Profiles
            var kycProfilesResponse = await _kycProfilesService.ListKycProfilesAsync();
            if (!kycProfilesResponse.Success)
                return Ok(kycProfilesResponse);
            var kycProfilesList = (List<KycProfile>)kycProfilesResponse.Resource;

            // Org selected Methods
            var orgMethodsResponse = await _organizationKycMethodsService.GetOrganizationKycMethodsByOrgIdAsync(orgId);
            if (!orgMethodsResponse.Success)
                return Ok(orgMethodsResponse);
            var selectedMethods = (List<string>)orgMethodsResponse.Resource;

            // Org selected Profiles
            var orgProfilesResponse = await _organizationKycMethodsService.GetOrganizationKycProfilesByOrgIdAsync(orgId);
            if (!orgProfilesResponse.Success)
                return Ok(orgProfilesResponse);
            var selectedProfiles = (List<string>)orgProfilesResponse.Resource;

            // Map methods
            var methods = kycMethodsList.Select(m => new KycMethodsViewModel
            {
                Name = m.Name,
                DisplayName = m.DisplayName,
                IsSelected = selectedMethods.Contains(m.Name)
            }).ToList();

            // Map profiles
            var profiles = kycProfilesList.Select(p => new KycProfilesViewModel
            {
                Name = p.Name,
                DisplayName = p.DisplayName,
                IsSelected = selectedProfiles.Contains(p.Name)
            }).ToList();

            var result = new KycDataViewModel
            {
                Methods = methods,
                Profiles = profiles
            };

            return Json(result);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateKycMethods
            ([FromBody] OrganizationKycMethodsViewModel model)
        {
            OrganizationKycMethod organizationKycMethod = new OrganizationKycMethod
            {
                OrganizationId = model.OrganizationId,
                KycMethods = JsonConvert.SerializeObject(model.SelectedKycMethodNames),
                KycProfiles = JsonConvert.SerializeObject(model.SelectedKycProfileNames)
            };
            var result = await _organizationKycMethodsService.
                UpdateOrganizationKycMethodAsync(organizationKycMethod);

            return Ok(result);
        }
    }
}
