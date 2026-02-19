using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CsvHelper;
using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;

using DTPortal.Web.Enums;

using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Core.Exceptions;
using DTPortal.Core.ExtensionMethods;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using DTPortal.Web.ViewModel.Subscriber;
using DTPortal.Web.ViewModel.AdminLogReports;
using DocumentFormat.OpenXml.InkML;
using DTPortal.Web.Constants;
using Newtonsoft.Json;
using NuGet.Common;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Web.ViewModel.OnboardingFailedLogReports;
using DTPortal.Web.ViewModel.SigningFailedLogReports;
using DTPortal.Web.ViewModel.AuthenticationFailedLogReports;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;

namespace DTPortal.Web.Utilities
{
    public class DataExportService
    {
        private readonly ILogReportService _logReportService;
        private readonly IEmailSender _emailSender;
        private readonly IClientService _clientService;
        private readonly IRazorRendererHelper _razorRendererHelper;
        private readonly IConverter _converter;
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly ILogger<DataExportService> _logger;
        private const string FOLDER_PATH = "Reports";

        public DataExportService(ILogReportService logReportService,
            IEmailSender emailSender,
            IClientService clientService,
            IConverter converter,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<DataExportService> logger,
            IRazorRendererHelper razorRendererHelper)
        {
            _logReportService = logReportService;
            _emailSender = emailSender;
            _clientService = clientService;
            _converter = converter;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
            _razorRendererHelper = razorRendererHelper;
        }

        public async Task ExportAdminReportToFile(string exportType, string name, string email, string startDate, string endDate,
            string userName = null, string moduleName = null, int totalCount = 0)
        {
            _logger.LogInformation("Admin Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("_Id"),
                                        new DataColumn("User Name"),
                                        new DataColumn("Module Name"),
                                        new DataColumn("Service Name"),
                                        new DataColumn("Activity Name"),
                                        new DataColumn("Log Message"),
                                        new DataColumn("Status"),
                                        new DataColumn("Data Transformation"),
                                        new DataColumn("Time Stamp"),
                                         new DataColumn("Integrity"),
                                        });

            var logReports = await _logReportService.GetAdminLogReportAsync(startDate,
                endDate,
                userName,
                moduleName,
                perPage: totalCount);
            if (logReports == null)
            {
                _logger.LogError("Failed to export admin reports");

                return;
            }

            foreach (var report in logReports)
            {
                report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;
                dt.Rows.Add(report._id, report.UserName, report.ModuleName,
                    report.ServiceName, report.ActivityName, report.LogMessage, report.LogMessageType,
                    report.DataTransformation, report.Timestamp, report.IsChecksumValid ? "Valid" : "Invalid");
            }

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    AdminLogReportsPdfViewModel viewModel = new AdminLogReportsPdfViewModel();
                    viewModel.AdminLogReports = logReports;
                    var partialName = "/Views/AdminLogReports/AdminLogReportsPdfView.cshtml";
                    fileName = await ExportToPdf(viewModel,partialName);
                    break;
            }

            _logger.LogInformation($"Successfully exported admin reports");

