using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using DTPortal.Web.Enums;
using DTPortal.Web.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.ViewModel.Subscriber;

using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Domain.Services;
using NuGet.Common;
//using static ClosedXML.Excel.XLPredefinedFormat;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Registration Authority")]
    [Authorize(Roles = "Subscriber")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    //[Route("[controller]")]
    public class SubscriberController : BaseController
    {
        private readonly ISubscriberService _subscriberService;
        private readonly BackgroundService _backgroundService;
        private readonly IClientService _clientService;
        private readonly IConfiguration _configuration;
        private readonly ILogReportService _reportsService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SubscriberController> _logger;

        public SubscriberController(ISubscriberService subscriberService,
            BackgroundService backgroundService,
            IClientService clientService,
            IConfiguration configuration,
            ILogReportService reportsService,
            IEmailSender emailSender,
            ILogger<SubscriberController> logger,
            ILogClient logClient) : base(logClient)
        {
            _subscriberService = subscriberService;
            _backgroundService = backgroundService;
            _clientService = clientService;
            _configuration = configuration;
            _reportsService = reportsService;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]
        //[Route("Search")]
        public IActionResult SearchSubscriber()
        {
            TempData.Clear();
            return View(new SubscriberSearchViewModel());
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<string[]> GetSubscribers(int type, string value)
        {
            return await _subscriberService.GetSubscribersNamesAysnc(type, value);
        }

        [HttpGet]
        public async Task<IActionResult> Redirects(int IdentifierType, string IdentifierValue)
        {
            string logMessage;

            //change for ICP
            var subscriberDetails = await _subscriberService.GetSubscriberDetailsAsync(IdentifierType, IdentifierValue);
            SubscriberSearchViewModel viewModel = new SubscriberSearchViewModel
            {
                IdentifierValue = IdentifierValue,
                IdentifierType = (SubscriberIdentifier)IdentifierType
            };

            if (subscriberDetails == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Subscriber Details", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else if (!subscriberDetails.IsDetailsAvailable)
            {
                // Push the log to Admin Log Server
                logMessage = $"Subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue} is not found";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Subscriber Details", LogMessageType.FAILURE.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = "Subscriber not found" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }

            // Push the log to Admin Log Server
            logMessage = $"Successfully received subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
            SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                "Get Subscriber Details", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SubscriberSearchDetails"] = JsonConvert.SerializeObject(
                new
                {
                    SearchType = (int)viewModel.IdentifierType,
                    SearchValue = viewModel.IdentifierValue,
                    Email = subscriberDetails.Email
                });

            viewModel.SubscriberDetails = subscriberDetails;
            return View("SearchSubscriber", viewModel);
        }
        //[HttpGet]
        ////[Route("[action]")]
        //public async Task<PartialViewResult> RevokeCertificate(string subscriberUniqueId)
        //{
        //    string logMessage;

        //    RevokeCertificateViewModel viewModel = new RevokeCertificateViewModel
        //    {
        //        SubscriberUniqueId = subscriberUniqueId,
        //        RevokeReasons = await _subscriberService.GetAllRevokeReasonsAsync()
        //    };

        //    // Push the log to Admin Log Server
        //    logMessage = $"Successfully received revoke reasons list";
        //    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
        //        "Get Revoke Reasons List", LogMessageType.SUCCESS.GetValue(), logMessage);

        //    return PartialView("_RevokeCertificate", viewModel);
        //}

        [HttpGet]
        //[Route("[action]")]
        public PartialViewResult RevokeCertificate(string subscriberUniqueId)
        {
            RevokeCertificateViewModel viewModel = new RevokeCertificateViewModel
            {
                SubscriberUniqueId = subscriberUniqueId
            };

            return PartialView("_RevokeCertificate", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberAuthenticationReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberAuthenticationReports"] as string, definition);
            TempData.Keep("SubscriberAuthenticationReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            var digitalAuthenticationReports = await _reportsService.GetSubscriberSpecificDigitalAuthenticationLogReportAsync(
                searchDetails.StartDate,
                searchDetails.EndDate,
                searchDetails.Identifier,
                searchDetails.TransactionStatus,
                page: page);
            if (digitalAuthenticationReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully received digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                var clients = await _clientService.EnumClientIds();
                if (clients != null)
                {
                    foreach (var digitalAuthenticationReport in digitalAuthenticationReports)
                    {
                        digitalAuthenticationReport.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (digitalAuthenticationReport.ServiceProviderAppName != null)
                            digitalAuthenticationReport.ServiceProviderAppName
                                = clients.TryGetValue(digitalAuthenticationReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            digitalAuthenticationReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(digitalAuthenticationReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            digitalAuthenticationReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(digitalAuthenticationReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            digitalAuthenticationReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var digitalAuthenticationReport in digitalAuthenticationReports)
                    {
                        digitalAuthenticationReport.IsChecksumValid = true;
                        digitalAuthenticationReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(digitalAuthenticationReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            digitalAuthenticationReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(digitalAuthenticationReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            digitalAuthenticationReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                return PartialView("_AuthenticationReports", digitalAuthenticationReports);
            }
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberSigningReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSigningReports"] as string, definition);
            TempData.Keep("SubscriberSigningReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            var signingReports = await _reportsService.GetSubscriberSpecificSigningLogReportAsync(
                searchDetails.StartDate,
                searchDetails.EndDate,
                searchDetails.Identifier,
                searchDetails.TransactionStatus,
                page: page);
            if (signingReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Signing Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully received signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Signing Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                var clients = await _clientService.EnumClientIds();
                if (clients != null)
                {
                    foreach (var signingReport in signingReports)
                    {
                        signingReport.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (signingReport.ServiceProviderAppName != null)
                            signingReport.ServiceProviderAppName
                                = clients.TryGetValue(signingReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            signingReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(signingReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            signingReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(signingReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            signingReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var signingReport in signingReports)
                    {
                        signingReport.IsChecksumValid = _reportsService.VerifyChecksum(signingReport).Success;
                        signingReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(signingReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            signingReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(signingReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            signingReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }

                return PartialView("_SigningReports", signingReports);
            }
        }
        [HttpGet]
        public async Task<IActionResult> SubscriberAllReportsByPage(int page)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };
            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberAllReports"] as string, definition);
            TempData.Keep("SubscriberAllReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            var AllReports = await _reportsService.GetSubscriberSpecificAllLogReportAsync(
                searchDetails.StartDate,
                searchDetails.EndDate,
                searchDetails.Identifier,
                searchDetails.TransactionStatus,
                page: page);
            if (AllReports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get Immigartion reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Immigartion Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully received Immigartion reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Immigartion Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                var clients = await _clientService.EnumClientIds();
                if (clients != null)
                {
                    foreach (var allReport in AllReports)
                    {
                        allReport.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (allReport.ServiceProviderAppName != null)
                            allReport.ServiceProviderAppName
                                = clients.TryGetValue(allReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            allReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(allReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            allReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(allReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            allReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var allReport in AllReports)
                    {
                        allReport.IsChecksumValid = true;
                        allReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(allReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            allReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(allReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            allReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }

                return PartialView("_AllReports", AllReports);
            }
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberAuthenticationServiceReports(string correlationId)
        {
            string logMessage;

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var reports = await _reportsService.GetDigitalAuthenticationLogReportByCorrelationIDAsync(correlationId);
            if (reports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get digital authentication subtransaction reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Subtransaction Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully received digital authentication subtransaction reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Subtransaction Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                var clients = await _clientService.EnumClientIds();
                if (clients != null)
                {
                    foreach (var report in reports)
                    {
                        report.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (report.ServiceProviderAppName != null)
                            report.ServiceProviderAppName
                                = clients.TryGetValue(report.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            report.ServiceProviderAppName = "N/A";
                        try
                        {
                            // Get IST timezone
                            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                            // Convert StartTime
                            if (DateTime.TryParse(report.StartTime, out DateTime utcStart))
                            {
                                var istStart = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(utcStart, DateTimeKind.Utc),
                                    istZone);

                                report.StartTime = istStart.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            // Convert EndTime
                            if (DateTime.TryParse(report.EndTime, out DateTime utcEnd))
                            {
                                var istEnd = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(utcEnd, DateTimeKind.Utc),
                                    istZone);

                                report.EndTime = istEnd.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Datetime Conversion Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var report in reports)
                    {
                        report.IsChecksumValid = true;

                        report.ServiceProviderAppName = "N/A";
                        try
                        {
                            // Get IST timezone
                            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                            // Convert StartTime
                            if (DateTime.TryParse(report.StartTime, out DateTime utcStart))
                            {
                                var istStart = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(utcStart, DateTimeKind.Utc),
                                    istZone);

                                report.StartTime = istStart.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            // Convert EndTime
                            if (DateTime.TryParse(report.EndTime, out DateTime utcEnd))
                            {
                                var istEnd = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTime.SpecifyKind(utcEnd, DateTimeKind.Utc),
                                    istZone);

                                report.EndTime = istEnd.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Datetime Conversion Exception in log messages");
                        }
                    }
                }
                return PartialView("_SubReports", reports);
            }
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberSigningServiceReports(string correlationId)
        {
            string logMessage;

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var reports = await _reportsService.GetSigningServiceLogReportByCorrelationIDAsync(correlationId);
            if (reports == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get signing subtransaction reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Signing Subtransaction Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else
            {
                // Push the log to Admin Log Server
                logMessage = $"Successfully received signing subtransaction reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Signing Subtransaction Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                var clients = await _clientService.EnumClientIds();
                if (clients != null)
                {
                    foreach (var report in reports)
                    {
                        report.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (report.ServiceProviderAppName != null)
                            report.ServiceProviderAppName
                                = clients.TryGetValue(report.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            report.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(report.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            report.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(report.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            report.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var report in reports)
                    {
                        report.IsChecksumValid = true;
                        report.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(report.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            report.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(report.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            report.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                return PartialView("_SubReports", reports);
            }
        }

        [HttpGet]
        //[Route("[action]")]
        public async Task<IActionResult> SubscriberOboardingServiceReports(string fullname, string identifier)
        {
            string logMessage;

            if (String.IsNullOrEmpty(identifier))
            {
                return BadRequest();
            }

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };

            TempData["SubscriberFullName"] = fullname;
            TempData.Keep("SubscriberFullName");

            TempData["Identifier"] = identifier;
            TempData.Keep("Identifier");

            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var certIssuanceLogs = await _reportsService.GetSubscriberSpecificCertIssuanceLogReportAsync(subscriberSearchDetails.Email, identifier);
            if (certIssuanceLogs == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get onboarding reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Onboarding Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            var obLogs = await _reportsService.GetSubscriberSpecificOnboardingLogReportAsync(subscriberSearchDetails.Email, identifier);
            if (obLogs == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get onboarding reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Onboarding Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            certIssuanceLogs.AddRange(obLogs);

            // Push the log to Admin Log Server
            logMessage = $"Successfully received onboarding reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                "Get Onboarding Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["OnboardingLogsTotalCount"] = certIssuanceLogs.Count;
            TempData.Keep("OnboardingLogsTotalCount");

            foreach (var report in certIssuanceLogs)
            {
                report.IsChecksumValid = _reportsService.VerifyChecksum(report).Success;
                report.ServiceProviderAppName = "N/A";
            }
            return PartialView("_OnboardingReports", certIssuanceLogs);
        }

        [HttpGet]
        //[Route("[action]")]
        public PartialViewResult PlayLivelinessVideo(string subscriberUniqueId)
        {
            string logMessage;
            string baseAddress = _configuration["APIServiceLocations:LivelinessVideoBaseAddress"];
            string endPoint = $"api/get/live/video?subscriberUid={subscriberUniqueId}";
            Uri subscriberLivelinessVideoURL = new Uri($"{baseAddress}{endPoint}");
            LivelinessVideoViewModel viewModel = new LivelinessVideoViewModel
            {
                url = subscriberLivelinessVideoURL
            };

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            // Push the log to Admin Log Server
            logMessage = $"Successfully played liveliness video for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                "Play Liveliness video", LogMessageType.SUCCESS.GetValue(), logMessage);
            return PartialView("_LivelinessVideo", viewModel);
        }

        [HttpGet]
        //[Route("[action]")]
        public JsonResult ExportAuthenticationReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };

            var subscriberFullName = TempData["SubscriberFullName"].ToString();
            TempData.Keep("SubscriberFullName");

            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberAuthenticationReports"] as string, definition);
            TempData.Keep("SubscriberAuthenticationReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to export digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Export Digital Authentication Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export Digital Authentication Report", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportSubscriberReportToFile(exportType, subscriberFullName, TransactionType.Authentication, subscriberSearchDetails.SearchType, subscriberSearchDetails.SearchValue,
                fullName, email, searchDetails.StartDate, searchDetails.EndDate, searchDetails.Identifier, searchDetails.TransactionStatus, searchDetails.TotalCount);
            });

            return Json(new { Status = "Success", Title = "Export Digital Authentication Report", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        }


        [HttpGet]
        public JsonResult ExportAllReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };

            var subscriberFullName = TempData["SubscriberFullName"].ToString();
            TempData.Keep("SubscriberFullName");

            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberAllReports"] as string, definition);
            TempData.Keep("SubscriberAllReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to export All reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Export All Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export All Reports", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportSubscriberReportToFile(exportType, subscriberFullName, TransactionType.All, subscriberSearchDetails.SearchType, subscriberSearchDetails.SearchValue,
                fullName, email, searchDetails.StartDate, searchDetails.EndDate, searchDetails.Identifier, searchDetails.TransactionStatus, searchDetails.TotalCount);
            });

            return Json(new { Status = "Success", Title = "Export All Reports", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        }

        [HttpGet]
        //[Route("[action]")]
        public PartialViewResult ExportTypes()
        {
            return PartialView("_ExportTypes");
        }


        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetOrganizationDetails(string subscriberUniqueId)
        {
            string logMessage;
            var orgnizationDetails = await _subscriberService.GetOrganizationDetailsAsync(subscriberUniqueId);
            if (orgnizationDetails == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get orgnization details";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get oragnization details", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }

            //GetOrganizationDetailsAsync
            return PartialView("_OrganizaionDetails", orgnizationDetails);
        }

        [HttpPost]
        //[Route("Search")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchSubscriber([FromForm] SubscriberSearchViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            //change for ICP
            var subscriberDetails = await _subscriberService.GetSubscriberDetailsAsync((int)viewModel.IdentifierType, viewModel.IdentifierValue);

            if (subscriberDetails == null)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to get subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Subscriber Details", LogMessageType.FAILURE.GetValue(), logMessage);

                return NotFound();
            }
            else if (!subscriberDetails.IsDetailsAvailable)
            {
                // Push the log to Admin Log Server
                logMessage = $"Subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue} is not found";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Subscriber Details", LogMessageType.FAILURE.GetValue(), logMessage);

                AlertViewModel alert = new AlertViewModel { Message = "Subscriber not found" };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            //subscriberDetails.BlackListed = "NO";
            // Push the log to Admin Log Server
            logMessage = $"Successfully received subscriber details with {viewModel.IdentifierType.GetDisplayName()} {viewModel.IdentifierValue}";
            SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                "Get Subscriber Details", LogMessageType.SUCCESS.GetValue(), logMessage);

            TempData["SubscriberSearchDetails"] = JsonConvert.SerializeObject(
                new
                {
                    SearchType = (int)viewModel.IdentifierType,
                    SearchValue = viewModel.IdentifierValue,
                    Email = subscriberDetails.Email
                });

            viewModel.SubscriberDetails = subscriberDetails;
            return View(viewModel);
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RevokeCertificate([FromForm] RevokeCertificateViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return Json(new { Status = "Failed", Title = "Revoke Certificate", Message = "Please enter remarks" });
            }

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var response = await _subscriberService.RevokeCertificateAsync(viewModel.SubscriberUniqueId, -2, viewModel.Remarks, subscriberSearchDetails.SearchValue, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to revoke certificate for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Revoke Certificate", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Revoke Certificate", Message = response.Message });
            }
            else
            {
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for revoke certificate for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue} has sent for approval";
                else
                    logMessage = $"Successfully revoked certificate for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";

                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                   "Revoke Certificate", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Revoke Certificate", Message = response.Message });
            }
        }

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeregisterDevice([FromQuery] string subscriberUniqueId)
        {
            string logMessage;

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var response = await _subscriberService.DeregisterDeviceAsync(subscriberUniqueId, subscriberSearchDetails.SearchValue, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to de-register the device for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                   "Deregister Device", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Deregister Device", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for deregister certificate for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue} has sent for approval";
                else
                    logMessage = $"Successfully deregistered the device for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                   "Deregister Device", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Deregister Device", Message = response.Message });
            }
        }

        //change for ICP

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscriberReports1([FromForm] SubscriberReportsSearchViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            IList<PaginatedList<LogReportDTO>> reports =
                new List<PaginatedList<LogReportDTO>>(new PaginatedList<LogReportDTO>[Enum.GetValues(typeof(TransactionType)).Length - 1]);

            string[] dates = viewModel.DateRange.Split('-');

            var clients = await _clientService.EnumClientIds();

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            TempData["SubscriberFullName"] = viewModel.SubscriberFullname;

            if (viewModel.TransactionType == TransactionType.Authentication || viewModel.TransactionType == TransactionType.All)
            {
                var digitalAuthenticationReports = await _reportsService.GetSubscriberSpecificDigitalAuthenticationLogReportAsync(
                Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                 Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                 viewModel.Identifier,
                 viewModel.TransactionStatus);
                if (digitalAuthenticationReports == null)
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Failed to get digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                        "Get Digital Authentication Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                    return NotFound();
                }

                // Push the log to Admin Log Server
                logMessage = $"Successfully received digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                if (clients != null)
                {
                    foreach (var digitalAuthenticationReport in digitalAuthenticationReports)
                    {
                        digitalAuthenticationReport.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (digitalAuthenticationReport.ServiceProviderAppName != null)
                            digitalAuthenticationReport.ServiceProviderAppName
                                = clients.TryGetValue(digitalAuthenticationReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            digitalAuthenticationReport.ServiceProviderAppName = "N/A";
                    }
                }
                else
                {
                    foreach (var digitalAuthenticationReport in digitalAuthenticationReports)
                    {
                        digitalAuthenticationReport.IsChecksumValid = true;
                        digitalAuthenticationReport.ServiceProviderAppName = "N/A";
                    }
                }
                reports[0] = digitalAuthenticationReports;

                TempData["SubscriberAuthenticationReports"] = JsonConvert.SerializeObject(
                new
                {
                    Identifier = viewModel.Identifier,
                    TransactionType = (int)viewModel.TransactionType,
                    TransactionStatus = viewModel.TransactionStatus,
                    StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                    TotalCount = digitalAuthenticationReports.TotalCount
                });
            }

            //if (viewModel.TransactionType == TransactionType.Signing || viewModel.TransactionType == TransactionType.All)
            //{
            //    var signingReports = await _reportsService.GetSubscriberSpecificSigningLogReportAsync(
            //    Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
            //     Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
            //    viewModel.Identifier,
            //    viewModel.TransactionStatus);
            //    if (signingReports == null)
            //    {
            //        // Push the log to Admin Log Server
            //        logMessage = $"Failed to get signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            //        SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
            //            "Get Signing Reports", LogMessageType.FAILURE.GetValue(), logMessage);

            //        return NotFound();
            //    }

            //    // Push the log to Admin Log Server
            //    logMessage = $"Successfully received signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            //    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
            //        "Get Signing Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            //    if (clients != null)
            //    {
            //        foreach (var signingReport in signingReports)
            //        {
            //            signingReport.IsChecksumValid = _reportsService.VerifyChecksum(signingReport).Success;

            //            // Get Service Provider name by client id
            //            if (signingReport.ServiceProviderAppName != null)
            //                signingReport.ServiceProviderAppName
            //                    = clients.TryGetValue(signingReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
            //            else
            //                signingReport.ServiceProviderAppName = "N/A";
            //        }
            //    }
            //    else
            //    {
            //        foreach (var signingReport in signingReports)
            //        {
            //            signingReport.IsChecksumValid = _reportsService.VerifyChecksum(signingReport).Success;
            //            signingReport.ServiceProviderAppName = "N/A";
            //        }
            //    }
            //    reports[1] = signingReports;

            //    TempData["SubscriberSigningReports"] = JsonConvert.SerializeObject(
            //    new
            //    {
            //        Identifier = viewModel.Identifier,
            //        TransactionType = (int)viewModel.TransactionType,
            //        TransactionStatus = viewModel.TransactionStatus,
            //        StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
            //        EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
            //        TotalCount = signingReports.TotalCount
            //    });
            //}

            return PartialView("_SubscriberReports", reports);
        }


        //change for ICP

        [HttpPost]
        //[Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscriberReports([FromForm] SubscriberReportsSearchViewModel viewModel)
        {
            string logMessage;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            IList<PaginatedList<LogReportDTO>> reports =
                new List<PaginatedList<LogReportDTO>>(new PaginatedList<LogReportDTO>[Enum.GetValues(typeof(TransactionType)).Length]);

            string[] dates = viewModel.DateRange.Split('-');

            var clients = await _clientService.EnumClientIds();

            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            TempData["SubscriberFullName"] = viewModel.SubscriberFullname;

            if (viewModel.TransactionType == TransactionType.Authentication)
            {
                var digitalAuthenticationReports =
                    await _reportsService.GetSubscriberSpecificDigitalAuthenticationLogReportAsync(
                        Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                        Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                        viewModel.Identifier,
                        viewModel.TransactionStatus);

                if (digitalAuthenticationReports == null)
                {
                    logMessage = $"Failed to get digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";

                    SendAdminLog(ModuleNameConstants.RegistrationAuthority,
                        ServiceNameConstants.Subscriber,
                        "Get Digital Authentication Reports",
                        LogMessageType.FAILURE.GetValue(),
                        logMessage);

                    return NotFound();
                }

                // Success Log
                logMessage = $"Successfully received digital authentication reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";

                SendAdminLog(ModuleNameConstants.RegistrationAuthority,
                    ServiceNameConstants.Subscriber,
                    "Get Digital Authentication Reports",
                    LogMessageType.SUCCESS.GetValue(),
                    logMessage);

                foreach (var report in digitalAuthenticationReports)
                {
                    report.IsChecksumValid = true;

                    // Resolve Service Provider Name
                    if (clients != null &&
                        !string.IsNullOrEmpty(report.ServiceProviderAppName) &&
                        clients.TryGetValue(report.ServiceProviderAppName, out var serviceProviderName))
                    {
                        report.ServiceProviderAppName = serviceProviderName;
                    }
                    else
                    {
                        report.ServiceProviderAppName = "N/A";
                    }
                }

                reports[0] = digitalAuthenticationReports;

                TempData["SubscriberAuthenticationReports"] = JsonConvert.SerializeObject(
                    new
                    {
                        Identifier = viewModel.Identifier,
                        TransactionType = (int)viewModel.TransactionType,
                        TransactionStatus = viewModel.TransactionStatus,
                        StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                        EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                        TotalCount = digitalAuthenticationReports.TotalCount
                    });
            }


            //if (viewModel.TransactionType == TransactionType.Signing)
            //{
            //    var signingReports = await _reportsService.GetSubscriberSpecificSigningLogReportAsync(
            //    Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
            //     Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
            //    viewModel.Identifier,
            //    viewModel.TransactionStatus);
            //    if (signingReports == null)
            //    {
            //        // Push the log to Admin Log Server
            //        logMessage = $"Failed to get signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            //        SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
            //            "Get Signing Reports", LogMessageType.FAILURE.GetValue(), logMessage);

            //        return NotFound();
            //    }

            //    // Push the log to Admin Log Server
            //    logMessage = $"Successfully received signing reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
            //    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
            //        "Get Signing Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

            //    if (clients != null)
            //    {
            //        foreach (var signingReport in signingReports)
            //        {
            //            signingReport.IsChecksumValid = _reportsService.VerifyChecksum(signingReport).Success;

            //            // Get Service Provider name by client id
            //            if (signingReport.ServiceProviderAppName != null)
            //                signingReport.ServiceProviderAppName
            //                    = clients.TryGetValue(signingReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
            //            else
            //                signingReport.ServiceProviderAppName = "N/A";
            //            try
            //            {
            //                var starttime = Convert.ToDateTime(signingReport.StartTime);
            //                starttime = starttime.AddHours(-1).AddMinutes(-30);
            //                signingReport.StartTime = starttime.ToString();
            //                var endtime = Convert.ToDateTime(signingReport.EndTime);
            //                endtime = endtime.AddHours(-1).AddMinutes(-30);
            //                signingReport.EndTime = endtime.ToString();
            //            }
            //            catch (Exception ex)
            //            {
            //                _logger.LogError("Datetime Matching Exception in log messages");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        foreach (var signingReport in signingReports)
            //        {
            //            signingReport.IsChecksumValid = _reportsService.VerifyChecksum(signingReport).Success;
            //            signingReport.ServiceProviderAppName = "N/A";
            //            try
            //            {
            //                var starttime = Convert.ToDateTime(signingReport.StartTime);
            //                starttime = starttime.AddHours(-1).AddMinutes(-30);
            //                signingReport.StartTime = starttime.ToString();
            //                var endtime = Convert.ToDateTime(signingReport.EndTime);
            //                endtime = endtime.AddHours(-1).AddMinutes(-30);
            //                signingReport.EndTime = endtime.ToString();
            //            }
            //            catch (Exception ex)
            //            {
            //                _logger.LogError("Datetime Matching Exception in log messages");
            //            }
            //        }
            //    }
            //    reports[1] = signingReports;

            //    TempData["SubscriberSigningReports"] = JsonConvert.SerializeObject(
            //    new
            //    {
            //        Identifier = viewModel.Identifier,
            //        TransactionType = (int)viewModel.TransactionType,
            //        TransactionStatus = viewModel.TransactionStatus,
            //        StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
            //        EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
            //        TotalCount = signingReports.TotalCount
            //    });
            //}

            if (viewModel.TransactionType == TransactionType.Wallet)
            {
                var walletTransactionReports = await _reportsService.GetSubscriberSpecificWalletTransactionLogReportAsync(
                     Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                 Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                    viewModel.Identifier,
                    viewModel.TransactionStatus);
                if (walletTransactionReports == null)
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Failed to get wallet reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                        "Get Wallet Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                    return NotFound();
                }

                // Push the log to Admin Log Server
                logMessage = $"Successfully received wallet reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get wallet Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                if (clients != null)
                {
                    foreach (var walletTransactionReport in walletTransactionReports)
                    {
                        walletTransactionReport.IsChecksumValid = _reportsService.VerifyChecksum(walletTransactionReport).Success;

                        // Get Service Provider name by client id
                        if (walletTransactionReport.ServiceProviderAppName != null)
                            walletTransactionReport.ServiceProviderAppName
                                = clients.TryGetValue(walletTransactionReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            walletTransactionReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(walletTransactionReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            walletTransactionReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(walletTransactionReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            walletTransactionReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                else
                {
                    foreach (var walletTransactionReport in walletTransactionReports)
                    {
                        walletTransactionReport.IsChecksumValid = _reportsService.VerifyChecksum(walletTransactionReport).Success;
                        walletTransactionReport.ServiceProviderAppName = "N/A";
                        try
                        {
                            var starttime = Convert.ToDateTime(walletTransactionReport.StartTime);
                            starttime = starttime.AddHours(-1).AddMinutes(-30);
                            walletTransactionReport.StartTime = starttime.ToString();
                            var endtime = Convert.ToDateTime(walletTransactionReport.EndTime);
                            endtime = endtime.AddHours(-1).AddMinutes(-30);
                            walletTransactionReport.EndTime = endtime.ToString();
                        }
                        catch (Exception)
                        {
                            _logger.LogError("Datetime Matching Exception in log messages");
                        }
                    }
                }
                reports[2] = walletTransactionReports;

                TempData["SubscriberWalletReports"] = JsonConvert.SerializeObject(
                new
                {
                    Identifier = viewModel.Identifier,
                    TransactionType = (int)viewModel.TransactionType,
                    TransactionStatus = viewModel.TransactionStatus,
                    StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                    TotalCount = walletTransactionReports.TotalCount
                });
            }
            if (viewModel.TransactionType == TransactionType.All)
            {
                var allReports = await _reportsService.GetSubscriberSpecificAllLogReportAsync(
                Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                 Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                viewModel.Identifier,
                viewModel.TransactionStatus);
                if (allReports == null)
                {
                    // Push the log to Admin Log Server
                    logMessage = $"Failed to get all reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                    SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                        "Get All Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                    return NotFound();
                }

                // Push the log to Admin Log Server
                logMessage = $"Successfully received all reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Get All Reports", LogMessageType.SUCCESS.GetValue(), logMessage);

                if (clients != null)
                {
                    foreach (var report in allReports)
                    {
                        report.IsChecksumValid = true;

                        // Get Service Provider name by client id
                        if (report.ServiceProviderAppName != null)
                            report.ServiceProviderAppName
                                = clients.TryGetValue(report.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                        else
                            report.ServiceProviderAppName = "N/A";

                    }
                }
                else
                {
                    foreach (var report in allReports)
                    {
                        report.IsChecksumValid = true;
                        report.ServiceProviderAppName = "N/A";

                    }
                }
                reports[1] = allReports;

                TempData["SubscriberAllReports"] = JsonConvert.SerializeObject(
                new
                {
                    Identifier = viewModel.Identifier,
                    TransactionType = (int)viewModel.TransactionType,
                    TransactionStatus = viewModel.TransactionStatus,
                    StartDate = Convert.ToDateTime(dates[0].Trim()).ToString("yyyy-MM-dd 00:00:00"),
                    EndDate = Convert.ToDateTime(dates[1].Trim()).ToString("yyyy-MM-dd 23:59:59"),
                    TotalCount = allReports.TotalCount
                });
            }


            return PartialView("_SubscriberReportsNew", reports);
        }

        [HttpPost]
        public async Task<IActionResult> GetDeviceHistory(string subscriberUniqueId)
        {
            var viewModel = new List<DeviceHistoryViewModel>();

            var deviceHistoryDTO = await _subscriberService.GetDeviceHistory(subscriberUniqueId);

            if (deviceHistoryDTO == null || !deviceHistoryDTO.Success)
            {
                return NotFound();
            }

            var deviceHistory = (List<System.DateTime>)deviceHistoryDTO.Resource;

            foreach (var item in deviceHistory)
            {
                var DeviceHistoryModel = new DeviceHistoryViewModel()
                {
                    CreatedDate = item
                };
                viewModel.Add(DeviceHistoryModel);
            }

            return PartialView("_DeviceHistory", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ActivateSubscriber(string subscriberId)
        {
            string logMessage;
            var definition = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition);
            TempData.Keep("SubscriberSearchDetails");

            var response = await _subscriberService.ActivateSubscriber(subscriberId, UUID);
            if (!response.Success)
            {
                // Push the log to Admin Log Server
                //logMessage = $"Failed to Activate the Subscriber Suspended Account";
                logMessage = $"Failed to Activate the Subscriber Suspended account with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                   " Activate Suspended Account", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Activate Account", Message = response.Message });
            }
            else
            {
                // Push the log to Admin Log Server
                if (response.Message == "Your request has sent for approval")
                    logMessage = $"Request for Activating Suspended account has sent for approval";
                else

                    //logMessage = $"Successfully Activated the Suspended Account";
                    logMessage = $"Successfully Activated the Subscriber Suspended account with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                       "Activate Suspended Account", LogMessageType.SUCCESS.GetValue(), logMessage);

                return Json(new { Status = "Success", Title = "Activate Account", Message = response.Message });
            }
        }

        [HttpGet]
        //[Route("[action]")]
        public JsonResult ExportSubscriberOnboardingReports(string exportType)
        {
            string logMessage;

            var definition = new
            {
                Identifier = "",
                TransactionType = 0,
                TransactionStatus = "",
                StartDate = "",
                EndDate = "",
                TotalCount = 0
            };

            var subscriberFullName = TempData["SubscriberFullName"].ToString();
            TempData.Keep("SubscriberFullName");

            var searchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberAuthenticationReports"] as string, definition);
            TempData.Keep("SubscriberAuthenticationReports");

            var definition2 = new
            {
                SearchType = 0,
                SearchValue = "",
                Email = ""
            };
            var subscriberSearchDetails = JsonConvert.DeserializeAnonymousType(TempData["SubscriberSearchDetails"] as string, definition2);
            TempData.Keep("SubscriberSearchDetails");

            if (searchDetails.TotalCount > 1000000)
            {
                // Push the log to Admin Log Server
                logMessage = $"Failed to export subscriber onboarding reports for subscriber with {((SubscriberIdentifier)subscriberSearchDetails.SearchType).GetDisplayName()} {subscriberSearchDetails.SearchValue}";
                SendAdminLog(ModuleNameConstants.RegistrationAuthority, ServiceNameConstants.Subscriber,
                    "Export Subscriber Onboarding Reports", LogMessageType.FAILURE.GetValue(), logMessage);

                return Json(new { Status = "Failed", Title = "Export Subscriber Onboarding Reports", Message = "Cannot export more than 1 million records at a time. Please select another filter" });
            }

            string fullName = base.FullName;
            string email = base.Email;
            _backgroundService.FireAndForgetAsync<DataExportService>(async (sender) =>
            {
                await sender.ExportSubscriberOnboardingReportToFile(exportType, searchDetails.Identifier, subscriberFullName, subscriberSearchDetails.Email, fullName, email);
            });

            return Json(new { Status = "Success", Title = "Export Subscriber Onboarding Reports", Message = "Your request has been processed successfully. Please check your email to download the reports" });
        }
    }
}
