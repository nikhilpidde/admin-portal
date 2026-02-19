using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.CredentialApproval;
using DTPortal.Web.ViewModel.QrCredentialApproval;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    public class QrCredentialApprovalController : BaseController
    {
        private readonly IQrCredentialService _qrCredentialService;
        private readonly ICategoryService _categoryService;
        private readonly IOrganizationService _organizationService;
        public QrCredentialApprovalController(IQrCredentialService qrCredentialService,
            ICategoryService categoryService,
            IOrganizationService organizationService,
            ILogClient logClient) : base(logClient)
        {
            _qrCredentialService = qrCredentialService;
            _categoryService = categoryService;
            _organizationService = organizationService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _qrCredentialService.GetCredentialList();
            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credentialList = (List<QrCredentialDTO>)response.Resource;

            var viewModel = new List<QrCredentialListViewModel>();

            foreach (var credential in credentialList)
            {
                var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.organizationId);
                var OrganizationName = "";
                if (organizationDetails != null && organizationDetails.Success)
                {
                    var organization = (OrganizationDTO)organizationDetails.Resource;
                    OrganizationName = organization.OrganizationName;
                }
                viewModel.Add(new QrCredentialListViewModel
                {
                    Id = credential.Id,
                    organizationName = OrganizationName,
                    createdDate = credential.createdDate,
                    CredentialName=credential.displayName,
                    status = credential.status
                });
            }
            return View(viewModel);
        }
        public async Task<IActionResult> CredentialDetails(int Id)
        {
            var response = await _qrCredentialService.GetCredentialById(Id);

            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credential = (QrCredentialDTO)response.Resource;

            var model = new QrCredentialDetailsViewModel()
            {
                Id = credential.Id,
                credentialUId = credential.credentialUId,
                CredentialName = credential.credentialName,
                status = credential.status,
                organizationName = credential.organizationId,
                remarks = credential.remarks,
                createdDate = credential.createdDate.ToString("yyyy-MM-dd"),
                dataAttributes = credential.dataAttributes,

            };

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

            var response = await _qrCredentialService.ActivateCredential(uid);
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
            var response = await _qrCredentialService.RejectCredential(uid, remarks);

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