            _logger.LogInformation("Sending email...");

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download the admin report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, "Admin report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation("Admin Report Export end");
        }

        public async Task ExportOnboardingFailedReportToFile(string exportType, string name, string email, string startDate, string endDate,
            string documentNumber, int totalCount = 0)
        {
            _logger.LogInformation("Onboarding Failed Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("Document Number"),
                                        new DataColumn("Subscriber Type"),
                                        new DataColumn("Onboarding Method"),
                                        new DataColumn("Message"),
                                        new DataColumn("Timestamp")
                                        });

            var logReports = await _logReportService.GetOnboardingFailedLogReportAsync(startDate,
                endDate,
                documentNumber,
                perPage: totalCount);
            if (logReports == null)
            {
                _logger.LogError("Failed to export admin reports");

                return;
            }

            foreach (var report in logReports)
            {
                var logMessageArray = report.LogMessage.Split('|');
                if (logMessageArray.Length > 0)
                {
                    report.DocumentNumber = logMessageArray[0];
                    report.SubscriberType = logMessageArray[1];
                    report.OnboardingMethod = logMessageArray[2];
                    report.Message = logMessageArray[3];
                }

                report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;

                dt.Rows.Add(report.DocumentNumber, report.SubscriberType,
                    report.OnboardingMethod, report.Message, report.Timestamp);
            }

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    OnboardingFailedLogReportPdfViewModel viewModel = new OnboardingFailedLogReportPdfViewModel();
                    viewModel.OnboardingFailedLogReports = logReports;
                    var partialName = "/Views/OnboardingFailedLogReports/OnboardingFailedReportPdfView.cshtml";
                    fileName = await ExportToPdf(viewModel, partialName);
                    break;
            }

            _logger.LogInformation($"Successfully exported admin reports");

            _logger.LogInformation("Sending email...");

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download the onboarding failed report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, "Onboarding failed report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation("Onboarding Failed Report Export end");
        }

        public async Task ExportSigningFailedReportToFile(string exportType, string name, string email, string startDate, string endDate,
			 int totalCount = 0)
        {
            _logger.LogInformation("Signing Failed Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("Operation"),
                                        new DataColumn("Application Name"),
                                        new DataColumn("Message"),
                                        new DataColumn("Timestamp")
                                        });

            var logReports = await _logReportService.GetSigningFailedLogReportAsync(startDate,
                endDate,
				perPage: totalCount);
            if (logReports == null)
            {
                _logger.LogError("Failed to export signing reports");

                return;
            }

            var clients = await _clientService.EnumClientIds();

            if (clients != null)
            {
                foreach (var logReport in logReports)
                {
                    // Get Service Provider name by client id
                    if (logReport.ServiceProviderAppName != null)
                        logReport.ServiceProviderAppName
                            = clients.TryGetValue(logReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                    else
                        logReport.ServiceProviderAppName = "N/A";
                }
            }
            else
            {
                foreach (var logReport in logReports)
                {
                    logReport.ServiceProviderAppName = "N/A";
                }
            }

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    SigningFailedLogReportsPdfViewModel viewModel = new SigningFailedLogReportsPdfViewModel();
                    viewModel.SigningFailedLogReports = logReports;
                    var partialName = "/Views/SigningFailedLogReports/SigningFailedReportsPdfView.cshtml";
                    fileName = await ExportToPdf(viewModel, partialName);
                    break;
            }

            _logger.LogInformation($"Successfully exported Signing reports");

            _logger.LogInformation("Sending email...");

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download the signing failed report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, "signing failed report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation("Signing Failed Report Export end");
        }

        public async Task ExportAuthenticationFailedReportToFile(string exportType, string name, string email, string startDate, string endDate,
			int totalCount = 0)
        {
            _logger.LogInformation("Authentication Failed Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("Operation"),
                                        new DataColumn("Application Name"),
                                        new DataColumn("Message"),
                                        new DataColumn("Timestamp")
                                        });

            var logReports = await _logReportService.GetAuthenticationFailedLogReportAsync(startDate,
                endDate,

                perPage: totalCount);
            if (logReports == null)
            {
                _logger.LogError("Failed to export Authentication reports");

                return;
            }

            var clients = await _clientService.EnumClientIds();

            if (clients != null)
            {
                foreach (var logReport in logReports)
                {
                    // Get Service Provider name by client id
                    if (logReport.ServiceProviderAppName != null)
                        logReport.ServiceProviderAppName
                            = clients.TryGetValue(logReport.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                    else
                        logReport.ServiceProviderAppName = "N/A";
                }
            }
            else
            {
                foreach (var logReport in logReports)
                {
                    logReport.ServiceProviderAppName = "N/A";
                }
            }

            //foreach (var report in logReports)
            //{
            //    var logMessageArray = report.LogMessage.Split('|');
            //    if (logMessageArray.Length > 0)
            //    {
            //        //report.DocumentNumber = logMessageArray[0];
            //        report.TransactionType = logMessageArray[0];
            //        report.ServiceName = logMessageArray[1];
            //        report.LogMessage = logMessageArray[2];
            //    }

            //    report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;

            //    dt.Rows.Add(report.TransactionType,
            //        report.ServiceName, report.LogMessage, report.Timestamp);
            //}

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    AuthenticationFailedLogPdfViewModel viewModel = new AuthenticationFailedLogPdfViewModel();
                    viewModel.AuthenticationFailedLogReports = logReports;
                    var partialName = "/Views/AuthenticationFailedLogReports/AuthenticationFailedReportsPdfView.cshtml";
                    fileName = await ExportToPdf(viewModel, partialName);
                    break;
            }

            _logger.LogInformation($"Successfully exported Authentication reports");

            _logger.LogInformation("Sending email...");

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download the Authentication failed report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, "Authentication failed report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation("Authentication Failed Report Export end");
        }


        public async Task ExportSubscriberReportToFile(string exportType, string subscriberFullName, TransactionType transactionType, int searchType, string searchValue, string name, string email, string startDate, string endDate,
            string identifier, string transactionStatus, int totalCount = 0)
        {
            _logger.LogInformation($"Subscriber {transactionType.GetDisplayName()} Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("_Id"),
                                        new DataColumn("Subscriber Identifier"),
                                        new DataColumn("Transaction ID"),
                                        new DataColumn("Service Name"),
                                        new DataColumn("Start Time"),
                                        new DataColumn("End Time"),
                                        new DataColumn("Log Message"),
                                        new DataColumn("Status"),
                                        new DataColumn("Service Provider Name"),
                                        new DataColumn("Service Provider AppName"),
                                        new DataColumn("Integrity"),
                                        new DataColumn("__V")
                                        });

            PaginatedList<LogReportDTO> logReports = null;
            if (transactionType == TransactionType.Authentication)
            {
                logReports = await _logReportService.GetSubscriberSpecificDigitalAuthenticationLogReportAsync(
                   startDate,
                   endDate,
                   identifier,
                   transactionStatus,
                   perPage: totalCount);
            }
            //else if (transactionType == TransactionType.Signing)
            //{
            //    logReports = await _logReportService.GetSubscriberSpecificSigningLogReportAsync(
            //    startDate,
            //    endDate,
            //    identifier,
            //    transactionStatus,
            //    perPage: totalCount);
            //}

            if (logReports == null)
            {
                // Push the log to Admin Log Server
                if (transactionType == TransactionType.Authentication)
                {
                    _logger.LogError($"Failed to export signing reports for subscriber with {((SubscriberIdentifier)searchType).GetDisplayName()} { searchValue}");
                }
                //else if (transactionType == TransactionType.Signing)
                //{
                //    _logger.LogError($"Failed to export digital authentication reports for subscriber with {((SubscriberIdentifier)searchType).GetDisplayName()} { searchValue}");
                //}

                return;
            }

            var clients = await _clientService.EnumClientIds();
            if (clients != null)
            {
                foreach (var report in logReports)
                {
                    report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;

                    // Get Service Provider name by client id
                    if (report.ServiceProviderAppName != null)
                        report.ServiceProviderAppName
                            = clients.TryGetValue(report.ServiceProviderAppName, out var outServiceProviderAppName) ? outServiceProviderAppName : "N/A";
                    else
                        report.ServiceProviderAppName = "N/A";

                    dt.Rows.Add(report._id, report.Identifier, report.TransactionID, report.ServiceName,
                        report.StartTime, report.EndTime, report.LogMessage, report.LogMessageType,
                        report.ServiceProviderName, report.ServiceProviderAppName, report.IsChecksumValid ? "Valid" : "Invalid", report.__v);
                }
            }
            else
            {
                foreach (var report in logReports)
                {
                    report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;
                    report.ServiceProviderAppName = "N/A";
                    dt.Rows.Add(report._id, report.Identifier, report.TransactionID, report.ServiceName,
                           report.StartTime, report.EndTime, report.LogMessage, report.LogMessageType,
                           report.ServiceProviderName, report.ServiceProviderAppName, report.IsChecksumValid ? "Valid" : "Invalid", report.__v);
                }
            }

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    fileName = await ExportLog(logReports, transactionType, subscriberFullName);
                    break;
            }

            if (transactionType == TransactionType.Authentication)
            {
                _logger.LogInformation($"Successfully exported digital authentication reports for subscriber with {((SubscriberIdentifier)searchType).GetDisplayName()} { searchValue}");
            }
            //else if (transactionType == TransactionType.Signing)
            //{
            //    _logger.LogInformation($"Successfully exported signing reports for subscriber with {((SubscriberIdentifier)searchType).GetDisplayName()} { searchValue}");
            //}

            _logger.LogInformation("Sending email...");

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download {transactionType.GetDisplayName()} report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, $"Subscriber {transactionType.GetDisplayName()} report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation($"Subscriber {transactionType.GetDisplayName()} Report Export end");
        }
        public string ExportBeneficiariesToFile(IList<BeneficiaryAddDTO> beneficiaryList)
        {
            string digitalSignature;
            string signatureValidity;
            string signatureValidFrom;
            string signatureValidTo;
            string userSubscription;
            string subscriptionValidity;
            string subscriptionValidFrom;
            string subscriptionValidTo;

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("Email"),
                                       // new DataColumn("National Id"),
                                        new DataColumn("NIN"),
                                        new DataColumn("Passport"),
                                        //new DataColumn("Mobile Number"),
                                         new DataColumn("Mobile_Number"),
                                        //new DataColumn("Digital Signature"),


                                         new DataColumn("Signature_Permission"),
                                         new DataColumn("User_Annual_Subscription_Permission"),


                                       // new DataColumn("Signature Validity"),
                                       new DataColumn("Signature_Validity_Required"),
                                       new DataColumn("User_Annual_Subscription_Validity_Required"),


                                        new DataColumn("Signature_Valid_From"),
                                        new DataColumn("Signature_Valid_Upto"),
                                        //new DataColumn("User Subscription"),
                                        //new DataColumn("Subscription Validity"),
                                        new DataColumn("Subscription_Valid_From"),
                                        new DataColumn("Subscription_Valid_Upto")
                                        });

            foreach(var item in  beneficiaryList)
            {
                
                var service = item.beneficiaryValidities.FirstOrDefault(v => v.privilegeServiceId == 1);
                if(service != null)
                {
                    digitalSignature = "1";
                    signatureValidity = service.validityApplicable ? "1":"0";
                    
                    signatureValidFrom = service.validFrom.HasValue ? service.validFrom.Value.ToString("yyyy-MM-dd") : "";
                    signatureValidTo= service.validUpTo.HasValue ? service.validUpTo.Value.ToString("yyyy-MM-dd") : "";
                }
                else
                {
                    digitalSignature = "0";
                    signatureValidity = "0";
                    signatureValidFrom = "";
                    signatureValidTo = "";
                }

                var service1 = item.beneficiaryValidities.FirstOrDefault(v => v.privilegeServiceId == 3);
                if (service1 != null)
                {
                    userSubscription = "1";
                    subscriptionValidity = service1.validityApplicable ? "1" : "0";
                    subscriptionValidFrom = service1.validFrom.HasValue ? service.validFrom.Value.ToString("yyyy-MM-dd") : "";
                    subscriptionValidTo = service1.validUpTo.HasValue ? service.validUpTo.Value.ToString("yyyy-MM-dd") : "";
                }
                else
                {
                    userSubscription = "0";
                    subscriptionValidity = "0";
                    subscriptionValidFrom = "";
                    subscriptionValidTo = "";
                }
                dt.Rows.Add(item.beneficiaryUgpassEmail, item.beneficiaryNin, item.beneficiaryPassport, item.beneficiaryMobileNumber,
                    digitalSignature, userSubscription, signatureValidity, subscriptionValidity, signatureValidFrom, signatureValidTo, 
                    subscriptionValidFrom, subscriptionValidTo);
            }

            string fileName = string.Empty;
            
            fileName = ExportToCSVUsingCSVWriter(dt);
            
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            return fileName;
        }
        public async Task ExportSubscriberOnboardingReportToFile(string exportType, string identifier, string subscriberFullname, string subscriberEmail, string name, string email)
        {
            _logger.LogInformation($"Subscriber Onboarding Report Export start");

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[] {
                                        new DataColumn("Operation"),
                                        new DataColumn("Application Name"),
                                        new DataColumn("Message"),
                                        new DataColumn("Status"),
                                        new DataColumn("Integrity"),
                                        new DataColumn("Duration")
                                        });

            var certIssuanceLogs = await _logReportService.GetSubscriberSpecificCertIssuanceLogReportAsync(subscriberEmail, identifier);
            if (certIssuanceLogs == null)
            {
                _logger.LogError("Failed to get onboarding reports for subscriber with "+ subscriberEmail);
            }

            var obLogs = await _logReportService.GetSubscriberSpecificOnboardingLogReportAsync(subscriberEmail, identifier);
            if (obLogs == null)
            {
                _logger.LogError("Failed to get onboarding reports for subscriber with " + subscriberEmail);
            }

            certIssuanceLogs.AddRange(obLogs);
            CultureInfo cultures = new CultureInfo("en-US");

            foreach (var report in certIssuanceLogs)
            {
                report.IsChecksumValid = _logReportService.VerifyChecksum(report).Success;
                report.ServiceProviderAppName = "N/A";

                dt.Rows.Add(report.ServiceName,
                            report.ServiceProviderAppName,
                            report.LogMessage,
                            report.LogMessageType,
                            report.IsChecksumValid ? "Valid" : "Invalid",
                            (Convert.ToDateTime(report.StartTime, cultures)) +"-"+
                                                (Convert.ToDateTime(report.EndTime, cultures)));
            }

            string fileName = string.Empty;
            switch (exportType.ToLower())
            {
                case "excel":
                    fileName = ExportToExcel(dt);
                    break;

                case "csv":
                    fileName = ExportToCSVUsingCSVWriter(dt);
                    break;
                case "pdf":
                    LogReportsPdfViewModel viewModel = new LogReportsPdfViewModel();
                    viewModel.LogReports = certIssuanceLogs;
                    viewModel.SubscriberFullName = subscriberFullname;
                    var partialName = "/Views/Subscriber/OnboardingReportPdfView.cshtml";
                    fileName = await ExportToPdf(viewModel, partialName);
                    break;
            }

            _logger.LogInformation($"Successfully exported onboarding reports for subscriber with "+ subscriberEmail);

            string issuer = _configuration["PortalLink"];
            string encodedUrl = WebUtility.UrlEncode($"fileName={fileName}");
            string downloadLink = string.Format("{0}FileManager/Download?value={1}", issuer, encodedUrl);

            int hours = Convert.ToInt32(_configuration["ReportExpirationHours"]);
            string content = string.Format($"Hi {name},\nPlease click the below link to download subscriber onboarding report. The link is valid for {hours} hours\n{downloadLink}\n");
            Message message = new Message(new string[] { email }, $"Subscriber onboarding report download link", content);
            if (await _emailSender.SendEmail(message) != 0)
            {
                _logger.LogError("Failed to send email");
            }
            else
            {
                _logger.LogInformation("Email sent successfully");
            }

            // Delete the expired files
            DeleteExpiredFiles(Convert.ToInt32(_configuration["ReportExpirationHours"]));

            _logger.LogInformation($"Subscriber Onboarding Report Export end");
        }

        public string ExportToExcel(DataTable dataTable)
        {
            _logger.LogInformation("ExportToExcel start...");

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = string.Format(@"{0}.xlsx", Guid.NewGuid());
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dataTable);
                ws.Columns().AdjustToContents();
                using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    wb.SaveAs(stream);
                }
            }

            _logger.LogInformation("ExportToExcel end...");

            return fileName;
        }

        public string ExportToCSVUsingCSVWriter(DataTable dataTable)
        {
            _logger.LogInformation("ExportToCSVUsingCSVWriter start...");

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = string.Format(@"{0}.csv", Guid.NewGuid());
            using (StreamWriter textWriter = new StreamWriter(new FileStream(Path.Combine(path, fileName), FileMode.CreateNew)))
            {
                using (CsvWriter csv = new CsvWriter(textWriter, CultureInfo.CurrentCulture))
                {
                    // Write columns
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    // Write row values
                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (var i = 0; i < dataTable.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }

            _logger.LogInformation("ExportToCSVUsingCSVWriter end...");

            return fileName;
        }

        public string ExportToCSVUsingStringBuilder(DataTable dataTable)
        {
            _logger.LogInformation("ExportToCSVUsingStringBuilder start...");

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = string.Format(@"{0}.csv", Guid.NewGuid());

            StringBuilder data = new StringBuilder();

            //Taking the column names.
            for (int column = 0; column < dataTable.Columns.Count; column++)
            {
                //Making sure that end of the line, shoould not have comma delimiter.
                if (column == dataTable.Columns.Count - 1)
                    data.Append(dataTable.Columns[column].ColumnName.ToString().Replace(",", ";"));
                else
                    data.Append(dataTable.Columns[column].ColumnName.ToString().Replace(",", ";") + ',');
            }

            data.Append(Environment.NewLine);//New line after appending columns.

            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int column = 0; column < dataTable.Columns.Count; column++)
                {
                    ////Making sure that end of the line, shoould not have comma delimiter.
                    if (column == dataTable.Columns.Count - 1)
                        data.Append(dataTable.Rows[row][column].ToString().Replace(",", ";"));
                    else
                        data.Append(dataTable.Rows[row][column].ToString().Replace(",", ";") + ',');
                }

                //Making sure that end of the file, should not have a new line.
                if (row != dataTable.Rows.Count - 1)
                    data.Append(Environment.NewLine);
            }

            File.WriteAllText(Path.Combine(path, fileName), data.ToString());

            _logger.LogInformation("ExportToCSVUsingStringBuilder end...");

            return fileName;
        }

        public async Task<string> ExportLog(PaginatedList<LogReportDTO> logReports, TransactionType transactionType, string subscriberFullName)
        {
            var partialName = string.Empty;
            string fileName = string.Empty;

            LogReportsPdfViewModel viewModel = new LogReportsPdfViewModel();
            viewModel.LogReports = logReports;
            viewModel.SubscriberFullName = subscriberFullName;
            viewModel.TransactionType = transactionType;
            partialName = "/Views/Subscriber/LogReportPdfView.cshtml";

            fileName = await ExportToPdf(viewModel, partialName);

            //if (transactionType == TransactionType.Authentication)
            //{
            //    LogReportsPdfViewModel viewModel = new LogReportsPdfViewModel();
            //    viewModel.LogReports = logReports;
            //    partialName = "/Views/Subscriber/LogReportPdfView.cshtml";

            //    fileName = await ExportToPdf(viewModel, partialName);
            //}

            //if (transactionType == TransactionType.Signing)
            //{
            //    LogReportsPdfViewModel viewModel = new LogReportsPdfViewModel();
            //    viewModel.LogReports = logReports;
            //    partialName = "/Views/Subscriber/LogReportPdfView.cshtml";
            //    fileName = await ExportToPdf(viewModel, partialName);
            //} 

            return fileName;
        }

        public async Task<string> ExportToPdf<TModel>(TModel viewModel, string partialName)
        {
            //FileStream writeStream = null;

            string fileName = string.Format(@"{0}.pdf", Guid.NewGuid());
            var htmlContent = _razorRendererHelper.RenderPartialToString(partialName,viewModel);
            byte[] pdfBytes = GeneratePdf(htmlContent);

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);

            using(FileStream fs = File.Create(Path.Combine(path, fileName)))
            {
                await fs.WriteAsync(pdfBytes);
            }

            //writeStream = System.IO.File.Create(Path.Combine(path, fileName));
            //writeStream.WriteAsync(pdfBytes);
            //writeStream.Close();

            return fileName;
        }

        public byte[] DownloadFile(string fileName, int fileExpirationHours)
        {
            _logger.LogInformation("DownloadFile start...");

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);
            if (Directory.Exists(path))
            {
                if (fileName == null || fileName.Contains("../") || fileName.Contains(@"..\"))
                {
                    throw new ArgumentException("Invalid file path");
                }
                string filePath = $"{path}//{fileName}";
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    int differenceHours = (int)(DateTime.Now - fileInfo.CreationTime).TotalHours;
                    if (differenceHours > fileExpirationHours)
                    {
                        _logger.LogError($"File {fileName} has expired");
                        throw new NotFoundException("File doesn't exists");
                    }

                    _logger.LogInformation("DownloadFile end...");

                    return File.ReadAllBytes(filePath);
                }

                _logger.LogError($"File {fileName} not found");
                throw new NotFoundException("File not found");
            }

            _logger.LogError($"Directory {path} not found");
            throw new NotFoundException("Directory not found");
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 18, Bottom = 18 },
                DPI = 300
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                //HeaderSettings = { FontSize = 10, Right = "Page [page] of [toPage]", Line = true },
                //FooterSettings = { FontSize = 8, Center = "PDF demo from JeminPro", Line = true },
            };

            var htmlToPdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };

            return _converter.Convert(htmlToPdfDocument);
        }

        private void DeleteExpiredFiles(int time)
        {
            _logger.LogInformation("DeleteExpiredFiles start...");

            string path = Path.Combine(_environment.WebRootPath, FOLDER_PATH);
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        DateTime creationTime = fileInfo.CreationTime;
                        if (creationTime < DateTime.Now.AddHours(-time))
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }

            _logger.LogInformation("DeleteExpiredFiles end...");
        }
    }
}
