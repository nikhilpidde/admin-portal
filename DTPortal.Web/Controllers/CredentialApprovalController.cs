using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.CredentialApproval;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class CredentialApprovalController : BaseController
    {
        private readonly ICredentialService _credentialService;
        private readonly ICategoryService _categoryService;
        private readonly IOrganizationService _organizationService;
        public CredentialApprovalController(ICredentialService credentialService,
            ICategoryService categoryService,
            IOrganizationService organizationService,
            ILogClient logClient) : base(logClient)
        {
            _credentialService = credentialService;
            _credentialService = credentialService;
            _categoryService = categoryService;
            _organizationService = organizationService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _credentialService.GetCredentialList();
            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credentialList = (List<CredentialDTO>)response.Resource;

            var viewModel = new List<CredentialListViewModel>();

            foreach (var credential in credentialList)
            {
                var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.organizationId);
                var OrganizationName = "";
                if (organizationDetails != null && organizationDetails.Success)
                {
                    var organization = (OrganizationDTO)organizationDetails.Resource;
                    OrganizationName = organization.OrganizationName;
                }
                viewModel.Add(new CredentialListViewModel
                {
                    Id = credential.Id,
                    CredentialName = credential.displayName,
                    organizationName = OrganizationName,
                    createdDate = credential.createdDate,
                    authenticationScheme = credential.authenticationScheme,
                    verificationDocType = credential.verificationDocType,
                    status = credential.status
                });


            }

            return View(viewModel);
        }

        public async Task<IActionResult> CredentialDetails(int Id)
        {
            var response = await _credentialService.GetCredentialById(Id);

            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credential = (CredentialDTO)response.Resource;

            var model = new CredentialDetailsViewModel()
            {
                Id = credential.Id,
                credentialUId = credential.credentialUId,
                DisplayName = credential.displayName,
                CredentialName = credential.credentialName,
                authenticationScheme = credential.authenticationScheme,
                verificationDocType = credential.verificationDocType,
                status = credential.status,
                categoryName = credential.categoryId,
                organizationName = credential.organizationId,
                remarks = credential.remarks,
                logo = credential.logo,
                createdDate = credential.createdDate.ToString("yyyy-MM-dd"),
                dataAttributes = credential.dataAttributes,
                signedDocument = credential.signedDocument,
                trustUrl=credential.trustUrl

            };

            var categoryName = await _categoryService.GetCategoryNamebyUIdAsync(credential.categoryId);

            if (!string.IsNullOrEmpty(categoryName))
            {
                model.categoryName = categoryName;
            }

            var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.organizationId);

            if (organizationDetails != null && organizationDetails.Success)
            {
                var organization = (OrganizationDTO)organizationDetails.Resource;
                model.organizationName = organization.OrganizationName;
            }

            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> Approve(string uid)
        {

            var response = await _credentialService.ActivateCredential(uid);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = false, message = "Failed to approve the credential." });
            }

            Alert successAlert = new Alert { IsSuccess = true, Message = response.Message };
            TempData["Alert"] = JsonConvert.SerializeObject(successAlert);
            return Json(new { success = true, message = "Credential approved successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string uid, string remarks)
        {
            var response = await _credentialService.RejectCredential(uid, remarks);

            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = false, message = "Failed to reject the credential." });
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = true, message = "Credential rejected successfully!" });
            }
        }
    }
}
