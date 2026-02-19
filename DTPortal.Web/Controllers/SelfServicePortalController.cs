using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.SelfServicePortal;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class SelfServicePortalController : BaseController
    {
        private readonly ILogger<SelfServicePortalController> _logger;
        private readonly ISelfPortalService _selfPortalService;
        private readonly ISubscriberService _subscriberService;
        private readonly IMCValidationService _mcValidationService;
        public SelfServicePortalController(ILogger<SelfServicePortalController> logger,
            ISelfPortalService selfPortalService,
            ILogClient logClient,
            ISubscriberService subscriberService,
            IMCValidationService mcValidationService) : base(logClient)
        {
            _logger = logger;
            _selfPortalService = selfPortalService;
            _subscriberService = subscriberService;
            _mcValidationService = mcValidationService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var selfServiceOrgList = await _selfPortalService.GetAllSelfServiceOrganizationListAsync();
            if (selfServiceOrgList == null)
            {
                return NotFound();
            }

            SelfServiceNewOrganization viewModel = new SelfServiceNewOrganization()
            {
                OrganizationList = selfServiceOrgList
            };
            return View(viewModel);
        }

        //[HttpGet]
        //public Task<IActionResult> ApproveOrganization(int id)
        //{
        //    //ApproveOrganizationConsentViewModel viewModel = new ApproveOrganizationConsentViewModel()
        //    //{
        //    //    OrganizationFormId = id
        //    //};
        //    //return PartialView("_ApproveOrganizationConsent", viewModel);
        //    if (true)
        //    {
        //        ApproveOrganizationConsentViewModel viewModel = new ApproveOrganizationConsentViewModel()
        //        { OrganizationFormId = id };
        //        return PartialView("_ApproveOrganizationConsent", viewModel);
        //    }
        //    else
        //    {
        //        return await ApproveInternal(id);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> ApproveOrganization(int id)
        {
            var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.OnboardingApprovalRequestActivityId);
            if (!isEnabled)
            {
                ApproveNewOrganizationviewModel viewModel = new ApproveNewOrganizationviewModel()
                {
                    OrgDetailsId = id
                };
                return PartialView("_ApproveOrganizationConsent", viewModel);
            }
            else
            {
                return await ApproveInternal(id);
            }
        }
        public async Task<IActionResult> DownloadDocument(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return BadRequest();

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(fileUrl);

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            // Detect file name
            var fileName = "document";
            if (response.Content.Headers.ContentDisposition != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName?.Trim('"')
                           ?? fileName;
            }

            // Detect content type
            var contentType =
                response.Content.Headers.ContentType?.ToString()
                ?? "application/octet-stream";

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet]
        public IActionResult Reject(int orgFormId)
        {
            RejectNewOrganizationViewModel viewModel = new RejectNewOrganizationViewModel()
            {
                OrgDetailsId = orgFormId
            };

            //var rejectReason = await _selfPortalService.GetRejectedReasonAsync();
            //if (rejectReason.Success)
            //{
            //    viewModel.RejectedReasonList = (IList<string>)rejectReason.Resource;
            //}

            return PartialView("_RejectOrganization", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrganizationDetailsOld(int id)
        {
            var orgdetails = await _selfPortalService.GetSelfServiceOrganizationDetailsAsync(id);
            if (orgdetails == null)
            {
                return NotFound();
            }

            string[] address = orgdetails.OrgCorporateAddress.Split(';');

            OrganizationDetailsViewModel viewModel = new OrganizationDetailsViewModel()
            {
                OrgOnboardingFormsId = orgdetails.OrgOnboardingFormsId,
                OrgRegIdNumber = orgdetails.OrgRegIdNumber,
                OrganizationName = orgdetails.OrgName,
                OrgOfficialContactNumber = orgdetails.OrgOfficialContactNumber,
                CorporateOfficeAddress1 = address[0],
                CorporateOfficeAddress2 = address[1],
                Country = address[2],
                Pincode = address[3],
                OrgWebUrl = orgdetails.OrgWebUrl,
                OrgTanTaxNumber = orgdetails.OrgTanTaxNumber,
                UrsbCertificate = orgdetails.UrsbCertificate,
                ApprovalLetter = orgdetails.ApprovalLetter,
                ObFormStatus = orgdetails.ObFormStatus,
                OrgApprovalStatus = orgdetails.OrgApprovalStatus,
                RejectedReason = orgdetails.OrgObRejectedReason,
                SignApprByBrmStaff = orgdetails.SignApprByBrmStaff,
                OrgUid = orgdetails.OrgUid,
                OrgCategory = orgdetails.OrgCategory,
                SpocSuid = orgdetails.SpocSuid,
                OtpVerification = orgdetails.OtpVerification
            };

            if (orgdetails.OrgFinancialAuditorDetailsDTO != null)
            {
                viewModel.FinancialAuditorName = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorName;
                viewModel.FinancialAuditorLicenseNum = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorLicenseNum;
                //viewModel.FinancialAuditorNin = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorNin;
                viewModel.FinancialAuditorIdDocNumber = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorIdDocumentNumber;
                viewModel.OrgFinancialAuditorDetailsId = orgdetails.OrgFinancialAuditorDetailsDTO.OrgFinancialAuditorDetailsId;
                viewModel.AuditorUgPassEmail = orgdetails.OrgFinancialAuditorDetailsDTO.FinancialAuditorUgPassEmail;
                viewModel.financialAuditorTinNumber = orgdetails.OrgFinancialAuditorDetailsDTO.financialAuditorTinNumber;
            }

            if (orgdetails.OrganisationSpocDetailsDTO != null)
            {
                viewModel.spocSuid = orgdetails.OrganisationSpocDetailsDTO.SpocSuid;
                viewModel.SpocName = orgdetails.OrganisationSpocDetailsDTO.SpocName;
                viewModel.SpocFaceCaptured = orgdetails.OrganisationSpocDetailsDTO.Spoc_face_captured;
                viewModel.SpocFaceFromUgpass = orgdetails.OrganisationSpocDetailsDTO.SpocFaceFromUgpass;
                viewModel.SpocFaceMatchStatus = true;
                viewModel.SpocIdDocNo = orgdetails.OrganisationSpocDetailsDTO.SpocIdDocumentNumber;
                viewModel.SpocOfficeEmail = orgdetails.OrganisationSpocDetailsDTO.SpocOfficeEmail;
                viewModel.SpocOtpVerfyStatus = orgdetails.OrganisationSpocDetailsDTO.SpocOtpVerfyStatus;
                //viewModel.SpocPassport = orgdetails.OrganisationSpocDetailsDTO.SpocPassport;
                viewModel.SpocSocialSecurityNum = orgdetails.OrganisationSpocDetailsDTO.SpocSocialSecurityNum;
                viewModel.SpocTaxNum = orgdetails.OrganisationSpocDetailsDTO.SpocTaxNum;
                viewModel.SpocUgpassEmail = orgdetails.OrganisationSpocDetailsDTO.SpocUgpassEmail;
                viewModel.SpocUgpassMobNum = orgdetails.OrganisationSpocDetailsDTO.SpocUgpassMobNum;
                viewModel.OrgSpocDetailsId = orgdetails.OrganisationSpocDetailsDTO.OrgSpocDetailsId;

                //here 3 is for email
                var subscriberDetails = await _subscriberService.GetSubscriberDetailsAsync(3, orgdetails.OrganisationSpocDetailsDTO.SpocUgpassEmail);
                if (subscriberDetails != null)
                {
                    viewModel.SpocRAFaceCaptured = subscriberDetails.SubscriberPhoto;
                }
                else
                {
                    _logger.LogInformation("No SPOC details found for email " + orgdetails.OrganisationSpocDetailsDTO.SpocUgpassEmail);
                }
            }

            if (orgdetails.OrgCeoDetailsDTO != null)
            {
                viewModel.CeoPanTaxNum = orgdetails.OrgCeoDetailsDTO.CeoPanTaxNum;
                viewModel.OrgCeoDetailsiId = orgdetails.OrgCeoDetailsDTO.OrgCeoDetailsiId;
                viewModel.CeoName = orgdetails.OrgCeoDetailsDTO.CeoName;
                viewModel.CeoEmail = orgdetails.OrgCeoDetailsDTO.CeoEmail;
                viewModel.ceoIdDocumentNumber = orgdetails.OrgCeoDetailsDTO.ceoIdDocumentNumber;
            }

            if(orgdetails.UraReportsDTO != null)
            {
                viewModel.auditorUraPdf = orgdetails.UraReportsDTO.auditorUraPdf;
                viewModel.spocUraPdf = orgdetails.UraReportsDTO.spocUraPdf;
                viewModel.ceoUraPdf = orgdetails.UraReportsDTO.ceoUraPdf;
                viewModel.orgUraPdf = orgdetails.UraReportsDTO.orgUraPdf;
            }

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> OrganizationDetails(int id)
        {
            var org = await _selfPortalService.GetSelfServiceNewOrganizationDetailsAsync(id);

            if (org == null)
            {
                return NotFound();
            }

            //var org = response.Result;

            var viewModel = new OrgNewDetailsViewModel
            {
                Ouid = Guid.TryParse(org.Ouid, out var guid) ? guid : Guid.Empty,

                OrgDetailsId = org.OrgDetailsId,

                OrgName = org.OrgName,
                OrgNo = org.OrgNo,
                TaxNumber = org.TaxNumber,
                OrgType = org.OrgType,
                RegNo = org.RegNo,

                OrgEmail = org.OrgEmail,
                Address = org.Address,
                Status = org.Status,

                SpocName = org.SpocName,
                SpocOfficialEmail = org.SpocOfficialEmail,
                SpocDocumentNumber = org.SpocDocumentNumber,

                AuditorName = org.AuditorName,
                AuditorDocumentNumber = org.AuditorDocumentNumber,
                AuditorOfficialEmail = org.AuditorOfficialEmail,

                CreatedOn = org.CreatedOn,
                Documents=org.Documents
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> OrganizationDetailsNewModal(int OrgDetailsId)
        {
            // 1. Fetch organization details from NEW service
            var org = await _selfPortalService.GetSelfServiceNewOrganizationDetailsAsync(OrgDetailsId);

            if (org == null)
            {
                return NotFound();
            }

            // 2. Map to NEW ViewModel
            var viewModel = new OrgNewDetailsViewModel
            {
                Ouid = Guid.TryParse(org.Ouid, out var guid) ? guid : Guid.Empty,
                OrgDetailsId = org.OrgDetailsId,

                OrgName = org.OrgName,
                OrgNo = org.OrgNo,
                TaxNumber = org.TaxNumber,
                OrgType = org.OrgType,
                RegNo = org.RegNo,

                OrgEmail = org.OrgEmail,
                Address = org.Address,
                Status = org.Status,

                SpocName = org.SpocName,
                SpocOfficialEmail = org.SpocOfficialEmail,
                SpocDocumentNumber = org.SpocDocumentNumber,

                AuditorName = org.AuditorName,
                AuditorDocumentNumber = org.AuditorDocumentNumber,
                AuditorOfficialEmail = org.AuditorOfficialEmail,

                CreatedOn = org.CreatedOn
            };

            // 3. Return Partial View (Modal)
            return PartialView("_OrganizationDetailsModal", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveNew([FromForm] ApproveNewOrganizationviewModel viewModel)
        {
            var orgDetails = new SelfOrganizationNewDTO()
            {
                OrgDetailsId = viewModel.OrgDetailsId,
                CreatedBy = UUID,
                SpocOfficialEmail = viewModel.SpocOfficialEmail
            };


            var response = await _selfPortalService.ApproveOrganizationNewAsync(orgDetails);
            if (!response.Success)
            {
                //AlertViewModel alert = new AlertViewModel { Message = response.Message };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);

                //return RedirectToAction("ApproveOrganization", new { id = viewModel.OrganizationFormId });
                //return RedirectToAction("List");
                return Json(new { Status = "Failed", Title = "Approve Organization", Message = response.Message });
            }
            else
            {
                //AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                //return RedirectToAction("List");
                //return RedirectToAction("ApproveOrganization", new { id = viewModel.OrganizationFormId });

                return Json(new { Status = "Success", Title = "Approve Organization", Message = response.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectNew(RejectNewOrganizationViewModel viewModel)
        {
            var orgDetails = new SelfOrganizationNewDTO()
            {
                OrgDetailsId = viewModel.OrgDetailsId,
                CreatedBy = UUID,
                SpocOfficialEmail = viewModel.SpocOfficialEmail
            };

            var response = await _selfPortalService.RejectOrganizationNewAsync(orgDetails);
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("OrganizationDetails", new { id = viewModel.OrgDetailsId });
                //return View(viewModel);
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetQuestions(int id)
        {
            //id = 541;
            var list = await _selfPortalService.GetQuestionsAsync(id);
            if (list == null)
            {
                return NotFound();
            }
            BusinessRequirementViewModel viewModel = new BusinessRequirementViewModel()
            {
                QuestionAnswerList = list.AnswersList,
                SoftwareList = list.SoftwareRecommendations
            };
            if (list.AnswersList.Count > 0)
                viewModel.OrganizationFormId = list.AnswersList[0].orgOnboardingFormId;

            return View("BusinessRequirement", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve([FromForm] ApproveOrganizationConsentViewModel viewModel)
        {
            var orgDetails = new SelfServiceOrganizationDTO()
            {
                OrgOnboardingFormsId = viewModel.OrganizationFormId,
                AdminUgpassEmail = viewModel.AdminUgpassEmail,
                CreatedBy = UUID
            };

            var response = await _selfPortalService.ApproveOrganizationAsync(orgDetails);
            if (!response.Success)
            {
                //AlertViewModel alert = new AlertViewModel { Message = response.Message };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);

                //return RedirectToAction("ApproveOrganization", new { id = viewModel.OrganizationFormId });
                //return RedirectToAction("List");
                return Json(new { Status = "Failed", Title = "Approve Organization", Message = response.Message });
            }
            else
            {
                //AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                //TempData["Alert"] = JsonConvert.SerializeObject(alert);
                //return RedirectToAction("List");
                //return RedirectToAction("ApproveOrganization", new { id = viewModel.OrganizationFormId });

                return Json(new { Status = "Success", Title = "Approve Organization", Message = response.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(RejectOrganizationViewModel viewModel)
        {
            var orgDetails = new SelfServiceOrganizationDTO()
            {
                OrgOnboardingFormsId = viewModel.OrgFormId,
                OrgObRejectedReason = viewModel.RejectedReason + "-" + viewModel.Remarks,
                CreatedBy = UUID
            };

            var response = await _selfPortalService.RejectOrganizationAsync(orgDetails);
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("OrganizationDetails", new { id = viewModel.OrgFormId });
                //return View(viewModel);
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SoftwareRecommendation(BusinessRequirementViewModel viewModel)
        {
            RecommendedSoftwareDTO softwareDTO = new RecommendedSoftwareDTO()
            {
                OrgOnboardingFormId = viewModel.OrganizationFormId,
                BrmSuggestionToOrganisation = viewModel.SoftwareRecommended
            };

            var response = await _selfPortalService.RecommendSoftwareAsync(softwareDTO);
            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }
        }

        private async Task<IActionResult> ApproveInternal(int organizationFormId)
        {
            var viewModel = new ApproveOrganizationConsentViewModel { OrganizationFormId = organizationFormId };
            var orgDetails = new SelfServiceOrganizationDTO
            {
                OrgOnboardingFormsId = viewModel.OrganizationFormId,
                AdminUgpassEmail = viewModel.AdminUgpassEmail,
                CreatedBy = UUID
            };

            var response = await _selfPortalService.ApproveOrganizationAsync(orgDetails);
            if (!response.Success)
            {
                return Json(new { Status = "Failed", Title = "Approve Organization", Message = response.Message });
            }
            else
            {
                return Json(new { Status = "Success", Title = "Approve Organization", Message = response.Message });
            }
        }

    }
}
