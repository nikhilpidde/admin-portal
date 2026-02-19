using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.ESealRegistration;

using DTPortal.Core.DTOs;
using DTPortal.Core.Enums;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using Google.Api.Gax.ResourceNames;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Math;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Core.Services;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Organizations")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    [Route("Organizations")]
    public class ESealRegistrationController : BaseController
    {
        private readonly IOrganizationService _organizationService;
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly DataExportService _dataExportService;
        private readonly IMakerCheckerService _makerCheckerService;
        private readonly ISubscriberService _subscriberService;
        private readonly IClientService _clientService;
        private readonly IConfiguration _configuration;
        private readonly IAllPaymentHistoryService _allPaymentHistoryService;
        private readonly ILogger<ESealRegistrationController> _logger;
        private IWebHostEnvironment _environment;
        private readonly IPrivilegeRequestService _privilegeRequestService;

        public ESealRegistrationController(IOrganizationService organizationService,
        IRazorRendererHelper razorRendererHelper,
        DataExportService dataExportService,
        IMakerCheckerService makerCheckerService,
        ISubscriberService subscriberService,
        IClientService clientService,
        IConfiguration configuration,
        ILogger<ESealRegistrationController> logger,
        IWebHostEnvironment environment,
        IAllPaymentHistoryService allPaymentHistoryService,
        IPrivilegeRequestService privilegeRequestService,
        ILogClient logClient) : base(logClient)
        {
            _organizationService = organizationService;
            _razorRendererHelper = razorRendererHelper;
            _dataExportService = dataExportService;
            _makerCheckerService = makerCheckerService;
            _subscriberService = subscriberService;
            _clientService = clientService;
            _configuration = configuration;
            _logger = logger;
            _allPaymentHistoryService = allPaymentHistoryService;
            _environment = environment;
            _privilegeRequestService = privilegeRequestService;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Add()
        {
            try
            {
                string logMessage;

                var signatureTemplates = await _organizationService.GetSignatureTemplateListAsyn();
                if (signatureTemplates == null)
                {
                    logMessage = $"Failed to get signature templates";
                    SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                        "Get Signature Templates", LogMessageType.FAILURE.ToString(), logMessage);
                    _logger.LogError(logMessage);
                    return NotFound();
                }
                _logger.LogError("Get Signature Templates found " + signatureTemplates.Count);
                ESealRegistrationAddViewModel viewModel = new ESealRegistrationAddViewModel
                {
                    TemplateList = signatureTemplates
                };
                _logger.LogError(viewModel.TemplateList.ToString());
                return PartialView("_Add", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString(), " : Error in ESealRegistrationController Add method");
                return NotFound();
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Edit(string name) 
        {
            string logMessage;

            var organizationDetails = await _organizationService.GetOrganizationDetailsAsync(name);
            if (organizationDetails == null)
            {
                logMessage = $"Failed to get organization details with name {name}";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Organization details", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }
            else if (!organizationDetails.IsDetailsAvailable)
            {
                // Push the log to Admin Log Server
                logMessage = $"Organization details with name {name} is not found";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Organization details", LogMessageType.FAILURE.ToString(), logMessage);

                //return Json(new { Status = "Failed", Title = "Organization details", Message = "Organization details not found" });
                return Json(null);
            }

            var templateList = await _organizationService.GetSignatureTemplateListAsyn();
            if (templateList == null)
            {
                logMessage = $"Failed to get signature templates";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Signature Templates", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }


            //new code=========

            //bool IsPaymentDetailsPresent = false;
            //var response = await _allPaymentHistoryService.GetOrganizationPaymentHistoryAsync(organizationDetails.OrganizationUid);
            //if (response != null && response.Success)
            //{
            //    // Push the log to Admin Log Server
            //    IsPaymentDetailsPresent = true;
            //}

            //==================

            //Eseal-Certificate Status===

            //var esealCertificateStatus = await _organizationService.GetEsealCertificateStatus(organizationDetails.OrganizationUid);
            //if (esealCertificateStatus == null)
            //{
            //    logMessage = $"Failed to get Eseal certificate status";
            //    SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
            //        "Get Eseal Certificate Status", LogMessageType.FAILURE.ToString(), logMessage);

            //    return NotFound();

            //}
          
           


            //==================

            string[] address = organizationDetails.CorporateOfficeAddress.Split(';');
            ESealRegistrationEditViewModel viewModel = new ESealRegistrationEditViewModel
            {
                OrganizationId = organizationDetails.OrganizationId,
                OrganizationUID = organizationDetails.OrganizationUid,
                OrganizationName = organizationDetails.OrganizationName,
                OrganizationEmail = organizationDetails.OrganizationEmail,
                EmailDomain = organizationDetails.EmailDomain,
                UniqueRegdNo = organizationDetails.UniqueRegdNo,
                TaxNo = organizationDetails.TaxNo,
                CorporateOfficeAddress1 = address[0],
                CorporateOfficeAddress2 = address[1],
                Country = address[2],
                Pincode = address[3],
                SignatoryEmailList = organizationDetails.SubscriberEmailList,
                DirectorsEmailList = organizationDetails.DirectorsEmailList,
                Status = organizationDetails.Status,
                //ESealImageBase64 = organizationDetails.ESealImage,
                ResizedEsealImage = organizationDetails.ESealImage,
                AuthorizedLetterForSignatoriesBase64 = organizationDetails.AuthorizedLetterForSignatories,
                IncorporationBase64 = organizationDetails.Incorporation,
                TaxBase64 = organizationDetails.Tax,
                AdditionalLegalDocumentBase64 = organizationDetails.OtherLegalDocument,
                SpocUgpassEmail = organizationDetails.SpocUgpassEmail,
                AgentUrl = organizationDetails.AgentUrl,
                EnablePostPaidOption = organizationDetails.EnablePostPaidOption,
                SignatureTemplate = organizationDetails.TemplateId[0],
                ESealTemplate = organizationDetails.TemplateId[1],
                TemplateList = templateList,
                CreatedBy = organizationDetails.CreatedBy,
                //PaymentRecordPresent = IsPaymentDetailsPresent,
                walletCertificateStatus= organizationDetails.walletCertificateStatus
                //certificateStatus = esealCertificateStatus.certificateStatus,
                //certificateEndDate= esealCertificateStatus.certificateEndDate,
                //certificateStartDate= esealCertificateStatus.certificateStartDate
            };
            //if (esealCertificateStatus.Success)
            //{
            //    var esealStatus = JsonConvert.DeserializeObject<EsealCertificateStatusDTO>(esealCertificateStatus.Resource.ToString());
            //    viewModel.certificateStatus = esealStatus.certificateStatus;
            //    viewModel.certificateEndDate = esealStatus.certificateEndDate;
            //    viewModel.certificateStartDate = esealStatus.certificateStartDate;
            //}
            //else
            //{
            //    viewModel.certificateStatus = esealCertificateStatus.Message;
            //}

            viewModel.OrganizationUsersList = GetOrganizationUsers(organizationDetails.OrgUserList).ToList();

            foreach (var user in viewModel.OrganizationUsersList)
            {
                user.Digitalforms = user.DigitalFormPrivilege;
            }

            viewModel.DocumentListCheckbox = GetCheckedDocuments(viewModel.DocumentListCheckbox, organizationDetails.DocumentListCheckbox);

            logMessage = $"Successfully received organization details with name {name}";
            SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                "Get Organization details", LogMessageType.SUCCESS.ToString(), logMessage);

            //return Json(new { Status = "Success", Title = "Organization details", Message = "Organization details found", Object = PartialView("_edit", viewModel) });
            return PartialView("_Edit", viewModel);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetOrganizationById(string organizationUid)
        {
            string logMessage;

            var organizationDetails = await _organizationService.GetOrganizationDetailsByUId(organizationUid);
            if (organizationDetails == null)
            {
                logMessage = $"Failed to get organization details ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Organization details", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }
            else if (!organizationDetails.IsDetailsAvailable)
            {
                // Push the log to Admin Log Server
                logMessage = $"Organization details not found";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Organization details", LogMessageType.FAILURE.ToString(), logMessage);

                //return Json(new { Status = "Failed", Title = "Organization details", Message = "Organization details not found" });
                return Json(null);
            }

            var templateList = await _organizationService.GetSignatureTemplateListAsyn();
            if (templateList == null)
            {
                logMessage = $"Failed to get signature templates";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Signature Templates", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }


            //new code=========

            bool IsPaymentDetailsPresent = false;
            var response = await _allPaymentHistoryService.GetOrganizationPaymentHistoryAsync(organizationUid);
            if (response != null && response.Success)
            {
                // Push the log to Admin Log Server
                IsPaymentDetailsPresent = true;
            }

            //==================

            //Eseal-Certificate Status===

            var esealCertificateStatus = await _organizationService.GetEsealCertificateStatus(organizationUid);
            if (esealCertificateStatus == null)
            {
                logMessage = $"Failed to get Eseal certificate status";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Eseal Certificate Status", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();

            }


            //==================

            string[] address = organizationDetails.CorporateOfficeAddress.Split(';');
            ESealRegistrationEditViewModel viewModel = new ESealRegistrationEditViewModel
            {
                OrganizationId = organizationDetails.OrganizationId,
                OrganizationUID = organizationDetails.OrganizationUid,
                OrganizationName = organizationDetails.OrganizationName,
                OrganizationEmail = organizationDetails.OrganizationEmail,
                EmailDomain = organizationDetails.EmailDomain,
                UniqueRegdNo = organizationDetails.UniqueRegdNo,
                TaxNo = organizationDetails.TaxNo,
                CorporateOfficeAddress1 = address[0],
                CorporateOfficeAddress2 = address[1],
                Country = address[2],
                Pincode = address[3],
                SignatoryEmailList = organizationDetails.SubscriberEmailList,
                DirectorsEmailList = organizationDetails.DirectorsEmailList,
                Status = organizationDetails.Status,
                //ESealImageBase64 = organizationDetails.ESealImage,
                ResizedEsealImage = organizationDetails.ESealImage,
                AuthorizedLetterForSignatoriesBase64 = organizationDetails.AuthorizedLetterForSignatories,
                IncorporationBase64 = organizationDetails.Incorporation,
                TaxBase64 = organizationDetails.Tax,
                AdditionalLegalDocumentBase64 = organizationDetails.OtherLegalDocument,
                SpocUgpassEmail = organizationDetails.SpocUgpassEmail,
                AgentUrl = organizationDetails.AgentUrl,
                EnablePostPaidOption = organizationDetails.EnablePostPaidOption,
                SignatureTemplate = organizationDetails.TemplateId[0],
                ESealTemplate = organizationDetails.TemplateId[1],
                TemplateList = templateList,
                CreatedBy = organizationDetails.CreatedBy,
                PaymentRecordPresent = IsPaymentDetailsPresent,
                //certificateStatus = esealCertificateStatus.certificateStatus,
                //certificateEndDate= esealCertificateStatus.certificateEndDate,
                //certificateStartDate= esealCertificateStatus.certificateStartDate
            };
            if (esealCertificateStatus.Success)
            {
                var esealStatus = JsonConvert.DeserializeObject<EsealCertificateStatusDTO>(esealCertificateStatus.Resource.ToString());
                viewModel.certificateStatus = esealStatus.certificateStatus;
                viewModel.certificateEndDate = esealStatus.certificateEndDate;
                viewModel.certificateStartDate = esealStatus.certificateStartDate;
            }
            else
            {
                viewModel.certificateStatus = esealCertificateStatus.Message;
            }


            viewModel.OrganizationUsersList = GetOrganizationUsers(organizationDetails.OrgUserList).ToList();
            viewModel.DocumentListCheckbox = GetCheckedDocuments(viewModel.DocumentListCheckbox, organizationDetails.DocumentListCheckbox);

            logMessage = $"Successfully received organization details";
            SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                "Get Organization details", LogMessageType.SUCCESS.ToString(), logMessage);

            //return Json(new { Status = "Success", Title = "Organization details", Message = "Organization details found", Object = PartialView("_edit", viewModel) });
            return PartialView("_Edit", viewModel);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<string[]> GetOrganizations(string value)
        {
            var organizationList = await _organizationService.GetOrganizationNamesAysnc(value);
            if (organizationList == null)
            {
                return null;
            }

            return organizationList;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<string[]> GetActiveSubscribersEmailList(string value)
        {
            var activeSubscribersEmailList = await _organizationService.GetActiveSubscribersEmailListAsync(value);
            if (activeSubscribersEmailList == null)
            {
                return null;
            }
            return activeSubscribersEmailList;
        }

        [HttpGet]
        [Route("[action]")]
        public PartialViewResult RevokeCertificate(string organizationUID, string organizationName)
        {
            RevokeCertificateViewModel viewModel = new RevokeCertificateViewModel
            {
                OrganizationUID = organizationUID,
                OrganizationName = organizationName
            };

            return PartialView("_RevokeCertificate", viewModel);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<bool> IsOrganizationExists(string organizationName) 
        {
            return !(await _organizationService.IsOrganizationExists(organizationName));
        }

        [HttpGet]
        [Route("[action]")]
        public PartialViewResult UploadSignedDocument(DocumentVerificationViewModel viewModel)
        {
            DocumentVerificationViewModel documentVerificationViewModel = new DocumentVerificationViewModel
            {
                OrganizationUID = viewModel.OrganizationUID,
                OrganizationName = viewModel.OrganizationName,
                TransactionId = viewModel.TransactionId,
                EnablePostPaidOption = viewModel.EnablePostPaidOption,
                ESealActionMethod = viewModel.ESealActionMethod,
                Emails = viewModel.Emails
            };

            return PartialView("_UploadSignedDocument", documentVerificationViewModel);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RegisteredClients(string organizationUid)
        {
            var clients = await _clientService.ListKycClientByOrgUidAsync(organizationUid);
            //var clients = await _clientService.ListClientByOrgUidAsync(organizationUid);
            if (clients == null)
            {
                return NotFound();
            }

            return PartialView("_RegisteredClients", clients);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult AddStakeHoldersCSV()
        {
            return PartialView("_AddStakeholdersCSV");
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult AddStakeHolder()
        {
            return PartialView("_AddStakeHolder");
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> AddStakeHolder([FromBody] IList<StakeholderViewModel> stakeholders)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Add StakeHoldder", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }
            var tempData = TempData["OrganizationUID"].ToString();
            TempData.Keep("OrganizationUID");

            // Create a list to store all the StakeholderDTO objects
            IList<StakeholderDTO> stakeholderDtoList = new List<StakeholderDTO>();

            foreach (var viewModel in stakeholders)
            {
                StakeholderDTO stakeholder = new StakeholderDTO
                {
                    name = viewModel.name,
                    spocUgpassEmail = viewModel.spocUgpassEmail,
                    referenceId = viewModel.referenceId,
                    organizationUid = viewModel.organizationUid,
                    referredBy = tempData,
                };

                stakeholderDtoList.Add(stakeholder); // Add the current stakeholder to the list
            }

            // Send the entire list to the service method
            var response = await _organizationService.AddStakeHolder(stakeholderDtoList);

            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to Create Stakeholder";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Create Stakeholder", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Add StakeHolder", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully Created Stakeholder";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Create Stakeholder", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Add StakeHolder", Message = response.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> Add([FromForm] ESealRegistrationAddViewModel viewModel)
        {
            string logMessage;
            bool makerCheckerEnabled = false;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Add Organization", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var activity = (await _makerCheckerService.GetAllActivities()).FirstOrDefault(x => x.Id == 41);
            if (activity != null && activity.McEnabled)
            {
                makerCheckerEnabled = true;
            }

            //if (viewModel.OrganizationUsersList == null || viewModel.OrganizationUsersList.Count == 0)
            //{
            //    return Json(new { Status = "Failed", Title = "Add Organization", Message = "Please add atleast one business user" });
            //}

            if (viewModel.OrganizationUsersList != null && viewModel.OrganizationUsersList.Any())
            {
                foreach (var user in viewModel.OrganizationUsersList)
                {
                    user.DigitalFormPrivilege = user.Digitalforms;
                }
            }

            OrganizationDTO organization = new OrganizationDTO
            {
                OrganizationName = viewModel.OrganizationName?.Trim(),
                OrganizationEmail = viewModel.OrganizationEmail,
                EmailDomain = viewModel.EmailDomain,
                UniqueRegdNo = viewModel.UniqueRegdNo,
                TaxNo = viewModel.TaxNo,
                CorporateOfficeAddress = string.Join(';', viewModel.CorporateOfficeAddress1, viewModel.CorporateOfficeAddress2, viewModel.Country, viewModel.Pincode),
                OrgUserList = viewModel.OrganizationUsersList,
                DirectorsEmailList = viewModel.DirectorsEmailList,
                SpocUgpassEmail = viewModel.SpocUgpassEmail,
                EnablePostPaidOption = viewModel.EnablePostPaidOption,
                AgentUrl = viewModel.AgentUrl,
                CreatedBy = UUID
            };

            organization.TemplateId = new List<int>();
            organization.TemplateId.Add(viewModel.SignatureTemplate);
            organization.TemplateId.Add(viewModel.ESealTemplate);

            if (makerCheckerEnabled == false)
            {
                foreach (var document in viewModel.DocumentListCheckbox)
                {
                    bool canContinue = true;
                    string errorMessage = string.Empty;
                    if (document.IsSelected)
                    {
                        switch (document.Id)
                        {
                            case 1:
                                if (viewModel.AuthorizedLetterForSignatories == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Authorized letter for signatories document is checked but not provided";
                                }
                                break;

                            case 2:
                                if (viewModel.ESealImage == null)
                                {
                                    if (viewModel.ResizedEsealImage == null)
                                    {
                                        canContinue = false;
                                        errorMessage = "E-sealImage document is checked but not provided";
                                    }
                                }
                                break;

                            case 3:
                                if (viewModel.Incorporation == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Incorporation document is checked but not provided";
                                }
                                break;

                            case 4:
                                if (viewModel.Tax == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Tax document is checked but not provided";
                                }
                                break;

                            case 5:
                                if (viewModel.AdditionalLegalDocument == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Additional legal document is checked but not provided";
                                }
                                break;
                        }

                        if (!canContinue)
                            return Json(new { Status = "Failed", Title = "Add Organization", Message = errorMessage });

                        organization.DocumentListCheckbox.Add(document.DisplayName);
                    }
                }
            }

            //if (viewModel.ESealImage != null)
            //{
            //    using (var stream = new MemoryStream())
            //    {
            //        viewModel.ESealImage.CopyTo(stream);
            //        //organization.ESealImage = $"data:image/{Path.GetExtension(viewModel.ESealImage.FileName).Replace(".", "")};base64," + Convert.ToBase64String(stream.ToArray());
            //        organization.ESealImage = Convert.ToBase64String(stream.ToArray());
            //    }
            //}
            if (viewModel.ESealImage != null)
            {  
                if (string.IsNullOrEmpty(viewModel.ResizedEsealImage))
                {
                    using (var stream = new MemoryStream())
                    {
                        viewModel.ESealImage.CopyTo(stream);
                        organization.ESealImage = Convert.ToBase64String(stream.ToArray());
                    }
                }
                else
                {
                    organization.ESealImage = viewModel.ResizedEsealImage;
                }
            }

            if (viewModel.AuthorizedLetterForSignatories != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.AuthorizedLetterForSignatories.CopyTo(stream);
                    organization.AuthorizedLetterForSignatories = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.Incorporation != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.Incorporation.CopyTo(stream);
                    organization.Incorporation = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.Tax != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.Tax.CopyTo(stream);
                    organization.Tax = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.AdditionalLegalDocument != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.AdditionalLegalDocument.CopyTo(stream);
                    organization.OtherLegalDocument = Convert.ToBase64String(stream.ToArray());
                }
            }

            var response = await _organizationService.AddOrganizationAsync(organization);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to create organization with name {viewModel.OrganizationName}";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Create Organization", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Add Organization", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for add organization with name {viewModel.OrganizationName} has sent for approval";
                else
                    logMessage = $"Successfully created organization with name {viewModel.OrganizationName}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Create Organization", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Add Organization", Message = response.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> Update([FromForm] ESealRegistrationEditViewModel viewModel)
        {
            string logMessage;
            bool makerCheckerEnabled = false;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Update Organization", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var activity = (await _makerCheckerService.GetAllActivities()).FirstOrDefault(x => x.Id == 41);
            if (activity != null && activity.McEnabled)
            {
                makerCheckerEnabled = true;
            }

            //if (viewModel.OrganizationUsersList == null || viewModel.OrganizationUsersList.Count == 0)
            //{
            //    return Json(new { Status = "Failed", Title = "Add Organization", Message = "Please add atleast one business user" });
            //}


            if (viewModel.OrganizationUsersList != null && viewModel.OrganizationUsersList.Any())
            {
                foreach (var user in viewModel.OrganizationUsersList)
                {
                    user.DigitalFormPrivilege = user.Digitalforms;
                }
            }

            
            OrganizationDTO organization = new OrganizationDTO
            {
                OrganizationId = viewModel.OrganizationId,
                OrganizationUid = viewModel.OrganizationUID,
                OrganizationName = viewModel.OrganizationName,
                OrganizationEmail = viewModel.OrganizationEmail,
                EmailDomain = viewModel.EmailDomain,
                UniqueRegdNo = viewModel.UniqueRegdNo,
                TaxNo = viewModel.TaxNo,
                CorporateOfficeAddress = string.Join(';', viewModel.CorporateOfficeAddress1, viewModel.CorporateOfficeAddress2, viewModel.Country, viewModel.Pincode),
                OrgUserList = viewModel.OrganizationUsersList,
                DirectorsEmailList = viewModel.DirectorsEmailList,
                AuthorizedLetterForSignatories = viewModel.AuthorizedLetterForSignatoriesBase64,
                ESealImage = viewModel.ResizedEsealImage,
                Incorporation = viewModel.IncorporationBase64,
                Tax = viewModel.TaxBase64,
                OtherLegalDocument = viewModel.AdditionalLegalDocumentBase64,
                SpocUgpassEmail = viewModel.SpocUgpassEmail,
                AgentUrl = viewModel.AgentUrl,
                EnablePostPaidOption = viewModel.EnablePostPaidOption,
                CreatedBy = viewModel.CreatedBy,
                Status = viewModel.Status,
                UpdatedBy = UUID
            };

            organization.TemplateId = new List<int>();
            organization.TemplateId.Add(viewModel.SignatureTemplate);
            organization.TemplateId.Add(viewModel.ESealTemplate);

            if (makerCheckerEnabled == false)
            {
                foreach (var document in viewModel.DocumentListCheckbox)
                {
                    bool canContinue = true;
                    string errorMessage = string.Empty;
                    if (document.IsSelected)
                    {
                        switch (document.Id)
                        {
                            case 1:
                                if (viewModel.AuthorizedLetterForSignatories == null && viewModel.AuthorizedLetterForSignatoriesBase64 == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Authorized letter for signatories document is checked but not provided";
                                }
                                break;

                            case 2:
                                if (viewModel.ESealImage == null && viewModel.ResizedEsealImage == null)
                                {
                                    canContinue = false;
                                    errorMessage = "E-sealImage document is checked but not provided";
                                }
                                break;

                            case 3:
                                if (viewModel.Incorporation == null && viewModel.IncorporationBase64 == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Incorporation document is checked but not provided";
                                }
                                break;

                            case 4:
                                if (viewModel.Tax == null && viewModel.TaxBase64 == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Tax document is checked but not provided";
                                }
                                break;

                            case 5:
                                if (viewModel.AdditionalLegalDocument == null && viewModel.AdditionalLegalDocumentBase64 == null)
                                {
                                    canContinue = false;
                                    errorMessage = "Additional legal document is checked but not provided";
                                }
                                break;
                        }

                        if (!canContinue)
                            return Json(new { Status = "Failed", Title = "Update Organization", Message = errorMessage });

                        organization.DocumentListCheckbox.Add(document.DisplayName);
                    }
                }
            }

            //if (viewModel.ESealImage != null)
            //{
            //    using (var stream = new MemoryStream())
            //    {
            //        viewModel.ESealImage.CopyTo(stream);
            //        organization.ESealImage = Convert.ToBase64String(stream.ToArray());
            //    }
            //}
            if (viewModel.ESealImage != null)
            {
                if (string.IsNullOrEmpty(viewModel.ResizedEsealImage))
                {
                    using (var stream = new MemoryStream())
                    {
                        viewModel.ESealImage.CopyTo(stream);
                        //organization.ESealImage = $"data:image/{Path.GetExtension(viewModel.ESealImage.FileName).Replace(".", "")};base64," + Convert.ToBase64String(stream.ToArray());
                        organization.ESealImage = Convert.ToBase64String(stream.ToArray());
                    }
                }
                else
                {
                    organization.ESealImage = viewModel.ResizedEsealImage;
                }
            }

            if (viewModel.AuthorizedLetterForSignatories != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.AuthorizedLetterForSignatories.CopyTo(stream);
                    organization.AuthorizedLetterForSignatories = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.Incorporation != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.Incorporation.CopyTo(stream);
                    organization.Incorporation = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.Tax != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.Tax.CopyTo(stream);
                    organization.Tax = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (viewModel.AdditionalLegalDocument != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.AdditionalLegalDocument.CopyTo(stream);
                    organization.OtherLegalDocument = Convert.ToBase64String(stream.ToArray());
                }
            }

            if (makerCheckerEnabled == false && viewModel.SignedPdf != null)
            {
                using (var stream = new MemoryStream())
                {
                    viewModel.SignedPdf.CopyTo(stream);
                    organization.SignedPdf = Convert.ToBase64String(stream.ToArray());
                }
            }

            var response = await _organizationService.UpdateOrganizationAsync(organization);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to update organization with name {viewModel.OrganizationName}";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Update Organization", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Update Organization", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for update organization with name {viewModel.OrganizationName} has sent for approval";
                else
                    logMessage = $"Successfully updated organization with name {viewModel.OrganizationName}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Update Organization", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Update Organization", Message = response.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> GenerateEseal(string organizationUID, string organizationName, string transactionId)
        {
            string logMessage;

            if (String.IsNullOrEmpty(organizationUID))
            {
                return Json(new { Status = "Failed", Title = "Generate E-seal", Message = "Organization UID is null or empty" });
            }

            if (String.IsNullOrEmpty(organizationName))
            {
                return Json(new { Status = "Failed", Title = "Generate E-seal", Message = "Organization Name is null or empty" });
            }

            if (String.IsNullOrEmpty(transactionId))
            {
                return Json(new { Status = "Failed", Title = "Generate E-seal", Message = "Transaction Id is null or empty" });
            }

            var response = await _organizationService.IssueCertificateAsync(organizationUID, UUID, transactionId);
            //_logger.LogInformation(response.Resource.ToString());
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to generate eseal for organization with name {organizationName} ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Generate E-seal", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Generate E-seal", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for generate eseal for organization with name {organizationName} has sent for approval";
                else
                    logMessage = $"Successfully generated eseal for organization with name {organizationName}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Generate E-seal", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Generate E-seal", Message = response.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> RevokeCertificate([FromForm] RevokeCertificateViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return Json(new { Status = "Failed", Title = "Revoke Certificate", Message = "Invalid selection" });
            }

            var response = await _organizationService.RevokeCertificateAsync(viewModel.OrganizationUID, -2, viewModel.Remarks, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to Revoke Certificate for organization with name {viewModel.OrganizationName} ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Revoke Certificate", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Revoke Certificate", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for Revoke Certificate for organization with name {viewModel.OrganizationName} has sent for approval";
                else
                    logMessage = $"Successfully Revoke Certificate for organization with name {viewModel.OrganizationName}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Revoke Certificate", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Revoke Certificate", Message = response.Message, OrganizationName = viewModel.OrganizationName });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> ValidateSignatoryEmailList(List<string> values)
        {
            var response = await _organizationService.ValidateEmailListAsync(values);
            if (response.Success)
            {
                return Json(new { Status = "Success", Title = "Email List Validation", Message = response.Message, Result = response.Resource });
            }
            return Json(new { Status = "Failed", Title = "Email List Validation", Message = response.Message, Result = response.Resource });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> VerifySignedDocument([FromForm] DocumentVerificationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Document Verification", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            string baseSignedPdf = null;
            using (var stream = new MemoryStream())
            {
                viewModel.SignedPdf.CopyTo(stream);
                baseSignedPdf = Convert.ToBase64String(stream.ToArray());
            }

            var response = await _organizationService.VerifyDocumentSignatureAsync(viewModel.OrganizationUID, UUID, DocumentType.Pades.GetValue(), baseSignedPdf, viewModel.Emails);
            if (response.Success)
            {
                return Json(new { Status = "Success", Title = "Document Verification", Message = response.Message, Result = response.Resource });
            }
            return Json(new { Status = "Failed", Title = "Document Verification", Message = response.Message, Result = response.Resource });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public JsonResult GetPDFBytes([FromBody] PDFViewModel viewModel)
        {
            var image = Path.Combine(_environment.WebRootPath, _configuration["PDFLogoPath"]);
            byte[] imageArray = System.IO.File.ReadAllBytes(image);
            string base64Image = Convert.ToBase64String(imageArray);
            viewModel.UserName = FullName;
            viewModel.PdfLogo = "data:image/png;base64, " + base64Image;

            var partialName = "/Views/eSealRegistration/PdfView.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, viewModel);
            byte[] pdfBytes = _dataExportService.GeneratePdf(htmlContent);

            return Json(new { Status = "Success", Title = "Generate PDF", Message = "Successfully Generated PDF bytes", Result = pdfBytes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> DelinkOrganizationEmployee(string employeeEmail, string organizationUID, string endDate)
        {
            string logMessage;

            if (String.IsNullOrEmpty(organizationUID))
            {
                return Json(new { Status = "Failed", Title = "Delink Email", Message = "Organization UID is null or empty" });
            }

            if (String.IsNullOrEmpty(employeeEmail))
            {
                return Json(new { Status = "Failed", Title = "Delink Email", Message = "Employee Email is null or empty" });
            }

            var delinkEmployee = new DelinkOrganizationEmployeeDTO()
            {
                EmployeeEmail = employeeEmail,
                OrganizationUID = organizationUID,
                EndDate = endDate
            };

            var response = await _organizationService.DelinkOrganizationEmployeeAsync(delinkEmployee);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to delink organization email {delinkEmployee.EmployeeEmail} ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Delink organization email", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Generate E-seal", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                //if (response.Message == "Your request has sent for approval")
                //    logMessage = $"Request for generate eseal for organization with name {organizationName} has sent for approval";
                //else
                logMessage = $"Successfully delink organization email {delinkEmployee.EmployeeEmail}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Generate E-seal", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Delink Email", Message = response.Message });
            }
        }

        [NonAction]
        private List<DocumentListItem> GetCheckedDocuments(List<DocumentListItem> documentListItems,
            List<string> checkedDocumentList = null)
        {
            if (checkedDocumentList != null)
            {
                foreach (var document in documentListItems)
                {
                    if (checkedDocumentList.Contains(document.DisplayName))
                    {
                        document.IsSelected = true;
                    }
                }
            }

            return documentListItems;
        }

        private IEnumerable<OrganizationUser> GetOrganizationUsers(IList<OrganizationUser> orgUserList)
        {
            if (orgUserList != null)
            {
                foreach (var orgUser in orgUserList)
                    yield return orgUser;
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetStakeholderList(string organizationUid)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(organizationUid))
                {
                    return Json(new { Status = "Failed", Title = "Get Stakeholder List", Message = "Failed to get Stakeholder List" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Get Stakeholder List", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var response = await _organizationService.GetStakeholdersAsync(organizationUid);
            if (response == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get Stakeholder List for organization";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Stakeholder List", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received Stakeholder List for organization";
            SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                "Get Stakeholder List", LogMessageType.SUCCESS.ToString(), logMessage);

            if (response.Success)
            {
                return PartialView("_organizationStakeholderList", JsonConvert.DeserializeObject<IEnumerable<StakeholderDTO>>(response.Resource.ToString()));
            }
            else
            {
                return Ok(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizationPaymentHistory(string organizationName, string organizationUid)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(organizationUid))
                {
                    return Json(new { Status = "Failed", Title = "Get Organization Payment History", Message = "Failed to get organization unique identifier" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Get Organization Payment History", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var response = await _allPaymentHistoryService.GetOrganizationPaymentHistoryAsync(organizationUid);
            if (response == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get payment history for organization {organizationName}";
                SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                    "Get Organization Payment History", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received payment history for organization {organizationName}";
            SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
                "Get Organization Payment History", LogMessageType.SUCCESS.ToString(), logMessage);

            if (response.Success)
            {
                return PartialView("_OrganizationPaymentStatus", JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryStatusDTO>>(response.Result.ToString()));
            }
            else
            {
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult DownloadCSV()
        {
            // Get the path to the CSV file on the server
            string csvFilePath = Path.Combine(_environment.WebRootPath, "Samples/EsealRegistration_Stakeholders.csv");

            // Check if the file exists
            if (!System.IO.File.Exists(csvFilePath))
            {
                return NotFound(); // or return appropriate error response
            }

            // Set the response content type and headers
            string contentType = "text/csv";
            string fileName = Path.GetFileName(csvFilePath);
            return PhysicalFile(csvFilePath, contentType, fileName);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult DownloadCSV_BusinessUser()
        {
            // Get the path to the CSV file on the server
            string csvFilePath = Path.Combine(_environment.WebRootPath, "Samples/SampleOrganisation_Users_new.csv");

            // Check if the file exists
            if (!System.IO.File.Exists(csvFilePath))
            {
                return NotFound(); // or return appropriate error response
            }

            // Set the response content type and headers
            string contentType = "text/csv";
            string fileName = Path.GetFileName(csvFilePath);
            return PhysicalFile(csvFilePath, contentType, fileName);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult MakeTransparentImage(EsealImageDTO esealImageDTO)
        {
            APIResponse response = new APIResponse();
            var base64 =  ImageProcesser.MakeImageTransparent(esealImageDTO.base64);
            if (base64 == null)
            {
                response.Success= false;
                return Ok(response);
            }
            else
            {
                response.Success= true;
                response.Result = base64;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetVendorsList(string organizationUid)
        {
            string logMessage;
            var vendorListViewModel = new List<VendorListViewModel>();
            if (!ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(organizationUid))
                {
                    return Json(new { Status = "Failed", Title = "Get Vendors List", Message = "Failed to get Vendors List" });
                }

                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Get Vendors List", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }

            var response = await _organizationService.GetVendorsAsync(organizationUid);
            if (response == null || !response.Success)
            {
                logMessage = $"Failed to get Vendors List for organization";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Get Vendors List", LogMessageType.FAILURE.ToString(), logMessage);

                return NotFound();
            }
            var vendorsList = (List<VendorListDTO>)response.Resource;

            logMessage = $"Successfully received Vendors List for organization";
            SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                "Get Vendors List", LogMessageType.SUCCESS.ToString(), logMessage);
            foreach (var vendor in vendorsList)
            {
                var vendorModel = new VendorListViewModel()
                {
                    vendorId = vendor.SponsorExternalId,
                    organizationName = vendor.BeneficiaryName,
                    status = vendor.Status
                };
                vendorListViewModel.Add(vendorModel);
            }
            if (response.Success)
            {
                return PartialView("_organizationVendorsList", vendorListViewModel);
            }
            else
            {
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> AddVendor()
        {
            var addVendorModel = new AddVendorViewModel();
            var organizationList = await _organizationService.GetOrganizationNamesAndIdAysnc();

            if (organizationList == null)
            {
                return NotFound();
            }

            var list = new List<SelectListItem>();

            if (organizationList == null)
            {
                return NotFound();
            }

            foreach (var item in organizationList)
            {
                var org = item.Split(',');

                list.Add(new SelectListItem { Text = org[0], Value = org[1] });
            }

            addVendorModel.organizationList = list;
            return PartialView("_AddVendor", addVendorModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> AddVendor([FromBody] AddVendorViewModel Vendor)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                var keys = from item in ModelState
                           where item.Value.Errors.Count > 0
                           select item.Key;
                return Json(new { Status = "Failed", Title = "Add StakeHoldder", Message = $"{keys.FirstOrDefault()} : {errors.FirstOrDefault().ErrorMessage}" });
            }
            var tempData = TempData["OrganizationUID"].ToString();
            TempData.Keep("OrganizationUID");

            var OrganizationName = TempData["OrganizationName"].ToString();
            TempData.Keep("OrganizationName");
            var addVendor = new AddVendorDTO
            {
                sponsorExternalId = Vendor.vendorId,
                sponsorName = OrganizationName,
                beneficiaryDigitalId = Vendor.orgId,
                sponsorDigitalId = tempData,
                beneficiaryName = Vendor.orgName,
                sponsorType = "ORGANIZATION",
                beneficiaryType = "ORGANIZATION",
                signaturePhoto = "",
                designation = ""
            };
            var beneficiary = new BeneficiaryValidity()
            {
                privilegeServiceId = 1,
                validityApplicable = false,
                validFrom = null,
                validUpTo = null,
                status = "ACTIVE"
            };
            var beneficiary2 = new BeneficiaryValidity()
            {
                privilegeServiceId = 2,
                validityApplicable = false,
                validFrom = null,
                validUpTo = null,
                status = "ACTIVE"
            };
            addVendor.beneficiaryValidities = new List<BeneficiaryValidity>();
            addVendor.beneficiaryValidities.Add(beneficiary);
            addVendor.beneficiaryValidities.Add(beneficiary2);
            var response = await _organizationService.AddVendor(addVendor);

            if (!response.Success)
            {
                logMessage = $"Failed to Create Vendor";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Create Vendor", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Add Vendor", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully Created Vendor";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Create Vendor", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Add Vendor", Message = response.Message });
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<JsonResult> VerifyVendor(string orgId, string vendorId)
        {
            var response = await _organizationService.VerifyVendor(orgId, vendorId);
            if (response.Success)
            {
                return Json(new { Status = "Success", Title = "Verify Vendor", Message = response.Message });
            }
            else
            {
                return Json(new { Status = "Failed", Title = "Verify Vendor", Message = response.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<JsonResult> GenerateWallet(string organizationUID, string organizationName, string transactionId)
        {
            string logMessage;

            if (String.IsNullOrEmpty(organizationUID))
            {
                return Json(new { Status = "Failed", Title = "Generate Wallet", Message = "Organization UID is null or empty" });
            }

            if (String.IsNullOrEmpty(organizationName))
            {
                return Json(new { Status = "Failed", Title = "Generate Wallet", Message = "Organization Name is null or empty" });
            }

            //if (String.IsNullOrEmpty(transactionId))
            //{
            //    return Json(new { Status = "Failed", Title = "Generate Wallet", Message = "Transaction Id is null or empty" });
            //}

            var response = await _organizationService.IssueWalletCertificateAsync(organizationUID, UUID, transactionId);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to generate Wallet for organization with name {organizationName} ";
                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                    "Generate E-seal", LogMessageType.FAILURE.ToString(), logMessage);

                return Json(new { Status = "Failed", Title = "Generate Wallet", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for generate Wallet for organization with name {organizationName} has sent for approval";
                else
                    logMessage = $"Successfully generated Wallet for organization with name {organizationName}";

                SendAdminLog(ModuleNameConstants.Organizations, ServiceNameConstants.Organization,
                  "Generate E-seal", LogMessageType.SUCCESS.ToString(), logMessage);

                return Json(new { Status = "Success", Title = "Generate Wallet", Message = response.Message });
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetWalletDataHistory(string organizationName, string organizationUid) 
        {
            if (string.IsNullOrEmpty(organizationUid))
            {
                return Json(new
                {
                    Status = "Failed",
                    Title = "Get Wallet Payment History",
                    Message = "Organization unique identifier is missing."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new
                {
                    Status = "Failed",
                    Title = "Get Wallet Payment History",
                    Message = string.Join(", ", errors)
                });
            }

            var response = await _allPaymentHistoryService.GetWalletHistoryAsync(organizationUid);
            Console.WriteLine(response.Message);
            if (response == null)
            {
                var logMessage = $"Failed to get payment history for organization {organizationName}.";
                SendAdminLog(
                    ModuleNameConstants.PriceModel,
                    ServiceNameConstants.WalletPaymentHistory,
                    "Get Wallet Payment History",
                    LogMessageType.FAILURE.ToString(),
                    logMessage
                );

                return NotFound(new
                {
                    Status = "Failed",
                    Title = "Get Wallet Payment History",
                    Message = "Payment history not found."
                });
            }

            var successLogMessage = $"Successfully retrieved payment history for organization {organizationName}.";
            SendAdminLog(
                ModuleNameConstants.PriceModel,
                ServiceNameConstants.WalletPaymentHistory,
                "Get Organization Payment History",
                LogMessageType.SUCCESS.ToString(),
                successLogMessage
            );

            if (response.Success)
            {
                var paymentHistory = JsonConvert.DeserializeObject<IEnumerable<OrganizationPaymentHistoryStatusDTO>>(response.Result.ToString());
                return PartialView("_WalletPaymentStatus", paymentHistory);
            }
            else
            {
                return Json(new
                {
                    Status = "Failed",
                    Title = "Get Wallet Payment History",
                    // Message = response.ErrorMessage
                });
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetAllAndOrganizationPrivileges(string organizationUid)
        {
            var response = await _privilegeRequestService.GetAllUniquePreviligesAsync();
            if (response == null || !response.Success)
            {
                return NotFound();
            }

            var allPrivileges = (List<string>)response.Resource;


            var response2 = await _privilegeRequestService.GetPrivilegesByOrganizationIdAsync(organizationUid);
            if (response2 == null || !response2.Success)
            {
                return NotFound();
            }

            var organizationPrivileges = (OrganizationPrivilegeDTO)response2.Resource;

            // Create a list of PrivilegeSelectionViewModel
            List<PrivilegeSelectionViewModel> privilegeSelectionList = allPrivileges.Select(p => new PrivilegeSelectionViewModel
            {
                privilege = p,
                isSelected = organizationPrivileges.privileges.Any(op => op == p)
            }).ToList();

            // Wrap it with OrganizationId
            var viewModel = new PrivilegeSelectionWithOrgViewModel
            {
                organizationId = organizationUid,
                privileges = privilegeSelectionList
            };

            return PartialView("_SelectedPrivilegesView", viewModel);


        }

        //[HttpGet]
        //public IActionResult DownloadCSV()
        //{
        //    // Get the path to the CSV file on the server
        //    string csvFilePath = Path.Combine(_environment.WebRootPath, "Samples/SampleOrganisation_Users.csv");

        //    // Check if the file exists
        //    if (!System.IO.File.Exists(csvFilePath))
        //    {
        //        return NotFound(); // or return appropriate error response
        //    }

        //    // Set the response content type and headers
        //    string contentType = "text/csv";
        //    string fileName = Path.GetFileName(csvFilePath);
        //    return PhysicalFile(csvFilePath, contentType, fileName);
        //}

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdatePrivileges([FromBody] UpdatePrivilegesModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.OrganizationId) || model.SelectedPrivileges == null)
            {
                return BadRequest("Invalid data received.");
            }
            UpdateOrganizationPrivilegesDTO orgPrivilegesModel = new UpdateOrganizationPrivilegesDTO
            {
                orgId = model.OrganizationId,
                privileges = model.SelectedPrivileges,
                modifiedBy = FullName
            };
            var response = await _privilegeRequestService.UpdateOrganizationPrivilegesAsync(orgPrivilegesModel);
            if (!response.Success)
            {
                return BadRequest(new { message = "Failed to update privilege request." });
            }
            return Ok(new { message = "Privilege request approved successfully." });
        }
    }
}
