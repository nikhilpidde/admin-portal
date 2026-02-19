using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.CredentialApproval;
using DTPortal.Web.ViewModel.CredentialVerifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class WalletVerificationController : BaseController
    {
        private readonly ICredentialService _credentialService;
        private readonly IOrganizationService _organizationService;
        private readonly ICredentialVerifiersService _credentialVerifiersService;

        public WalletVerificationController(ICredentialService credentialService,
    IOrganizationService organizationService, ICredentialVerifiersService credentialVerifiersService,
    ILogClient logClient) : base(logClient)
        {
            _credentialVerifiersService = credentialVerifiersService;
            _credentialService = credentialService;
            _credentialService = credentialService;
            _organizationService = organizationService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> VerificationList()
        {
            var response = await _credentialVerifiersService.GetCredentialVerifierDTOsListAsync();
            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credentialVerifierList = (List<CredentialVerifierDTO>)response.Resource;

            var viewModel = new List<CredentialVerifierViewModel>();

            foreach (var credential in credentialVerifierList)
            {
                var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.organizationId);
                var credentialDetails= await _credentialService.GetCredentialByUid(credential.credentialId);
                var OrganizationName = "";
                var CredentialName = "";
                if (organizationDetails != null && organizationDetails.Success)
                {
                    var organization = (OrganizationDTO)organizationDetails.Resource;
                    OrganizationName = organization.OrganizationName;
                }
                if (credentialDetails != null && credentialDetails.Success)
                {
                    var credentiall = (CredentialDTO)credentialDetails.Resource;
                    CredentialName = credentiall.credentialName;
                }
                viewModel.Add(new CredentialVerifierViewModel
                {
                    Id = credential.id,
                    credentialName =CredentialName,
                    organizationName = OrganizationName,
                    CreatedDate = credential.createdDate,
                    UpdatedDate=credential.updatedDate,
                    status = credential.status
                });


            }

            return View(viewModel);
        }

        public async Task<IActionResult> CredentialVerificationDetails(int Id)
        {
            var response = await _credentialVerifiersService.GetCredentialVerifierByIdAsync(Id);
            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credentialVerifierList = (CredentialVerifierDTO)response.Resource;
            var model=new CredentialVerifierViewModel
            {
                Id = credentialVerifierList.id,
                credentialName = credentialVerifierList.credentialId,
                organizationName = credentialVerifierList.organizationId,
                CreatedDate = credentialVerifierList.createdDate,
                UpdatedDate = credentialVerifierList.updatedDate,
                status = credentialVerifierList.status,
                attributes= credentialVerifierList.attributes,
                remarks=credentialVerifierList.remarks,
                emails=credentialVerifierList.emails,
                configuration=credentialVerifierList.configuration
            };
            var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credentialVerifierList.organizationId);

            if (organizationDetails != null && organizationDetails.Success)
            {
                var organization = (OrganizationDTO)organizationDetails.Resource;
                model.organizationName = organization.OrganizationName;
            }
            var credentialDetails = await _credentialService.GetCredentialByUid(credentialVerifierList.credentialId);

            if (organizationDetails != null && organizationDetails.Success)
            {
                var credential = (CredentialDTO)credentialDetails.Resource;
                model.credentialName = credential.credentialName;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateCredential(int id)
        {
            var response = await _credentialVerifiersService.ActivateCredentialById(id);

            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = false, message = "Failed to Activate the credential." });
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = true, message = "Credential Activated successfully!" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> RejectCredential(int id, string remarks)
        {
            var response = await _credentialVerifiersService.RejectCredentialById(id,remarks);

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
