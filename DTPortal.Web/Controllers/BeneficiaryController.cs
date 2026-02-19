//using DTPortal.Core.Domain.Services;
//using DTPortal.Core.DTOs;

//using DTPortal.Core.Services;
//using DTPortal.Web.Constants;
//using DTPortal.Web.ViewModel;
//using DTPortal.Web.ViewModel.Beneficiary;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ActionConstraints;
//using Newtonsoft.Json;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using DTPortal.Core.Utilities;
//using Microsoft.Extensions.Hosting;
//using System.Globalization;
//using System.IO;
//using Microsoft.Extensions.Logging;
//using Microsoft.AspNetCore.Hosting;
//using DTPortal.Web.Utilities;
//using Microsoft.Extensions.Configuration;
//using DTPortal.Web.Enums;

//namespace DTPortal.Web.Controllers
//{
//    [Authorize(Roles = "Beneficiaries")]
//    public class BeneficiaryController : BaseController
//    {
//        //private readonly IBeneficiaryService _beneficiaryService;

//        //public BeneficiaryController(IBeneficiaryService beneficiaryService)
//        //{
//        //    beneficiaryService = _beneficiaryService;
//        //}


//        private readonly IRazorRendererHelper _razorRendererHelper;
//        private readonly DataExportService _dataExportService;
//        private readonly IMakerCheckerService _makerCheckerService;
//        private readonly ISubscriberService _subscriberService;
//        private readonly IClientService _clientService;
//        private readonly IConfiguration _configuration;
//        private readonly IAllPaymentHistoryService _allPaymentHistoryService;


//        private readonly IOrganizationService _organizationService;
//        private readonly IBeneficiaryService _beneficiaryService;
//        private readonly ILogger<BeneficiaryController> _logger;
//        private readonly IWebHostEnvironment _environment;
//        public BeneficiaryController(ILogClient logClient,
//            IOrganizationService organizationService,
//            ILogger<BeneficiaryController> logger,
//            IConfiguration configuration,
//            IWebHostEnvironment environment,
//             IAllPaymentHistoryService allPaymentHistoryService,
//            IBeneficiaryService beneficiaryService): base(logClient)
//        {
//            _organizationService = organizationService;
//            _beneficiaryService = beneficiaryService;
//            _logger =   logger;
//            _configuration = configuration;
//            _environment = environment;
//            _allPaymentHistoryService = allPaymentHistoryService;
//        }
//        public IActionResult Index(string orgName)
//        {
//            BeneficiaryListViewModel beneficiaryListViewModel = new BeneficiaryListViewModel()
//            {
//                OrganizationName = orgName
//            };
//            return View(beneficiaryListViewModel);
//        }

//        [HttpGet]
//        [Route("[action]")]
//        public async Task<IActionResult> List(string orgName)
//        {
//            var organizationDetails = await _organizationService.GetOrganizationDetailsAsync(orgName);
//            if (organizationDetails.IsDetailsAvailable == false)
//            {
//                return Json(null);
//            }

//            if (organizationDetails == null)
//            {
//                return NotFound();
//            }

//            var response = await _beneficiaryService.GetAllBeneficiariesListByOrgIdAsync(organizationDetails.OrganizationUid);
//            var beneficiariesList = (IEnumerable<SponsorBeneficiaryDTO>)response.Resource;
//            var model = new SponsorBeneficiaryListViewModel()
//            {
//                BeneficiaryList = beneficiariesList,
//                OrgId = organizationDetails.OrganizationUid,
//                OrgName = orgName

//            };

//            return PartialView("_List", model);

//        }


//        //public async Task<IActionResult>  Add(string orgId)
//        //{
//        //    var response = await _beneficiaryService.GetBeneficiaryPrivilegesAsync();
//        //    var beneficiaries = (IEnumerable<BeneficiaryPrivilegesDTO>)response.Resource;
//        //    List<BeneficiaryPrivilegesDTO> servicePrivilages = beneficiaries.ToList();

//        //    BeneficiariesAddViewModel viewModel = new BeneficiariesAddViewModel()
//        //    {
//        //        ServicePrevilage = servicePrivilages,
//        //        OrganizationUid = orgId
//        //    };


//        //    return View(viewModel);
//        //}
//        [HttpGet]
//        [Route("[action]")]
//        public async Task<IActionResult> Addq(string orgId, string OrgName)
//        {
//            var response = await _beneficiaryService.GetBeneficiaryPrivilegesAsync();
//            var beneficiaries = (IEnumerable<BeneficiaryPrivilegeDTO>)response.Resource;
//            List<BeneficiaryPrivilegeDTO> servicePrivilages = beneficiaries.ToList();

