using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using DTPortal.Web.ViewModel.SMTP;

using DTPortal.Core.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using DTPortal.Web.ViewModel;
using Newtonsoft.Json;
using DTPortal.Core.Utilities;
using Newtonsoft.Json.Serialization;
using DTPortal.Web.Enums;
using System.Linq;
using System.Security.Claims;
using DTPortal.Web.Constants;
using DTPortal.Web.Attribute;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "SMTP")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class SMTPController : BaseController
    {
        private readonly ISMTPService _smtpService;

        public SMTPController(ILogClient logClient, ISMTPService smtpService) : base(logClient)
        {
            _smtpService = smtpService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var smtp = await _smtpService.GetSMTPSettingsAsync(1);
            if (smtp == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Get smtp configuration", LogMessageType.FAILURE.ToString(), "Fail to get SMTP configuration");
                return NotFound();
            }

            var smtpViewModel = new SMTPViewModel()
            {
                Id = smtp.Id,
                SMTPHost = smtp.SmtpHost,
                SMTPPort = smtp.SmtpPort,
                SMTPUserName = smtp.SmtpUserName,
                SMTPPassword = smtp.SmtpPwd,
                FromName = smtp.FromName,
                FromEmailAddress = smtp.FromEmailAddr,
                MailSubject = smtp.MailSubject,
                Template = smtp.Template,
                RequireAuthentication = smtp.RequireAuth,
                RequiresSSL = smtp.RequiresSsl
            };
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Get smtp configuration", LogMessageType.SUCCESS.ToString(), "Get SMTP configuration success");
            return View(smtpViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SMTPViewModel viewModel, string actionType)
        {
            var smtpInDb = await _smtpService.GetSMTPSettingsAsync(viewModel.Id);
            if (smtpInDb == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Submit smtp configuration", LogMessageType.FAILURE.ToString(), "Fail to get SMTP configuration in Submit");
                return NotFound();
            }

            smtpInDb.SmtpHost = viewModel.SMTPHost;
            smtpInDb.SmtpPort = viewModel.SMTPPort;
            smtpInDb.SmtpUserName = viewModel.SMTPUserName;
            smtpInDb.SmtpPwd = viewModel.SMTPPassword;
            smtpInDb.FromName = viewModel.FromName;
            smtpInDb.FromEmailAddr = viewModel.FromEmailAddress;
            smtpInDb.MailSubject = viewModel.MailSubject;
            smtpInDb.Template = viewModel.Template;
            smtpInDb.RequireAuth = viewModel.RequireAuthentication;
            smtpInDb.RequiresSsl = viewModel.RequiresSSL;
            smtpInDb.UpdatedBy = UUID;

            if (actionType == "Test SMTP Configuration")
            {
                // Test Settings
                var response = await _smtpService.TestSMTPConnectionAsync(smtpInDb);
                if (response ==null || !response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Submit smtp configuration", LogMessageType.FAILURE.ToString(), "Fail to test SMTP configuration");
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("Index", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Submit smtp configuration", LogMessageType.SUCCESS.ToString(), "Test SMTP configuration success" );
                    Alert alert = new Alert { IsSuccess = true, Message = (string.IsNullOrEmpty(response.Message) ? "SMTP connection Successful" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("Index", viewModel);
                }
            }
            else
            {
                // Update Settings
                var response = await _smtpService.UpdateSMTPSettingsAsync(smtpInDb);
                if (response ==null || !response.Success)
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Submit smtp configuration", LogMessageType.FAILURE.ToString(), "Fail to update SMTP configuration");
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("Index", viewModel);
                }
                else
                {
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Smtp, "Submit smtp configuration", LogMessageType.SUCCESS.ToString(), "Update SMTP configuration success");
                    Alert alert = new Alert { IsSuccess = true, Message = "Record Updated Successful" };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    return View("Index", viewModel);
                }
            }
        }
    }
}
