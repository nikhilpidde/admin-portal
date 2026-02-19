using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Core;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel.KycDashboard;
using Google.Apis.Logging;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class KycDashboardController : BaseController
    {
        private readonly IIdValidationService _idValidatonService;
        private readonly IClientService _clientService;
        private readonly IOrganizationService _organizationService;
        private readonly IKycDevicesService _kycDevicesService;
        private readonly ILogger<KycDashboardController> _logger;
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly DataExportService _dataExportService;
        public KycDashboardController(IIdValidationService idValidatonService, IClientService clientService,
            IOrganizationService organizationService, IKycDevicesService kycDevicesService,
            ILogger<KycDashboardController>logger, ILogClient logClient,
            IRazorRendererHelper razorRendererHelper, DataExportService dataExportService) : base(logClient)
        {
            _idValidatonService = idValidatonService;
            _clientService = clientService;
            _organizationService = organizationService;
            _kycDevicesService = kycDevicesService;
            _logger = logger;
            _razorRendererHelper = razorRendererHelper;
            _dataExportService = dataExportService;
        }

        public async Task<IActionResult> Index()
        {
            var kycSummary = await _idValidatonService.GetIdValidationSummaryAsync();

            var kycDevicesCountResult = await _kycDevicesService.GetAllKycDevicesCount();

            if (kycDevicesCountResult?.Success == true && kycDevicesCountResult.Resource is int count)
            {
                kycSummary.TotalKycDevices = count;
            }
            else
            {
                kycSummary.TotalKycDevices = 0;
            }


            var reports = await _idValidatonService.GetIdValidationLogReportAsync("", "", "", null, "", "", 1);

            if (reports == null || reports.Count == 0)
            {
                //return Json(new
                //{
                //    totalCount = 0,
                //    records = new List<IdValidationResponseDTO>()
                //});

                var viewModel1 = new IdValidationDashboardModal
                {
                    Reports = new List<IdValidationResponseDTO>(),
                    KycSummary = kycSummary
                };

                return View(viewModel1);
            }

            var orgDictionary = await _organizationService.GetOrganizationsDictionary();
            var clientDictionary = await _clientService.GetApplicationsDictionary();

            List<IdValidationResponseDTO> res = new();

            foreach (var report in reports)
            {
                report.orgName = !string.IsNullOrWhiteSpace(report.orgName) && orgDictionary?.ContainsKey(report.orgName) == true
                    ? orgDictionary[report.orgName]
                    : "NA";

                report.applicationName = !string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary?.ContainsKey(report.applicationName) == true
                    ? clientDictionary[report.applicationName]
                    : "NA";

                res.Add(report);
            }
            

            var viewModel = new IdValidationDashboardModal
            {
                Reports = res,
                KycSummary = kycSummary
            };

            return View(viewModel);
        }

        [HttpGet("GetDashboardDataAsync")]
        public async Task<IActionResult> GetDashboardDataAsync()
        {
            var kycSummary = await _idValidatonService.GetIdValidationSummaryAsync();
            var kycdevicesCount = await _kycDevicesService.GetAllKycDevicesCount();
            kycSummary.TotalKycDevices = (int)(kycdevicesCount?.Resource ?? 0);

            var reports = await _idValidatonService.GetIdValidationLogReportAsync("", "", "", null, "", "", 1);

            var orgDictionary = await _organizationService.GetOrganizationsDictionary();
            var clientDictionary = await _clientService.GetApplicationsDictionary();

            List<IdValidationResponseDTO> res = new();

            foreach (var report in reports ?? new List<IdValidationResponseDTO>())
            {
                report.orgName = !string.IsNullOrWhiteSpace(report.orgName) && orgDictionary?.ContainsKey(report.orgName) == true
                    ? orgDictionary[report.orgName]
                    : "NA";

                report.applicationName = !string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary?.ContainsKey(report.applicationName) == true
                    ? clientDictionary[report.applicationName]
                    : "NA";

                res.Add(report);
            }

            return Json(new
            {
                summary = new
                {
                    TotalFailedKycs = kycSummary?.TotalFailedKycs ?? 0,
                    TotalSuccessfulKycsThisMonth = kycSummary?.TotalSuccessfulKycsThisMonth ?? 0,
                    TotalSuccessfulKycs = kycSummary?.TotalSuccessfulKycs ?? 0,
                    TotalServiceProviders = kycSummary?.TotalServiceProviders ?? 0,
                    NewServiceProvidersThisMonth = kycSummary?.NewServiceProvidersThisMonth ?? 0,
                    TotalKycDevices = kycSummary?.TotalKycDevices ?? 0
                },
                records = res
            });
        }


        [HttpGet("serviceProvider")]
        public IActionResult serviceProvider()
        {
            return PartialView("_serviceProvider");
        }


        [HttpPost("GetServiceProvidersList")]
        public async Task<IActionResult> GetServiceProvidersList()
        {

            var data = await _idValidatonService.GetAllServiceProvidersDetails();
            return Json(data);
            //var imageUrl = Url.Content("~/images/dtLogo.png");
            //return Json(new
            //{
            //    recordsTotal = 25,
            //    recordsFiltered = 25,
            //    data = new[]
            //    {
            //        new { OrganizationName = "FinSecure Corp", SegmentOrMinistry = "Finance", LogoUrl = imageUrl, TotalKYC = 1240, KYCThisMonth = 120, KYCMethods = new[] { "Video KYC", "OTP KYC", "AI-based KYC" }, RegistrationId = "REG-FIN-0001", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "BlockTrust Ltd", SegmentOrMinistry = "Blockchain", LogoUrl = imageUrl, TotalKYC = 980, KYCThisMonth = 85, KYCMethods = new[] { "Video KYC", "Biometric", "OTP KYC", "Document Upload", "Face Match" }, RegistrationId = "REG-BLK-0002", LastKYCDate = "2025-06-28", Status = "Inactive" },
            //        new { OrganizationName = "TrustNet Global", SegmentOrMinistry = "Telecom", LogoUrl = imageUrl, TotalKYC = 675, KYCThisMonth = 64, KYCMethods = new[] { "In-Person", "OTP KYC" }, RegistrationId = "REG-TEL-0003", LastKYCDate = "2025-06-27", Status = "Registered" },
            //        new { OrganizationName = "CoreVerify Solutions", SegmentOrMinistry = "Govt Services", LogoUrl = imageUrl, TotalKYC = 1420, KYCThisMonth = 134, KYCMethods = new[] { "Biometric", "OTP KYC", "Face Match", "Liveness Check", "AI-based KYC", "Aadhaar eKYC" }, RegistrationId = "REG-GOV-0004", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "IDMatrix Systems", SegmentOrMinistry = "Banking", LogoUrl = imageUrl, TotalKYC = 800, KYCThisMonth = 77, KYCMethods = new[] { "OTP KYC", "Video KYC" }, RegistrationId = "REG-BNK-0005", LastKYCDate = "2025-06-29", Status = "Inactive" },
            //        new { OrganizationName = "Digital Identity Works", SegmentOrMinistry = "IT Services", LogoUrl = imageUrl, TotalKYC = 400, KYCThisMonth = 35, KYCMethods = new[] { "Biometric", "Video KYC", "Liveness Detection", "Selfie Upload", "Document OCR" }, RegistrationId = "REG-IT-0006", LastKYCDate = "2025-06-26", Status = "Registered" },
            //        new { OrganizationName = "KYCFlow Inc", SegmentOrMinistry = "eCommerce", LogoUrl = imageUrl, TotalKYC = 2150, KYCThisMonth = 210, KYCMethods = new[] { "OTP KYC" }, RegistrationId = "REG-ECM-0007", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "ChainVerify", SegmentOrMinistry = "Insurance", LogoUrl = imageUrl, TotalKYC = 1100, KYCThisMonth = 98, KYCMethods = new[] { "OTP KYC", "In-Person", "Video KYC" }, RegistrationId = "REG-INS-0008", LastKYCDate = "2025-06-29", Status = "Inactive" },
            //        new { OrganizationName = "VerifyPrime", SegmentOrMinistry = "FinTech", LogoUrl = imageUrl, TotalKYC = 3050, KYCThisMonth = 300, KYCMethods = new[] { "Biometric", "AI-based KYC", "Video KYC", "Liveness Detection", "Geo-tag Verification", "OTP KYC" }, RegistrationId = "REG-FIN-0009", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "IdentiSure", SegmentOrMinistry = "Healthcare", LogoUrl = imageUrl, TotalKYC = 620, KYCThisMonth = 55, KYCMethods = new[] { "OTP KYC" }, RegistrationId = "REG-HLT-0010", LastKYCDate = "2025-06-27", Status = "Registered" },
            //        new { OrganizationName = "NeoID Systems", SegmentOrMinistry = "Smart City", LogoUrl = imageUrl, TotalKYC = 1500, KYCThisMonth = 200, KYCMethods = new[] { "Video KYC", "Biometric", "Face Match", "OTP KYC", "Selfie Upload", "Document Verification" }, RegistrationId = "REG-SMT-0011", LastKYCDate = "2025-06-25", Status = "Active" },
            //        new { OrganizationName = "eKYC Now", SegmentOrMinistry = "Travel", LogoUrl = imageUrl, TotalKYC = 730, KYCThisMonth = 65, KYCMethods = new[] { "OTP KYC", "Document Upload" }, RegistrationId = "REG-TRV-0012", LastKYCDate = "2025-06-29", Status = "Inactive" },
            //        new { OrganizationName = "ProofID Global", SegmentOrMinistry = "Education", LogoUrl = imageUrl, TotalKYC = 890, KYCThisMonth = 90, KYCMethods = new[] { "OTP KYC", "Biometric" }, RegistrationId = "REG-EDU-0013", LastKYCDate = "2025-06-28", Status = "Registered" },
            //        new { OrganizationName = "TrustBridge Tech", SegmentOrMinistry = "Defense", LogoUrl = imageUrl, TotalKYC = 320, KYCThisMonth = 30, KYCMethods = new[] { "AI-based KYC", "Document Verification", "OTP KYC", "Video KYC", "Geo-tag Check" }, RegistrationId = "REG-DEF-0014", LastKYCDate = "2025-06-26", Status = "Active" },
            //        new { OrganizationName = "SecureScan Pvt Ltd", SegmentOrMinistry = "Transport", LogoUrl = imageUrl, TotalKYC = 410, KYCThisMonth = 50, KYCMethods = new[] { "OTP KYC", "Selfie Match" }, RegistrationId = "REG-TRN-0015", LastKYCDate = "2025-06-25", Status = "Inactive" },
            //        new { OrganizationName = "BioKYC Systems", SegmentOrMinistry = "HealthTech", LogoUrl = imageUrl, TotalKYC = 520, KYCThisMonth = 45, KYCMethods = new[] { "Biometric", "Liveness Detection", "OTP KYC", "Aadhaar eKYC", "Video KYC", "Voice Match" }, RegistrationId = "REG-HLT-0016", LastKYCDate = "2025-06-30", Status = "Registered" },
            //        new { OrganizationName = "IDLogic Technologies", SegmentOrMinistry = "Legal", LogoUrl = imageUrl, TotalKYC = 760, KYCThisMonth = 85, KYCMethods = new[] { "Document Upload", "OTP KYC" }, RegistrationId = "REG-LGL-0017", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "DocuProof Services", SegmentOrMinistry = "Legal", LogoUrl = imageUrl, TotalKYC = 980, KYCThisMonth = 100, KYCMethods = new[] { "Video KYC", "Biometric", "Voice Match", "Geo-tag Check", "OTP KYC" }, RegistrationId = "REG-LGL-0018", LastKYCDate = "2025-06-29", Status = "Inactive" },
            //        new { OrganizationName = "TrueTrust Co", SegmentOrMinistry = "Education", LogoUrl = imageUrl, TotalKYC = 670, KYCThisMonth = 40, KYCMethods = new[] { "OTP KYC", "Video KYC" }, RegistrationId = "REG-EDU-0019", LastKYCDate = "2025-06-28", Status = "Registered" },
            //        new { OrganizationName = "SecureIdent India", SegmentOrMinistry = "e-Governance", LogoUrl = imageUrl, TotalKYC = 1350, KYCThisMonth = 155, KYCMethods = new[] { "Video KYC", "Biometric", "OTP KYC", "Document Scan", "Aadhaar eKYC" }, RegistrationId = "REG-EGV-0020", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "TrustX Solutions", SegmentOrMinistry = "FinTech", LogoUrl = imageUrl, TotalKYC = 1450, KYCThisMonth = 120, KYCMethods = new[] { "OTP KYC", "Face Recognition", "Voice Verification" }, RegistrationId = "REG-FIN-0021", LastKYCDate = "2025-06-29", Status = "Inactive" },
            //        new { OrganizationName = "ProofWise", SegmentOrMinistry = "Telecom", LogoUrl = imageUrl, TotalKYC = 1050, KYCThisMonth = 110, KYCMethods = new[] { "Video KYC", "AI-based KYC", "Selfie Upload", "OTP KYC", "Geo-tag Check" }, RegistrationId = "REG-TEL-0022", LastKYCDate = "2025-06-30", Status = "Active" },
            //        new { OrganizationName = "VeriMark", SegmentOrMinistry = "Retail", LogoUrl = imageUrl, TotalKYC = 890, KYCThisMonth = 95, KYCMethods = new[] { "OTP KYC", "In-Person" }, RegistrationId = "REG-RTL-0023", LastKYCDate = "2025-06-27", Status = "Registered" },
            //        new { OrganizationName = "IDEdge", SegmentOrMinistry = "Smart Tech", LogoUrl = imageUrl, TotalKYC = 1580, KYCThisMonth = 185, KYCMethods = new[] { "Biometric", "Voice Match", "OTP KYC", "Document OCR", "Video KYC", "Geo-fencing" }, RegistrationId = "REG-SMT-0024", LastKYCDate = "2025-06-28", Status = "Inactive" },
            //        new { OrganizationName = "IDGenX", SegmentOrMinistry = "General", LogoUrl = imageUrl, TotalKYC = 1230, KYCThisMonth = 105, KYCMethods = new[] { "OTP KYC", "Face Match" }, RegistrationId = "REG-GEN-0025", LastKYCDate = "2025-06-29", Status = "Active" }
            //    }
            //});
        }

        [HttpPost("ServiceProviderPage")]
        public async Task<IActionResult> ServiceProviderPage(string id, int page, int perpage)
        {
            List<string> services = ["CARD_STATUS_WITH_EID_READER", "CARD_STATUS_WITH_OCR", "CARD_AND_FACE_STATUS_WITH_OCR", "CARD_AND_FACE_VERIFY_WITH_OCR", "CARD_AND_FINGERPRINT_VERIFICATION_WITH_EID_READER_AND_BIOMETRIC_SENSOR", "CARD_STATUS_WITH_MANUAL_ENTRY", "CARD_AND_FACE_STATUS_REMOTE_VERIFICATION", "CARD_AND_FACE_AUTH_WITH_MANUAL_ENTRY", "CARD_AND_FACE_VERIFY_WITH_MANUAL_ENTRY", "CARD_AND_FINGERPRINT_STATUS_WITH_MANUAL_ENTRY", "PASSPORT_STATUS_WITH_MANUAL_ENTRY", "PASSPORT_STATUS_WITH_OCR", "PASSPORT_AND_FACE_VERIFY_OCR", "PASSPORT_AND_FACE_VERIFY_MANUAL_ENTRY"];
            var reports = await _idValidatonService.GetIdValidationLogReportAsync("", "", id, services, "","", page, perpage);
            var orgdata = await _idValidatonService.GetOrganizationIdValidationSummaryAsync(id);
            var kycdevicesCount = await _kycDevicesService.GetAllKycDevicesCountByOrganization(id);
            orgdata.TotalKycDevices = (int)kycdevicesCount.Resource;

            var kycDevicesList = await _kycDevicesService.GetAllKycDeviceOfOrganization(id);
            if (kycDevicesList != null && kycDevicesList.Resource is List<string> deviceList)
            {
                orgdata.KycDevices = deviceList;
            }
            else
            {
                orgdata.KycDevices = new List<string>();
            }

            if (reports == null || reports.Count == 0)
            {
                var model = new ServiceProviderPageViewModel()
                {
                    Reports = new List<IdValidationResponseDTO>(),
                    OrgData = orgdata,
                    TotalCount = 0
                };
                _logger.LogError("No reports found for organization ID: {Id}", id);
                return View(model);
            }

            var orgDictionary = await _organizationService.GetOrganizationsDictionary();
            var clientDictionary = await _clientService.GetApplicationsDictionary();

            foreach (var report in reports)
            {
                report.orgName = !string.IsNullOrWhiteSpace(report.orgName) && orgDictionary?.ContainsKey(report.orgName) == true
                    ? orgDictionary[report.orgName]
                    : "NA";

                report.applicationName = !string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary?.ContainsKey(report.applicationName) == true
                    ? clientDictionary[report.applicationName]
                    : "NA";
            }

            var viewModel = new ServiceProviderPageViewModel
            {
                Reports = reports,
                OrgData = orgdata,
                TotalCount=reports.TotalCount,
            };

            return View(viewModel);
        }


        [HttpPost("UpdateServiceProviderPage")]
        public async Task<IActionResult> UpdateServiceProviderPage(string id, int page, int perpage)
        {
            var orgdata = await _idValidatonService.GetOrganizationIdValidationSummaryAsync(id);
            var kycdevicesCount = await _kycDevicesService.GetAllKycDevicesCountByOrganization(id);
            orgdata.TotalKycDevices = (int)kycdevicesCount.Resource;

            var kycDevicesList = await _kycDevicesService.GetAllKycDeviceOfOrganization(id);
            if (kycDevicesList != null && kycDevicesList.Resource is List<string> deviceList)
            {
                orgdata.KycDevices = deviceList;
            }
            else
            {
                orgdata.KycDevices = new List<string>();
            }

            var viewModel = new ServiceProviderPageViewModel
            {
                OrgData = orgdata,
            };

            return Json(viewModel);
        }



        [HttpPost("GetPaginatedIdUsers")]
        public async Task<IActionResult> GetPaginatedIdUsers()
        {
            var pageNumber = Convert.ToInt32(Request.Form["pageNumber"].FirstOrDefault());
            var status = Request.Form["status"].FirstOrDefault();
            var fromDate = Request.Form["fromDate"].FirstOrDefault();
            var toDate = Request.Form["toDate"].FirstOrDefault();
            var searchValue = Request.Form["searchValue"].FirstOrDefault();
            var id= Request.Form["id"].FirstOrDefault();
            var perpage = 10;           

            List<string> services = ["CARD_STATUS_WITH_EID_READER", "CARD_STATUS_WITH_OCR", "CARD_AND_FACE_STATUS_WITH_OCR", "CARD_AND_FACE_VERIFY_WITH_OCR", "CARD_AND_FINGERPRINT_VERIFICATION_WITH_EID_READER_AND_BIOMETRIC_SENSOR", "CARD_STATUS_WITH_MANUAL_ENTRY", "CARD_AND_FACE_STATUS_REMOTE_VERIFICATION", "CARD_AND_FACE_AUTH_WITH_MANUAL_ENTRY", "CARD_AND_FACE_VERIFY_WITH_MANUAL_ENTRY", "CARD_AND_FINGERPRINT_STATUS_WITH_MANUAL_ENTRY", "PASSPORT_STATUS_WITH_MANUAL_ENTRY", "PASSPORT_STATUS_WITH_OCR", "PASSPORT_AND_FACE_VERIFY_OCR", "PASSPORT_AND_FACE_VERIFY_MANUAL_ENTRY"];

            var reports = await _idValidatonService.GetIdValidationLogReportAsync(searchValue, status, id, services, fromDate, toDate, pageNumber+1, perpage);

            if (reports == null || !reports.Any())
            {
                return Json(new { totalCount = 0, records = new List<IdValidationResponseDTO>() });
            }


            var orgDictionary = await _organizationService.GetOrganizationsDictionary();
            var clientDictionary = await _clientService.GetApplicationsDictionary();

            foreach (var report in reports)
            {
                report.orgName = !string.IsNullOrWhiteSpace(report.orgName) && orgDictionary?.ContainsKey(report.orgName) == true
                    ? orgDictionary[report.orgName]
                    : "NA";

                report.applicationName = !string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary?.ContainsKey(report.applicationName) == true
                    ? clientDictionary[report.applicationName]
                    : "NA";
            }

            return Json(new
            {
                totalCount = reports.TotalCount,
                data = reports 
            });


        }




        [HttpPost("GetBatchPaginatedIdUsers")]
        public async Task<IActionResult> GetBatchPaginatedIdUsers()
        {
            var pageNumber = Convert.ToInt32(Request.Form["pageNumber"].FirstOrDefault());
            var status = Request.Form["status"].FirstOrDefault();
            var fromDate = Request.Form["fromDate"].FirstOrDefault();
            var toDate = Request.Form["toDate"].FirstOrDefault();
            var searchValue = Request.Form["searchValue"].FirstOrDefault();
            var id = Request.Form["id"].FirstOrDefault();
            var perpage = 10;
            

            var reports = await _idValidatonService.GetIdValidationLogReportAsync(searchValue, status, id, ["BATCH_CARD_STATUS"], fromDate, toDate, pageNumber + 1, perpage);

            if (reports == null || !reports.Any())
            {
                return Json(new { totalCount = 0, records = new List<IdValidationResponseDTO>() });
            }


            var orgDictionary = await _organizationService.GetOrganizationsDictionary();
            var clientDictionary = await _clientService.GetApplicationsDictionary();

            foreach (var report in reports)
            {
                report.orgName = !string.IsNullOrWhiteSpace(report.orgName) && orgDictionary?.ContainsKey(report.orgName) == true
                    ? orgDictionary[report.orgName]
                    : "NA";

                report.applicationName = !string.IsNullOrWhiteSpace(report.applicationName) && clientDictionary?.ContainsKey(report.applicationName) == true
                    ? clientDictionary[report.applicationName]
                    : "NA";
            }

            return Json(new
            {
                totalCount = reports.TotalCount,
                data = reports
            });


        }




        [HttpPost("DeregisterDevice")]
        public async Task<IActionResult> DeregisterDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return BadRequest(new { success = false, message = "Device ID is required" });
            }

            var result = await _kycDevicesService.DeregisterKycDevice(deviceId);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message});
            }
            else
            {
                return BadRequest(new { success = false, message = result.Message });
            }
        }




        [HttpPost("PrintIdValidationData")]
        public IActionResult PrintIdValidationData(string SignedData, string VerificationResponse)
        {
            // Deserialize JSON back to objects
            var verificationResponse = JsonConvert.DeserializeObject<IdValidationResponseDTO>(VerificationResponse);
            var signedData = JsonConvert.DeserializeObject<VerifiedIdValidationResponseDTO>(SignedData);

            TimeZoneInfo gstZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"); // GST = UTC+4
            DateTime gstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gstZone);

            string timeStamp = gstTime.ToString("dd MMMM yyyy, HH:mm:ss") + " GST";


            //var viewModel = new PrintViewModel
            //{
            //    Name = signedData?.name,
            //    EmirateId = signedData?.idNumber,
            //    ServiceProvider = verificationResponse?.orgName,
            //    VerificationMethod = verificationResponse?.kycMethod,
            //    RequestDate = verificationResponse?.validationDateTime,
            //    ProcessingTime = "4.5 seconds",
            //    ICPReference = "ICP-202501002",
            //    VerificationResult = verificationResponse?.status,
            //    OverallConfidence = "95%",
            //    VerificationCost = "7 AED",
            //    ConsentObtained = "Yes",
            //    DataRetention = "As per UAE regulations",
            //    Status = "Signed & Verified",
            //    CertificateAuthority = "ICP Root CA",
            //    Signatory = "ID Validation Platform",
            //    TimeStamp = timeStamp,
            //    ReportId = verificationResponse?.identifier
            //};

            var viewModel = new PrintViewModel
            {
                Name = string.IsNullOrWhiteSpace(signedData?.name) ? "N/A" : signedData.name,
                EmirateId = string.IsNullOrWhiteSpace(signedData?.idNumber) ? "N/A" : signedData.idNumber,
                ServiceProvider = string.IsNullOrWhiteSpace(verificationResponse?.orgName) ? "N/A" : verificationResponse.orgName,
                VerificationMethod = string.IsNullOrWhiteSpace(verificationResponse?.kycMethod) ? "N/A" : verificationResponse.kycMethod,
                RequestDate = verificationResponse?.validationDateTime?.ToString() ?? "N/A",
                ProcessingTime = "TBD",
                ICPReference = "TBD",
                VerificationResult = string.IsNullOrWhiteSpace(verificationResponse?.status) ? "N/A" : verificationResponse.status,
                OverallConfidence = "TBD",
                VerificationCost = "TBD",
                ConsentObtained = "Yes",
                DataRetention = "As per UAE regulations",
                Status = "Signed & Verified",
                CertificateAuthority = "ICP Root CA",
                Signatory = "ID Validation Platform",
                TimeStamp = string.IsNullOrWhiteSpace(timeStamp?.ToString()) ? "N/A" : timeStamp.ToString(),
                ReportId = string.IsNullOrWhiteSpace(verificationResponse?.identifier) ? "N/A" : verificationResponse.identifier
            };



            return View("PrintIdValidationData", viewModel);
        }


        [HttpPost]
        public IActionResult PrintIdValidationReport(PrintViewModel viewModel)
        {
            // render cshtml → html string
            var partialName = "/Views/KycDashboard/PrintIdValidationReport.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, viewModel);

            // html → pdf
            var pdfBytes = _dataExportService.GeneratePdf(htmlContent);

            return File(pdfBytes, "application/pdf", $"ValidationReport_{viewModel.ReportId}.pdf");
        }

        [HttpPost]
        public IActionResult PrintIdValidationReportDirect(PrintViewModel viewModel)
        {
            var partialName = "/Views/KycDashboard/PrintIdValidationReport.cshtml";
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName, viewModel);

            // html → pdf
            var pdfBytes = _dataExportService.GeneratePdf(htmlContent);
            return File(pdfBytes, "application/pdf");
        }

    }
}