//            BeneficiariesAddViewModel viewModel = new BeneficiariesAddViewModel()
//            {
//                orgName = OrgName,
//                Service = servicePrivilages,
//                OrganizationUid = orgId
//            };


//            return View(viewModel);
//        }


//        [HttpPost]
//        public async Task<IActionResult> AddBeneficiariesUser([FromBody] BeneficiariesAddViewModel formData)
//        {
//            string logMessage;
//            if (!ModelState.IsValid)
//            {
//                //if (String.IsNullOrEmpty(viewModel.OrgId))
//                //{
//                //    AlertViewModel alert = new AlertViewModel { Message = "Failed to get organization unique identifier" };
//                //    TempData["Alert"] = JsonConvert.SerializeObject(alert);
//                //}
//                //return View(viewModel);
//            }
//            if (formData.ServicePrevilage.Count == 0)
//            {
//                return Json(new { success = false, message = "Select atleast one Service" });
//            }
//            BeneficiaryAddDTO beneficiaryAddDTO = new BeneficiaryAddDTO()
//            {

//                beneficiaryOfficeEmail = string.IsNullOrEmpty(formData.EmployeeEmail) ? null : formData.EmployeeEmail,
//                sponsorDigitalId = formData.OrganizationUid,
//                sponsorType = "ORGANIZATION",
//                beneficiaryType = "INDIVIDUAL",
//                sponsorName=formData.orgName,
//                beneficiaryMobileNumber = string.IsNullOrEmpty(formData.MobileNumber) ? null : formData.MobileNumber,
//                beneficiaryUgpassEmail = string.IsNullOrEmpty(formData.UgpassEmail) ? null : formData.UgpassEmail,
//                beneficiaryNin = string.IsNullOrEmpty(formData.NationalIdNumber) ? null : formData.NationalIdNumber,
//                beneficiaryPassport = string.IsNullOrEmpty(formData.PassportNumber) ? null : formData.PassportNumber,
//                designation = string.IsNullOrEmpty(formData.Designation) ? null : formData.Designation,
//                beneficiaryValidities = (List<BeneficiaryValidityDto>)formData.ServicePrevilage,
//                signaturePhoto = formData.SignaturePhoto
//            };

//            var response = await _beneficiaryService.AddBeneficiaryAsync(beneficiaryAddDTO, UUID);
//            if (response==null || !response.Success)
//            {
//                // Push the log to Admin Log Server
//                logMessage = $"Failed to Benificiary for the organization ";
//                /*SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
//                    "Add Organization Payment History", LogMessageType.FAILURE.ToString(), logMessage);*/
//                //AlertViewModel aalert = new AlertViewModel { Message = response.Message };
//                //TempData["Alert"] = JsonConvert.SerializeObject(aalert);

//                //return Json(new { Status = "Failed", Title = "Add Organization Payment History", Message = response.Message });
//                //var orgName = formData.orgName;
//                //return RedirectToAction("Index", orgName);
//                //return Ok("Failed");
//                return Json(new { success = false, message = response == null ? "Internal error please contact to admin" : response.Message });
//            }
//            else
//            {
//                // Push the log to Admin Log Server
//                if (response.Message == "Your request has sent for approval")
//                    logMessage = $"Request for add payment history for organization has sent for approval";
//                else
//                    logMessage = $"Successfully added payment history for organization";

//                /*SendAdminLog(ModuleNameConstants.PriceModel, ServiceNameConstants.OrganizationPaymentHistory,
//                  "Add Organization Payment History", LogMessageType.SUCCESS.ToString(), logMessage);*/

//                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
//                TempData["Alert"] = JsonConvert.SerializeObject(alert);
//                var orgName = formData.orgName;
//                return Json(new { success = true, message = response.Message });
//                //return Ok("Success");
//                //return RedirectToAction("Index", orgName);
//                //return View("Index", new BeneficiaryListViewModel { OrganizationName = orgName });

//            }
//        }


//        public async Task<IActionResult> EditBeneficiary(int id, string orgName)
//        {
//            /*var response = await _beneficiaryService.GetBeneficiaryDetailsById(id);
//            if (response == null)
//            {
//                return NotFound();
//            }
            
//            return View(response);*/

//            var response1 = await _beneficiaryService.GetBeneficiaryPrivilegesAsync();
//            if (response1 == null || !response1.Success)
//            {
//                return NotFound();
//            }
//            var beneficiaries = (IEnumerable<BeneficiaryPrivilegeDTO>)response1.Resource;
//            List<BeneficiaryPrivilegeDTO> servicePrivilages = beneficiaries.ToList();

//            var response2 = await _beneficiaryService.GetBeneficiaryDetailsById(id);
//            if (response2 == null || !response2.Success)
//            {
//                return NotFound();
//            }

//            //for (var i =0; i)
//            var response = (BeneficiaryEditDTO)response2.Resource;

//            IList<BeneficiaryValidityDto> validityDto = response.BeneficiaryValidities;
//            List<int> validityId = new List<int>();
//            foreach (var validity in validityDto)
//            {
//                validityId.Add(validity.privilegeServiceId);
//            }
//            BeneficiaryEditViewModel beneficiariesEditViewModel = new BeneficiaryEditViewModel
//            {
//                id = id,
//                orgName = orgName,
//                OrganizationUid = response.SponsorDigitalId,
//                EmployeeEmail = response.BeneficiaryOfficeEmail,
//                Designation = response.Designation,
//                SignaturePhoto = response.SignaturePhoto,
//                UgpassEmail = response.BeneficiaryUgPassEmail,
//                PassportNumber = response.BeneficiaryPassport,
//                NationalIdNumber = response.BeneficiaryNin,
//                MobileNumber = response.BeneficiaryMobileNumber,
//                ConsentAcquired = response.beneficiaryConsentAcquired == "true" ? true : false,
//                SponsorType = "ORGANIZATION",
//                BeneficiaryType = "INDIVIDUAL",
//                ServicePrevilage = response.BeneficiaryValidities,
//                Service = servicePrivilages,
//                ServiceIds = validityId,
//                CountryCode = null,
//                SponsorExternalId=response.SponsorExternalId,
//                BeneficiaryDigitalId=response.BeneficiaryDigitalId,
//                BeneficiaryName = response.BeneficiaryName,
//                SponsorDigitalId=response.SponsorDigitalId
//            };
//            var mobileCountryCode = _configuration.GetValue<string>("CountryCode");
//            if (!string.IsNullOrEmpty(response.BeneficiaryMobileNumber))
//            {

//                if (response.BeneficiaryMobileNumber.StartsWith(mobileCountryCode))
//                {
//                    string[] mobileParts = new string[2];
//                    mobileParts[0] = mobileCountryCode;
//                    mobileParts[1] = response.BeneficiaryMobileNumber.Substring(4);
//                    beneficiariesEditViewModel.CountryCode = mobileParts[0];
//                    beneficiariesEditViewModel.MobileNumber = mobileParts[1];
//                }
//                else if (response.BeneficiaryMobileNumber.StartsWith("+91"))
//                {
//                    string[] mobileParts = new string[2];
//                    mobileParts[0] = "+91";
//                    mobileParts[1] = response.BeneficiaryMobileNumber.Substring(3);
//                    beneficiariesEditViewModel.CountryCode = mobileParts[0];
//                    beneficiariesEditViewModel.MobileNumber = mobileParts[1];
//                }
//                else
//                {
//                    beneficiariesEditViewModel.CountryCode ="";
//                    beneficiariesEditViewModel.MobileNumber ="";
//                }
//            }
//            return View(beneficiariesEditViewModel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UpadateBeneficiariesUser([FromBody] BeneficiaryEditViewModel beneficiariesUser)
//        {
//            string logMessage;
//            if (!ModelState.IsValid)
//            {
//                //if (String.IsNullOrEmpty(viewModel.OrgId))
//                //{
//                //    AlertViewModel alert = new AlertViewModel { Message = "Failed to get organization unique identifier" };
//                //    TempData["Alert"] = JsonConvert.SerializeObject(alert);
//                //}
//                //return View(viewModel);
//            }
//            if(beneficiariesUser.ServicePrevilage.Count == 0)
//            {
//                return Json(new { success = false, message = "Select atleast one Service" });
//            }
//            BeneficiaryUpdateDTO beneficiaryDTO = new BeneficiaryUpdateDTO()
//            {
//                Id= beneficiariesUser.id,
//                BeneficiaryOfficeEmail = string.IsNullOrEmpty(beneficiariesUser.EmployeeEmail) ? null : beneficiariesUser.EmployeeEmail,
//                SponsorDigitalId = beneficiariesUser.OrganizationUid,
//                SponsorType = "ORGANIZATION",
//                BeneficiaryType = "INDIVIDUAL",
//                SponsorName= beneficiariesUser.orgName,
//                BeneficiaryMobileNumber = string.IsNullOrEmpty(beneficiariesUser.MobileNumber) ? null : beneficiariesUser.MobileNumber,
//                BeneficiaryUgpassEmail = string.IsNullOrEmpty(beneficiariesUser.UgpassEmail) ? null : beneficiariesUser.UgpassEmail,
//                BeneficiaryNin = string.IsNullOrEmpty(beneficiariesUser.NationalIdNumber) ? null : beneficiariesUser.NationalIdNumber,
//                BeneficiaryPassport = string.IsNullOrEmpty(beneficiariesUser.PassportNumber) ? null : beneficiariesUser.PassportNumber,
//                Designation = string.IsNullOrEmpty(beneficiariesUser.Designation) ? null : beneficiariesUser.Designation,
//                BeneficiaryDigitalId = string.IsNullOrEmpty(beneficiariesUser.BeneficiaryDigitalId) ? null : beneficiariesUser.BeneficiaryDigitalId,
//                BeneficiaryName = string.IsNullOrEmpty(beneficiariesUser.BeneficiaryName) ? null : beneficiariesUser.BeneficiaryName,
//                BeneficiaryValidities = (List<BeneficiaryValidityDto>)beneficiariesUser.ServicePrevilage,
//                SponsorExternalId= string.IsNullOrEmpty(beneficiariesUser.SponsorExternalId) ? null : beneficiariesUser.SponsorExternalId,
//                SignaturePhoto = beneficiariesUser.SignaturePhoto
//            };

//            var response = await _beneficiaryService.EditBeneficiaryAsync(beneficiaryDTO, UUID);
//            if (response==null || !response.Success)
//            {
//                logMessage = $"Failed to  Benificiary for the organization ";

//                return Json(new { success = false, message = response == null ? "Internal error please contact to admin" : response.Message });

//            }
//            else
//            {
//                // Push the log to Admin Log Server
//                if (response.Message == "Your request has sent for approval")
//                    logMessage = $"Request  has sent for approval";
//                else
//                    logMessage = $"Successfully updated Beneficiary for organization";

//                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
//                TempData["Alert"] = JsonConvert.SerializeObject(alert);

//                var orgName = beneficiariesUser.orgName;
//                return Json(new { success = true, message = response.Message });
//                //return View("Index", new BeneficiaryListViewModel { OrganizationName = orgName });

//            }


//        }

//        [HttpPost]
//        public async Task<IActionResult> AddMultipleBeneficiariesUser([FromBody] SponsorBeneficiaryListViewModel SponsorBeneficiaryList)
//        {
//            string logMessage;
//            _logger.LogInformation("Add mutliple Beneficiaries " + JsonConvert.SerializeObject(SponsorBeneficiaryList));
//            AddMultipleBeneficiaries rawCsvDataDTO = new AddMultipleBeneficiaries()
//            {
//                ListA = SponsorBeneficiaryList.CsvData
//            };
//            IList<BeneficiaryAddDTO> beneficiaries = new List<BeneficiaryAddDTO>();
//            foreach (var data in rawCsvDataDTO.ListA)
//            {
//                BeneficiaryAddDTO beni = new()
//                {
//                    sponsorDigitalId = SponsorBeneficiaryList.OrgId,
//                    sponsorName = SponsorBeneficiaryList.OrgName,
//                    beneficiaryUgpassEmail = string.IsNullOrEmpty(data.UgpassEmail) ? null : data.UgpassEmail,
//                    sponsorType = "ORGANIZATION",
//                    beneficiaryType = "INDIVIDUAL",
//                    //designation = string.IsNullOrEmpty(data.Designation) ? null : data.Designation,
//                    beneficiaryMobileNumber = string.IsNullOrEmpty(data.MobNo) ? null : data.MobNo,
//                    beneficiaryPassport = string.IsNullOrEmpty(data.PassportNumber) ? null : data.PassportNumber,
//                    beneficiaryNin = string.IsNullOrEmpty(data.NIN) ? null : data.NIN,
//                    //signaturePhoto = string.IsNullOrEmpty(data.SignatureImage) ? null : data.SignatureImage,
//                    beneficiaryValidities = new()
//                };
//                if (data.Signature_Permission == "1")
//                {
//                    BeneficiaryValidityDto signatureValidity = new()
//                    {
//                        privilegeServiceId = 1,
//                        validityApplicable = data.Signature_Validity_Required == "1" ? true : false,

//                    };
//                    if (!string.IsNullOrEmpty(data.Signature_Valid_From))
//                    {
//                        if (DateTime.TryParseExact(data.Signature_Valid_From, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidFrom))
//                        {
//                            signatureValidity.validFrom = parsedValidFrom;
//                        }
//                    }

//                    if (!string.IsNullOrEmpty(data.Signature_Valid_Upto))
//                    {
//                        if (DateTime.TryParseExact(data.Signature_Valid_Upto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidUpTo))
//                        {
//                            signatureValidity.validUpTo = parsedValidUpTo;
//                        }
//                    }
//                    beni.beneficiaryValidities.Add(signatureValidity);
//                }

//                //if (data.Eseal_Permission == "1")
//                //{
//                //    BeneficiaryValidityDto esealValidity = new()
//                //    {
//                //        privilegeServiceId = 2,
//                //        validityApplicable = data.Eseal_Validity_Flag == "TRUE" ? true : false,

//                //    };
//                //    if (!string.IsNullOrEmpty(data.Eseal_Valid_From))
//                //    {
//                //        if (DateTime.TryParseExact(data.Signature_Valid_From, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidFrom))
//                //        {
//                //            esealValidity.validFrom = parsedValidFrom;
//                //        }
//                //    }

//                //    if (!string.IsNullOrEmpty(data.Eseal_Valid_Upto))
//                //    {
//                //        if (DateTime.TryParseExact(data.Signature_Valid_Upto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidUpTo))
//                //        {
//                //            esealValidity.validUpTo = parsedValidUpTo;
//                //        }
//                //    }
//                //    beni.beneficiaryValidities.Add(esealValidity);
//                //}

//                if (data.User_Annual_Subscription_Permission == "1")
//                {
//                    BeneficiaryValidityDto userValidity = new()
//                    {
//                        privilegeServiceId = 3,
//                        validityApplicable = data.User_Annual_Subscription_Validity_Required == "1" ? true : false,

//                    };
//                    if (!string.IsNullOrEmpty(data.User_Annual_Subscription_Valid_From))
//                    {
//                        if (DateTime.TryParseExact(data.User_Annual_Subscription_Valid_From, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidFrom))
//                        {
//                            userValidity.validFrom = parsedValidFrom;
//                        }
//                    }

//                    if (!string.IsNullOrEmpty(data.User_Annual_Subscription_Valid_Upto))
//                    {
//                        if (DateTime.TryParseExact(data.User_Annual_Subscription_Valid_Upto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedValidUpTo))
//                        {
//                            userValidity.validUpTo = parsedValidUpTo;
//                        }
//                    }

//                    beni.beneficiaryValidities.Add(userValidity);
//                }

//                if ((data.Signature_Permission == "0" || data.Signature_Permission == "") && (data.User_Annual_Subscription_Permission == "0" || data.User_Annual_Subscription_Permission == ""))
//                {
//                    //AlertViewModel alert = new AlertViewModel { IsSuccess = false, Message = "At least select one of the services" };
//                    //TempData["Alert"] = JsonConvert.SerializeObject(alert);
//                    return Json(new { success = false, message = "At least select one of the services" });
//                }

//                beneficiaries.Add(beni);
//            }
//            var response = await _beneficiaryService.AddMultipleBeneficiariesAsync(beneficiaries, UUID);
//            if (response == null || !response.Success)
//            {
//                logMessage = $"Failed to  Benificiary for the organization ";

//                //AlertViewModel alert = new AlertViewModel { IsSuccess = false, Message = response.Message };
//                //TempData["Alert"] = JsonConvert.SerializeObject(alert);

//                return Json(new { success = false, message = response == null ? "Internal error please contact to admin" : response.Message });

//            }
//            else
//            {
//                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
//                TempData["Alert"] = JsonConvert.SerializeObject(alert);

//                var orgName = SponsorBeneficiaryList.OrgName;
//                return Json(new { success = true, message = response.Message });
//            }
//        }
//        public IActionResult DownloadCSV()
//        {
//            string csvFilePath = Path.Combine(_environment.WebRootPath, "Samples/SampleAddMultipleBenificiariesUsingCSV.csv");
//            if (!System.IO.File.Exists(csvFilePath))
//            {
//                return NotFound(); // or return appropriate error response
//            }
//            string contentType = "text/csv";
//            string fileName = Path.GetFileName(csvFilePath);
//            return PhysicalFile(csvFilePath, contentType, fileName);
//        }









//    }
//}
